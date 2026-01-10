# Décisions Prises - Système Shows Quotidiens

## Vue d'ensemble

Ce document récapitule toutes les décisions prises concernant l'implémentation du système jour par jour avec shows quotidiens.

---

## Décisions de Design

### 1. Migration des Shows Existants

**Décision** : Migration manuelle
- Les shows existants qui utilisent encore `Week` ne seront **pas** convertis automatiquement
- L'utilisateur devra replanifier manuellement les shows existants vers des dates précises
- Raison : Plus de contrôle pour l'utilisateur, évite les conversions automatiques incorrectes

**Implémentation** :
- Ajouter un outil de migration dans l'UI permettant de voir les shows avec `Week` et de les replanifier
- Afficher un avertissement si des shows utilisent encore `Week` lors de l'ouverture du calendrier

---

### 2. Vue Calendrier

**Décision** : Vue mensuelle complète
- Calendrier classique avec tous les jours du mois visible
- Navigation mois par mois (précédent/suivant)
- Affichage des shows directement dans les cases du calendrier

**Implémentation** :
- Créer `MonthlyCalendarView.axaml` avec grille calendrier
- Chaque case de jour affiche :
  - Numéro du jour
  - Icônes/indicateurs pour shows planifiés
  - Couleur différente selon type de show (TV, PPV, House, etc.)
- Option future : Vue hebdomadaire en complément

---

### 3. Création Rapide de Shows

**Décision** : Clic sur jour → Menu contextuel
- Clic sur un jour dans le calendrier → Menu contextuel apparaît
- Menu propose les types de shows disponibles :
  - Weekly Show (TV)
  - PPV
  - House Show
  - Tour (créer plusieurs shows)
  - Youth Show

**Implémentation** :
- Menu contextuel (`ContextMenu`) sur chaque case de jour
- Sélection d'un type → Création rapide avec valeurs par défaut
- Option "Formulaire complet" pour personnaliser tous les détails

---

### 4. Shows Récurrents

**Décision** : Système de templates
- Créer des templates de shows récurrents (ex: "Monday Night Raw")
- Template définit :
  - Nom du show
  - Type (TV, House, etc.)
  - Jour de la semaine (ou pattern)
  - Durée par défaut
  - Lieu par défaut
- Générer automatiquement les shows à partir du template

**Implémentation** :
- Table `ShowTemplates` en base de données
- Interface pour créer/gérer templates
- Bouton "Générer depuis template" dans le calendrier
- Génération automatique pour les prochaines N semaines

---

### 5. Contrôle Child Companies

**Décision** : Niveaux complets (comme compagnie principale)
- Les child companies peuvent avoir les mêmes niveaux de contrôle que la compagnie principale :
  - **Spectator** : IA contrôle 100%
  - **Producer** : IA propose, joueur valide
  - **CoBooker** : Partage responsabilités (joueur = main events, IA = midcard)
  - **Dictator** : Contrôle total du joueur

**Implémentation** :
- Table `ChildCompanyBookingControl` avec colonne `ControlLevel`
- Interface similaire à celle de la compagnie principale
- Possibilité de changer le niveau à tout moment
- Impact immédiat sur les prochains shows à booker

---

### 6. Planification Automatique IA

**Décision** : Planification à l'avance (4-8 semaines)
- Les compagnies IA planifient automatiquement leurs shows pour les 4-8 prochaines semaines
- Planification basée sur :
  - `OwnerDecisionEngine.GetOptimalShowFrequency()` : Fréquence préférée de l'owner
  - Disponibilité des venues
  - Conflits avec autres compagnies/brands

**Implémentation** :
- `DailyShowSchedulerService.PlanifierShowsAutomatiques()` appelé :
  - Au démarrage d'une nouvelle partie
  - Quand le joueur avance significativement dans le temps (ex: +1 mois)
  - Option manuelle : "Planifier shows IA"
- Créer shows avec `Status = ABOOKER`
- Booker IA génère automatiquement les cartes selon `ControlLevel`

---

### 7. Segments Diffusés

**Décision** : Toutes les features (indicateurs + contraintes + analytics)

#### 7.1 Indicateurs Visuels
- Icône 📺 sur segments qui seront diffusés
- Badge "Dark Match" sur segments non diffusés
- Couleur différente dans la liste des segments

#### 7.2 Contraintes TV
- Validation selon contraintes du `TvDeal` :
  - Durée minimale/maximale du show
  - Segments obligatoires (ex: promo d'ouverture)
  - Restrictions sur certains types de matchs (ex: pas de hardcore en prime time)
- Avertissements si contraintes non respectées

#### 7.3 Analytics
- Historique audience par segment
- Comparaison segments diffusés vs non diffusés
- Recommandations pour améliorer audience
- Graphiques d'évolution

**Implémentation** :
- Ajouter champ `IsBroadcast` à `SegmentDefinition` (dérivé du show)
- `BookingValidator` vérifie contraintes TV
- Nouvelle vue `SegmentAnalyticsView` pour analytics

---

### 8. Gestion Conflits de Calendrier

**Décision** : Avertir si même compagnie, permettre si brands différentes

**Règles** :
- **Brand = Entité séparée** : Une compagnie peut avoir plusieurs brands
- **Même compagnie, même jour** : Avertir l'utilisateur mais permettre (cas rares mais possibles)
- **Brands différentes** : Pas de conflit, plusieurs shows OK le même jour
- **Même brand, même jour** : Conflit détecté, avertir fortement

**Implémentation** :
- Ajouter colonne `BrandId` à table `Shows` (nullable, peut être null pour compagnies sans brands)
- `ShowSchedulerService.DetecterConflitCalendrier()` vérifie :
  - Si `BrandId` identique → Conflit
  - Si `BrandId` différent ou null → Pas de conflit
- Afficher avertissement avec niveau de sévérité selon le cas

**Exemples** :
- ✅ WWE Raw (Brand A) + WWE SmackDown (Brand B) le même jour → OK
- ⚠️ WWE Raw (Brand A) + WWE Raw (Brand A) le même jour → Avertir
- ✅ Compagnie sans brand + Autre compagnie → OK

---

## Modifications du Plan d'Implémentation

### Ajouts nécessaires

1. **Table `Brands`** :
   ```sql
   CREATE TABLE IF NOT EXISTS Brands (
       BrandId TEXT PRIMARY KEY,
       CompanyId TEXT NOT NULL,
       Name TEXT NOT NULL,
       CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
       FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
   );
   ```

2. **Colonne `BrandId` dans `Shows`** :
   ```sql
   ALTER TABLE Shows ADD COLUMN BrandId TEXT;
   CREATE INDEX IF NOT EXISTS idx_shows_brand_date ON Shows(BrandId, Date);
   ```

3. **Table `ShowTemplates`** :
   ```sql
   CREATE TABLE IF NOT EXISTS ShowTemplates (
       TemplateId TEXT PRIMARY KEY,
       CompanyId TEXT NOT NULL,
       Name TEXT NOT NULL,
       ShowType TEXT NOT NULL,
       RecurrencePattern TEXT NOT NULL, -- 'Weekly', 'BiWeekly', 'Monthly', 'Custom'
       DayOfWeek INTEGER, -- 0-6 (Lundi-Dimanche)
       DefaultDuration INTEGER NOT NULL,
       DefaultVenueId TEXT,
       IsActive INTEGER NOT NULL DEFAULT 1,
       FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
   );
   ```

4. **Vue mensuelle** :
   - Créer `MonthlyCalendarView.axaml` au lieu de vue hebdomadaire
   - Navigation mois par mois
   - Affichage compact des shows dans chaque case

5. **Menu contextuel création** :
   - `ContextMenu` sur chaque case de jour
   - Options : Types de shows + "Formulaire complet"

6. **Système de brands** :
   - Interface pour créer/gérer brands
   - Assignation de brand lors de création de show
   - Validation conflits basée sur brands

---

## Priorités d'Implémentation Révisées

### Phase 1 : Fondations
1. Migration DB (colonnes Date, BrandId, tables ShowTemplates, ChildCompanyBookingControl)
2. Modèles Core (ajout BrandId, ShowTemplate, etc.)
3. Migration manuelle des shows existants (outil UI)

### Phase 2 : Vue Calendrier
1. MonthlyCalendarView (vue mensuelle)
2. Menu contextuel création rapide
3. Affichage shows dans calendrier

### Phase 3 : Création et Planification
1. ShowSchedulerService.CreerShowRapide()
2. Système templates shows récurrents
3. DailyShowSchedulerService (planification IA)

### Phase 4 : Child Companies
1. ChildCompanyBookingService
2. Interface contrôle booking
3. Intégration avec planification automatique

### Phase 5 : Segments et Diffusion
1. Indicateurs visuels diffusion
2. Contraintes TV dans validation
3. Analytics audience par segment

### Phase 6 : Gestion Brands et Conflits
1. Système brands (CRUD)
2. Détection conflits basée sur brands
3. Avertissements et résolution

---

## Questions Résolues

✅ Migration : Manuelle (utilisateur replanifie)  
✅ Vue calendrier : Mensuelle  
✅ Création show : Clic → Menu contextuel  
✅ Shows récurrents : Système templates  
✅ Child companies : Niveaux complets  
✅ Planification IA : À l'avance (4-8 semaines)  
✅ Segments diffusés : Toutes features  
✅ Conflits : Avertir si même compagnie, OK si brands différentes  

---

## Prochaines Étapes

1. Valider ces décisions avec l'utilisateur
2. Mettre à jour le plan d'implémentation détaillé
3. Commencer l'implémentation selon les priorités définies

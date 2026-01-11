# Flux Système Shows Quotidiens - Ring General

## Vue d'ensemble

Ce document présente les diagrammes de flux pour le système jour par jour avec shows quotidiens, et clarifie ce qui existe déjà vs ce qui doit être développé.

---

## État actuel du système

### ✅ Déjà implémenté

1. **Segments de shows** :
   - Types de segments : `match`, `promo`, `angle_backstage`
   - Modèle `SegmentDefinition` avec participants, durée, intensité
   - Templates de segments dans `specs/library/segments.fr.json`
   - Système de validation (`BookingValidator`)

2. **Types de matchs** :
   - Singles, Tag Team, Triple Threat, Fatal 4-Way, Battle Royal, Ladder Match, Cage Match
   - Table `match_types` en base de données
   - `MatchTypeViewModel` dans l'UI

3. **Diffusion TV** :
   - Modèle `TvDeal` avec revenus, audience, contraintes
   - Calcul d'audience basé sur Reach, ShowScore, Stars, Saturation
   - Revenus TV calculés dans `ShowSimulationEngine`
   - Table `tv_deals` en base de données

4. **Simulation de shows** :
   - `ShowSimulationEngine` : Simule les segments et calcule les résultats
   - `ShowDayOrchestrator` : Orchestre le flux complet d'un show
   - Application des impacts (finances, popularité, titres, blessures)

### ⚠️ Partiellement implémenté

1. **Système jour par jour** :
   - `TimeOrchestratorService.PasserJourSuivant()` existe
   - `GameRepository.IncrementerJour()` existe
   - Mais les shows sont encore référencés par `Week` dans plusieurs endroits

2. **Création de shows** :
   - `ShowSchedulerService.CreerShow()` existe avec `DateOnly`
   - Mais pas de création rapide depuis le calendrier
   - Pas de système de shows récurrents

3. **Child Companies** :
   - Modèle `ChildCompanyExtended` avec `HasFullAutonomy`
   - Mais pas de système de contrôle de booking dédié

### ❌ À développer

1. **Vue calendrier jour par jour** : Affichage des shows par jour (actuellement par semaine)
2. **Création rapide de shows** : Bouton "Ajouter show" sur chaque jour du calendrier
3. **Shows récurrents** : Templates pour shows hebdomadaires (ex: "Monday Night Raw" chaque lundi)
4. **Gestion contrôle child companies** : Interface pour prendre le contrôle ou déléguer
5. **Planification automatique IA** : Création automatique de shows pour compagnies IA

---

## Diagrammes de flux

### Flux 1 : Création rapide d'un show depuis le calendrier

```mermaid
flowchart TD
    A[Utilisateur clique sur un jour dans le calendrier] --> B{Type de show?}
    B -->|Weekly Show| C[ShowSchedulerService.CreerShowRapide]
    B -->|PPV| D[Formulaire complet PPV]
    B -->|Tour| E[Créer plusieurs shows consécutifs]
    B -->|House Show| C
    
    C --> F[Valeurs par défaut:<br/>- Durée: 120 min<br/>- Type: TV<br/>- Broadcast: TBA<br/>- Status: ABOOKER]
    F --> G[Vérifier conflits calendrier]
    G -->|Conflit| H[Afficher erreur]
    G -->|OK| I[Créer ShowSchedule avec DateOnly]
    I --> J[Ajouter CalendarEntry]
    J --> K[Show ajouté à ShowsParJour]
    K --> L[Calendrier mis à jour visuellement]
    
    D --> M[Formulaire avec tous les champs]
    M --> N[Validation complète]
    N --> I
    
    E --> O[Créer ShowTemplate]
    O --> P[Générer shows pour chaque date]
    P --> I
```

### Flux 2 : Progression jour par jour avec détection de shows

```mermaid
flowchart TD
    A[Utilisateur clique 'Jour suivant'] --> B[TimeOrchestratorService.PasserJourSuivant]
    B --> C[GameRepository.IncrementerJour]
    C --> D[CurrentDay++, CurrentDate++]
    D --> E[DailyServices.UpdateDailyStats]
    E --> F[Mise à jour fatigue/blessures workers]
    
    F --> G[DailyShowSchedulerService.PlanifierShowsAutomatiques]
    G --> H{Compagnies IA?}
    H -->|Oui| I[OwnerDecisionEngine.GetOptimalShowFrequency]
    I --> J[Créer shows automatiquement si nécessaire]
    J --> K[Booker IA génère cartes]
    
    H -->|Non| L[ShowDayOrchestrator.DetecterShowAVenir]
    K --> L
    
    L --> M{Show détecté aujourd'hui?}
    M -->|Oui| N{Compagnie joueur?}
    M -->|Non| O[DailyTickResult avec événements]
    
    N -->|Oui| P[Afficher notification:<br/>'Show prévu aujourd'hui']
    N -->|Non| Q[Simuler automatiquement]
    
    P --> R{Utilisateur veut simuler?}
    R -->|Oui| S[ShowDayOrchestrator.ExecuterFluxComplet]
    R -->|Non| T[Reporter à plus tard]
    
    Q --> S
    S --> U[Simulation complète:<br/>- Générer résultats<br/>- Appliquer impacts<br/>- Frais d'apparition<br/>- Moral workers]
    U --> O
```

### Flux 3 : Booking d'un show (création de segments)

```mermaid
flowchart TD
    A[Utilisateur ouvre un show à booker] --> B[ShowBookingViewModel.LoadBooking]
    B --> C[Charger ShowContext:<br/>- Workers disponibles<br/>- Titres<br/>- Storylines actives]
    
    C --> D{Contrôle booking?}
    D -->|Dictator| E[Utilisateur crée manuellement]
    D -->|CoBooker| F[Utilisateur crée main event<br/>IA complète midcard]
    D -->|Producer| G[IA propose carte complète<br/>Utilisateur valide/modifie]
    D -->|Spectator| H[IA génère tout automatiquement]
    
    E --> I[Créer segment:<br/>1. Choisir type segment]
    I --> J{Type segment?}
    J -->|Match| K[Sélectionner type match:<br/>Singles/Tag/Triple/etc.]
    J -->|Promo| L[Sélectionner participants]
    J -->|Angle Backstage| L
    
    K --> M[Sélectionner participants<br/>selon type match]
    M --> N[Configurer segment:<br/>- Durée<br/>- Intensité<br/>- Storyline?<br/>- Titre?]
    
    L --> N
    N --> O{Main event?}
    O -->|Oui| P[Marquer EstMainEvent = true]
    O -->|Non| Q[Valider segment]
    
    P --> Q
    Q --> R[BookingValidator.Valider]
    R --> S{Validation OK?}
    S -->|Non| T[Afficher erreurs:<br/>- Durée totale<br/>- Disponibilité workers<br/>- Fatigue excessive]
    S -->|Oui| U[Sauvegarder segment]
    
    T --> N
    
    U --> V{Show complet?}
    V -->|Non| I
    V -->|Oui| W[Option: Simuler show]
    
    F --> X[BookerAIEngine.GenerateAutoBooking]
    G --> X
    H --> X
    X --> Y[Générer segments selon:<br/>- Préférences booker<br/>- Mémoires<br/>- Storylines actives<br/>- Contraintes]
    Y --> U
```

### Flux 4 : Simulation d'un show avec diffusion TV

```mermaid
flowchart TD
    A[Simuler show] --> B[ShowDayOrchestrator.ExecuterFluxComplet]
    B --> C[Charger ShowContext complet]
    C --> D[ShowSimulationEngine.Simuler]
    
    D --> E[Pour chaque segment:<br/>Calculer note individuelle]
    E --> F{Type segment?}
    F -->|Match| G[Calculer note match:<br/>- Stats workers<br/>- Chimie<br/>- Durée<br/>- Type match<br/>- Intensité]
    F -->|Promo| H[Calculer note promo:<br/>- Entertainment<br/>- Story<br/>- Popularité]
    F -->|Angle Backstage| I[Calculer note angle:<br/>- Story<br/>- Heat storyline]
    
    G --> J[Note globale show]
    H --> J
    I --> J
    
    J --> K{Show diffusé?}
    K -->|Oui| L[Charger TvDeal]
    K -->|Non| M[Calculer seulement:<br/>- Billetterie<br/>- Merch]
    
    L --> N[Calculer audience:<br/>- Reach + ReachBonus<br/>- ShowScore<br/>- Stars<br/>- Saturation]
    N --> O[Calculer revenus TV:<br/>BaseRevenue + Audience * RevenuePerPoint]
    O --> P{Vérifier contraintes TV}
    P -->|Audience < Minimum| Q[Appliquer pénalité]
    P -->|OK| R[Revenus TV complets]
    
    Q --> R
    R --> S[Total revenus:<br/>Billetterie + Merch + TV]
    M --> S
    
    S --> T[Appliquer impacts:<br/>- Finances<br/>- Popularité workers<br/>- Popularité compagnie<br/>- Momentum<br/>- Titres changés]
    T --> U[Traiter frais d'apparition<br/>pour tous participants]
    U --> V[Moral workers non utilisés]
    V --> W[ShowDayFinalizationResult]
    W --> X[Afficher résultats:<br/>- Note globale<br/>- Audience<br/>- Revenus<br/>- Changements titres]
```

### Flux 5 : Gestion Child Company autonome

```mermaid
flowchart TD
    A[Child Company avec HasFullAutonomy = true] --> B[DailyShowSchedulerService.PlanifierShowsAutomatiques]
    B --> C[OwnerDecisionEngine.GetOptimalShowFrequency]
    C --> D{Fréquence?}
    D -->|Weekly| E[Créer show chaque lundi]
    D -->|BiWeekly| F[Créer show tous les 14 jours]
    D -->|Monthly| G[Créer show dernier dimanche]
    
    E --> H[ShowSchedulerService.CreerShowRapide]
    F --> H
    G --> H
    
    H --> I[Show créé avec Status = ABOOKER]
    I --> J[ChildCompanyBookingService.GetBookingControlLevel]
    J --> K{Contrôle actuel?}
    
    K -->|Spectator| L[Booker IA génère carte complète]
    K -->|Producer| M[Booker IA propose<br/>Owner valide]
    K -->|CoBooker| N[Owner crée main event<br/>Booker complète]
    K -->|Dictator| O[Owner contrôle total]
    
    L --> P[ShowStatus = BOOKE]
    M --> Q{Owner valide?}
    Q -->|Oui| P
    Q -->|Non| R[Modifier segments]
    R --> P
    
    N --> P
    O --> P
    
    P --> S[Jour du show arrive]
    S --> T[ShowDayOrchestrator.DetecterShowAVenir]
    T --> U[Simuler automatiquement]
    U --> V[Résultats appliqués]
```

### Flux 6 : Prise de contrôle d'une Child Company

```mermaid
flowchart TD
    A[Utilisateur ouvre ChildCompanyBookingView] --> B[Afficher liste child companies]
    B --> C[Sélectionner child company]
    C --> D[Afficher état actuel:<br/>- HasFullAutonomy<br/>- ControlLevel<br/>- Shows planifiés]
    
    D --> E{Action utilisateur?}
    E -->|Prendre contrôle| F[Changer ControlLevel]
    E -->|Déléguer à IA| G[Set HasFullAutonomy = true]
    E -->|Voir calendrier| H[Afficher shows child company]
    
    F --> I{Quel niveau?}
    I -->|Dictator| J[Contrôle total<br/>Pas d'IA]
    I -->|CoBooker| K[Partage responsabilités]
    I -->|Producer| L[IA propose, valide]
    I -->|Spectator| M[IA contrôle tout]
    
    J --> N[ChildCompanyBookingService.SetBookingControlLevel]
    K --> N
    L --> N
    M --> N
    
    G --> O[ChildCompanyBookingService.SetAutonomy]
    O --> P[Booker IA prend contrôle]
    
    N --> Q[Prochains shows nécessitent<br/>intervention utilisateur]
    P --> R[Prochains shows générés<br/>automatiquement]
    
    H --> S[Afficher calendrier avec shows:<br/>- Date<br/>- Nom<br/>- Statut<br/>- Nombre segments]
```

---

## Détails techniques : Segments et Diffusion

### Types de segments existants

D'après `specs/library/segments.fr.json` et le code :

1. **Match** (`typeSegment: "match"`)
   - Types de matchs : Singles, Tag, Triple Threat, Fatal 4-Way, Battle Royal, Ladder, Cage
   - Paramètres : Participants, Durée, Intensité, Titre (optionnel), Storyline (optionnel)
   - Calcul de note basé sur : Stats workers, Chimie, Durée, Type match

2. **Promo** (`typeSegment: "promo"`)
   - Participants : 1-3 workers généralement
   - Paramètres : Durée (4-10 min), Intensité
   - Calcul de note basé sur : Entertainment, Story, Popularité

3. **Angle Backstage** (`typeSegment: "angle_backstage"`)
   - Participants : 2+ workers
   - Paramètres : Durée (3-8 min), Intensité
   - Calcul de note basé sur : Story, Heat storyline

### Système de diffusion TV

**Modèle actuel** (`TvDeal`) :
- `NetworkName` : Nom du diffuseur
- `ReachBonus` : Bonus de reach (0-100)
- `AudienceCap` : Plafond d'audience
- `MinimumAudience` : Audience minimale requise
- `BaseRevenue` : Revenu fixe par show
- `RevenuePerPoint` : Revenu par point d'audience
- `Penalty` : Pénalité si audience < minimum

**Calcul d'audience** (`AudienceModel`) :
```
Audience = f(Reach, ShowScore, Stars, Saturation)
- Reach : Reach de la compagnie + ReachBonus du deal
- ShowScore : Note globale du show (0-100)
- Stars : Star power des participants
- Saturation : Pénalité si trop de shows récents
```

**Calcul revenus TV** :
```
Revenu = BaseRevenue + (Audience * RevenuePerPoint)
Si Audience < MinimumAudience :
  Revenu = Revenu - Penalty
```

### Ce qui doit être développé

1. **Affichage diffusion dans segments** :
   - Indicateur visuel si segment sera diffusé
   - Impact de la diffusion sur la note du segment
   - Segments "dark matches" (non diffusés)

2. **Gestion contraintes TV** :
   - Durée minimale/maximale de show selon deal
   - Types de segments obligatoires (ex: promo d'ouverture)
   - Restrictions sur certains types de matchs

3. **Analytics diffusion** :
   - Historique audience par segment
   - Comparaison audience segments diffusés vs non diffusés
   - Recommandations pour améliorer audience

---

## Recommandations de développement

### Priorité 1 : Système jour par jour
- Migration shows Week → Date
- Vue calendrier hebdomadaire
- Création rapide de shows

### Priorité 2 : Child Companies
- Interface contrôle booking
- Planification automatique IA
- Gestion autonomie

### Priorité 3 : Améliorations segments/diffusion
- Indicateurs visuels diffusion
- Contraintes TV dans validation
- Analytics audience par segment

### Priorité 4 : Features avancées
- Shows récurrents (templates)
- Tours multi-dates
- Suggestions intelligentes de planification

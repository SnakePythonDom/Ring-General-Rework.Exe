# 🎭 Ring General — Wrestling Promotion Manager

**Un jeu de gestion de compagnie de catch professionnel** (style Football Manager × Total Extreme Wrestling)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.0.6-8B44AC)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Architecture](https://img.shields.io/badge/architecture-8.5%2F10-success)](docs/ARCHITECTURE_REVIEW_FR.md)

---

## 📊 État Actuel du Projet

**Version :** Phase 2.0+ — ~55-60% complété
**Dernière mise à jour :** Janvier 2026

### ✅ Ce Qui Est Fait

- **Architecture exemplaire** : 30+ repositories spécialisés créés et enregistrés en DI
- **Refactoring majeur réussi** : GameRepository transformé en façade orchestrant les repositories spécialisés
- **Systèmes backstage sophistiqués** : Moral, Rumeurs, Népotisme, Crises, IA Booker/Propriétaire
- **40 attributs de performance** détaillés (In-Ring, Entertainment, Story, Mental)
- **25+ profils de personnalité** automatiques (style Football Manager)
- **🆕 Système d'Auto-Booking IA** : Le Booker génère automatiquement des cartes complètes 🎯
- **🆕 Flux Show Day complet** : Simulation de bout en bout avec impacts automatiques
- **70+ ViewModels** créés avec navigation complète
- **Base de données SQLite** avec 30 migrations et import automatique BAKI
- **Dependency Injection complète** : Microsoft.Extensions.DependencyInjection intégré dans App.axaml.cs
- **Compilation réussie** : Solution complète avec 0 erreurs, 1 avertissement mineur

### ⏳ En Cours

- Interface utilisateur (13+ vues créées, autres en développement)
- Boucle de jeu hebdomadaire (éléments séparés, orchestration en cours)
- Composants UI réutilisables
- Documentation des nouveaux systèmes backstage

---

## 🚀 Démarrage Rapide

### Prérequis

- **.NET 8.0 SDK** ou ultérieur
- **Windows/Linux/macOS** (Avalonia cross-platform)
- **Visual Studio 2022+** / **Rider** / **VS Code** recommandé

### Installation

```bash
# Cloner le repository
git clone https://github.com/SnakePythonDom/Ring-General-Rework.Exe.git
cd Ring-General-Rework.Exe

# Restaurer les dépendances
dotnet restore RingGeneral.sln

# Lancer l'application
dotnet run --project src/RingGeneral.UI/RingGeneral.UI.csproj
```

**Pour plus de détails :** Consultez le [Guide de démarrage rapide](docs/QUICK_START_GUIDE.md)

---

## 📚 Documentation

### 📌 Documents de Référence

| Document | Description |
|----------|-------------|
| **[docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md)** | ⭐ État consolidé du projet (source de vérité unique) |
| **[docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)** | Analyse architecture (v2.3, Note: 8.5/10) |
| **[docs/ROADMAP_MISE_A_JOUR.md](docs/ROADMAP_MISE_A_JOUR.md)** | Plan de développement (Phases 1-5, Release Avril 2026) |
| **[docs/INDEX.md](docs/INDEX.md)** | Index complet de toute la documentation |

### 📖 Guides Utilisateur

- **[docs/QUICK_START_GUIDE.md](docs/QUICK_START_GUIDE.md)** — Guide de démarrage rapide
- **[docs/DEV_GUIDE_FR.md](docs/DEV_GUIDE_FR.md)** — Guide de développement & modding
- **[docs/DATABASE_GUIDE_FR.md](docs/DATABASE_GUIDE_FR.md)** — Guide de la base de données SQLite
- **[docs/IMPORT_GUIDE_FR.md](docs/IMPORT_GUIDE_FR.md)** — Import de bases de données

---

## 🏗️ Architecture & Technologies

### Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Framework** | .NET | 8.0 LTS |
| **Langage** | C# | 12 |
| **UI Framework** | Avalonia | 11.0.6 |
| **Reactive UI** | ReactiveUI | (via Avalonia) |
| **Base de données** | SQLite | 8.0.0 |

### Architecture

```
┌─────────────────────────────────────┐
│  UI (Avalonia MVVM)                 │ RingGeneral.UI (70+ ViewModels)
├─────────────────────────────────────┤
│  Business Logic (Domain Services)   │ RingGeneral.Core (50+ Services)
├─────────────────────────────────────┤
│  Data Access (30+ Repositories)     │ RingGeneral.Data
├─────────────────────────────────────┤
│  Configuration (JSON Specs)         │ RingGeneral.Specs
└─────────────────────────────────────┘
```

**Points forts :**
- ✅ 30+ repositories spécialisés (modulaire et maintenable)
- ✅ GameRepository transformé en façade orchestrant les repositories
- ✅ Immutable records (C# 12)
- ✅ Dependency Injection complète (Microsoft.Extensions.DependencyInjection)
- ✅ Clean architecture (pas de dépendances circulaires)
- ✅ Configuration data-driven (JSON specs)
- ✅ 30 migrations SQL pour schéma évolutif

**Pour plus de détails :** Consultez l'[Analyse d'architecture](docs/ARCHITECTURE_REVIEW_FR.md)

---

## 📁 Structure du Projet

```
Ring-General-Rework.Exe/
├── src/                    # Code source C# (.NET 8.0)
│   ├── RingGeneral.UI/     # Interface Avalonia (70+ ViewModels, 14 Views)
│   ├── RingGeneral.Core/   # Logique métier (205 fichiers C#)
│   ├── RingGeneral.Data/   # Accès données (60 fichiers C#, 18 SQL)
│   ├── RingGeneral.Specs/  # Configuration JSON (10 fichiers)
│   └── RingGeneral.Tools.* # Outils CLI (BakiImporter, DbManager)
├── sql/                    # Scripts SQL (schema, imports, seeds)
├── specs/                  # Fichiers JSON de configuration
├── docs/                   # Documentation complète (24 docs actifs)
├── data/                   # Assets & base de test (BAKI1.1.db)
│   └── migrations/         # 30 migrations SQL
├── tests/                  # Tests unitaires
└── _archived_files/        # Archives (30+ docs obsolètes)
```

---

## 🎯 Vision Produit

**Ring General** est un jeu de gestion de compagnie de catch professionnel combinant :
- La profondeur de **Football Manager** (attributs détaillés, personnalité, moral)
- La complexité de **Total Extreme Wrestling** (booking, storylines, heat)
- Une interface moderne inspirée de **Football Manager 2026**

### Boucle de Jeu Hebdomadaire

1. **Inbox** — Emails, incidents, demandes, offres
2. **Scouting** — Rapports, découverte de talents
3. **Négociations** — Contrats, partenariats, diffusion
4. **Préparation Show** — Booking, scripts, consignes
5. **Show** — Exécution en direct
6. **Résultats** — Ratings, heat, blessures, finances
7. **Gestion** — Staff, formation, médical, discipline

---

## 🎮 Flux des Actions du Joueur

Chaque action du joueur suit un flux précis qui impacte l'écosystème du jeu. Voici le détail de chaque action :

### 📬 1. Inbox (Boîte de Réception)

**Objectif** : Traiter les messages entrants et événements automatiques

**Flux** :
1. **Réception automatique** des événements hebdomadaires :
   - Actualités et rumeurs du monde du catch
   - Notifications de blessures
   - Alertes de fins de contrat (30 jours avant expiration)
   - Rapports de scouting hebdomadaires
   - Offres TV (nouveaux deals, renouvellements)
   - Incidents backstage (morale, rumeurs, crises)
   - Génération de nouveaux workers (si activée)

2. **Actions du joueur** :
   - Consulter les messages par catégorie
   - Marquer comme lu/non lu
   - Répondre aux offres (contrats, TV deals)
   - Archiver les messages traités

3. **Sorties** :
   - Dossiers organisés par type
   - Actions rapides disponibles (signer contrat, accepter offre TV)
   - Notifications persistantes jusqu'à traitement

**Déclencheurs** : Génération automatique chaque semaine via `WeeklyLoopService`

---

### 🔍 2. Scouting (Recherche de Talents)

**Objectif** : Découvrir et évaluer de nouveaux talents pour le roster

**Flux** :
1. **Génération hebdomadaire** :
   - Rapports de scouting automatiques (niveau 0/1/2)
   - Découverte de workers libres
   - Tryouts disponibles

2. **Actions du joueur** :
   - Consulter les rapports de scouting
   - Filtrer par attributs, popularité, disponibilité
   - Ajouter à la shortlist
   - Lancer un tryout (évaluation approfondie)
   - Générer un nouveau worker (si système activé)

3. **Niveaux de scouting** :
   - **Niveau 0** : Informations basiques (nom, âge, région)
   - **Niveau 1** : Attributs In-Ring et Entertainment visibles
   - **Niveau 2** : Tous les attributs visibles (y compris Mental)

4. **Sorties** :
   - Shortlists personnalisées
   - Rapports détaillés avec recommandations
   - Notifications de nouveaux talents disponibles

**Déclencheurs** : Génération hebdomadaire via `ScoutingService`

---

### 💼 3. Négociations (Contrats & Partenariats)

**Objectif** : Gérer les contrats des workers et les partenariats TV

**Flux** :

#### 3.1 Négociation de Contrats

1. **Déclenchement** :
   - Contrats arrivant à échéance (30 jours avant)
   - Offres spontanées de workers libres
   - Contre-offres après refus initial

2. **Actions du joueur** :
   - Consulter les offres reçues
   - Négocier les termes :
     - Salaire mensuel garanti (0-100% du total)
     - Frais d'apparition (per-appearance)
     - Durée du contrat
     - Type (Exclusif, PPA, Handshake)
   - Accepter ou refuser l'offre
   - Faire une contre-offre

3. **Calcul financier** :
   - **FLUX 1** : Paiement mensuel garanti (dernier jour du mois)
   - **FLUX 2** : Frais d'apparition (immédiatement après chaque show)

4. **Sorties** :
   - Contrats signés ajoutés au roster
   - Refus enregistrés (possibilité de renégocier plus tard)
   - Alertes d'expiration pour contrats existants

#### 3.2 Négociation TV Deals

1. **Déclenchement** :
   - Offres de nouvelles chaînes
   - Renouvellement de deals existants
   - Annulation de deals (si ratings trop bas)

2. **Actions du joueur** :
   - Consulter les offres TV
   - Négocier les termes :
     - Montant par show
     - Durée du contrat
     - Exclusivité
   - Accepter ou refuser

3. **Sorties** :
   - Deals TV actifs
   - Revenus TV ajoutés aux finances après chaque show

**Déclencheurs** : Génération hebdomadaire via `WeeklyLoopService` et `TvDealNegotiationService`

---

### 📝 4. Booking (Préparation du Show)

**Objectif** : Construire la carte complète du show avant l'exécution

**Flux** :

#### 4.1 Booking Manuel

1. **Sélection du show** :
   - Choisir un show à booker (statut : À Booker)
   - Consulter les contraintes (durée, workers disponibles, storylines actives)

2. **Construction de la carte** :
   - Créer des segments (matches, promos, angles backstage)
   - Sélectionner les participants pour chaque segment
   - Définir le vainqueur (si match)
   - Assigner des titres (si match de championnat)
   - Lier à des storylines existantes
   - Définir l'ordre des segments
   - Valider la durée totale

3. **Validation** :
   - Vérifier les disponibilités (blessures, fatigue)
   - Valider les contraintes Owner (budget, workers interdits)
   - Confirmer la cohérence narrative

4. **Sorties** :
   - Carte complète sauvegardée
   - Show prêt à être simulé (statut : Prêt)

#### 4.2 Auto-Booking IA (🤖)

1. **Déclenchement** :
   - Bouton "🤖 Laisser le Booker préparer le show"
   - Peut compléter une carte partiellement remplie ou créer une carte complète

2. **Logique IA** :
   - Le Booker IA analyse :
     - Storylines actives
     - Mémoires des événements passés
     - Préférences du Booker (style de produit, favoris)
     - Contraintes Owner (budget, fatigue max, workers interdits)
   - Génère automatiquement :
     - 4-8 segments selon la durée du show
     - Main event avec storyline ou titre
     - Mid-card matches
     - Promos et angles backstage

3. **Styles de produit** :
   - **Hardcore** : Matches violents, hardcore brawls
   - **Puroresu** : Style japonais, matches longs et techniques
   - **Technical** : Lutte technique pure
   - **Entertainment** : Promos, angles, drama
   - **Balanced** : Mix équilibré de tout

4. **Actions du joueur** :
   - Consulter la carte générée
   - Modifier les segments générés
   - Valider ou régénérer

5. **Sorties** :
   - Carte complète générée automatiquement
   - Show prêt à être simulé

**Déclencheurs** : Action manuelle du joueur via `BookerAIEngine`

---

### 🎬 5. Show Day (Exécution du Show)

**Objectif** : Simuler l'exécution complète du show et appliquer tous les impacts

**Flux** :

1. **Détection automatique** :
   - Le système détecte un show à venir pour le jour actuel
   - Vérifie que le show est booké (statut : Prêt)

2. **Chargement du contexte** :
   - Charge le `ShowContext` complet :
     - Définition du show
     - Tous les segments bookés
     - Workers participants avec leurs attributs
     - Storylines actives
     - Titres en jeu
     - Compagnie et finances

3. **Simulation segment par segment** :
   - Pour chaque segment :
     - Calcul de la note basé sur les attributs des participants
     - Facteurs de qualité (chimie, storyline heat, type de match)
     - Génération d'incidents aléatoires (blessures, accidents)
     - Calcul de l'audience et des revenus

4. **Application des impacts** (via `ShowDayOrchestrator.ExecuterFluxComplet()`) :
   - **Finances** :
     - Revenus billetterie, merch, TV
     - **FLUX 2** : Déduction immédiate des frais d'apparition
   - **Popularité** :
     - Gain/perte selon performance et résultat
   - **Momentum** :
     - Vainqueurs gagnent du momentum
     - Perdants perdent du momentum
   - **Fatigue** :
     - Augmentation selon durée et intensité du match
   - **Blessures** :
     - Risque de blessure selon Safety et type de match
   - **Titres** :
     - Changement de détenteur si match de championnat
     - Prestige du titre ajusté
   - **Storylines** :
     - Heat augmenté selon qualité du segment
     - Progression automatique des phases (BUILD → PEAK → BLOWOFF)
   - **Moral** :
     - Workers utilisés : moral stable ou amélioré
     - **Workers non utilisés : -3 points de moral** (impact négatif)

5. **Finalisation** :
   - Statut du show changé à "Simulé"
   - Résultats enregistrés dans l'historique
   - InboxItems créés pour événements importants (blessures, changements de titre)

6. **Sorties** :
   - Rapport complet du show (note globale, audience, revenus)
   - Détails segment par segment
   - Liste des changements (titres, popularité, moral)
   - Notifications dans l'Inbox

**Déclencheurs** : Bouton "Continuer" sur Dashboard ou détection automatique via `ShowDayOrchestrator`

---

### 📊 6. Résultats (Analyse Post-Show)

**Objectif** : Consulter les résultats détaillés et leurs impacts

**Flux** :

1. **Affichage automatique** après simulation :
   - Note globale du show (/100)
   - Audience totale
   - Revenus détaillés (billetterie, merch, TV)

2. **Détails par segment** :
   - Note individuelle de chaque segment
   - Participants et leurs performances
   - Résultats (vainqueur/perdant)
   - Impacts sur popularité et momentum

3. **Impacts globaux** :
   - Changements de popularité des workers
   - Changements de popularité de la compagnie
   - Progression des storylines (heat)
   - Changements de titres
   - Blessures survenues

4. **Actions du joueur** :
   - Consulter l'historique des shows
   - Comparer avec les shows précédents
   - Analyser les tendances (ratings, audience)

5. **Sorties** :
   - Historique complet des shows
   - Graphiques de progression
   - Statistiques détaillées

**Déclencheurs** : Automatique après simulation via `ShowSimulationResult`

---

### 🏥 7. Gestion (Staff, Médical, Discipline)

**Objectif** : Gérer les aspects internes de la compagnie

**Flux** :

#### 7.1 Gestion Médicale

1. **Consultation** :
   - Liste des workers blessés
   - Durée de récupération restante
   - Gravité des blessures

2. **Actions** :
   - Consulter les rapports médicaux
   - Planifier le retour des workers
   - Gérer les remplacements temporaires

3. **Sorties** :
   - Rapports médicaux détaillés
   - Alertes de retours imminents

#### 7.2 Gestion du Staff

1. **Consultation** :
   - Liste du staff (Booker, Owner, etc.)
   - Rôles et responsabilités
   - Performance et satisfaction

2. **Actions** :
   - Embaucher/renvoyer du staff
   - Ajuster les responsabilités
   - Gérer les compagnies filles (staff partagé)

3. **Sorties** :
   - Staff actif et disponible
   - Notifications de départs

#### 7.3 Discipline & Backstage

1. **Consultation** :
   - Incidents backstage
   - Moral de la compagnie
   - Rumeurs actives
   - Crises en cours

2. **Actions** :
   - Résoudre les crises
   - Appliquer des sanctions
   - Gérer le moral (réunions, événements)

3. **Sorties** :
   - Moral amélioré/dégradé
   - Crises résolues
   - Rumeurs dissipées

**Déclencheurs** : Consultation manuelle ou notifications automatiques

---

### ⏭️ 8. Passage de Semaine (Avancement du Temps)

**Objectif** : Faire progresser le jeu d'une semaine complète

**Flux** :

1. **Déclenchement** :
   - Bouton "Passer à la semaine suivante" sur Dashboard
   - Vérification que toutes les actions critiques sont complétées

2. **Exécution automatique** (via `WeeklyLoopService.PasserSemaineSuivante()`) :
   - **Incrémentation** : Semaine +1
   - **Récupération de fatigue** : Réduction automatique de la fatigue hebdomadaire
   - **Finances hebdomadaires** : Application des coûts fixes
   - **Génération d'événements** :
     - Génération de nouveaux workers (si activée)
     - Simulation backstage (incidents, morale, rumeurs)
     - Génération de news du monde
     - Vérification des contrats (expirations, alertes)
     - Vérification des offres expirantes
     - Simulation du monde vivant (autres compagnies)
     - Génération de scouting hebdomadaire
   - **Progression des systèmes** :
     - Progression du moral et des rumeurs
     - Progression des crises
     - Déclin des mémoires du Booker (oubli progressif)
     - Auto-booking des shows 1-2 semaines à l'avance (compagnies IA)
     - Analyse structurelle et tendances
     - Progression des transitions d'ADN

3. **Génération d'InboxItems** :
   - Tous les événements générés sont ajoutés à l'Inbox
   - Notifications visibles immédiatement

4. **Mise à jour de l'interface** :
   - Rafraîchissement des données de session
   - Chargement de la nouvelle Inbox
   - Mise à jour du show actuel

5. **Sorties** :
   - Nouvelle semaine active
   - Inbox remplie avec nouveaux événements
   - Écosystème mis à jour (popularité, tendances, etc.)

**Déclencheurs** : Action manuelle du joueur via `GameSessionViewModel.PasserSemaineSuivante()`

---

### 🔄 Cycle Complet d'une Semaine Type

```
LUNDI
├─ Inbox : Traiter les événements de la semaine précédente
├─ Scouting : Consulter les nouveaux rapports
└─ Négociations : Répondre aux offres de contrat/TV

MARDI-MERCREDI
├─ Booking : Préparer la carte du show (manuel ou IA)
└─ Validation : Vérifier et finaliser le booking

JEUDI (SHOW DAY)
├─ Détection : Le système détecte le show à venir
├─ Simulation : Exécution complète du show
├─ Impacts : Application automatique de tous les changements
└─ Résultats : Affichage du rapport complet

VENDREDI
├─ Résultats : Analyser les performances détaillées
└─ Gestion : Gérer le médical, staff, discipline si nécessaire

WEEKEND
└─ Passage de Semaine : Avancer au lundi suivant
```

---

**Note** : Tous ces flux sont orchestrés par des services spécialisés (`ShowDayOrchestrator`, `WeeklyLoopService`, `BookerAIEngine`, etc.) qui garantissent la cohérence et l'automatisation des processus complexes.

---

## 🔄 Flux Fonctionnels des Systèmes

Cette section détaille les flux de traitement internes de chaque système principal du jeu.

### 🎬 Système Show Day (ShowDayOrchestrator)

**Flux complet** : De la détection du show à la finalisation avec impacts

```
1. DÉTECTION
   └─ DetecterShowAVenir(companyId, currentDay)
      ├─ Charger shows planifiés
      ├─ Filtrer statut "À Booker"
      └─ Retourner ShowDayDetectionResult

2. CHARGEMENT CONTEXTE
   └─ ChargerShowContext(showId)
      ├─ ShowDefinition (détails show)
      ├─ Segments (carte complète)
      ├─ Workers (snapshots avec attributs)
      ├─ Storylines (actives)
      ├─ Titres (en jeu)
      └─ Chimies (compatibilités)

3. SIMULATION
   └─ ShowSimulationEngine.Simuler(context)
      ├─ Pour chaque segment :
      │  ├─ Calcul Note In-Ring (40%)
      │  ├─ Calcul Note Entertainment (30%)
      │  ├─ Calcul Note Story (30%)
      │  ├─ Note Globale Segment
      │  ├─ Calcul Audience
      │  ├─ Calcul Revenus (billetterie, merch, TV)
      │  └─ Risque Blessure
      └─ Note Globale Show

4. APPLICATION IMPACTS
   └─ ImpactApplier.AppliquerImpacts()
      ├─ Finances (revenus - frais apparition)
      ├─ Blessures (InjuryRecord + RecoveryPlan)
      ├─ Popularité (workers + compagnie)
      ├─ Titres (changements automatiques)
      ├─ Momentum (workers)
      ├─ Storylines (heat progression)
      └─ Fatigue (participants)

5. MORAL POST-SHOW
   └─ MoraleEngine.UpdateMorale()
      ├─ Workers utilisés → +3 à +5 moral
      └─ Workers non utilisés → -3 moral

6. FINALISATION
   └─ FinaliserShow()
      ├─ Statut show → "Terminé"
      ├─ Enregistrer résultats
      └─ Générer InboxItem résumé
```

### 🤖 Système Auto-Booking IA (BookerAIEngine)

**Flux** : Génération automatique de cartes complètes

```
1. INITIALISATION
   ├─ Charger Booker (préférences, mémoires)
   ├─ Vérifier CanAutoBook()
   └─ Charger contraintes Owner

2. FILTRAGE WORKERS
   ├─ Exclure blessés
   ├─ Exclure déjà utilisés
   ├─ Filtrer selon budget
   └─ Appliquer contraintes Owner

3. GÉNÉRATION SEGMENTS
   ├─ Calculer durée restante
   ├─ Boucle : Tant que durée > 0
   │  ├─ Déterminer type segment
   │  │  ├─ Main event (si manquant)
   │  │  ├─ Storyline (si active)
   │  │  ├─ Titre (si disponible)
   │  │  └─ Midcard (sinon)
   │  ├─ Sélection participants
   │  │  ├─ Selon préférences Booker
   │  │  ├─ Consulter mémoires
   │  │  └─ Appliquer créativité
   │  └─ Créer SegmentDefinition
   └─ Retourner carte complète

4. VALIDATION
   └─ Vérifier contraintes respectées
```

### 📈 Système de Storylines (StorylineService)

**Flux** : Cycle de vie complet d'une storyline

```
1. CRÉATION
   └─ Creer()
      ├─ Phase = Setup
      ├─ Heat = 0
      ├─ Status = Active
      └─ Participants

2. PROGRESSION HEAT
   └─ Après chaque segment lié
      ├─ Calculer delta (basé sur note)
      └─ Heat = Clamp(Heat + delta, 0, 100)

3. AVANCEMENT PHASE
   ├─ Setup → Rising (après 2-3 segments)
   ├─ Rising → Climax (Heat > 60)
   ├─ Climax → Fallout (après match principal)
   └─ Fallout → Completed (Heat >= 80)

4. ARCHIVAGE
   └─ Status = Archived (quand Completed)
```

### 💰 Système Financier (DailyFinanceService)

**Deux flux distincts** :

#### FLUX 1 : Paiement Mensuel Garanti
```
DÉCLENCHEMENT : Dernier jour du mois

1. Détection fin du mois
2. Charger contrats actifs
3. Pour chaque contrat :
   ├─ Si MonthlyWage > 0
   ├─ Vérifier non déjà payé
   ├─ Créer transaction (-MonthlyWage)
   └─ Mettre à jour LastPaymentDate
4. Appliquer transactions en batch
```

#### FLUX 2 : Frais d'Apparition
```
DÉCLENCHEMENT : Immédiatement après show

1. Extraire participants du show
2. Pour chaque participant :
   ├─ Si AppearanceFee > 0
   ├─ Vérifier non déjà payé (date)
   ├─ Créer transaction (-AppearanceFee)
   └─ Mettre à jour LastAppearanceDate
3. Appliquer transactions en batch
```

### 🏥 Système Médical (InjuryService)

**Flux** : Gestion complète des blessures

```
1. DÉCLENCHEMENT
   └─ Pendant simulation show
      ├─ Calculer risque blessure
      └─ Si déclenché → AppliquerBlessure()

2. APPLICATION
   ├─ Déterminer sévérité
   ├─ Créer InjuryRecord
   ├─ Créer RecoveryPlan
   └─ Ajouter MedicalNote

3. SUIVI
   └─ Chaque semaine
      ├─ Vérifier blessures actives
      ├─ Si semaine >= EndWeek → Guérison
      └─ Si lutte malgré blessure → Risque aggravation
```

### 😊 Système de Moral (MoraleEngine)

**Flux** : Gestion moral individuel et compagnie

```
1. MISE À JOUR INDIVIDUEL
   └─ UpdateMorale(workerId, eventType, impact)
      ├─ Charger BackstageMorale actuel
      ├─ Calculer changement selon eventType
      ├─ Appliquer changement
      └─ Enregistrer

2. RECALCUL COMPAGNIE
   └─ RecalculateCompanyMorale()
      ├─ Charger tous les moraux
      ├─ Calculer moyenne
      ├─ Identifier alertes (< 50 ou > 80)
      └─ Enregistrer CompanyMorale

3. ÉVÉNEMENTS DÉCLENCHEURS
   ├─ Après show (utilisés/non utilisés)
   ├─ Changements de push
   ├─ Gestion titres
   └─ Actions management
```

### 📢 Système de Rumeurs (RumorEngine)

**Flux** : Émergence et propagation des rumeurs

```
1. DÉCLENCHEMENT
   └─ Événement significatif détecté
      ├─ Sévérité >= 3 → automatique
      └─ Sévérité >= 2 → 40% chance

2. GÉNÉRATION
   ├─ Générer texte rumeur
   ├─ Créer Rumor (Stage = "Emerging")
   └─ AmplificationScore = 10

3. AMPLIFICATION
   ├─ Worker influent répand → +10 score
   ├─ Stage selon score :
   │  ├─ < 40 → "Emerging"
   │  ├─ 40-69 → "Growing"
   │  └─ >= 70 → "Widespread"
   └─ Enregistrer

4. PROGRESSION NATURELLE
   └─ Hebdomadaire
      ├─ Amplification +5 à +15
      ├─ Mise à jour stage
      └─ Nettoyer résolues (> 90 jours)

5. RÉSOLUTION
   └─ Action joueur
      ├─ Qualité intervention (0-100)
      ├─ Calcul chance succès
      └─ Si succès → Stage = "Resolved"
```

### ⚠️ Système de Crises (CrisisEngine)

**Flux** : Gestion des crises majeures

```
1. DÉCLENCHEMENT
   └─ Événement majeur
      ├─ Rumeur Widespread + Severity >= 4
      ├─ Incident backstage grave
      ├─ Perte contrat TV majeur
      └─ Départ worker star

2. ESCALATION
   └─ Hebdomadaire
      ├─ Escalation += 10-20
      ├─ Stage selon escalation :
      │  ├─ < 30 → "Detected"
      │  ├─ 30-59 → "Growing"
      │  ├─ 60-79 → "Critical"
      │  └─ >= 80 → "Crisis"
      └─ Impact moral compagnie

3. TENTATIVE RÉSOLUTION
   └─ Joueur intervient
      ├─ Qualité intervention
      ├─ Calcul chance succès
      ├─ Si succès → Stage = "Resolved"
      └─ Si échec → Réduction modérée
```

### 🔍 Système de Scouting (ScoutingService)

**Flux** : Découverte et évaluation de talents

```
1. CRÉATION MISSION
   └─ CreerMission()
      ├─ Définir paramètres (région, focus)
      └─ Créer ScoutMission (Statut = "active")

2. DÉCOUVERTE HEBDO
   └─ RafraichirHebdo()
      ├─ Générer rapports
      ├─ Sélectionner workers libres
      ├─ Filtrer selon région/focus
      └─ Créer ScoutReport

3. CRÉATION RAPPORT
   ├─ Charger ScoutingTarget
   ├─ Créer ScoutReport
   └─ Vérifier non-duplication

4. CONSULTATION
   └─ ChargerRapports()
      ├─ Filtrer par critères
      └─ Retourner liste rapports
```

### ⏰ Système de Gestion du Temps (TimeOrchestratorService)

**Flux** : Passage du temps jour par jour

```
DÉCLENCHEMENT : "Passer au jour suivant"

1. INCRÉMENTATION
   ├─ IncrementerJour()
   └─ GetCurrentDate()

2. MISE À JOUR STATS
   └─ UpdateDailyStats()
      ├─ Récupération fatigue
      └─ Progression blessures

3. PLANIFICATION SHOWS
   └─ Si jour % 30 == 0
      └─ PlanifierShowsAutomatiques()

4. GÉNÉRATION ÉVÉNEMENTS
   └─ GenerateDailyEvents()
      ├─ Offres contrat
      ├─ Offres TV
      └─ Événements backstage

5. DÉTECTION SHOW DAY
   └─ DetecterShowAVenir()
      └─ Si show détecté → Marquer pour simulation

6. FIN DE MOIS
   └─ Si EstFinDuMois()
      └─ ProcessMonthlyPayroll()
```

### 🏆 Système de Titres (TitleService)

**Flux** : Gestion des titres et changements

```
1. CRÉATION
   └─ CreerTitre()
      ├─ Définir paramètres
      └─ Créer Title (ChampionId = null)

2. MATCH DE TITRE
   └─ Pendant simulation
      ├─ Segment avec TitreId
      └─ Si PerdantId == ChampionId → Changement

3. CHANGEMENT
   └─ AppliquerChangementTitre()
      ├─ Créer nouveau TitleReign
      ├─ Clôturer règne précédent
      ├─ Mettre à jour Prestige
      └─ Enregistrer

4. GESTION CONTENDERS
   └─ ContenderService
      ├─ Calculer classement
      ├─ Déterminer #1 Contender
      └─ Mettre à jour hebdomadaire
```

---

Pour plus de détails techniques sur les flux, consultez [docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md#12-schémas-de-flux-des-systèmes)

---

## 📊 Diagramme de Flux Principal

Voici le diagramme de flux complet du jeu, de la création d'une compagnie jusqu'à la simulation d'un show :

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    🎮 FLUX PRINCIPAL DU JEU                              │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐
│  DÉMARRAGE      │
│  Création       │
│  Compagnie      │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    📅 BOUCLE HEBDOMADAIRE                               │
└─────────────────────────────────────────────────────────────────────────┘

    ┌─────────────────────────────────────────────────────────────┐
    │  LUNDI : INBOX & PLANNING                                    │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ WeeklyLoopService.PasserSemaineSuivante()             │  │
    │  │  ├─ Génération événements hebdomadaires               │  │
    │  │  ├─ Simulation backstage (morale, rumeurs, crises)   │  │
    │  │  ├─ Génération scouting                               │  │
    │  │  ├─ Vérification contrats (expirations)               │  │
    │  │  └─ Simulation monde vivant (autres compagnies)      │  │
    │  └───────────────────────────────────────────────────────┘  │
    │                                                               │
    │  Actions Joueur :                                            │
    │  ├─ Consulter Inbox (InboxViewModel)                        │
    │  ├─ Scouting (ScoutingService)                               │
    │  └─ Négociations (ContractNegotiationService)               │
    └─────────────────────────────────────────────────────────────┘
         │
         ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  MARDI-MERCREDI : BOOKING                                    │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ BookingViewModel                                     │  │
    │  │  ├─ Mode Manuel :                                    │  │
    │  │  │   └─ BookingBuilderService                        │  │
    │  │  │      └─ Création segments manuelle                │  │
    │  │  │                                                    │  │
    │  │  └─ Mode Auto-Booking IA :                           │  │
    │  │      └─ BookerAIEngine.GenerateAutoBooking()         │  │
    │  │         ├─ Analyse storylines actives                │  │
    │  │         ├─ Utilise mémoires du Booker                │  │
    │  │         ├─ Respecte contraintes Owner                │  │
    │  │         └─ Génère carte complète (4-8 segments)      │  │
    │  └───────────────────────────────────────────────────────┘  │
    │                                                               │
    │  Validation :                                                │
    │  └─ BookingValidator.ValiderBooking()                       │
    └─────────────────────────────────────────────────────────────┘
         │
         ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  JEUDI : SHOW DAY (Match Day)                                │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ ShowDayOrchestrator.ExecuterFluxComplet()            │  │
    │  │                                                       │  │
    │  │  1. Détection Show                                   │  │
    │  │     └─ DetecterShowAVenir()                         │  │
    │  │                                                       │  │
    │  │  2. Chargement Contexte                              │  │
    │  │     └─ ChargerShowContext()                          │  │
    │  │        ├─ ShowDefinition                            │  │
    │  │        ├─ Segments                                  │  │
    │  │        ├─ Workers (attributs complets)              │  │
    │  │        ├─ Storylines actives                        │  │
    │  │        └─ Titres                                    │  │
    │  │                                                       │  │
    │  │  3. Simulation                                       │  │
    │  │     └─ ShowSimulationEngine.Simuler()               │  │
    │  │        ├─ Pour chaque segment :                     │  │
    │  │        │   ├─ Calcul note (InRing, Ent, Story)     │  │
    │  │        │   ├─ Facteurs qualité (chimie, heat)      │  │
    │  │        │   ├─ Risque blessure                      │  │
    │  │        │   └─ Calcul audience/revenus              │  │
    │  │        └─ Note globale du show                     │  │
    │  │                                                       │  │
    │  │  4. Application Impacts                              │  │
    │  │     └─ ImpactApplier.AppliquerImpacts()             │  │
    │  │        ├─ Finances (billetterie, merch, TV)        │  │
    │  │        ├─ Popularité workers/compagnie             │  │
    │  │        ├─ Momentum                                 │  │
    │  │        ├─ Fatigue                                  │  │
    │  │        ├─ Storylines (heat progression)            │  │
    │  │        └─ Titres (changements)                     │  │
    │  │                                                       │  │
    │  │  5. Finances FLUX 2                                  │  │
    │  │     └─ DailyFinanceService.ProcessAppearanceFees() │  │
    │  │        └─ Déduction frais d'apparition             │  │
    │  │                                                       │  │
    │  │  6. Moral Post-Show                                 │  │
    │  │     └─ MoraleEngine.UpdateMorale()                  │  │
    │  │        ├─ Workers utilisés : stable                │  │
    │  │        └─ Workers NON utilisés : -3 points ⚠️      │  │
    │  │                                                       │  │
    │  │  7. Finalisation                                    │  │
    │  │     └─ FinaliserShow()                              │  │
    │  │        ├─ Changements de titres                     │  │
    │  │        ├─ InboxItems (blessures, titres)           │  │
    │  │        └─ Statut → "Simulé"                        │  │
    │  └───────────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────────┘
         │
         ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  VENDREDI : RÉSULTATS & GESTION                             │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ Actions Joueur :                                     │  │
    │  │ ├─ Consulter résultats (ShowResultsView)            │  │
    │  │ ├─ Analyser performances                             │  │
    │  │ ├─ Gérer médical (InjuryService)                     │  │
    │  │ ├─ Gérer staff                                        │  │
    │  │ └─ Gérer discipline (DisciplineService)              │  │
    │  └───────────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────────┘
         │
         ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  WEEKEND : PASSAGE DE SEMAINE                               │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ TimeOrchestratorService.PasserJourSuivant()           │  │
    │  │  ├─ Incrémentation jour                              │  │
    │  │  ├─ Mise à jour stats quotidiennes                   │  │
    │  │  ├─ Génération événements quotidiens                 │  │
    │  │  └─ Vérification show à venir                         │  │
    │  │                                                       │  │
    │  │  Si dernier jour du mois :                           │  │
    │  │  └─ DailyFinanceService.ProcessMonthlyPayroll()      │  │
    │  │     └─ FLUX 1 : Paiement mensuel garanti             │  │
    │  └───────────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────────┘
         │
         └───► Retour au LUNDI (boucle continue)


┌─────────────────────────────────────────────────────────────────────────┐
│                    🔄 FLUX FINANCIER                                     │
└─────────────────────────────────────────────────────────────────────────┘

FLUX 1 : Paiement Mensuel Garanti
┌─────────────────────────────────────────────────────────────┐
│ Dernier jour du mois                                        │
│ └─ DailyFinanceService.ProcessMonthlyPayroll()             │
│    └─ Pour chaque contrat avec MonthlyWage > 0             │
│       └─ Déduction du budget compagnie                     │
└─────────────────────────────────────────────────────────────┘

FLUX 2 : Frais d'Apparition (Per-Appearance)
┌─────────────────────────────────────────────────────────────┐
│ Immédiatement après chaque show                            │
│ └─ DailyFinanceService.ProcessAppearanceFees()              │
│    └─ Pour chaque participant du show                      │
│       └─ Déduction AppearanceFee du budget                 │
└─────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                    🤖 FLUX AUTO-BOOKING IA                               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Joueur clique "🤖 Laisser le Booker préparer le show"     │
│ └─ BookerAIEngine.GenerateAutoBooking()                    │
│    │                                                         │
│    ├─ Analyse contexte                                      │
│    │  ├─ Storylines actives (heat, phase)                   │
│    │  ├─ Titres disponibles                                 │
│    │  ├─ Workers disponibles (fatigue, blessures)          │
│    │  └─ Mémoires du Booker (événements passés)            │
│    │                                                         │
│    ├─ Application contraintes Owner                         │
│    │  ├─ Budget disponible                                 │
│    │  ├─ Workers interdits                                  │
│    │  ├─ Fatigue maximale autorisée                         │
│    │  └─ Durée cible du show                                │
│    │                                                         │
│    ├─ Génération segments                                   │
│    │  ├─ Main Event (storyline ou titre)                    │
│    │  ├─ Mid-card matches                                   │
│    │  ├─ Promos et angles                                   │
│    │  └─ Respect style produit (5 styles)                 │
│    │                                                         │
│    └─ Retour carte complète                                 │
│       └─ Joueur peut modifier avant validation              │
└─────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                    📊 FLUX DE SIMULATION                                 │
└─────────────────────────────────────────────────────────────────────────┘

ShowSimulationEngine.Simuler(ShowContext)
│
├─ Pour chaque segment dans l'ordre :
│  │
│  ├─ Calcul Note In-Ring
│  │  ├─ Moyenne attributs In-Ring des participants
│  │  ├─ Bonus chimie entre workers
│  │  ├─ Bonus type de match
│  │  └─ Pénalité fatigue
│  │
│  ├─ Calcul Note Entertainment
│  │  ├─ Moyenne attributs Entertainment
│  │  ├─ Bonus charisme
│  │  └─ Bonus storyline heat
│  │
│  ├─ Calcul Note Story
│  │  ├─ Moyenne attributs Story
│  │  ├─ Bonus storyline active
│  │  └─ Bonus cohérence narrative
│  │
│  ├─ Note Globale Segment
│  │  └─ Moyenne pondérée (InRing 40%, Ent 30%, Story 30%)
│  │
│  ├─ Calcul Audience
│  │  ├─ Popularité moyenne participants
│  │  ├─ Popularité compagnie
│  │  └─ Facteur qualité segment
│  │
│  ├─ Calcul Revenus
│  │  ├─ Billetterie (audience × prix ticket)
│  │  ├─ Merchandise (popularité × facteur merch)
│  │  └─ TV (deal actif × audience)
│  │
│  ├─ Risque Blessure
│  │  ├─ Attribut Safety des participants
│  │  ├─ Type de match (hardcore = +risque)
│  │  └─ Fatigue actuelle
│  │
│  └─ Impacts Immédiats
│     ├─ Fatigue +X selon durée/intensité
│     ├─ Momentum ajusté (vainqueur/perdant)
│     └─ Popularité ajustée (performance)
│
├─ Calcul Note Globale Show
│  └─ Moyenne segments + bonus cohérence
│
└─ Retour ShowSimulationResult
   ├─ RapportShow (notes, audience, revenus)
   ├─ GameStateDelta (tous les changements)
   └─ Segments (détails par segment)
```

---

## 🔗 Flux de Données entre Services

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ARCHITECTURE DES FLUX                             │
└─────────────────────────────────────────────────────────────────────┘

UI Layer (ViewModels)
    │
    ├─► DashboardViewModel
    │   └─► ShowDayOrchestrator.ExecuterFluxComplet()
    │
    ├─► BookingViewModel
    │   ├─► BookingBuilderService (manuel)
    │   └─► BookerAIEngine (auto-booking)
    │
    ├─► InboxViewModel
    │   └─► WeeklyLoopService.PasserSemaineSuivante()
    │
    └─► FinanceViewModel
        └─► DailyFinanceService
            ├─► ProcessMonthlyPayroll() (FLUX 1)
            └─► ProcessAppearanceFees() (FLUX 2)

Core Services Layer
    │
    ├─► ShowDayOrchestrator
    │   ├─► ShowSimulationEngine
    │   ├─► ImpactApplier
    │   ├─► TitleService
    │   ├─► MoraleEngine
    │   └─► DailyFinanceService
    │
    ├─► WeeklyLoopService
    │   ├─► ScoutingService
    │   ├─► MoraleEngine
    │   ├─► RumorEngine
    │   ├─► CrisisEngine
    │   ├─► BookerAIEngine
    │   └─► RosterAnalysisService
    │
    └─► TimeOrchestratorService
        ├─► DailyFinanceService
        ├─► EventGeneratorService
        └─► ShowDayOrchestrator

Data Layer (Repositories)
    │
    ├─► GameRepository (Façade)
    │   ├─► ShowRepository
    │   ├─► CompanyRepository
    │   ├─► WorkerRepository
    │   ├─► BackstageRepository
    │   └─► ... (30+ repositories)
    │
    └─► RepositoryContainer
        └─► Tous les repositories spécialisés
```

### Systèmes Clés

- **Booking** : Construction de cartes, validation, templates
- **🆕 Auto-Booking IA** : Génération automatique de cartes complètes par le Booker
  - 5 styles de produit : Hardcore, Puroresu, Technical, Entertainment, Balanced
  - Respect des préférences du Booker (Underdog, Veteran, Fast Rise, Slow Burn)
  - Utilisation du système de mémoire pour décisions cohérentes
  - Contraintes Owner personnalisables (budget, workers interdits, fatigue)
- **Storylines** : Feuds, heat progression, phases (BUILD/PEAK/BLOWOFF)
- **Attributs** : 40 attributs de performance (4 dimensions)
- **Personnalité** : 25+ profils automatiques (FM-like)
- **Backstage** : Moral, rumeurs, népotisme, crises
- **Simulation** : Engine sophistiqué de calcul de ratings
- **🆕 Show Day** : Flux complet de simulation avec impacts automatiques (finances, titres, blessures, moral)
- **IA** : Booker et Propriétaire avec décisions automatiques

---

## 🗺️ Roadmap

| Phase | Description | Status | Cible |
|-------|-------------|--------|-------|
| **Phase 0** | Infrastructure & Architecture | ✅ **Complet** | - |
| **Phase 1** | Fondations UI/UX & Gameplay de base | ✅ **Complet** | - |
| **Phase 1.5** | Systèmes Personnalité & Attributs | ✅ **Complet** | - |
| **Phase 1.9** | 🆕 Flux Show Day & Auto-Booking | ✅ **Complet** | - |
| **Phase 2** | Intégration Données & Features avancées | ⚠️ **En cours** | Jan 2026 |
| **Phase 3** | Fonctionnalités Métier complètes | ⚠️ **En cours** (15%) | Jan 2026 |
| **Phase 4** | Performance & Optimisation | ❌ **À démarrer** | Mar 2026 |
| **Phase 5** | QA & Polish | ❌ **À démarrer** | Avr 2026 |

**Roadmap complète :** [docs/ROADMAP_MISE_A_JOUR.md](docs/ROADMAP_MISE_A_JOUR.md)

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Consultez :
- **[docs/DEV_GUIDE_FR.md](docs/DEV_GUIDE_FR.md)** pour le guide de développement
- **[docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)** pour comprendre l'architecture

### Standards de Code

- C# 12 avec nullable reference types
- Immutable records pour les modèles du domaine
- MVVM avec ReactiveUI
- Naming conventions en français (cohérent avec le projet)

---

## 📄 License

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

## 🔗 Liens Utiles

- **Documentation complète :** [docs/INDEX.md](docs/INDEX.md)
- **État du projet :** [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md)
- **Architecture :** [docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)
- **Rapport de vérification (8 jan 2026) :** [docs/RAPPORT_VERIFICATION_ARCHITECTURE_2026-01-08.md](docs/RAPPORT_VERIFICATION_ARCHITECTURE_2026-01-08.md)

---

**Développé avec ❤️ en C# et Avalonia**

*Ring General est un projet personnel de simulation de gestion de catch. Il n'est pas affilié à WWE, AEW, NJPW ou toute autre organisation de catch professionnel.*

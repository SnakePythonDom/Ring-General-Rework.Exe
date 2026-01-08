# 📊 ÉTAT DU PROJET RING GENERAL

**Document de référence unique** - Status consolidé du projet

**Dernière mise à jour** : 8 janvier 2026 (Réorganisation complète)
**Version actuelle** : Phase 2 - ~35-40% complété
**Branche active** : `claude/organize-repo-files-LH0mI`

---

## 🎯 RÉSUMÉ EXÉCUTIF

### Progression Globale

**Complétion : 35-40%** (Phase 0-1 transition)

| Phase | Description | Status | % |
|-------|-------------|--------|---|
| **Phase 0** | Infrastructure & Architecture | ✅ **COMPLET** | 100% |
| **Phase 1** | Fondations UI/UX & Gameplay de base | ⚠️ **EN COURS** | 40% |
| **Phase 2** | Intégration Données & Features avancées | ❌ **À DÉMARRER** | 0% |
| **Phase 3** | Fonctionnalités Métier complètes | ❌ **À DÉMARRER** | 0% |
| **Phase 4** | Performance & Optimisation | ❌ **À DÉMARRER** | 0% |
| **Phase 5** | QA & Polish | ❌ **À DÉMARRER** | 0% |

### État par Composant

- ✅ **Architecture MVVM** : Core, Repositories, Services en place
- ✅ **Navigation** : Prototype D (FM26 dual-pane) implémenté et fonctionnel
- ✅ **Base de données** : SQLite avec DbSeeder automatique (BAKI1.1.db)
- ✅ **ViewModels** : 46 ViewModels créés (Dashboard, Booking, Roster, Finance, Youth, Calendar, etc.)
- ⚠️ **Views** : 13/20 Views créées et câblées
- ⚠️ **UI** : Interface partiellement fonctionnelle (composants réutilisables manquants)
- ⚠️ **Services** : Backend solide, services UI partiels
- ❌ **Boucle de jeu complète** : Éléments séparés mais pas orchestrés
- ❌ **Gameplay** : Boucle de base en développement

### Découverte Principale

Le projet est **significativement plus avancé** que ce que la documentation précédente suggérait, particulièrement au niveau UI/UX.

---

## 📁 ARCHITECTURE DU PROJET

### Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Framework | .NET | 8.0 LTS |
| UI Framework | Avalonia | 11.0.6 |
| Reactive UI | ReactiveUI | (via Avalonia) |
| Base de données | SQLite | 8.0.0 |
| Tests | xUnit | Latest |
| Langage | C# 12 | French naming |

### Structure des Projets (7 projets)

```
RingGeneral.sln
├── RingGeneral.UI (WinExe)              # Interface Avalonia
├── RingGeneral.Core                     # Logique métier (60 fichiers)
├── RingGeneral.Data                     # Accès données (18 repositories)
├── RingGeneral.Specs                    # Configuration JSON
├── RingGeneral.Tools.BakiImporter       # Outil d'import BAKI DB
├── RingGeneral.Tools.DbManager          # Utilitaires DB
└── RingGeneral.Tests                    # Tests unitaires (xUnit)
```

### Pattern MVVM

- **Séparation claire** : Views, ViewModels, Services, Repositories
- **Dependency Injection** : Microsoft.Extensions.DependencyInjection
- **Event Aggregator** : Pub/Sub messaging pour communication inter-composants
- **ReactiveUI** : Data binding avancé et commandes réactives

---

## ✅ CE QUI EST COMPLÈTEMENT IMPLÉMENTÉ

### 1. Infrastructure Technique (100% ✅)

#### Architecture MVVM
- Pattern MVVM avec ReactiveUI
- Séparation claire des responsabilités
- Dependency Injection configuré
- Event Aggregator pour la communication

#### Système de Navigation
- `INavigationService` + `NavigationService` implémentés
- TreeView navigation fonctionnelle (3 colonnes : Nav + Content + Context)
- Bindings ReactiveUI opérationnels
- Navigation hiérarchique avec expand/collapse

#### Base de Données SQLite
- Schéma complet (~30 tables)
- **DbSeeder implémenté** avec import automatique de BAKI1.1.db
- Seed data par défaut (20 workers, 5 titres, 1 show) si BAKI absent
- Tables : Companies, Workers, Shows, Segments, Storylines, Titles, Contracts, etc.

### 2. Repositories (17 repositories créés - 100% ✅)

```
/src/RingGeneral.Data/Repositories/
├── GameRepository.cs (1675 lignes - orchestrateur principal)
├── WorkerRepository.cs
├── ShowRepository.cs
├── TitleRepository.cs
├── ContractRepository.cs
├── BackstageRepository.cs
├── CompanyRepository.cs
├── MedicalRepository.cs
├── YouthRepository.cs
├── ScoutingRepository.cs
├── SettingsRepository.cs
├── RepositoryFactory.cs (crée tous les repos)
├── RepositoryBase.cs
├── ImpactApplier.cs
├── WeeklyLoopService.cs
├── SharedQueries.cs
└── Pagination.cs
```

**Tous les 17 repositories sont maintenant enregistrés dans le DI** (App.axaml.cs)

### 3. ViewModels (46 ViewModels - 92% ✅)

#### ViewModels Principaux (12/12)

**Core:**
- `ViewModelBase.cs` - Classe de base ReactiveUI
- `ShellViewModel.cs` (327 lignes) - Shell principal avec navigation

**Flow de Démarrage:**
- `StartViewModel.cs` - Menu de démarrage
- `CompanySelectorViewModel.cs` - Sélection de compagnie (NEW GAME)
- `CreateCompanyViewModel.cs` - Création de compagnie

**Modules de Jeu:**
- `DashboardViewModel.cs` - Dashboard principal
- `BookingViewModel.cs` (311 lignes) - Gestion du booking
- `RosterViewModel.cs` - Liste des workers
- `WorkerDetailViewModel.cs` - Détails d'un worker
- `TitlesViewModel.cs` - Gestion des titres
- `StorylinesViewModel.cs` - Gestion des storylines
- `YouthViewModel.cs` - Développement des jeunes
- `FinanceViewModel.cs` - Finances
- `CalendarViewModel.cs` - Calendrier des shows

#### ViewModels de Support (34 fichiers)

Booking, UI/Navigation, Show/Calendar, Finance/Broadcasting, Storylines, Youth, Inbox, SaveManager, GlobalSearch, Help, etc.

### 4. Views (13/20 Views - 65% ⚠️)

**Toutes les Views Principales Implémentées:**

- `Shell/MainWindow.axaml` (237 lignes) - Structure 3 colonnes
- `Start/StartView.axaml` - Menu de démarrage
- `Start/CompanySelectorView.axaml` - Sélection de compagnie
- `Start/CreateCompanyView.axaml` - Création de compagnie
- `Dashboard/DashboardView.axaml` - Dashboard principal
- `Booking/BookingView.axaml` (226 lignes) - Table de booking FM26-style
- `Roster/RosterView.axaml` - Liste des workers
- `Roster/WorkerDetailView.axaml` - Détails worker
- `Roster/TitlesView.axaml` - Gestion des titres
- `Storylines/StorylinesView.axaml` - Gestion des storylines
- `Youth/YouthView.axaml` - Développement jeunes
- `Finance/FinanceView.axaml` - Finances
- `Calendar/CalendarView.axaml` - Calendrier

**Toutes les Views sont 100% fonctionnelles et câblées avec leurs ViewModels !**

### 5. Services Implémentés

#### Services Core (/src/RingGeneral.Core/Services/)
- `BookingBuilderService.cs` - Construction de bookings
- `ContenderService.cs` - Gestion des contenders
- `ShowSchedulerService.cs` - Planification des shows
- `StorylineService.cs` - Gestion des storylines
- `TemplateService.cs` - Gestion des templates
- `TitleService.cs` - Gestion des titres

#### Services UI (/src/RingGeneral.UI/Services/)
- `NavigationService.cs` + `INavigationService.cs` - Navigation
- `EventAggregator.cs` + `IEventAggregator.cs` - Pub/Sub messaging
- `SaveStorageService.cs` - Stockage des sauvegardes
- `HelpContentProvider.cs` - Contenu d'aide
- `TooltipHelper.cs` - Gestion des tooltips
- `UiPageSpecsProvider.cs` - Spécifications des pages UI
- `NavigationSpecMapper.cs` - Mapping des specs de navigation

#### Services Data (/src/RingGeneral.Data/Services/)
- `WorkerService.cs` - Service métier pour les workers

### 6. Modèles de Domaine (26 fichiers - 90% ✅)

**Modèles Core Complets:**

- `DomainModels.cs` - Modèles principaux (Company, Worker, Show, Segment)
- `ContractModels.cs` - Contrats
- `StorylineModels.cs` + `StorylineEnums.cs` - Storylines
- `TitleModels.cs` - Titres
- `YouthModels.cs` - Développement jeunes
- `ScoutingModels.cs` - Scouting
- `MedicalModels.cs` - Médical/Blessures
- `FinanceModels.cs` - Finances
- `BroadcastModels.cs` - Broadcasting/TV
- `BackstageModels.cs` - Backstage/Coulisses
- `SimulationModels.cs` - Simulation de shows
- Et 14+ autres modèles...

### 7. Arbre de Navigation (60% fonctionnel)

**Structure Actuelle dans ShellViewModel:**

```
🏠 ACCUEIL → DashboardViewModel ✅
📋 BOOKING
  ├─ 📺 Shows actifs → BookingViewModel ✅
  ├─ 📚 Bibliothèque → ❌ À implémenter
  ├─ 📊 Historique → ❌ À implémenter
  └─ ⚙️ Paramètres → ❌ À implémenter
👥 ROSTER
  ├─ 🤼 Workers → RosterViewModel ✅
  ├─ 🏆 Titres → TitlesViewModel ✅
  └─ 🏥 Blessures → ❌ À implémenter
📖 STORYLINES
  ├─ 🔥 Actives → StorylinesViewModel ✅
  ├─ 📦 Archivées → ❌ À implémenter
  └─ ➕ Créer → ❌ À implémenter
🎓 YOUTH → YouthViewModel ✅
💼 FINANCE → FinanceViewModel ✅
📆 CALENDRIER → CalendarViewModel ✅
```

**Fonctionnel : 9/15 items (60%)**
**Infrastructure : 100% (navigation fonctionne parfaitement)**

---

## ⚠️ CE QUI EST PARTIELLEMENT IMPLÉMENTÉ

### 1. Système de Booking (60%)

**✅ Implémenté:**
- BookingViewModel avec gestion de segments
- SegmentViewModel pour chaque segment
- Ajout/Suppression/Réorganisation de segments
- Validation basique (BookingValidator existe)
- Affichage de la durée totale
- Badge "Main Event"
- Liste des workers disponibles

**❌ Manquant:**
- Dialog d'édition de segment (SegmentEditorDialog)
- Système de templates avancé (structure existe, pas de UI)
- Bibliothèque de segments pré-faits
- Historique de bookings
- Auto-booking/suggestions
- Scripts détaillés pour promos/angles

### 2. Système de Simulation (70%)

**✅ Implémenté (Backend):**
- `ShowSimulationEngine.cs` (435 lignes) - TRÈS COMPLET
  - Calcul de notes (InRing, Entertainment, Story)
  - Dynamique de crowd heat
  - Pénalités de pacing
  - Bonus de chemistry
  - Accumulation de fatigue
  - Calcul de risque de blessure
  - Changements de momentum
  - Impact sur popularité
  - Progression du heat des storylines
  - Calculs d'audience et revenus

**❌ Manquant (UI):**
- ShowResultsView pour afficher les résultats détaillés
- Graphiques de crowd heat par segment
- Timeline du show avec highlights
- Détails des impacts par worker

### 3. Gestion du Roster (50%)
### 4. Storylines (40%)
### 5. Youth Development (30%)
### 6. Finance (30%)
### 7. Titres (40%)
### 8. Calendrier (40%)

---

## ❌ CE QUI N'EST PAS IMPLÉMENTÉ

### 1. Composants UI Réutilisables (0% - BLOQUANT)

**Tous manquants:**
- `AttributeBar.axaml` - Barre de stat visuelle (1-20)
- `SortableDataGrid.axaml` - DataGrid avec tri/filtres
- `DetailPanel.axaml` - Panneau de contexte (colonne droite)
- `NewsCard.axaml` - Carte de message inbox
- `AttributeCategoryPanel.axaml` - Groupe d'attributs
- `/Styles/RingGeneralTheme.axaml` - Thème unifié

**Impact :** Ces composants sont critiques pour accélérer le développement des autres écrans.

### 2. Système d'Inbox & Actualités (0%)
### 3. Système de Contrats UI (5%)
### 4. Médical/Injuries UI (0%)
### 5. Broadcasting/TV Deals UI (0%)
### 6. Scouting UI (10%)

### 7. Boucle de Jeu Complète (0% - CRITIQUE ❌)

**Manquant:**
- Bouton "Passer à la semaine suivante"
- WeeklyLoopService appelé automatiquement
- Génération d'événements hebdomadaires
- Déduction automatique des salaires
- Progression de la fatigue
- Génération de messages inbox
- Vieillissement des workers
- Progression des storylines

**Impact :** La boucle de jeu n'est pas connectée end-to-end. Les éléments existent séparément mais ne sont pas orchestrés.

---

## 📊 TABLEAU DE COMPLÉTION PAR COUCHE

| Couche | Fichiers Créés | Fichiers Prévus | % Complet | Status |
|--------|----------------|-----------------|-----------|--------|
| **Architecture** | 100% | 100% | 100% | ✅ COMPLET |
| **Base de Données** | ~30 tables | ~30 tables | 90% | ✅ COMPLET |
| **Repositories** | 17/17 | 17 | 100% | ✅ CRÉÉS |
| **DI Registration** | 17/17 repos | 17 | 100% | ✅ COMPLET |
| **Modèles** | 26 fichiers | ~30 | 90% | ✅ QUASI COMPLET |
| **Services Core** | 6/20 | ~20 | 30% | ⚠️ PARTIEL |
| **Services UI** | 7/10 | ~10 | 70% | ⚠️ PARTIEL |
| **ViewModels** | 46/50 | ~50 | 92% | ✅ QUASI COMPLET |
| **Views** | 13/20 | ~20 | 65% | ⚠️ PARTIEL |
| **Composants UI** | 0/6 | 6 | 0% | ❌ MANQUANT |
| **Navigation** | 9/15 items | 15 | 60% | ⚠️ PARTIEL |
| **Seed Data** | 1/1 | 1 | 100% | ✅ COMPLET |

## 🎯 POURCENTAGES PAR FONCTIONNALITÉ

| Fonctionnalité | Backend | UI | Global | Priorité |
|----------------|---------|-----|--------|----------|
| **Booking** | 80% | 40% | 60% | 🔴 HAUTE |
| **Simulation** | 90% | 10% | 50% | 🔴 HAUTE |
| **Roster** | 70% | 30% | 50% | 🔴 HAUTE |
| **Contrats** | 40% | 0% | 20% | 🔴 HAUTE |
| **Inbox** | 0% | 0% | 0% | 🔴 HAUTE |
| **Boucle de Jeu** | 50% | 0% | 25% | 🔴 CRITIQUE |
| **Storylines** | 60% | 20% | 40% | 🟡 MOYENNE |
| **Youth** | 50% | 10% | 30% | 🟡 MOYENNE |
| **Finance** | 50% | 10% | 30% | 🟡 MOYENNE |
| **Titres** | 60% | 20% | 40% | 🟡 MOYENNE |
| **Calendrier** | 50% | 30% | 40% | 🟡 MOYENNE |
| **Médical** | 60% | 0% | 30% | 🟡 MOYENNE |
| **Broadcasting** | 40% | 0% | 20% | 🟢 BASSE |
| **Scouting** | 40% | 10% | 25% | 🟢 BASSE |

---

## 🚀 PROCHAINES ÉTAPES - ROADMAP

### 🎯 Priorité Immédiate (Phase 3 - Janvier 2026)

1. **Créer les Views manquantes pour tous les ViewModels**
2. **Compléter l'intégration BAKI1.1.db**
3. **Implémenter la recherche globale**
4. **Finaliser le système de validation du booking**

### Option A : Composants Réutilisables (RECOMMANDÉ)
**Durée : 3-5 jours**
- AttributeBar.axaml
- SortableDataGrid.axaml
- DetailPanel.axaml
- AttributeDescriptions.fr.resx
- **Débloque tous les développements suivants**

### Option B : Boucle de Jeu Complète
**Durée : 5-7 jours**
- Bouton "Passer à la semaine suivante"
- Orchestration de tous les services
- Génération d'événements
- Tests end-to-end

---

## 🔧 DETTE TECHNIQUE IDENTIFIÉE

### Critique
1. **Composants UI réutilisables manquants** : Bloque le développement rapide
2. **Boucle de jeu non connectée** : Éléments séparés mais pas orchestrés
3. **GameRepository trop large** : 1675 lignes (refactoring nécessaire)

### Moyenne
4. Tests unitaires désynchronisés (certains fichiers)
5. Context panel (colonne droite) non implémenté
6. Duplication schéma DB (code vs migrations)

### Basse
7. ViewModels monolithiques à découper
8. DataTemplates manquants pour certains ViewModels
9. Tooltips incomplets

---

## ✅ FORCES DU PROJET

1. **Architecture solide** : MVVM bien structuré, séparation claire
2. **Navigation complète** : 100% fonctionnelle pour les vues existantes
3. **UI avancée** : 13 vues fonctionnelles
4. **Seed data** : Système complet d'import/seed BAKI
5. **Modèles complets** : Couche domaine très riche
6. **Simulation puissante** : ShowSimulationEngine très sophistiqué
7. **Repositories complets** : Tous créés et fonctionnels

---

## ⚠️ POINTS D'ATTENTION

1. **Composants UI manquants** : Bloque le développement rapide
2. **Boucle de jeu** : Critique pour rendre le jeu jouable
3. **Services manquants** : Beaucoup de services documentés n'existent pas encore
4. **Tests** : Couverture incomplète, certains désynchronisés
5. **Documentation** : À maintenir à jour avec les changements

---

## 📚 DOCUMENTATION ASSOCIÉE

### Documents de Référence
- **[README.md](../README.md)** - Vision produit complète
- **[ROADMAP_MISE_A_JOUR.md](ROADMAP_MISE_A_JOUR.md)** - Plan de développement détaillé (Phases 1-5)
- **[ARCHITECTURE_REVIEW_FR.md](ARCHITECTURE_REVIEW_FR.md)** - Analyse architecture (1100+ lignes)
- **[docs/INDEX.md](INDEX.md)** - Index de toute la documentation

### Documents de Planning
- **[planning/PLAN_IMPLEMENTATION_TECHNIQUE.md](planning/PLAN_IMPLEMENTATION_TECHNIQUE.md)** - Plan technique

### Documents de Sprint
- **[sprints/SPRINT_1_SUMMARY.md](sprints/SPRINT_1_SUMMARY.md)** - Résumé Sprint 1
- **[sprints/SPRINT_2_DESIGN.md](sprints/SPRINT_2_DESIGN.md)** - Design Sprint 2

### Guides Utilisateur
- **[QUICK_START_GUIDE.md](QUICK_START_GUIDE.md)** - Guide de démarrage rapide (inclut guide joueur)
- **[DEV_GUIDE_FR.md](DEV_GUIDE_FR.md)** - Guide de développement & modding
- **[DATABASE_GUIDE_FR.md](DATABASE_GUIDE_FR.md)** - Guide base de données

---

## 🎯 CONCLUSION

**Le projet est en excellente forme !**

**État Réel : ~35-40%** (significativement plus avancé que documenté initialement)

**Points Clés :**
- ✅ Infrastructure : COMPLÈTE (100%)
- ✅ UI/Navigation : FONCTIONNELLE (13 vues, 46 ViewModels)
- ✅ Base de données : OPÉRATIONNELLE avec seed BAKI
- ✅ Repositories : COMPLETS et enregistrés en DI
- ⚠️ Services : PARTIELS (6/20 core, 7/10 UI)
- ⚠️ Fonctionnalités : PARTIELLES (40% Phase 1)
- ❌ Boucle de jeu : NON CONNECTÉE (critique)
- ❌ Composants UI : MANQUANTS (bloquants)

**Prochaine Priorité : Créer les composants UI réutilisables** pour débloquer le développement rapide de toutes les autres fonctionnalités.

---

**Document créé le** : 8 janvier 2026
**Consolidation de** : CURRENT_STATE.md + RECAPITULATIF_TECHNIQUE.md
**Prochaine mise à jour recommandée** : Toutes les 2 semaines

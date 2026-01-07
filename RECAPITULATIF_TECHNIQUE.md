# 📊 RÉCAPITULATIF TECHNIQUE - RING GENERAL
**Date**: 2026-01-07
**Analysé par**: Claude Code (Expert Fullstack)
**État**: Projet en cours de refactoring

---

## 🎯 RÉSUMÉ EXÉCUTIF

**Ring General** est une application desktop de simulation de gestion de catch (wrestling management game) développée en C# avec Avalonia UI. Le projet est actuellement en **transition architecturale** du prototype monolithique vers une architecture MVVM propre avec navigation dynamique.

### État Actuel
- ✅ **Backend solide** : Base SQLite, architecture en couches, repositories
- ⚠️ **Frontend en transition** : Deux systèmes UI coexistent (ancien prototype + nouveau Shell)
- ❌ **Navigation non fonctionnelle** : Les sous-onglets ne marchent pas (affichage monolithique)
- ❌ **Pages vides** : Pas de données affichées (DB vide par défaut)

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

---

## 🔴 PROBLÈMES CRITIQUES IDENTIFIÉS

### 1. Duplication de MainWindow (CRITIQUE)
**Impact**: Navigation non fonctionnelle

**Détails**:
- `/src/RingGeneral.UI/Views/MainWindow.axaml` - **OBSOLÈTE** (prototype monolithique)
- `/src/RingGeneral.UI/Views/Shell/MainWindow.axaml` - **ACTUEL** (navigation dynamique)

**Cause**: L'ancien prototype affiche TOUT le contenu en scrollant (1162 lignes de XAML), empêchant la navigation de fonctionner.

**Solution**: Supprimer l'ancien `/Views/MainWindow.axaml` et `/Views/MainWindow.axaml.cs`

### 2. Base de données vide par défaut
**Impact**: Pages vides au démarrage

**Détails**:
- Le fichier `ringgeneral.db` est créé vide
- Les données de test ne sont pas chargées
- Le fichier `BAKI1.1.db` (1.6 MB) existe mais n'est pas utilisé pour l'init

**Solution**:
- Implémenter un seed automatique au premier lancement
- Ou importer automatiquement depuis BAKI1.1.db

### 3. ViewModels incomplets
**Impact**: Sous-onglets non accessibles

**Détails** (voir ShellViewModel.cs lignes 122-250):
```csharp
// Beaucoup de ViewModels sont null:
booking.Children.Add(new NavigationItemViewModel(
    "booking.library",
    "Bibliothèque",
    "📚",
    null, // TODO: LibraryViewModel ❌
    booking
));
```

**ViewModels manquants**:
- ❌ DashboardViewModel (Accueil)
- ❌ LibraryViewModel (Bibliothèque)
- ❌ ShowHistoryViewModel (Historique)
- ❌ RosterViewModel (Workers)
- ❌ TitlesViewModel (Titres)
- ❌ InjuriesViewModel (Blessures)
- ❌ ActiveStorylinesViewModel
- ❌ YouthDashboardViewModel
- ❌ FinanceDashboardViewModel
- ❌ CalendarViewModel

**ViewModels existants**:
- ✅ BookingViewModel (Booking principal)
- ✅ GameSessionViewModel (Legacy, trop gros 84KB)

---

## 🏗️ ARCHITECTURE ACTUELLE

### Flux de Données
```
┌─────────────────────────────────────────────────────┐
│              Avalonia Desktop App                   │
├─────────────────────────────────────────────────────┤
│  MainWindow (Shell)                                 │
│    └─ ShellViewModel (Navigation)                   │
│        └─ NavigationService                         │
│            └─ CurrentViewModel Observable           │
│                └─ BookingViewModel / RosterViewModel│
├─────────────────────────────────────────────────────┤
│              Core Services                          │
│  - BookingValidator                                 │
│  - StorylineService                                 │
│  - ShowSimulationEngine                             │
│  - InjuryService                                    │
├─────────────────────────────────────────────────────┤
│              Repository Façade                      │
│  GameRepository (orchestration)                     │
│    ├─ ShowRepository                                │
│    ├─ CompanyRepository                             │
│    ├─ WorkerRepository                              │
│    ├─ ContractRepository                            │
│    ├─ YouthRepository                               │
│    └─ ...autres repositories                        │
├─────────────────────────────────────────────────────┤
│           SQLite Database                           │
│  ringgeneral.db (30+ tables)                        │
│  - Workers, Companies, Contracts                    │
│  - Shows, Segments, SegmentResults                  │
│  - Storylines, Titles, TitleReigns                  │
│  - Youth, Medical, Finances, Broadcast              │
└─────────────────────────────────────────────────────┘
```

### Pattern de Navigation
```csharp
// App.axaml.cs configure le DI
services.AddSingleton<INavigationService, NavigationService>();
services.AddSingleton<ShellViewModel>();

// ShellViewModel construit l'arbre de navigation
NavigationItems = BuildNavigationTree();

// L'utilisateur clique sur un item
NavigateToItem(item) {
    _navigationService.NavigateTo<BookingViewModel>();
}

// Le ViewModel change → UI update via binding
CurrentContentViewModel = vm;
```

### Système de Specs (JSON)
Le projet utilise des fichiers JSON comme "source de vérité" :
```
specs/
├── ui/pages/*.fr.json          # Définitions de pages
├── models/*.fr.json            # Specs des modèles
├── booking/*.fr.json           # Système de booking
├── youth/*.fr.json             # Youth development
├── help/*.fr.json              # Aide et tooltips
└── roadmap.fr.json             # Roadmap de développement
```

---

## 📊 BASE DE DONNÉES

### Schéma (30+ tables)
```sql
-- Géographie
Countries, Regions

-- Entités principales
Companies
  ├─ CompanyCustomization
  └─ NetworkRelations

Workers
  ├─ WorkerAttributes
  └─ WorkerPopularityByRegion

-- Système contractuel
Contracts

-- Système de titres
Titles
  └─ TitleReigns

-- Storytelling
Storylines
  └─ StorylineParticipants

-- Shows & Booking
Shows
  ├─ Segments
  └─ SegmentResults

-- Autres systèmes
Youth, Medical, Finances, Broadcast, Scouting
```

### Migrations SQL
16 fichiers de migration dans `/data/migrations/`:
- `001_init.sql` - Schéma de base
- `002_booking_segments.sql`
- `002_youth.sql` / `002_youth_v1.sql`
- `002_titles.sql`
- `002_scouting.sql`
- `002_medical.sql`
- `002_contracts_v1.sql`
- `002_finances.sql`
- `002_broadcast.sql` / `002_broadcast_v1.sql`
- `002_storylines.sql`
- `002_show_results.sql`
- `002_library.sql`
- `002_backstage.sql`
- `002_shows_calendar.sql`

### Fichier BAKI1.1.db
**Localisation**: `/home/user/Ring-General-Rework.Exe/BAKI1.1.db`
**Taille**: 1.6 MB
**Usage**: Base de données source pour import de données de test

**Outil d'import**: `/src/RingGeneral.Tools.BakiImporter/`
- Convertit les attributs BAKI (0-100) vers Ring General (1-20)
- Utilise un mapping quantile pour distribution statistique
- Génère workers avec contrats et données de base

---

## 📝 DETTE TECHNIQUE

### 1. GameRepository trop volumineux
**Fichier**: `src/RingGeneral.Data/Repositories/GameRepository.cs`
**Taille**: 1,675 lignes
**Statut**: En cours de split vers repositories spécialisés ✅
**Note**: Commentaire dans le code : "DETTE TECHNIQUE - DUPLICATION DE SCHÉMA"

### 2. GameSessionViewModel massif
**Fichier**: `src/RingGeneral.UI/ViewModels/GameSessionViewModel.cs`
**Taille**: 84 KB (legacy)
**Statut**: En cours de split vers ViewModels spécialisés
**Action**: Déjà partiellement refactorisé en BookingViewModel

### 3. Duplication de schéma DB
**Problème**: Tables créées à deux endroits:
- `GameRepository.Initialiser()` → snake_case
- `data/migrations/*.sql` → PascalCase

**Solution recommandée**: Garder uniquement les migrations SQL

### 4. Tests désynchronisés
**Fichiers**:
- `tests/RingGeneral.Tests/MedicalFlowTests.cs`
- `tests/RingGeneral.Tests/SimulationEngineTests.cs`

**Statut**: Signatures de méthodes obsolètes, tests ne compilent plus

---

## 📂 FICHIERS OBSOLÈTES IDENTIFIÉS

### À Supprimer
| Fichier | Raison | Priorité |
|---------|--------|----------|
| `/src/RingGeneral.UI/Views/MainWindow.axaml` | Prototype obsolète | 🔴 HAUTE |
| `/src/RingGeneral.UI/Views/MainWindow.axaml.cs` | Code-behind du prototype | 🔴 HAUTE |
| `/DIAGNOSTIC_CRASH_DEMARRAGE.md` | Diagnostic temporaire (résolu) | 🟡 MOYENNE |
| `/prototypes/*.axaml` | Prototypes de référence seulement | 🟢 BASSE |

### À Archiver (optionnel)
- `/prototypes/` - Garder pour référence UI design
- `/docs/PLAN_ACTION_FR.md` - Plan d'action détaillé (25KB)

---

## 🎯 ROADMAP ACTUELLE

**Source**: `/specs/roadmap.fr.json`

**Étapes (22 au total)**, toutes au statut `"a_faire"`:
1. **Étape 6**: Contrats v1 (négociation FM-style)
2. **Étape 7**: Inbox & News (boucle hebdo)
3. **Étape 8**: Scouting v1 (rapports & shortlist)
4. **Étape 9**: Youth v1 (structures + trainees)
5. **Étape 10**: Shows + Calendrier
6. **Étape 11**: Booking v1 (match/angle + validation)
7. **Étape 12**: Simulation show + ratings
8. **Étape 13**: Storylines + Heat + Momentum
9. **Étape 14**: Titres + historique + contenders
10. **Étape 15**: Finances + billetterie + merch + paie
11. **Étape 16**: Diffusion (TV/Streaming) + audience
12. **Étape 17**: Blessures/Fatigue + médical
13. **Étape 18**: Profondeur backstage (discipline, morale)
14. **Étape 19**: Bibliothèque segments + templates
15. **Étape 20**: Modding + import/export
16. **Étape 21**: QA & équilibrage
17. **Étape 22**: Packaging .exe + performance

---

## 🔧 CORRECTIONS PRIORITAIRES

### 1. Réparer la Navigation (BLOQUANT)
**Actions**:
```bash
# Supprimer l'ancien prototype
rm src/RingGeneral.UI/Views/MainWindow.axaml
rm src/RingGeneral.UI/Views/MainWindow.axaml.cs

# Le bon MainWindow est déjà dans Shell/
# App.axaml.cs l'utilise déjà correctement
```

### 2. Créer les Views Manquantes
**Pattern à suivre**:
```
src/RingGeneral.UI/Views/
├── Booking/
│   └── BookingView.axaml ✅
├── Roster/
│   └── RosterView.axaml ❌ À CRÉER
├── Storylines/
│   └── StorylinesView.axaml ❌ À CRÉER
└── ...
```

### 3. Peupler la DB avec BAKI1.1.db
**Options**:

**Option A - Seed automatique**:
```csharp
// Dans DbInitializer.cs
if (EstNouvelleDb()) {
    ImporterDepuisBaki("BAKI1.1.db");
}
```

**Option B - Commande manuelle**:
```bash
dotnet run --project src/RingGeneral.Tools.BakiImporter -- \
    --source BAKI1.1.db \
    --target ringgeneral.db
```

### 4. Mapper DB → Frontend
**Vérifier que**:
- Les ViewModels chargent les données depuis les repositories
- Les bindings XAML pointent vers les bonnes propriétés
- Les ObservableCollections sont bien rafraîchies

---

## 📈 MÉTRIQUES DU PROJET

| Métrique | Valeur |
|----------|--------|
| Lignes de code (Core) | ~2,500 |
| Lignes de code (UI) | ~4,500 |
| Lignes de code (Data) | ~250K (legacy + refactored) |
| Nombre de tables DB | 30+ |
| Nombre de migrations | 16 |
| Nombre de ViewModels | 33 fichiers |
| Nombre de tests | 18 fichiers |
| Couverture de tests | ~40-80% (variable) |

---

## 🎓 BONNES PRATIQUES EN PLACE

✅ **Architecture en couches** (UI / Core / Data)
✅ **Immutabilité** (sealed records pour les domain models)
✅ **Reactive programming** (ReactiveUI)
✅ **Repository pattern** avec façade
✅ **Dependency Injection** (Microsoft.Extensions.DI)
✅ **Configuration as code** (specs JSON)
✅ **Migrations SQL** versionnées
✅ **Type safety** (navigation générique `NavigateTo<T>()`)

---

## 🚀 PROCHAINES ÉTAPES RECOMMANDÉES

### Phase 1 - Stabilisation (URGENT)
1. ✅ Analyser l'architecture
2. ⏳ Supprimer les fichiers obsolètes
3. ⏳ Réparer la navigation (supprimer ancien MainWindow)
4. ⏳ Créer les ViewModels manquants
5. ⏳ Créer les Views correspondantes

### Phase 2 - Données (PRIORITAIRE)
6. ⏳ Implémenter le seed automatique depuis BAKI1.1.db
7. ⏳ Vérifier le mapping DB → ViewModels
8. ⏳ Tester l'affichage des données dans chaque page
9. ⏳ Corriger les bindings XAML si nécessaire

### Phase 3 - Tests (IMPORTANT)
10. Corriger les tests désynchronisés
11. Ajouter tests pour la navigation
12. Ajouter tests d'intégration UI

### Phase 4 - Polish (NORMAL)
13. Mettre à jour la documentation
14. Enrichir la roadmap avec l'état actuel
15. Optimiser les performances (cache, pagination)

---

## 📞 CONTACT & RESSOURCES

**Guides disponibles**:
- `/README.md` - Documentation principale
- `/QUICK_START_GUIDE.md` - Guide de démarrage
- `/docs/ARCHITECTURE_REVIEW_FR.md` - Revue d'architecture (40KB)
- `/docs/DEV_GUIDE_FR.md` - Guide développeur
- `/docs/IMPORT_GUIDE_FR.md` - Guide d'import BAKI

**Commandes utiles**:
```bash
# Build
dotnet build RingGeneral.sln

# Tests
dotnet test

# Lancer l'app
dotnet run --project src/RingGeneral.UI

# Import BAKI
dotnet run --project src/RingGeneral.Tools.BakiImporter
```

---

**Fin du récapitulatif** - Version 1.0 (2026-01-07)

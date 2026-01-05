# Ring General - Revue Architecture Complète

**Date**: 2026-01-05
**Version**: 2.0
**Statut**: En développement actif
**Langage**: C# / .NET 8.0

---

## Résumé Exécutif

**Ring General** est un jeu de gestion de compagnie de catch professionnel (style Football Manager/TEW) développé en .NET 8.0 avec Avalonia UI. Le projet suit une **architecture en couches** avec séparation claire entre UI, logique métier, accès aux données et spécifications. Le code est entièrement en **français** et démontre des patterns solides pour un système de gestion de jeu complexe.

### Métriques Clés

| Métrique | Valeur |
|----------|--------|
| Projets dans la solution | 7 |
| Fichiers C# sources | 131+ |
| Fichiers de tests | 18 |
| Framework | .NET 8.0 LTS |
| UI Framework | Avalonia 11.0.6 |
| Base de données | SQLite 8.0.0 |
| Fichiers de migration | 16 |
| Packages NuGet externes | 10 |

### Notation Globale: **7/10**

**Points forts**: Architecture modulaire, modèles immuables, couverture tests solide, dépendances minimales
**Points à améliorer**: Repository monolithique, absence de DI container, logging structuré manquant, ViewModels trop larges

---

## 1. Structure du Projet

### 1.1 Organisation de la Solution

```
RingGeneral.sln (7 projets)
│
├── Couche Core (Logique Métier)
│   ├── RingGeneral.Core (60 fichiers C#)
│   │   ├── Models/ - Entités du domaine (records immuables)
│   │   ├── Services/ - Services métier
│   │   ├── Simulation/ - Moteurs de simulation
│   │   ├── Medical/ - Système de blessures
│   │   ├── Contracts/ - Négociations de contrats
│   │   ├── Random/ - Générateur aléatoire déterministe
│   │   ├── Validation/ - Validation métier
│   │   └── Interfaces/ - Contrats de services
│   │
│   └── RingGeneral.Specs
│       ├── Models/ - Modèles de configuration
│       └── Services/ - Chargement JSON specs
│
├── Couche Data (Accès aux Données)
│   └── RingGeneral.Data
│       ├── Database/ - Initialisation & migrations
│       ├── Repositories/ - Pattern Repository
│       └── Models/ - DTOs & modèles de persistance
│
├── Couche Présentation
│   └── RingGeneral.UI (WinExe)
│       ├── Views/ - Vues Avalonia (AXAML)
│       ├── ViewModels/ - ViewModels MVVM (18 fichiers)
│       └── Services/ - Services UI
│
├── Outils
│   ├── RingGeneral.Tools.BakiImporter (CLI import DB BAKI)
│   └── RingGeneral.Tools.DbManager (Utilitaires DB)
│
└── Tests
    └── RingGeneral.Tests (18 fichiers xUnit)
```

### 1.2 Graphe de Dépendances

```
RingGeneral.UI (WinExe)
  ├─> RingGeneral.Core
  ├─> RingGeneral.Data
  └─> RingGeneral.Specs

RingGeneral.Data
  ├─> RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Specs
  └─> (Aucune dépendance - Pure configuration)

RingGeneral.Tools.*
  ├─> RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Tests
  ├─> RingGeneral.Core
  ├─> RingGeneral.Data
  └─> RingGeneral.Specs
```

**Analyse**: Dépendances unidirectionnelles correctes, pas de références circulaires. ✅

---

## 2. Architecture & Patterns

### 2.1 Pattern Architectural: **Layered Architecture avec influences DDD**

```
┌─────────────────────────────────────────┐
│  COUCHE PRÉSENTATION (UI)                │
│  - Avalonia MVVM                         │
│  - ReactiveUI pour bindings réactifs     │
│  - DataGrid pour affichage tabulaire     │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│  COUCHE LOGIQUE MÉTIER (Core)            │
│  - Modèles du domaine (records)          │
│  - Moteurs de simulation                 │
│  - Services métier                       │
│  - Validation & contrats                 │
│  - Système médical                       │
│  - Spécifications JSON                   │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│  COUCHE ACCÈS DONNÉES (Data)             │
│  - Pattern Repository                    │
│  - SQLite avec migrations                │
│  - Initialisation DB                     │
│  - Gestion sauvegardes                   │
└─────────────────────────────────────────┘
```

### 2.2 Modèles du Domaine

Tous les modèles utilisent des **C# sealed records** (immuables, sémantique par valeur):

#### Entités Principales

**WorkerSnapshot** (Profil catcheur/talent)
```csharp
public sealed record WorkerSnapshot(
    string WorkerId,
    string NomComplet,
    int InRing,           // Compétence ring (0-100)
    int Entertainment,    // Charisme (0-100)
    int Story,           // Storytelling (0-100)
    int Popularite,
    int Fatigue,
    string Blessure,      // Statut blessure
    int Momentum,
    string RoleTv,
    int Morale);
```

**ShowDefinition** (Définition d'un show)
```csharp
public sealed record ShowDefinition(
    string ShowId,
    string Nom,
    int Semaine,
    string Region,
    int DureeMinutes,
    string CompagnieId,
    string? DealTvId,
    string Lieu,
    string Diffusion);
```

**SegmentDefinition** (Segment TV - match/promo/angle)
```csharp
public sealed record SegmentDefinition(
    string SegmentId,
    string TypeSegment,     // "match", "promo", "angle_backstage"
    IReadOnlyList<string> Participants,
    int DureeMinutes,
    bool EstMainEvent,
    string? StorylineId,
    string? TitreId,
    int Intensite,
    string? VainqueurId,
    string? PerdantId,
    IReadOnlyDictionary<string, string>? Settings = null);
```

**StorylineInfo** (Storyline/Feud/Angle)
```csharp
public sealed record StorylineInfo(
    string StorylineId,
    string Nom,
    StorylinePhase Phase,  // BUILD, PEAK, BLOWOFF
    int Heat,
    StorylineStatus Status,
    string? Resume,
    IReadOnlyList<StorylineParticipant> Participants);
```

**GameStateDelta** (Résultats d'impacts de simulation)
```csharp
public sealed record GameStateDelta(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, string> Blessures,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteWorkersDelta,
    IReadOnlyDictionary<string, int> PopulariteCompagnieDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<FinanceTransaction> Finances);
```

### 2.3 Services Métier

**Localisation**: `src/RingGeneral.Core/Services/`

| Service | Responsabilité | Taille |
|---------|----------------|--------|
| `ShowSchedulerService` | Créer/gérer shows, valider runtime & billets | ~150 lignes |
| `BookingBuilderService` | Construire cartes de booking, gestion segments | ~200 lignes |
| `StorylineService` | Créer/mettre à jour storylines, tracking heat | ~180 lignes |
| `TitleService` | Création titres, règnes, gestion contenders | ~160 lignes |
| `ContenderService` | Classements, logique #1 contender | ~120 lignes |
| `TemplateService` | Templates de booking, patterns de segments | ~140 lignes |

### 2.4 Moteurs de Simulation

**Localisation**: `src/RingGeneral.Core/Simulation/`

| Moteur | Fonction | Taille |
|--------|----------|--------|
| `ShowSimulationEngine` | Simuler shows TV, calculer ratings, impacts | **434 lignes** |
| `FinanceEngine` | Calculer revenus, dépenses, trésorerie | 159 lignes |
| `WorkerGenerationService` | Générer workers pour youth & free agents | 320 lignes |
| `ScoutingService` | Rapports de scouting, découverte talents | 173 lignes |
| `YouthProgressionService` | Progression des élèves/trainees | 131 lignes |
| `WorldSimScheduler` | Simulation compagnies non-joueur | 118 lignes |
| `BackstageService` | Incidents backstage, moral | 133 lignes |
| `DisciplineService` | Appliquer discipline & pénalités | 57 lignes |

**Exemple de logique (ShowSimulationEngine)**:
- Calcule score de base à partir attributs workers (InRing, Entertainment, Story)
- Applique modificateurs: heat crowd, moral, chimie
- Détecte problèmes de rythme (promos consécutives, segments lents)
- Calcule impacts fatigue, momentum, heat storyline
- Utilise `IRandomProvider` pour random déterministe

### 2.5 Pattern Repository

**Localisation**: `src/RingGeneral.Data/Repositories/`

| Repository | Fonction | Taille |
|------------|----------|--------|
| `GameRepository` | CRUD principal pour toutes entités | **3,750 lignes** ⚠️ |
| `TitleRepository` | Gestion titres & règnes | 207 lignes |
| `MedicalRepository` | Tracking blessures & récupération | 49 lignes |
| `BackstageRepository` | Incidents backstage, discipline | 153 lignes |
| `WeeklyLoopService` | Orchestration simulation hebdomadaire | 381 lignes |

**RepositoryBase Pattern**:
```csharp
public abstract class RepositoryBase
{
    protected static void AjouterParametre(SqliteCommand commande, string nom, object valeur)
    {
        commande.Parameters.AddWithValue(nom, valeur ?? DBNull.Value);
    }
}
```

**⚠️ PROBLÈME IDENTIFIÉ**: `GameRepository` est un **monolithe de 3,750 lignes** gérant toutes les entités.

### 2.6 Couche UI (Avalonia MVVM)

**Localisation**: `src/RingGeneral.UI/`

**Stack Technologique**:
- **Avalonia 11.0.6** - Framework UI cross-platform
- **ReactiveUI** - MVVM + propriétés réactives
- **Avalonia.Controls.DataGrid** - Vues tabulaires
- **Avalonia.Themes.Fluent** - Design Fluent

**ViewModels Principaux** (18 fichiers):

| ViewModel | Fonction | Taille |
|-----------|----------|--------|
| `GameSessionViewModel` | Logique de jeu principale, binding | **2,092 lignes** ⚠️ |
| `ShellViewModel` | Navigation principale & gestion sauvegardes | ~400 lignes |
| `SaveManagerViewModel` | Système save/load | ~300 lignes |
| `SegmentViewModel` | Gestion carte de booking | ~250 lignes |
| `StorylineViewModel` | Gestion feuds/angles | ~200 lignes |
| Autres ViewModels spécialisés | Divers | Variable |

**⚠️ PROBLÈME IDENTIFIÉ**: `GameSessionViewModel` est **trop large** (2,092 lignes).

### 2.7 Spécifications (Configuration Data-Driven)

**Localisation**: `src/RingGeneral.Specs/`

Specs = **fichiers JSON chargés au runtime** pour définir le contenu du jeu:

```
specs/
├── navigation.fr.json (Structure sidebar/navigation UI)
├── ui/pages/*.fr.json (Définitions de pages)
├── booking/segment-types.fr.json (Catalogue types de segments)
├── help/*.fr.json (Aide en jeu/codex)
├── models/
│   ├── worker-generation.fr.json
│   ├── world-sim.fr.json
│   ├── contracts.fr.json
│   └── ... (specs domaine)
└── import/ (Mapping import de données)
```

**Service SpecsReader**:
```csharp
public sealed class SpecsReader
{
    public T Charger<T>(string chemin)
    {
        var json = File.ReadAllText(chemin);
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
```

**Avantage**: Configuration modifiable sans recompilation, support modding facilité.

---

## 3. Patterns de Conception Utilisés

| Pattern | Localisation | Exemple |
|---------|--------------|---------|
| **Repository** | Couche Data | `GameRepository`, `TitleRepository` |
| **Factory/Builder** | Services | `ShowSchedulerService.CreerShow()` |
| **Strategy** | Simulation | Modèles multiples de rating (AudienceModel, HeatModel) |
| **Observer** | UI bindings | Notifications ReactiveUI property change |
| **Specification/DTO** | Couche Specs | Specs domaine basées JSON |
| **Record Types** | Modèles | Toutes entités domaine = C# sealed records |
| **Template Method** | Validation | `BookingValidator.ValiderBooking()` |
| **Query Object** | Repositories | Requêtes complexes dans GameRepository |

---

## 4. Couche de Données

### 4.1 Technologie: SQLite 8.0.0

**Dépendance**: `Microsoft.Data.Sqlite` Version 8.0.0

**Pattern Connection Factory**:
```csharp
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public string DatabasePath { get; }

    public SqliteConnection OuvrirConnexion()
    {
        var connexion = new SqliteConnection(_connectionString);
        connexion.Open();
        return connexion;
    }
}
```

### 4.2 Stratégie de Migration

**Localisation**: `/data/migrations/` (16 fichiers)

**Migrations SQL versionnées**:

```
001_init.sql           (10.9 KB - Schéma core)
002_backstage.sql      (Incidents backstage)
002_booking_segments.sql
002_broadcast.sql      (Deals TV)
002_broadcast_v1.sql
002_contracts_v1.sql   (Système contrats)
002_finances.sql       (Tracking financier)
002_library.sql        (Bibliothèque segments)
002_medical.sql        (Système blessures)
002_scouting.sql       (Rapports scouting)
002_show_results.sql   (Historique shows)
002_shows_calendar.sql (Calendrier événements)
002_storylines.sql     (Système feuds)
002_titles.sql         (Titres/championnats)
002_youth.sql          (Système youth)
002_youth_v1.sql
```

**Exécution des Migrations** (`DbInitializer.cs`):
```csharp
public void ApplyMigrations(string cheminDb)
{
    using var connexion = new SqliteConnection($"Data Source={cheminDb}");
    connexion.Open();

    ActiverForeignKeys(connexion);  // PRAGMA foreign_keys = ON
    AssurerTableVersion(connexion); // Créer table SchemaVersion

    var migrations = ChargerMigrations();
    var versionsAppliquees = ChargerVersionsAppliquees(connexion);

    foreach (var migration in migrations.OrderBy(m => m.Version))
    {
        if (versionsAppliquees.Contains(migration.Version))
            continue;

        using var transaction = connexion.BeginTransaction();
        // Exécuter SQL migration
        // Enregistrer version dans table SchemaVersion
    }
}
```

### 4.3 Schéma de Base de Données

**Schéma Initial (001_init.sql)** - 150+ lignes:

**Tables Clés**:
```sql
-- Monde
Countries, Regions

-- Organisation
Companies, CompanyCustomization, NetworkRelations

-- Personnes
Workers, WorkerAttributes, WorkerPopularityByRegion

-- Contrats & Emploi
Contracts

-- Titres/Championnats
Titles, TitleReigns

-- Storylines/Feuds
Storylines, StorylineParticipants

-- Shows/Événements
Shows, ShowHistory, ShowSegments, SegmentParticipants

-- Système Médical
Injuries, MedicalNotes, RecoveryPlans

-- Développement Youth
YouthStructures, Trainees, TraineeProgress

-- Diffusion/Deals TV
TvDeals

-- Finances
FinanceTransactions

-- Scouting
ScoutReports, ScoutMissions

-- Backstage
BackstageIncidents

-- État du Jeu
SchemaVersion (pour migrations)
```

**Contraintes d'Intégrité**:
- Foreign keys activées (`PRAGMA foreign_keys = ON`)
- Contraintes NOT NULL sur champs critiques
- Index sur colonnes fréquemment requêtées

### 4.4 Gestion des Sauvegardes

**Localisation**: `src/RingGeneral.Data/Database/SaveGameManager.cs`

```csharp
public sealed class SaveGameManager
{
    public string SavesDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RingGeneral", "Saves");

    public IReadOnlyList<SaveGameInfo> ListerSaves()
    public SaveGameInfo CreerNouvellePartie(string? nom)
    public SaveGameInfo ImporterBase(string cheminSource)
    public SaveGameInfo DupliquerSauvegarde(string cheminSource)
    public void SupprimerSauvegarde(string cheminSource)
}
```

**Fonctionnalités**:
- Slots de sauvegarde multiples dans `%APPDATA%/RingGeneral/Saves/`
- Validation de sauvegardes (DbValidator)
- Import/export de bases de données
- Nommage auto avec timestamps

---

## 5. Build & Configuration

### 5.1 Système de Build: .NET 8.0 avec dotnet CLI

**Target Framework**: net8.0 (tous projets)

**Configuration Projet**:
```xml
<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Projet UI**: Application desktop WinExe
```xml
<OutputType>WinExe</OutputType>
```

**Projets Outils**: Applications console
```xml
<OutputType>Exe</OutputType>
```

### 5.2 Dépendances Externes

**Dépendances Core**:
- Microsoft.Data.Sqlite 8.0.0
- System.Text.Json (intégré, utilisé pour sérialisation)

**Dépendances UI**:
- Avalonia 11.0.6
- Avalonia.Desktop 11.0.6
- Avalonia.Controls.DataGrid 11.0.6
- Avalonia.Themes.Fluent 11.0.6
- Avalonia.Fonts.Inter 11.0.6
- Avalonia.ReactiveUI 11.0.6

**Dépendances Tests**:
- xunit 2.6.2
- xunit.runner.visualstudio 2.5.4
- Microsoft.NET.Test.Sdk 17.8.0

**✅ POINT FORT**: Dépendances externes minimales
- Pas d'ORM (Entity Framework) - SQL/ADO.NET direct
- Pas de conteneur DI (Microsoft.Extensions.DependencyInjection)
- Pas de framework de logging (Serilog, NLog)

### 5.3 CI/CD

**Localisation**: `.github/workflows/`

**Workflow CI** (`ci.yml`):
```yaml
on:
  push: [main]
  pull_request: [main]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - Setup .NET 8.0.x
      - dotnet restore RingGeneral.sln
      - dotnet build RingGeneral.sln -c Release --no-restore
```

**Build Release** (`build-windows.yml`):
```bash
dotnet publish src/RingGeneral.UI/RingGeneral.UI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o artifacts/win-x64
```

**Sortie**: Exécutable unique self-contained pour Windows

---

## 6. Tests & Qualité

### 6.1 Framework de Tests: xUnit 2.6.2

**Couverture Tests** (18 fichiers):

| Fichier de Test | Couverture |
|----------------|------------|
| `BookingTests` | CRUD segments & warnings validation |
| `SimulationEngineTests` | Calculs de ratings |
| `WorkerGenerationServiceTests` | Logique génération workers |
| `TitleServiceTests` | Gestion titres |
| `FinanceEngineTests` | Calculs revenus/dépenses |
| `MedicalFlowTests` / `MedicalWorkflowTests` | Système blessures |
| `ContractNegotiationTests` | Flux négociation contrats |
| `ScoutingServiceTests` | Logique missions scouting |
| `YouthProgressionServiceTests` | Développement trainees |
| `BackstageFlowTests` | Incidents backstage |
| `WorldSimSchedulerTests` | Simulation compagnies non-joueur |
| `SeededRandomProviderTests` | Random déterministe |
| `BakiAttributeConversionTests` | Conversion import |
| `TemplateServiceTests` | Templates booking |
| `HelpSpecsTests` | Validation contenu aide |
| `ContractSpecsTests` | Configuration contrats |

**Pattern de Test Exemple**:
```csharp
[Fact]
public void CrudSegment_Persist_And_Reload()
{
    var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral_{Guid.NewGuid():N}.db");
    try
    {
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
        var repository = new GameRepository(factory);
        repository.Initialiser();
        // ... opérations de test
    }
    finally
    {
        if (File.Exists(dbPath))
            File.Delete(dbPath);
    }
}
```

### 6.2 Gestion des Erreurs

**Pattern**: Lancement d'exceptions traditionnel avec validation d'entrée

**⚠️ PROBLÈME**: Pas de framework de logging dédié
- Implications:
  - Pas de logging structuré dans l'application
  - Pas de tracking d'erreurs centralisé
  - Debugging production difficile

**Exemples de Gestion d'Exceptions**:
```csharp
// SaveGameManager.cs
if (string.IsNullOrWhiteSpace(cheminSource))
    throw new InvalidOperationException("Chemin d'import manquant.");

// DbInitializer.cs
if (!File.Exists(cheminDb))
    throw new InvalidOperationException("Chemin de base de données invalide.");

// SpecsReader.cs
if (!File.Exists(chemin))
    throw new FileNotFoundException($"Spécification introuvable: {chemin}");
```

**Pattern de Validation**:
```csharp
public ValidationResult ValiderBooking(BookingPlan plan)
{
    var issues = new List<ValidationIssue>();

    if (plan.Segments.Count == 0)
    {
        issues.Add(new ValidationIssue(
            ValidationSeverity.Erreur,
            "booking.empty",
            "Aucun segment n'a été booké."));
    }

    return new ValidationResult(issues);
}
```

---

## 7. Analyse Critique

### 7.1 ✅ Points Forts Architecturaux

**1. Séparation Claire des Responsabilités**
- UI complètement séparée de la logique métier
- Pattern Repository isole l'accès aux données
- Modèles du domaine indépendants de l'infrastructure

**2. Modèles du Domaine Immuables**
- Toutes entités utilisent C# `sealed record`
- Empêche mutation accidentelle d'état
- Thread-safe par défaut

**3. Couverture Domaine Complète**
- Simulation gestion catch complète
- Algorithme ratings de shows complexe
- Génération workers multi-niveaux (youth + free agents)
- Système médical (blessures, plans récupération)
- Tracking financier

**4. Architecture Testable**
- Services acceptent dépendances via constructeur
- Interfaces repository permettent mocking
- 18 fichiers de tests avec bonne couverture
- Provider random déterministe pour simulations reproductibles

**5. Design Piloté par Spécifications**
- Configuration basée JSON pour UI/gameplay
- Facile à étendre sans changements code
- Approche data-driven pour support modding

**6. Dépendances Externes Minimales**
- Pas de frameworks lourds
- Utilisation directe ADO.NET
- Capacité déploiement self-contained

### 7.2 ⚠️ Problèmes & Anti-Patterns Identifiés

**1. GameRepository Monolithique (3,750 lignes)**
- **Problème**: Repository unique gère TOUS les types d'entités
- **Impact**: Difficile à tester, maintenir et comprendre
- **Recommandation**: Diviser en repositories spécifiques par domaine:
  ```
  IWorkerRepository
  IShowRepository
  ITitleRepository
  IStorylineRepository
  IContractRepository
  IMedicalRepository
  ```

**2. Absence de Conteneur d'Injection de Dépendances**
- **Problème**: Instanciation manuelle dans ViewModels
- **Impact**: Couplage fort, difficile d'échanger implémentations
- **Recommandation**: Ajouter Microsoft.Extensions.DependencyInjection
  ```csharp
  services.AddSingleton<SqliteConnectionFactory>();
  services.AddScoped<IGameRepository, GameRepository>();
  services.AddScoped<ShowSimulationEngine>();
  ```

**3. Absence de Framework de Logging Centralisé**
- **Problème**: Erreurs lancées mais pas loguées
- **Impact**: Debugging production difficile
- **Manque**: Intégration Serilog ou ILogger
- **Recommandation**: Ajouter logging structuré:
  ```csharp
  _logger.LogInformation("Simulation démarrée pour show {ShowId}", showId);
  _logger.LogError(ex, "Migration échouée pour version {Version}", version);
  ```

**4. ViewModel Large (GameSessionViewModel - 2,092 lignes)**
- **Problème**: ViewModel monolithique gérant toute logique jeu
- **Impact**: Complexe, difficile à tester
- **Recommandation**: Extraire en ViewModels plus petits et focalisés:
  ```
  BookingViewModel
  SimulationViewModel
  WorkerManagementViewModel
  FinancialViewModel
  ```

**5. Validation Faible dans Plusieurs Endroits**
- **Problème**: Logique validation éparpillée (BookingValidator, ShowSchedulerService, etc.)
- **Impact**: Règles de validation incohérentes
- **Recommandation**: Service de validation centralisé avec builder fluent

**6. Absence de Récupération d'Erreurs**
- **Problème**: Exceptions lancées, pas de mécanisme de récupération
- **Impact**: Crashes au lieu de dégradation gracieuse
- **Exemple**: Désérialisation JSON catch JsonException mais re-throw comme null
- **Recommandation**: Pattern Result<T, Error> ou monade Maybe

**7. Identification de Types Basée sur Strings**
- **Problème**: Types segments comme strings ("match", "promo", "angle_backstage")
- **Impact**: Erreurs runtime possibles, pas de sécurité compile-time
- **Recommandation**: Utiliser enums ou unions discriminées

### 7.3 ❌ Composants Manquants

**1. Absence de Couche de Cache**
- Recommandation: Ajouter cache en mémoire ou distribué pour entités fréquemment accédées

**2. Absence de Couche API**
- Statut: Desktop single-player uniquement
- Si multijoueur prévu: Ajouter projet API ASP.NET Core

**3. Absence d'Event Bus/Pub-Sub**
- Recommandation: Utiliser pour distribution événements simulation (WorkerInjured, ShowSimulated, etc.)

**4. Absence de Trail d'Audit**
- Manque: Qui a changé quoi et quand
- Recommandation: Ajouter tables audit ou event sourcing

**5. Absence de Monitoring de Performance**
- Manque: Timing exécution requêtes, hooks profiling mémoire
- Critique pour gérer 200k workers (mentionné dans README)

**6. Absence de Tâches en Background/Scheduling**
- Statut: Toutes opérations synchrones
- Impact: UI peut freezer pendant simulations lourdes
- Recommandation: Ajouter Hangfire ou BackgroundService

### 7.4 Observations Schéma de Base de Données

**Points Forts**:
- Contraintes foreign key activées
- Stratégie d'indexation appropriée (niveau schéma)
- Design normalisé
- Support transactions

**Problèmes**:
- Pas de documentation/commentaires colonnes
- Conventions nommage mixtes (CamelCase vs snake_case)
- Pas de génération ID auto-increment pour tables audit
- Hints optimisation requêtes limités

---

## 8. Recommandations Architecturales

### Priorité 1: Impact Élevé, Effort Moyen

**1. Implémenter Conteneur DI**
- Utiliser Microsoft.Extensions.DependencyInjection
- Réduire complexité ViewModels
- **Fichiers affectés**: `GameSessionViewModel.cs`, `ShellViewModel.cs`, `Program.cs`

**2. Ajouter Logging Structuré**
- Intégrer Serilog ou ILogger
- Ajouter wrapper try-catch pour opérations base de données
- **Fichiers affectés**: Tous repositories, simulation engines

**3. Diviser GameRepository**
- Créer repositories spécifiques par domaine (Worker, Show, Title, etc.)
- Implémenter interface IRepository de base
- **Fichiers affectés**: `GameRepository.cs` (split en 6-8 fichiers)

**4. Ajouter Gestion Configuration**
- Utiliser IConfiguration pour settings environnement
- Support appsettings.json pour chemins DB, settings simulation
- **Nouveau fichier**: `appsettings.json`, `ConfigurationService.cs`

### Priorité 2: Impact Moyen, Effort Moyen

**5. Implémenter Pattern Result<T>**
- Remplacer flux piloté par exceptions avec types Result
- Meilleure gestion erreurs et récupération
- **Fichiers affectés**: Tous services, repositories

**6. Ajouter Monitoring de Performance**
- Ajouter timing exécution requêtes
- Profiler bottlenecks moteur simulation
- **Nouveau fichier**: `PerformanceMonitor.cs`

**7. Extraire Composants MVVM**
- Diviser GameSessionViewModel en ViewModels plus petits
- Créer composants UI réutilisables
- **Fichiers affectés**: `GameSessionViewModel.cs` (split en 4-6 fichiers)

**8. Implémenter Cache**
- Cacher attributs workers, données compagnie
- Implémenter stratégie invalidation
- **Nouveau fichier**: `CacheService.cs`

### Priorité 3: Nice-to-Have, Effort Élevé

**9. Ajouter Event Bus**
- Activer architecture event-driven
- Découpler simulation des mises à jour UI
- **Nouveau package**: MediatR ou custom event bus

**10. Implémenter Trail d'Audit**
- Tracker toutes modifications
- Support replay/historique jeu
- **Nouveaux fichiers**: Tables audit, `AuditService.cs`

**11. Ajouter Simulation en Background**
- Simulation non-bloquante pour grands mondes
- UI de rapport de progression
- **Nouveau fichier**: `BackgroundSimulationService.cs`

**12. Créer API REST**
- Si multijoueur prévu
- Serveur séparé pour simulation monde
- **Nouveau projet**: `RingGeneral.API`

---

## 9. Exemples d'Implémentation

### 9.1 Modèle du Domaine

```csharp
// Sealed record - immuable, sémantique par valeur
public sealed record WorkerSnapshot(
    string WorkerId,
    string NomComplet,
    int InRing,           // Échelle 0-100
    int Entertainment,    // Échelle 0-100
    int Story,           // Échelle 0-100
    int Popularite,
    int Fatigue,
    string Blessure,
    int Momentum,
    string RoleTv,
    int Morale)
{
    // Peut ajouter propriétés calculées:
    public int OverallRating => (InRing + Entertainment + Story) / 3;
    public bool EstBlesse => Blessure != "AUCUNE";
}
```

### 9.2 Service (ShowSimulationEngine)

```csharp
public sealed class ShowSimulationEngine
{
    private readonly IRandomProvider _random;
    private readonly AudienceModel _audienceModel;

    public ShowSimulationEngine(IRandomProvider random, AudienceModel? model = null)
    {
        _random = random;
        _audienceModel = model ?? new AudienceModel();
    }

    public ShowSimulationResult Simuler(ShowContext context)
    {
        var fatigueDelta = new Dictionary<string, int>();
        var impacts = new GameStateDelta(...);

        foreach (var segment in context.Segments)
        {
            var baseScore = CalculerScore(segment, context);
            var crowdBonus = CalculerBonusFoule(context.Compagnie);
            var note = Math.Clamp(baseScore + crowdBonus, 0, 100);

            // Appliquer impacts aux workers
            ApplyerImpactSegment(segment, impacts);
        }

        return new ShowSimulationResult(impacts, details);
    }
}
```

### 9.3 Validation

```csharp
public sealed class BookingValidator : IValidator
{
    public ValidationResult ValiderBooking(BookingPlan plan)
    {
        var issues = new List<ValidationIssue>();

        // Vérifier booking vide
        if (plan.Segments.Count == 0)
            issues.Add(new ValidationIssue(
                ValidationSeverity.Erreur,
                "booking.empty",
                "Aucun segment n'a été booké."));

        // Vérifier durée
        var dureeTotale = plan.Segments.Sum(s => s.DureeMinutes);
        if (dureeTotale > plan.DureeShowMinutes)
            issues.Add(new ValidationIssue(
                ValidationSeverity.Erreur,
                "booking.duration.exceed",
                $"Durée dépasse: {dureeTotale} > {plan.DureeShowMinutes}"));

        // Vérifier force main event
        var mainEvent = plan.Segments.FirstOrDefault(s => s.EstMainEvent);
        if (mainEvent?.ParticipantsDetails?.Average(p => p.Popularite) < 45)
            issues.Add(new ValidationIssue(
                ValidationSeverity.Avertissement,
                "booking.main-event.weak",
                "Main event trop faible pour porter le show."));

        return new ValidationResult(issues);
    }
}
```

---

## 10. Déploiement & Distribution

### 10.1 Format de Publication

**Exécutable Windows Self-Contained**:
```bash
dotnet publish src/RingGeneral.UI/RingGeneral.UI.csproj \
  -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

**Sortie**: Fichier .exe unique (pas de runtime .NET requis sur machine cible)

### 10.2 Structure de Déploiement

```
RingGeneral/
├── RingGeneral.UI.exe (Application principale)
├── specs/ (Fichiers JSON de configuration - REQUIS)
│   ├── navigation.fr.json
│   ├── ui/pages/*.fr.json
│   ├── booking/segment-types.fr.json
│   └── ... (autres specs)
└── data/migrations/ (Migrations SQL - incluses dans build)
```

### 10.3 Emplacement des Données

**Base de Données SQLite**: `%APPDATA%/RingGeneral/Saves/`

**Fichiers de Configuration**: Fichiers JSON specs bundlés dans dossier `specs/` (doivent être présents)

---

## 11. Métriques de Code

| Métrique | Valeur |
|----------|--------|
| Total fichiers C# sources | 131+ |
| Fichiers de tests | 18 |
| Projets dans solution | 7 |
| Namespaces core | 20+ |
| Modèles domaine (sealed records) | 40+ |
| Classes Service | 15+ |
| Classes Repository | 8 |
| Fichiers migration | 16 |
| Fichier le plus grand | GameRepository.cs (3,750 lignes) |
| Deuxième plus grand | GameSessionViewModel.cs (2,092 lignes) |
| Packages NuGet externes | 10 |
| Version .NET | 8.0 LTS |
| Framework UI | Avalonia 11.0.6 |
| Framework Tests | xUnit 2.6.2 |

---

## 12. Conclusion

Ring General démontre une **architecture en couches solide** avec modélisation domaine claire et bon usage des fonctionnalités C# modernes (records, nullable reference types). Le design est testable et maintenable à petite échelle.

### Note Globale: **7/10**

**Points Forts Clés**:
- ✅ Immuabilité des modèles
- ✅ Séparation des responsabilités
- ✅ Couverture tests solide
- ✅ Dépendances minimales

**Améliorations Critiques Nécessaires**:
1. Implémentation conteneur DI
2. Raffinement pattern Repository (diviser monolithe)
3. Logging structuré
4. Standardisation gestion erreurs

**Évaluation Globale**: Bonne architecture fondationnelle avec espace pour maturation dans patterns enterprise et observabilité opérationnelle.

---

## 13. Prochaines Étapes Recommandées

### Court Terme (1-2 sprints)
1. Implémenter Microsoft.Extensions.DependencyInjection
2. Ajouter Serilog pour logging structuré
3. Diviser GameRepository en 6 repositories spécifiques

### Moyen Terme (3-6 sprints)
4. Extraire GameSessionViewModel en composants plus petits
5. Implémenter pattern Result<T> pour gestion erreurs
6. Ajouter monitoring performance et profiling

### Long Terme (6+ sprints)
7. Système d'audit complet
8. Event bus pour architecture event-driven
9. Support simulation en background pour grands mondes
10. API REST si multijoueur prévu

---

**Document généré le**: 2026-01-05
**Auteur**: Claude (Architecture Review Assistant)
**Version**: 1.0

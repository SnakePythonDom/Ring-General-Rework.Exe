# Ring General - Plan d'Action Détaillé

**Date**: 2026-01-05
**Basé sur**: Analyse du rapport d'architecture + vérification du code
**Objectif**: Stabiliser le projet, corriger les problèmes critiques, puis implémenter les fonctionnalités manquantes

---

## Vue d'Ensemble des Phases

| Phase | Objectif | Priorité | Prérequis |
|-------|----------|----------|-----------|
| **0** | Stabilisation critique | 🔴 BLOQUANT | - |
| **1** | Fondations solides | 🔴 HAUTE | Phase 0 |
| **2** | Fonctionnalités incomplètes | 🟡 MOYENNE | Phase 1 |
| **3** | Performance 200k workers | 🟡 MOYENNE | Phase 1 |
| **4** | Polish & QA | 🟢 NORMALE | Phases 2-3 |

---

## Phase 0: Stabilisation Critique

> **Objectif**: Rendre le projet buildable et testable avant tout développement

### Tâche 0.1: Corriger le pipeline CI/CD

**Fichier**: `.github/workflows/ci.yml`

**Actions**:
```yaml
# Ajouter après le build:
- name: Run tests
  run: dotnet test RingGeneral.sln --no-build -c Release --verbosity normal
```

**Critères d'acceptation**:
- [ ] Le workflow CI exécute `dotnet test`
- [ ] Les tests passent ou échouent explicitement
- [ ] Rapport de tests visible dans GitHub Actions

**Effort estimé**: 1 heure

---

### Tâche 0.2: Synchroniser les tests désynchronisés

**Fichiers concernés**:
- `tests/RingGeneral.Tests/MedicalFlowTests.cs`
- `tests/RingGeneral.Tests/SimulationEngineTests.cs`

**Actions**:
1. Identifier les signatures de méthodes obsolètes
2. Mettre à jour les appels de test pour correspondre aux APIs actuelles
3. Exécuter tous les tests localement

**Critères d'acceptation**:
- [ ] `MedicalFlowTests` compile et passe
- [ ] `SimulationEngineTests` compile et passe
- [ ] 0 erreurs de compilation dans le projet Tests

**Effort estimé**: 2-4 heures

---

### Tâche 0.3: Résoudre la duplication de schéma

**Problème**: Deux systèmes créent des tables:
1. `GameRepository.Initialiser()` → tables snake_case
2. `data/migrations/*.sql` → tables PascalCase

**Décision recommandée**: Garder les migrations SQL (plus maintenable)

**Actions**:
1. Supprimer les `CREATE TABLE` de `GameRepository.Initialiser()`
2. Garder uniquement l'appel à `DbInitializer.ApplyMigrations()`
3. Standardiser les noms de colonnes (PascalCase partout)
4. Mettre à jour les requêtes SQL dans `GameRepository` pour utiliser les noms PascalCase

**Fichiers à modifier**:
- `src/RingGeneral.Data/Repositories/GameRepository.cs` (lignes 24-400+)
- `src/RingGeneral.Data/Database/DbInitializer.cs`

**Critères d'acceptation**:
- [ ] Un seul système de création de tables
- [ ] Toutes les requêtes utilisent les noms corrects
- [ ] Les tests de persistance passent

**Effort estimé**: 4-8 heures

**Risque**: ÉLEVÉ - Peut casser des fonctionnalités existantes

---

## Phase 1: Fondations Solides

> **Objectif**: Consolider l'architecture pour supporter les développements futurs

### Tâche 1.1: Implémenter le cache mémoire

**Nouveau fichier**: `src/RingGeneral.Core/Cache/MemoryCache.cs`

**Architecture proposée**:
```csharp
public sealed class GameCache
{
    private readonly Dictionary<string, WorkerSnapshot> _workers = new();
    private readonly Dictionary<string, CompanyState> _companies = new();

    // TTL configurable
    public TimeSpan WorkerTtl { get; set; } = TimeSpan.FromMinutes(5);

    public WorkerSnapshot? GetWorker(string workerId);
    public void SetWorker(WorkerSnapshot worker);
    public void InvalidateWorker(string workerId);
    public void InvalidateCompanyWorkers(string companyId);
}
```

**Intégration**:
- Injecter `GameCache` dans `GameRepository`
- Utiliser le cache avant les requêtes DB
- Invalider le cache après les modifications

**Critères d'acceptation**:
- [ ] Le cache réduit les requêtes DB répétitives
- [ ] L'invalidation fonctionne correctement
- [ ] Tests unitaires du cache

**Effort estimé**: 1-2 jours

---

### Tâche 1.2: Ajouter la pagination des workers

**Fichier**: `src/RingGeneral.Data/Repositories/GameRepository.cs`

**Méthodes à modifier/créer**:
```csharp
// Avant (problème)
public IReadOnlyList<WorkerSnapshot> ChargerTousLesWorkers();

// Après (solution)
public PagedResult<WorkerSnapshot> ChargerWorkers(
    string? companyId = null,
    int page = 1,
    int pageSize = 50,
    string? orderBy = "Popularite",
    bool descending = true);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
```

**Critères d'acceptation**:
- [ ] Aucune requête ne charge tous les workers sans LIMIT
- [ ] L'UI supporte la pagination (next/prev)
- [ ] Performance acceptable avec 200k workers en DB

**Effort estimé**: 1-2 jours

---

### Tâche 1.3: Ajouter les index SQL manquants

**Fichier**: `data/migrations/003_performance_indexes.sql` (nouveau)

```sql
-- Index pour les requêtes fréquentes
CREATE INDEX IF NOT EXISTS idx_workers_company_popularity
    ON Workers(CompanyId, Popularity DESC);

CREATE INDEX IF NOT EXISTS idx_workers_nationality_popularity
    ON Workers(Nationality, Popularity DESC);

CREATE INDEX IF NOT EXISTS idx_contracts_worker_status
    ON Contracts(WorkerId, Status);

CREATE INDEX IF NOT EXISTS idx_contracts_company_end
    ON Contracts(CompanyId, EndDate);

CREATE INDEX IF NOT EXISTS idx_storylines_company_active
    ON Storylines(CompanyId, IsActive);

CREATE INDEX IF NOT EXISTS idx_injuries_worker_active
    ON Injuries(WorkerId, IsActive);

CREATE INDEX IF NOT EXISTS idx_finance_company_week
    ON FinanceTransactions(CompanyId, Week);
```

**Critères d'acceptation**:
- [ ] Migration s'applique sans erreur
- [ ] Requêtes filtrées plus rapides (mesurer avant/après)

**Effort estimé**: 0.5 jour

---

### Tâche 1.4: Implémenter le LOD pour les workers

**Concept**: Comme `WorldSimScheduler` utilise LOD pour les compagnies, faire pareil pour les workers.

**Nouveau fichier**: `src/RingGeneral.Core/Models/WorkerLod.cs`

```csharp
// LOD 0: Tous les détails (roster joueur)
public sealed record WorkerSnapshot(...); // Existant

// LOD 1: Attributs principaux (compagnies IA actives)
public sealed record WorkerSummary(
    string WorkerId,
    string NomComplet,
    int OverallRating,  // (InRing + Entertainment + Story) / 3
    int Popularite,
    string CompagnieId);

// LOD 2: Minimal (monde passif)
public sealed record WorkerReference(
    string WorkerId,
    string NomComplet);
```

**Fichier à modifier**: `src/RingGeneral.Data/Repositories/GameRepository.cs`

```csharp
public IReadOnlyList<WorkerSummary> ChargerWorkersSummary(string companyId);
public IReadOnlyList<WorkerReference> ChargerWorkersReferences(string? regionId = null);
```

**Critères d'acceptation**:
- [ ] 3 niveaux de détail disponibles
- [ ] WorldSimScheduler utilise LOD 1/2 pour workers
- [ ] Réduction mémoire mesurable

**Effort estimé**: 2-3 jours

---

## Phase 2: Fonctionnalités Incomplètes

> **Objectif**: Compléter les fonctionnalités partiellement implémentées

### Tâche 2.1: Créer la fiche Worker dédiée

**Problème actuel**: `OuvrirFicheWorker()` redirige vers la recherche globale au lieu d'une vraie fiche.

**Nouveaux fichiers**:
- `src/RingGeneral.UI/Views/WorkerDetailView.axaml`
- `src/RingGeneral.UI/ViewModels/WorkerDetailViewModel.cs`

**Structure de la fiche (onglets)**:
```
┌─────────────────────────────────────────────────┐
│ [Photo] John Cena                               │
│ Main Eventer • WWE • Popularité 95              │
├─────────────────────────────────────────────────┤
│ [Aperçu] [Attributs] [Contrat] [Santé] [Hist.]  │
├─────────────────────────────────────────────────┤
│                                                 │
│  Onglet Aperçu:                                 │
│  - Stats principales (barres visuelles)         │
│  - Momentum actuel                              │
│  - Storylines en cours                          │
│  - Titres détenus                               │
│                                                 │
│  Actions: [Booker] [Proposer contrat] [Repos]   │
│                                                 │
└─────────────────────────────────────────────────┘
```

**ViewModel**:
```csharp
public sealed class WorkerDetailViewModel : ViewModelBase
{
    public string WorkerId { get; }
    public WorkerSnapshot Worker { get; private set; }
    public IReadOnlyList<StorylineInfo> Storylines { get; }
    public IReadOnlyList<TitleDetail> Titres { get; }
    public ContractInfo? Contrat { get; }
    public IReadOnlyList<InjuryInfo> Blessures { get; }
    public IReadOnlyList<MatchHistoryEntry> Historique { get; }

    public ReactiveCommand<Unit, Unit> BookerCommand { get; }
    public ReactiveCommand<Unit, Unit> ProposerContratCommand { get; }
    public ReactiveCommand<Unit, Unit> ReposCommand { get; }

    public void Charger(string workerId);
}
```

**Critères d'acceptation**:
- [ ] Navigation `/worker/{id}` fonctionne
- [ ] Tous les onglets affichent les données
- [ ] Actions fonctionnelles
- [ ] Retour à la liste précédente

**Effort estimé**: 2-3 jours

---

### Tâche 2.2: Compléter l'UI Diffusion/TV Deals

**Problème actuel**: Backend existe (`TvDeal`, `DealRevenueModel`) mais pas d'UI de gestion.

**Nouveaux fichiers**:
- `src/RingGeneral.UI/Views/BroadcastView.axaml`
- `src/RingGeneral.UI/ViewModels/BroadcastViewModel.cs`

**Fonctionnalités**:
```
┌─────────────────────────────────────────────────┐
│ DIFFUSION TV/STREAMING                          │
├─────────────────────────────────────────────────┤
│ Deals Actifs                                    │
│ ┌─────────────────────────────────────────────┐ │
│ │ ESPN+  | Reach +15 | Min 45 | $5000/show   │ │
│ │ TNT    | Reach +20 | Min 50 | $8000/show   │ │
│ └─────────────────────────────────────────────┘ │
│                                                 │
│ [+ Négocier nouveau deal]                       │
├─────────────────────────────────────────────────┤
│ Historique Audience                             │
│ [Graphique semaine par semaine]                 │
│                                                 │
│ Reach Total: 65% | Audience Moy: 52            │
└─────────────────────────────────────────────────┘
```

**Critères d'acceptation**:
- [ ] Liste des deals actifs
- [ ] Création/modification de deals
- [ ] Graphique historique audience
- [ ] Calcul du reach total

**Effort estimé**: 2-3 jours

---

### Tâche 2.3: Ajouter le système de Scripts (Booking)

**Problème actuel**: Étape 11 manque le système de scripts pour promos/angles.

**Nouveaux fichiers**:
- `src/RingGeneral.Core/Models/ScriptModels.cs`
- `src/RingGeneral.Core/Services/ScriptService.cs`
- `data/migrations/003_scripts.sql`

**Modèles**:
```csharp
public sealed record SegmentScript(
    string ScriptId,
    string SegmentId,
    string Contenu,
    ScriptStatus Status,  // BROUILLON, EN_REVUE, APPROUVE
    string? Notes,
    int Version);

public sealed record PromoScript(
    string ScriptId,
    string SegmentId,
    string Contenu,
    PromoTon Ton,         // FACE, HEEL, TWEENER
    string? CibleWorkerId,
    PromoObjectif Objectif, // CHALLENGE, INSULTE, MOTIVATION, ANNONCE
    ScriptStatus Status,
    string? Notes,
    int Version);

public enum PromoTon { Face, Heel, Tweener }
public enum PromoObjectif { Challenge, Insulte, Motivation, Annonce, Celebration }
public enum ScriptStatus { Brouillon, EnRevue, Approuve, Rejete }
```

**Service**:
```csharp
public sealed class ScriptService
{
    public SegmentScript CreerScript(string segmentId, string contenu);
    public SegmentScript MettreAJour(string scriptId, string contenu);
    public SegmentScript Approuver(string scriptId);
    public SegmentScript Rejeter(string scriptId, string raison);
    public PromoScript CreerPromo(string segmentId, PromoTon ton, PromoObjectif objectif);
}
```

**Migration SQL**:
```sql
CREATE TABLE IF NOT EXISTS SegmentScripts (
    ScriptId TEXT PRIMARY KEY,
    SegmentId TEXT NOT NULL,
    Content TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'BROUILLON',
    Notes TEXT,
    Version INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT,
    FOREIGN KEY (SegmentId) REFERENCES ShowSegments(ShowSegmentId)
);

CREATE TABLE IF NOT EXISTS PromoScripts (
    ScriptId TEXT PRIMARY KEY,
    Ton TEXT NOT NULL,
    TargetWorkerId TEXT,
    Objective TEXT NOT NULL,
    FOREIGN KEY (ScriptId) REFERENCES SegmentScripts(ScriptId),
    FOREIGN KEY (TargetWorkerId) REFERENCES Workers(WorkerId)
);
```

**Critères d'acceptation**:
- [ ] CRUD scripts fonctionnel
- [ ] Workflow d'approbation
- [ ] Intégration avec SegmentViewModel
- [ ] Impact sur simulation (script approuvé = bonus?)

**Effort estimé**: 2-3 jours

---

### Tâche 2.4: Compléter l'UI Backstage/Discipline

**Problème actuel**: `BackstageService` et `DisciplineService` existent mais pas d'UI dédiée.

**Nouveaux fichiers**:
- `src/RingGeneral.UI/Views/BackstageView.axaml`
- `src/RingGeneral.UI/ViewModels/BackstageViewModel.cs`

**Fonctionnalités**:
```
┌─────────────────────────────────────────────────┐
│ BACKSTAGE & DISCIPLINE                          │
├─────────────────────────────────────────────────┤
│ Incidents Récents                    [Filtrer ▼]│
│ ┌─────────────────────────────────────────────┐ │
│ │ S12 | Randy Orton | Altercation | Grave     │ │
│ │     | [Appliquer sanction] [Ignorer]        │ │
│ │ S11 | CM Punk    | Retard      | Légère     │ │
│ │     | [Avertissement appliqué]              │ │
│ └─────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────┤
│ Moral du Roster                                 │
│ [Graphique distribution moral]                  │
│ Bas (<40): 3 | Moyen: 25 | Haut (>70): 12      │
├─────────────────────────────────────────────────┤
│ Actions Disciplinaires                          │
│ - SUSPENSION (gros impact moral, absence)       │
│ - AMENDE (impact financier, moral modéré)       │
│ - AVERTISSEMENT (faible impact)                 │
└─────────────────────────────────────────────────┘
```

**Critères d'acceptation**:
- [ ] Liste des incidents avec statut
- [ ] Application de sanctions depuis l'UI
- [ ] Vue du moral global du roster
- [ ] Historique des actions disciplinaires

**Effort estimé**: 1-2 jours

---

### Tâche 2.5: Enrichir le contenu News/Inbox

**Problème actuel**: `GenererNews()` utilise 3 templates hardcodés.

**Solution**: Système de templates JSON extensible.

**Nouveau fichier**: `specs/lore/news-templates.fr.json`

```json
{
  "newsTemplates": [
    {
      "id": "rumeur-talent",
      "type": "news",
      "titre": "Rumeur: {worker} intéresse {company}",
      "contenu": "Selon nos sources, {company} aurait approché {worker} pour des discussions préliminaires.",
      "conditions": {
        "workerPopularite": { "min": 60 },
        "workerContratRestant": { "max": 8 }
      },
      "frequence": 0.15
    },
    {
      "id": "blessure-concurrent",
      "type": "news",
      "titre": "Blessure chez {company}",
      "contenu": "{worker} de {company} sera absent plusieurs semaines suite à une blessure.",
      "conditions": {
        "companyNotPlayer": true
      },
      "frequence": 0.08
    },
    {
      "id": "record-audience",
      "type": "news",
      "titre": "Record d'audience pour {show}",
      "contenu": "{show} a atteint {audience} spectateurs, un record pour {company}.",
      "conditions": {
        "audienceAboveAverage": 1.2
      },
      "frequence": 0.10
    }
  ]
}
```

**Fichier à modifier**: `src/RingGeneral.Data/Repositories/WeeklyLoopService.cs`

```csharp
private IEnumerable<InboxItem> GenererNews(int semaine)
{
    var templates = _specsReader.Charger<NewsTemplatesSpec>("news-templates.fr.json");
    var context = BuildNewsContext(semaine);

    foreach (var template in templates.NewsTemplates)
    {
        if (_random.NextDouble() < template.Frequence &&
            EvaluerConditions(template.Conditions, context))
        {
            yield return InstancierNews(template, context, semaine);
        }
    }
}
```

**Critères d'acceptation**:
- [ ] 20+ templates de news variés
- [ ] Conditions dynamiques évaluées
- [ ] Variables substituées ({worker}, {company}, etc.)
- [ ] News pertinentes au contexte du jeu

**Effort estimé**: 1-2 jours

---

## Phase 3: Performance 200k Workers

> **Objectif**: Garantir que le jeu reste fluide avec 200 000 workers en base

### Tâche 3.1: Audit des requêtes SQL

**Action**: Identifier toutes les requêtes qui chargent des données sans limite.

**Fichiers à auditer**:
- `src/RingGeneral.Data/Repositories/GameRepository.cs`
- `src/RingGeneral.Data/Repositories/WeeklyLoopService.cs`

**Checklist**:
- [ ] Aucun `SELECT * FROM Workers` sans WHERE ou LIMIT
- [ ] Aucun `SELECT * FROM Contracts` sans filtre
- [ ] Toutes les listes utilisent la pagination

**Effort estimé**: 0.5 jour (audit) + 1-2 jours (corrections)

---

### Tâche 3.2: Implémenter le chargement différé (Lazy Loading)

**Concept**: Ne charger les détails que quand nécessaire.

**Exemple pour ShowContext**:
```csharp
// Avant
public ShowContext ChargerShowContext(string showId)
{
    var show = ChargerShow(showId);
    var workers = ChargerTousLesWorkers(); // PROBLÈME!
    var segments = ChargerSegments(showId);
    // ...
}

// Après
public ShowContext ChargerShowContext(string showId)
{
    var show = ChargerShow(showId);
    var segmentWorkerIds = ChargerWorkerIdsSegments(showId);
    var workers = ChargerWorkersByIds(segmentWorkerIds); // Seulement ceux nécessaires
    // ...
}
```

**Critères d'acceptation**:
- [ ] ShowContext ne charge que les workers des segments
- [ ] Recherche globale utilise des requêtes paginées
- [ ] Mémoire stable même avec 200k workers

**Effort estimé**: 2-3 jours

---

### Tâche 3.3: Tests de charge

**Nouveau fichier**: `tests/RingGeneral.Tests/PerformanceTests.cs`

```csharp
public class PerformanceTests
{
    [Fact]
    public void ChargerShowContext_With200kWorkers_CompletesUnder2Seconds()
    {
        // Arrange: DB avec 200k workers
        var db = CreateLargeTestDatabase(workerCount: 200_000);
        var repo = new GameRepository(db);

        // Act
        var sw = Stopwatch.StartNew();
        var context = repo.ChargerShowContext("SHOW-001");
        sw.Stop();

        // Assert
        Assert.True(sw.ElapsedMilliseconds < 2000);
        Assert.True(context.Workers.Count < 100); // Seulement roster actif
    }

    [Fact]
    public void PasserSemaine_With200kWorkers_CompletesUnder5Seconds()
    {
        // ...
    }
}
```

**Critères d'acceptation**:
- [ ] Tests de charge automatisés
- [ ] Seuils de performance documentés
- [ ] Rapport de performance dans CI

**Effort estimé**: 1-2 jours

---

## Phase 4: Polish & QA

> **Objectif**: Stabiliser, documenter, préparer la release

### Tâche 4.1: Compléter la couverture de tests

**Objectif**: Atteindre 70%+ de couverture sur les services critiques.

**Services prioritaires**:
- [ ] `ShowSimulationEngine` - Couverture actuelle ~80%
- [ ] `ContractNegotiationService` - Couverture actuelle ~60%
- [ ] `WeeklyLoopService` - Couverture actuelle ~40%
- [ ] `GameRepository` - Couverture actuelle ~30%

**Actions**:
1. Ajouter tests pour cas limites
2. Ajouter tests d'intégration
3. Configurer rapport de couverture dans CI

**Effort estimé**: 3-5 jours

---

### Tâche 4.2: Améliorer le packaging

**Fichier**: `.github/workflows/build-windows.yml`

**Améliorations**:
```yaml
- name: Package with specs
  run: |
    mkdir -p artifacts/RingGeneral
    cp -r specs artifacts/RingGeneral/
    cp -r data/migrations artifacts/RingGeneral/data/
    cp src/RingGeneral.UI/bin/Release/net8.0/win-x64/publish/* artifacts/RingGeneral/

- name: Create ZIP
  run: |
    cd artifacts
    zip -r RingGeneral-${{ github.ref_name }}.zip RingGeneral/

- name: Upload Release Asset
  uses: softprops/action-gh-release@v1
  with:
    files: artifacts/RingGeneral-*.zip
```

**Critères d'acceptation**:
- [ ] ZIP contient exe + specs + migrations
- [ ] Release automatique sur tag
- [ ] README inclus dans le package

**Effort estimé**: 0.5-1 jour

---

### Tâche 4.3: Documentation utilisateur

**Fichiers à créer/compléter**:
- `docs/QUICKSTART_FR.md` - Guide de démarrage rapide
- `docs/CONTROLS_FR.md` - Raccourcis clavier
- `docs/FAQ_FR.md` - Questions fréquentes

**Critères d'acceptation**:
- [ ] Nouveau joueur peut démarrer en 5 minutes
- [ ] Tous les raccourcis documentés
- [ ] FAQ couvre les problèmes courants

**Effort estimé**: 1-2 jours

---

## Récapitulatif des Dépendances

```
Phase 0 (Stabilisation)
    │
    ├── 0.1 CI/CD ──────────────┐
    ├── 0.2 Tests sync ─────────┼──► Phase 1
    └── 0.3 Schéma unique ──────┘
                                    │
Phase 1 (Fondations)                │
    │                               │
    ├── 1.1 Cache ──────────────────┤
    ├── 1.2 Pagination ─────────────┼──► Phase 3 (Performance)
    ├── 1.3 Index SQL ──────────────┤
    └── 1.4 LOD Workers ────────────┘
                                    │
Phase 2 (Fonctionnalités)           │
    │                               │
    ├── 2.1 Fiche Worker ───────────┤
    ├── 2.2 UI Diffusion ───────────┤
    ├── 2.3 Scripts ────────────────┼──► Phase 4 (Polish)
    ├── 2.4 UI Backstage ───────────┤
    └── 2.5 News enrichies ─────────┘
```

---

## Checklist de Validation Finale

Avant de considérer le projet "Release Ready":

### Technique
- [ ] Build sans erreurs
- [ ] Tous les tests passent
- [ ] Performance OK avec 200k workers
- [ ] Pas de crash sur les scénarios principaux

### Fonctionnel
- [ ] Boucle de jeu complète (booking → simulation → résultats → semaine suivante)
- [ ] Toutes les pages de navigation accessibles
- [ ] Sauvegarde/chargement fonctionnel
- [ ] Import de base Baki fonctionnel

### UX
- [ ] Pas de "dead ends" dans la navigation
- [ ] Messages d'erreur compréhensibles
- [ ] Tooltips sur les concepts complexes
- [ ] Aide/Codex accessible

---

## Notes pour l'Implémentation

### Conventions à respecter

1. **Nommage**:
   - Classes: `PascalCase`
   - Méthodes: `PascalCase` (français: `ChargerWorker`, pas `LoadWorker`)
   - Variables: `camelCase`
   - Constantes: `SCREAMING_SNAKE_CASE`

2. **Records vs Classes**:
   - Données immuables: `sealed record`
   - ViewModels avec état: `class` avec `INotifyPropertyChanged`

3. **SQL**:
   - Tables: `PascalCase` (après unification)
   - Colonnes: `PascalCase`
   - Toujours utiliser paramètres (`$param`) jamais de concaténation

4. **Tests**:
   - Un fichier de test par service
   - Nommage: `MethodName_Scenario_ExpectedResult`
   - Cleanup des fichiers temporaires dans `finally`

---

**Document créé le**: 2026-01-05
**Auteur**: Claude (Assistant Architecture)
**Version**: 1.0

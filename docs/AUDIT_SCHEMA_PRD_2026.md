# 🔍 AUDIT CROISÉ : PRD 2026 vs Implémentation Actuelle
## Ring General — Analyse Architecture .NET & SQL

**Date** : 8 janvier 2026  
**Expert** : Architecture .NET & SQL  
**Version PRD** : 2.0 (8 janvier 2026)  
**Phase Actuelle** : 1.9 (~50-55% complété)

---

## 📋 TABLE DES MATIÈRES

1. [Résumé Exécutif](#résumé-exécutif)
2. [Audit de Schéma SQL](#audit-de-schéma-sql)
3. [Analyse de Cohérence C#](#analyse-de-cohérence-c)
4. [Plan d'Action : Legacy → Procédural](#plan-daction--legacy--procédural)
5. [Recommandations Prioritaires](#recommandations-prioritaires)

---

## 📊 RÉSUMÉ EXÉCUTIF

### État Global
- **Schéma SQL** : ✅ Base solide (30+ tables), mais **colonnes manquantes identifiées**
- **Classes C#** : ✅ Architecture excellente (8.5/10), mais **propriétés à compléter**
- **Youth System** : ⚠️ **Partiel** (30% selon PRD) - Tables créées, mais intégration incomplète
- **Narrative Hooks** : ❌ **Non implémenté** - Aucune infrastructure procédurale narrative

### Gaps Critiques Identifiés

| Système | SQL Workers | SQL Companies | C# Workers | C# Companies | Impact |
|---------|------------|---------------|------------|--------------|--------|
| **Youth System** | ⚠️ 6 colonnes manquantes | ✅ OK (via YouthStructures) | ❌ 0 propriété | ✅ OK | 🔴 HAUT |
| **Narrative Hooks** | ❌ 8+ colonnes manquantes | ⚠️ 5 colonnes manquantes | ❌ 0 propriété | ⚠️ Partiel | 🔴 HAUT |

---

## 🔎 AUDIT DE SCHÉMA SQL

### 1. TABLE `Workers` - Colonnes Manquantes

#### 1.1 Youth System Support ⚠️ **CRITIQUE**

**État Actuel** : La table `Workers` possède les colonnes de base, mais **manque les liens explicites vers le Youth System**.

**Colonnes Manquantes** :

| Colonne | Type | Description | Requis PRD |
|---------|------|-------------|------------|
| `IsYouthTrainee` | INTEGER (0/1) | Indicateur si worker est actuellement en formation | ✅ Phase 3.3 |
| `YouthStructureId` | TEXT | FK vers `YouthStructures` (current training structure) | ✅ Phase 3.3 |
| `TrainingStartWeek` | INTEGER | Semaine de début de formation (pour progression) | ✅ Phase 3.3 |
| `TrainingEndWeek` | INTEGER | Semaine prévue de graduation (NULL si indéterminé) | ✅ Phase 3.3 |
| `TrainingPotential` | INTEGER (0-100) | Potentiel de développement (affecte vitesse d'apprentissage) | ⚠️ Recommandé |
| `LastTrainingProgressWeek` | INTEGER | Dernière semaine où progression a été calculée | ⚠️ Recommandé |
| `GraduatedFromYouthStructureId` | TEXT | FK vers YouthStructure (historique : d'où vient-il) | ⚠️ Recommandé |

**Impact** :
- Impossible de lier directement un `Worker` à sa formation youth
- La progression hebdomadaire nécessite des JOINs complexes
- Pas de tracking de "promising talent" découvert via youth

**Référence PRD** :
```
Phase 3.3 Youth Development & Scouting (1 semaine)
- YouthDetailView.axaml (détails jeune wrestler)
- TrainingPlanView.axaml (plans d'entraînement)
- AttributeImprovement simulation
```

#### 1.2 Narrative Hooks Support ❌ **CRITIQUE**

**État Actuel** : Aucune infrastructure pour génération procédurale d'événements narratifs basés sur le passé/background des workers.

**Colonnes Manquantes** :

| Colonne | Type | Description | Requis PRD |
|---------|------|-------------|------------|
| `NarrativeHooks` | TEXT | JSON array des hooks narratifs actifs (ex: ["FormerChampion", "InjuryProne", "RivalryLegend"]) | ✅ Vision 2026 |
| `BackgroundStory` | TEXT | Texte descriptif du passé/carrière (pour génération IA narrative) | ✅ Vision 2026 |
| `NarrativeFlags` | TEXT | JSON object des flags narratifs (ex: {"HasRedemptionArc": true, "HasBetrayalHistory": false}) | ✅ Vision 2026 |
| `LastNarrativeEventWeek` | INTEGER | Dernière semaine où un événement narratif a été généré | ⚠️ Recommandé |
| `NarrativeWeight` | INTEGER (0-100) | Poids narratif (plus élevé = plus d'événements générés) | ⚠️ Recommandé |
| `CharacterArchetype` | TEXT | Archétype narratif (ex: "Underdog", "Veteran", "RisingStar", "FallenHero") | ⚠️ Recommandé |
| `StorylineAffinity` | INTEGER (0-100) | Affinité naturelle pour créer des storylines (basé sur charisma + story attributes) | ⚠️ Recommandé |
| `ProceduralStorySeed` | TEXT | Seed aléatoire unique pour génération procédurale cohérente | ✅ Vision 2026 |

**Impact** :
- **Impossible** de générer des événements narratifs basés sur le background
- Pas de système de "story hooks" pour déclencher des storylines procédurales
- Le moteur procédural ne peut pas utiliser le passé des workers pour créer du contenu

**Référence PRD** :
```
Section 4. SPÉCIFICATIONS FONCTIONNELLES - Boucle Hebdomadaire
- Événements hebdomadaires aléatoires (0-3 par semaine)
- Progression automatique des storylines
- Génération procédurale de contenu narratif
```

### 2. TABLE `Companies` - Colonnes Manquantes

#### 2.1 Youth System Support ✅ **BON**

**État Actuel** : Les compagnies ont déjà la structure via `YouthStructures` (table séparée). Pas besoin de colonnes supplémentaires dans `Companies`.

**Note** : La relation `CompanyId` dans `YouthStructures` est suffisante.

#### 2.2 Narrative Hooks Support ⚠️ **PARTIEL**

**État Actuel** : Les tables `CompanyEras` et `CompanyMilestones` existent (migration 005), mais manquent de champs pour la génération procédurale.

**Colonnes Manquantes** :

| Colonne | Type | Description | Requis PRD |
|---------|------|-------------|------------|
| `NarrativeHooks` | TEXT | JSON array des hooks narratifs compagnie (ex: ["DecliningCompany", "RisingPower", "HistoricRivalry"]) | ✅ Vision 2026 |
| `NarrativeTraditions` | TEXT | JSON array des traditions narratives (ex: ["AlwaysPushesHomegrown", "KnownForHardcore"]) | ⚠️ Recommandé |
| `NarrativeFlags` | TEXT | JSON object des flags narratifs (ex: {"HasBeenAcquired": false, "HasMajorScandal": false}) | ✅ Vision 2026 |
| `LastProceduralEventWeek` | INTEGER | Dernière semaine où un événement procédural a été généré | ⚠️ Recommandé |
| `ProceduralStorySeed` | TEXT | Seed aléatoire unique pour génération procédurale cohérente | ✅ Vision 2026 |

**Tables Existant** (Migration 005) :
- ✅ `CompanyEras` (historique des eras)
- ✅ `CompanyMilestones` (jalons importants)

**Tables Manquantes** :

| Table | Description | Requis PRD |
|-------|-------------|------------|
| `CompanyNarrativeEvents` | Historique des événements narratifs générés procéduralement | ✅ Vision 2026 |
| `CompanyProceduralStorylines` | Storylines générées automatiquement (non créées par le joueur) | ⚠️ Recommandé |

**Impact** :
- Impossible de générer des événements narratifs basés sur l'histoire de la compagnie
- Pas de système de "company lore" pour enrichir le contexte narratif
- Les événements hebdomadaires aléatoires ne peuvent pas s'appuyer sur l'historique

---

## 💻 ANALYSE DE COHÉRENCE C#

### 1. CLASSE `Worker` (RingGeneral.Core/Models/Worker.cs)

#### 1.1 État Actuel ✅

**Propriétés Existantes** :
```csharp
public class Worker
{
    // ✅ BASIQUES
    public int Id { get; set; }
    public string Name { get; set; }
    public string? RealName { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // ✅ GÉOGRAPHIE
    public string? BirthCity { get; set; }
    public string? BirthCountry { get; set; }
    public string? ResidenceCity { get; set; }
    public string? ResidenceState { get; set; }
    public string? ResidenceCountry { get; set; }
    
    // ✅ CARRIÈRE
    public int Experience { get; set; }
    public bool IsActive { get; set; }
    public bool IsInjured { get; set; }
    
    // ✅ ATTRIBUTS (via navigation properties)
    public WorkerInRingAttributes? InRingAttributes { get; set; }
    public WorkerEntertainmentAttributes? EntertainmentAttributes { get; set; }
    public WorkerStoryAttributes? StoryAttributes { get; set; }
    
    // ✅ RELATIONS
    public List<WorkerRelation> RelationsAsWorker1 { get; set; }
    public List<WorkerRelation> RelationsAsWorker2 { get; set; }
    public List<WorkerNote> Notes { get; set; }
    public List<ContractHistory> ContractHistory { get; set; }
    public List<MatchHistoryItem> MatchHistory { get; set; }
}
```

#### 1.2 Propriétés Manquantes - Youth System ❌

**Propriétés à Ajouter** :

```csharp
// ====================================================================
// YOUTH SYSTEM SUPPORT (Phase 3.3)
// ====================================================================

/// <summary>
/// Indicateur si ce worker est actuellement en formation youth
/// </summary>
public bool IsYouthTrainee { get; set; }

/// <summary>
/// Youth structure où ce worker est actuellement en formation (FK)
/// </summary>
public string? YouthStructureId { get; set; }

/// <summary>
/// Semaine de début de formation (pour calcul de progression)
/// </summary>
public int? TrainingStartWeek { get; set; }

/// <summary>
/// Semaine prévue de graduation (NULL si indéterminé)
/// </summary>
public int? TrainingEndWeek { get; set; }

/// <summary>
/// Potentiel de développement (0-100) - affecte la vitesse d'apprentissage
/// </summary>
public int TrainingPotential { get; set; } = 50;

/// <summary>
/// Dernière semaine où la progression a été calculée
/// </summary>
public int? LastTrainingProgressWeek { get; set; }

/// <summary>
/// Youth structure d'où ce worker a gradué (historique)
/// </summary>
public string? GraduatedFromYouthStructureId { get; set; }

// Navigation property vers YouthStructure
public YouthStructure? CurrentYouthStructure { get; set; }
```

**Impact** :
- ❌ Impossible d'accéder directement à `worker.IsYouthTrainee`
- ❌ Requête complexe : `SELECT * FROM Workers w JOIN YouthTrainees yt ON ...`
- ❌ Pas de propriété calculée pour `worker.IsReadyToGraduate`

#### 1.3 Propriétés Manquantes - Narrative Hooks ❌

**Propriétés à Ajouter** :

```csharp
// ====================================================================
// NARRATIVE HOOKS SUPPORT (Vision 2026)
// ====================================================================

/// <summary>
/// Liste des hooks narratifs actifs (ex: "FormerChampion", "InjuryProne")
/// </summary>
public List<string> NarrativeHooks { get; set; } = new();

/// <summary>
/// Texte descriptif du passé/carrière (pour génération IA narrative)
/// </summary>
public string? BackgroundStory { get; set; }

/// <summary>
/// Flags narratifs (ex: HasRedemptionArc, HasBetrayalHistory)
/// </summary>
public Dictionary<string, bool> NarrativeFlags { get; set; } = new();

/// <summary>
/// Dernière semaine où un événement narratif a été généré
/// </summary>
public int? LastNarrativeEventWeek { get; set; }

/// <summary>
/// Poids narratif (0-100) - plus élevé = plus d'événements générés
/// </summary>
public int NarrativeWeight { get; set; } = 50;

/// <summary>
/// Archétype narratif (ex: "Underdog", "Veteran", "RisingStar")
/// </summary>
public string? CharacterArchetype { get; set; }

/// <summary>
/// Affinité naturelle pour créer des storylines (0-100)
/// Basé sur charisma + story attributes
/// </summary>
public int StorylineAffinity { get; set; }

/// <summary>
/// Seed aléatoire unique pour génération procédurale cohérente
/// </summary>
public string ProceduralStorySeed { get; set; } = Guid.NewGuid().ToString();

// Calculated property
/// <summary>
/// Retourne true si ce worker a un hook narratif spécifique
/// </summary>
public bool HasNarrativeHook(string hook) => NarrativeHooks.Contains(hook);
```

**Note** : Pour la persistance JSON (`NarrativeHooks`, `NarrativeFlags`), utiliser un `ValueConverter` EF Core ou une colonne TEXT avec sérialisation manuelle.

### 2. CLASSE `Company` / `CompanyState`

#### 2.1 État Actuel ✅

**Record Existant** (`DomainModels.cs`) :
```csharp
public sealed record CompanyState(
    string CompagnieId,
    string Nom,
    string Region,
    int Prestige,
    double Tresorerie,
    int AudienceMoyenne,
    int Reach,
    // Migration 005
    int FoundedYear = 2024,
    string CompanySize = "Local",
    string CurrentEra = "Foundation Era",
    string? CatchStyleId = null,
    bool IsPlayerControlled = false,
    double MonthlyBurnRate = 0.0,
    // Migration 004
    string? OwnerId = null,
    string? BookerId = null);
```

#### 2.2 Propriétés Manquantes - Narrative Hooks ⚠️

**Propriétés à Ajouter** :

```csharp
// Ajouter au record CompanyState :
public sealed record CompanyState(
    // ... propriétés existantes ...
    
    // NARRATIVE HOOKS (Vision 2026)
    IReadOnlyList<string> NarrativeHooks = default!,
    IReadOnlyList<string> NarrativeTraditions = default!,
    IReadOnlyDictionary<string, bool> NarrativeFlags = default!,
    int? LastProceduralEventWeek = null,
    string? ProceduralStorySeed = null
);
```

**Note** : Les records C# sont immutables. Pour la modification, créer une méthode `With(...)` ou utiliser une classe séparée pour l'entité mutables.

**Recommandation** : Créer une classe `Company` séparée (non-record) pour l'entité complète avec navigation properties :

```csharp
public class Company
{
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // ... propriétés existantes ...
    
    // NARRATIVE HOOKS
    public List<string> NarrativeHooks { get; set; } = new();
    public List<string> NarrativeTraditions { get; set; } = new();
    public Dictionary<string, bool> NarrativeFlags { get; set; } = new();
    public int? LastProceduralEventWeek { get; set; }
    public string ProceduralStorySeed { get; set; } = Guid.NewGuid().ToString();
    
    // Navigation properties
    public List<CompanyEra> Eras { get; set; } = new();
    public List<CompanyMilestone> Milestones { get; set; } = new();
    public List<CompanyNarrativeEvent> NarrativeEvents { get; set; } = new();
}
```

---

## 🎯 PLAN D'ACTION : LEGACY → PROCÉDURAL

### Vue d'Ensemble

**État Actuel (Legacy)** :
- ✅ Import statique depuis BAKI (`RingGeneral.Tools.BakiImporter`)
- ✅ Données fixes (workers, companies) importées une seule fois
- ✅ Systèmes fonctionnels avec données pré-remplies

**Vision 2026 (Procédural)** :
- ❌ Génération procédurale de workers (youth system)
- ❌ Génération procédurale d'événements narratifs
- ❌ Génération procédurale de storylines
- ❌ Monde vivant avec génération de contenu dynamique

### Les 3 Prochaines Étapes Prioritaires

#### 🥇 ÉTAPE 1 : Migration Schéma SQL (Youth + Narrative Hooks)
**Durée Estimée** : 3-5 jours  
**Priorité** : 🔴 **CRITIQUE**

**Actions** :

1. **Créer migration SQL** : `007_youth_narrative_hooks.sql`
   ```sql
   -- Ajouter colonnes Youth System à Workers
   ALTER TABLE Workers ADD COLUMN IsYouthTrainee INTEGER DEFAULT 0;
   ALTER TABLE Workers ADD COLUMN YouthStructureId TEXT;
   ALTER TABLE Workers ADD COLUMN TrainingStartWeek INTEGER;
   ALTER TABLE Workers ADD COLUMN TrainingEndWeek INTEGER;
   ALTER TABLE Workers ADD COLUMN TrainingPotential INTEGER DEFAULT 50;
   ALTER TABLE Workers ADD COLUMN LastTrainingProgressWeek INTEGER;
   ALTER TABLE Workers ADD COLUMN GraduatedFromYouthStructureId TEXT;
   
   -- Ajouter colonnes Narrative Hooks à Workers
   ALTER TABLE Workers ADD COLUMN NarrativeHooks TEXT DEFAULT '[]'; -- JSON array
   ALTER TABLE Workers ADD COLUMN BackgroundStory TEXT;
   ALTER TABLE Workers ADD COLUMN NarrativeFlags TEXT DEFAULT '{}'; -- JSON object
   ALTER TABLE Workers ADD COLUMN LastNarrativeEventWeek INTEGER;
   ALTER TABLE Workers ADD COLUMN NarrativeWeight INTEGER DEFAULT 50;
   ALTER TABLE Workers ADD COLUMN CharacterArchetype TEXT;
   ALTER TABLE Workers ADD COLUMN StorylineAffinity INTEGER DEFAULT 50;
   ALTER TABLE Workers ADD COLUMN ProceduralStorySeed TEXT;
   
   -- Ajouter colonnes Narrative Hooks à Companies
   ALTER TABLE Companies ADD COLUMN NarrativeHooks TEXT DEFAULT '[]';
   ALTER TABLE Companies ADD COLUMN NarrativeTraditions TEXT DEFAULT '[]';
   ALTER TABLE Companies ADD COLUMN NarrativeFlags TEXT DEFAULT '{}';
   ALTER TABLE Companies ADD COLUMN LastProceduralEventWeek INTEGER;
   ALTER TABLE Companies ADD COLUMN ProceduralStorySeed TEXT;
   
   -- Créer table CompanyNarrativeEvents
   CREATE TABLE IF NOT EXISTS CompanyNarrativeEvents (
       EventId TEXT PRIMARY KEY,
       CompanyId TEXT NOT NULL,
       EventType TEXT NOT NULL,
       Title TEXT NOT NULL,
       Description TEXT NOT NULL,
       Week INTEGER NOT NULL,
       WorkersInvolved TEXT, -- JSON array
       CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
       FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
   );
   
   -- Index
   CREATE INDEX idx_workers_youth ON Workers(IsYouthTrainee, YouthStructureId);
   CREATE INDEX idx_workers_narrative ON Workers(NarrativeWeight DESC, LastNarrativeEventWeek);
   CREATE INDEX idx_companies_narrative ON Companies(LastProceduralEventWeek);
   ```

2. **Mettre à jour les classes C#** :
   - Ajouter propriétés à `Worker.cs`
   - Ajouter propriétés à `Company` / `CompanyState`
   - Créer classe `CompanyNarrativeEvent.cs`

3. **Créer ValueConverters JSON** (si EF Core utilisé) :
   ```csharp
   // Pour sérialisation/désérialisation JSON
   modelBuilder.Entity<Worker>()
       .Property(w => w.NarrativeHooks)
       .HasConversion(
           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
           v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
   ```

4. **Mettre à jour les repositories** :
   - `IWorkerRepository` : méthodes pour query Youth trainees
   - `ICompanyRepository` : méthodes pour query narrative events

**Délivrables** :
- ✅ Migration SQL validée
- ✅ Classes C# mises à jour
- ✅ Repositories mis à jour
- ✅ Tests unitaires pour nouveaux champs

---

#### 🥈 ÉTAPE 2 : Service de Génération Procédurale de Workers (Youth)
**Durée Estimée** : 5-7 jours  
**Priorité** : 🟠 **HAUTE**

**Actions** :

1. **Créer `YouthWorkerGeneratorService`** :
   ```csharp
   public interface IYouthWorkerGeneratorService
   {
       Task<Worker> GenerateNewTraineeAsync(
           string youthStructureId,
           string companyId,
           string? archetype = null);
       
       Task<List<AttributeImprovement>> SimulateTrainingProgressAsync(
           string workerId,
           int weeks);
       
       Task<bool> CheckGraduationReadinessAsync(string workerId);
   }
   ```

2. **Implémenter génération procédurale** :
   - Générer attributs basés sur `TrainingPotential`
   - Générer personnalité basée sur attributs mentaux
   - Générer `BackgroundStory` procédural
   - Assigner `CharacterArchetype` (Underdog, RisingStar, etc.)
   - Générer `ProceduralStorySeed` unique

3. **Intégrer avec Youth System existant** :
   - Utiliser `YouthRepository` pour récupérer structures
   - Mettre à jour `Worker.IsYouthTrainee = true` lors de l'inscription
   - Calculer progression hebdomadaire basée sur philosophie youth

4. **Créer système de graduation** :
   - Vérifier seuils (PRD Phase 3.3) : `MinSemaines`, `SeuilMoyen`, `SeuilInRing`, etc.
   - Créer événement "Worker Graduated" pour inbox
   - Mettre à jour `Worker.IsYouthTrainee = false`
   - Générer premier contrat automatique

**Délivrables** :
- ✅ Service de génération implémenté
- ✅ Tests unitaires (génération, progression, graduation)
- ✅ Intégration avec Youth Repository
- ✅ UI `YouthDetailView` mise à jour pour afficher progression

---

#### 🥉 ÉTAPE 3 : Moteur d'Événements Narratifs Procéduraux
**Durée Estimée** : 7-10 jours  
**Priorité** : 🟡 **MOYENNE-HAUTE**

**Actions** :

1. **Créer `NarrativeEventEngine`** :
   ```csharp
   public interface INarrativeEventEngine
   {
       Task<List<NarrativeEvent>> GenerateWeeklyEventsAsync(
           string companyId,
           int currentWeek);
       
       Task<NarrativeEvent?> TriggerWorkerNarrativeHookAsync(
           string workerId,
           string hookType);
       
       Task<NarrativeEvent?> TriggerCompanyNarrativeHookAsync(
           string companyId,
           string hookType);
   }
   ```

2. **Implémenter génération basée sur hooks** :
   - Parser `Worker.NarrativeHooks` JSON
   - Parser `Company.NarrativeHooks` JSON
   - Générer événements basés sur hooks actifs
   - Utiliser `ProceduralStorySeed` pour cohérence

3. **Créer types d'événements narratifs** (selon PRD Section 1.3) :
   ```
   HIGH PROBABILITY (30%):
   - Rumeur backstage
   - Problème de morale
   - Demande de push
   - Offre d'un rival
   
   MEDIUM PROBABILITY (15%):
   - Blessure surprise
   - Walk-out d'un worker
   - Dispute backstage
   - Incident médiatique
   
   LOW PROBABILITY (5%):
   - Mort d'un personnage (storyline)
   - Strike du roster
   - Conflit staff majeur
   - Acquisition hostile
   ```

4. **Intégrer avec systèmes existants** :
   - `WeeklyLoopService` : appeler `GenerateWeeklyEventsAsync()` chaque semaine
   - `InboxItems` : créer items pour événements importants
   - `RumorEngine` : déclencher rumeurs basées sur événements narratifs
   - `StorylineService` : créer storylines procédurales basées sur hooks

5. **Créer système de "story seeds"** :
   - Générer `BackgroundStory` pour nouveaux workers (procédural)
   - Générer `NarrativeHooks` basés sur attributs (ex: "FormerChampion" si popularity > 80)
   - Générer `NarrativeTraditions` pour companies basées sur `CatchStyleId`

**Délivrables** :
- ✅ Moteur d'événements narratifs implémenté
- ✅ Types d'événements configurés (specs JSON)
- ✅ Intégration avec Weekly Loop
- ✅ UI pour afficher événements narratifs (Inbox)
- ✅ Tests unitaires (génération, probabilités, cohérence)

---

## ✅ RECOMMANDATIONS PRIORITAIRES

### Priorité Immédiate (Sprint Actuel)

1. **✅ ÉTAPE 1** : Migration SQL (3-5 jours)
   - **Pourquoi** : Bloque toutes les autres étapes
   - **Impact** : Dédoublonne la structure de données
   - **Risque** : Bas (migration SQL standard)

2. **✅ ÉTAPE 2** : Service Youth Generation (5-7 jours)
   - **Pourquoi** : Complète le Youth System (actuellement 30%)
   - **Impact** : Permet génération procédurale de workers
   - **Risque** : Moyen (complexité génération procédurale)

### Priorité Secondaire (Sprint Suivant)

3. **✅ ÉTAPE 3** : Moteur Narratif (7-10 jours)
   - **Pourquoi** : Cœur de la Vision 2026 (monde procédural)
   - **Impact** : Transforme le jeu en simulation vivante
   - **Risque** : Élevé (complexité algorithmique + équilibrage)

### Dépendances Identifiées

```
ÉTAPE 1 (Migration SQL)
  ↓
ÉTAPE 2 (Youth Generation)
  ↓
ÉTAPE 3 (Narrative Engine)
  ↓
Phase 3 : Gameplay Complet (PRD)
```

### Risques et Mitigation

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|------------|
| **Migration SQL échoue** | 🔴 Critique | 🟡 Moyenne | Tester sur DB de dev, backup avant migration |
| **Performance dégradée** (colonnes JSON) | 🟡 Moyen | 🟡 Moyenne | Index sur colonnes fréquentes, cache en mémoire |
| **Génération procédurale imprévisible** | 🟡 Moyen | 🟢 Basse | Utiliser seeds déterministes, tests unitaires |
| **Équilibrage narrative events** | 🟠 Haut | 🟡 Moyenne | Spécs JSON configurables, A/B testing |

---

## 📝 CONCLUSION

### État Actuel
- ✅ **Architecture solide** : Base SQL et C# excellente (8.5/10)
- ⚠️ **Gaps identifiés** : Youth System (30%) et Narrative Hooks (0%)
- ✅ **Roadmap claire** : 3 étapes prioritaires définies

### Prochaines Actions
1. **Immédiat** : Créer migration `007_youth_narrative_hooks.sql`
2. **Court terme** : Implémenter `YouthWorkerGeneratorService`
3. **Moyen terme** : Développer `NarrativeEventEngine`

### Alignement PRD 2026
- ✅ **Phase 3.3 Youth Development** : Partiellement couvert (tables SQL existent)
- ❌ **Vision 2026 Procédural** : Infrastructure manquante (Narrative Hooks)
- ⚠️ **Phase 3 Gameplay Complet** : Bloquée par gaps identifiés

---

**Document généré le** : 8 janvier 2026  
**Prochaine révision** : Après implémentation ÉTAPE 1  
**Contact** : Architecture Expert (.NET & SQL)

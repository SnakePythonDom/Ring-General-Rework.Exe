# 📥 PLAN D'IMPORTATION - Workers depuis BAKI1.1.db

**Chef de Projet**: Claude
**Date**: 2026-01-08
**Objectif**: Importer tous les workers existants de BAKI1.1.db vers le nouveau système avec 30 attributs

---

## 🎯 Vue d'Ensemble

### Problématique
- **Source**: Base de données BAKI1.1.db (1.6MB) avec structure ancienne
- **Cible**: Nouveau système avec 30 attributs détaillés
- **Challenge**: Convertir 3 attributs agrégés (in_ring, entertainment, story) en 30 attributs détaillés

### Différences Structurelles

| Ancien Système (BAKI1.1.db) | Nouveau Système |
|------------------------------|-----------------|
| `worker_id` TEXT (UUID) | `Id` INTEGER AUTO_INCREMENT |
| `nom` + `prenom` (séparés) | `Name` TEXT (combiné) |
| `in_ring` INTEGER (0-100) | 10 attributs InRing détaillés |
| `entertainment` INTEGER (0-100) | 10 attributs Entertainment détaillés |
| `story` INTEGER (0-100) | 10 attributs Story détaillés |
| `company_id` TEXT | Conservé |
| `popularite`, `fatigue`, `momentum`, `morale` | Conservés |
| `role_tv` TEXT | Converti en `TvRole` INTEGER + `Alignment` |
| Pas de géographie | `BirthCity`, `BirthCountry`, etc. |
| Pas de gimmick | `CurrentGimmick`, `Alignment`, `PushLevel` |

---

## 📋 STRATÉGIE D'IMPORTATION

### Phase 1: Préparation (30 min)
1. Backup de BAKI1.1.db
2. Analyse du nombre de workers à importer
3. Création de la nouvelle base vide avec nouveau schéma

### Phase 2: Migration Structure (1h)
1. Créer table temporaire `workers_legacy` pour ancienne structure
2. Importer données brutes depuis BAKI1.1.db
3. Créer table `Workers` avec nouvelle structure
4. Créer toutes les tables annexes (11 tables)

### Phase 3: Génération Attributs (2h)
1. **Algorithme de décomposition** des 3 attributs agrégés en 30 détaillés
2. **Génération intelligente** basée sur:
   - Valeur agrégée de base (in_ring, entertainment, story)
   - Variation aléatoire ±10% pour créer de la diversité
   - Respect des contraintes (0-100)
   - Cohérence (certains attributs corrélés)

### Phase 4: Mapping Data (1h)
1. Conversion worker_id (TEXT) → Id (INTEGER)
2. Combinaison nom + prenom → Name
3. Déduction Alignment depuis role_tv
4. Déduction PushLevel depuis popularite + momentum
5. Génération géographie par défaut (basée sur nom si possible)

### Phase 5: Import Relations/Factions (optionnel) (1h)
1. Importer storyline_participants comme Relations
2. Créer Factions pour groupes existants
3. Préserver historique si disponible

### Phase 6: Validation & Tests (1h)
1. Vérifier intégrité référentielle
2. Valider moyennes calculées
3. Tester avec UI ProfileView
4. Corriger anomalies

---

## 🛠️ SCRIPT SQL D'IMPORTATION

### Étape 1: Attacher ancienne DB

```sql
-- Attacher BAKI1.1.db comme source
ATTACH DATABASE '/chemin/vers/BAKI1.1.db' AS legacy;
```

### Étape 2: Créer Workers avec nouveau schéma

```sql
-- Créer table Workers avec structure complète
CREATE TABLE IF NOT EXISTS Workers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    RealName TEXT,
    Gender TEXT DEFAULT 'Male' CHECK(Gender IN ('Male', 'Female', 'Other')),
    Age INTEGER DEFAULT 25,
    DateOfBirth TEXT,
    Height INTEGER DEFAULT 180,
    Weight INTEGER DEFAULT 90,

    -- Géographie
    BirthCity TEXT,
    BirthCountry TEXT,
    ResidenceCity TEXT,
    ResidenceState TEXT,
    ResidenceCountry TEXT,

    -- Physique
    PhotoPath TEXT,
    Handedness TEXT DEFAULT 'Right' CHECK(Handedness IN ('Right', 'Left', 'Ambidextrous')),
    FightingStance TEXT DEFAULT 'Orthodox' CHECK(FightingStance IN ('Orthodox', 'Southpaw', 'Switch')),

    -- Gimmick & Push
    CurrentGimmick TEXT,
    Alignment TEXT DEFAULT 'Face' CHECK(Alignment IN ('Face', 'Heel', 'Tweener')),
    PushLevel TEXT DEFAULT 'MidCard' CHECK(PushLevel IN ('MainEvent', 'UpperMid', 'MidCard', 'LowerMid', 'Jobber')),
    TvRole INTEGER DEFAULT 50 CHECK(TvRole >= 0 AND TvRole <= 100),
    BookingIntent TEXT,

    -- Career
    Experience INTEGER DEFAULT 5,
    IsActive INTEGER DEFAULT 1,
    IsInjured INTEGER DEFAULT 0,

    -- Legacy fields (conservés)
    CompanyId TEXT,
    Popularite INTEGER DEFAULT 50,
    Fatigue INTEGER DEFAULT 0,
    Momentum INTEGER DEFAULT 50,
    Morale INTEGER DEFAULT 60
);
```

### Étape 3: Import Workers de base

```sql
-- Import des workers avec mapping de base
INSERT INTO Workers (
    Name,
    Age,
    Height,
    Weight,
    CompanyId,
    Popularite,
    Fatigue,
    Momentum,
    Morale,
    TvRole,
    Alignment,
    PushLevel,
    IsActive
)
SELECT
    nom || ' ' || prenom AS Name,
    30 AS Age,  -- Valeur par défaut (peut être raffinée)
    180 AS Height,
    90 AS Weight,
    company_id AS CompanyId,
    popularite AS Popularite,
    fatigue AS Fatigue,
    momentum AS Momentum,
    morale AS Morale,

    -- Conversion role_tv en TvRole (0-100)
    CASE role_tv
        WHEN 'Main Event' THEN 90
        WHEN 'Upper Mid-Card' THEN 75
        WHEN 'Mid-Card' THEN 50
        WHEN 'Lower Mid-Card' THEN 35
        WHEN 'Jobber' THEN 20
        ELSE 50
    END AS TvRole,

    -- Déduction Alignment (simplifiée)
    CASE
        WHEN popularite > 70 THEN 'Face'
        WHEN popularite < 40 THEN 'Heel'
        ELSE 'Tweener'
    END AS Alignment,

    -- Déduction PushLevel depuis role_tv
    CASE role_tv
        WHEN 'Main Event' THEN 'MainEvent'
        WHEN 'Upper Mid-Card' THEN 'UpperMid'
        WHEN 'Mid-Card' THEN 'MidCard'
        WHEN 'Lower Mid-Card' THEN 'LowerMid'
        WHEN 'Jobber' THEN 'Jobber'
        ELSE 'MidCard'
    END AS PushLevel,

    1 AS IsActive

FROM legacy.workers
WHERE worker_id IS NOT NULL;
```

### Étape 4: Génération Attributs In-Ring

**Algorithme**: Décomposer `in_ring` (agrégé) en 10 attributs avec variation

```sql
INSERT INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT
    w.Id AS WorkerId,

    -- Utiliser in_ring comme base, ajouter variation aléatoire ±10
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Striking,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Grappling,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS HighFlying,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Powerhouse,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Timing,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Selling,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Psychology,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Stamina,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Safety,
    CAST((lw.in_ring + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS HardcoreBrawl

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

-- Nettoyer les valeurs hors limites (0-100)
UPDATE WorkerInRingAttributes SET Striking = MAX(0, MIN(100, Striking));
UPDATE WorkerInRingAttributes SET Grappling = MAX(0, MIN(100, Grappling));
UPDATE WorkerInRingAttributes SET HighFlying = MAX(0, MIN(100, HighFlying));
UPDATE WorkerInRingAttributes SET Powerhouse = MAX(0, MIN(100, Powerhouse));
UPDATE WorkerInRingAttributes SET Timing = MAX(0, MIN(100, Timing));
UPDATE WorkerInRingAttributes SET Selling = MAX(0, MIN(100, Selling));
UPDATE WorkerInRingAttributes SET Psychology = MAX(0, MIN(100, Psychology));
UPDATE WorkerInRingAttributes SET Stamina = MAX(0, MIN(100, Stamina));
UPDATE WorkerInRingAttributes SET Safety = MAX(0, MIN(100, Safety));
UPDATE WorkerInRingAttributes SET HardcoreBrawl = MAX(0, MIN(100, HardcoreBrawl));
```

### Étape 5: Génération Attributs Entertainment

```sql
INSERT INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT
    w.Id AS WorkerId,

    -- Même algorithme avec entertainment comme base
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Charisma,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS MicWork,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Acting,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS CrowdConnection,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS StarPower,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Improvisation,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Entrance,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS SexAppeal,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS MerchandiseAppeal,
    CAST((lw.entertainment + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS CrossoverPotential

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

-- Nettoyer (même principe)
-- ... (10 UPDATE similaires)
```

### Étape 6: Génération Attributs Story

```sql
INSERT INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT
    w.Id AS WorkerId,

    -- Même algorithme avec story comme base
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS CharacterDepth,
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Consistency,

    -- HeelPerformance corrélé avec Alignment
    CASE
        WHEN w.Alignment = 'Heel' THEN CAST((lw.story + 15 + (ABS(RANDOM()) % 11 - 5)) AS INTEGER)
        ELSE CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER)
    END AS HeelPerformance,

    -- BabyfacePerformance corrélé avec Alignment
    CASE
        WHEN w.Alignment = 'Face' THEN CAST((lw.story + 15 + (ABS(RANDOM()) % 11 - 5)) AS INTEGER)
        ELSE CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER)
    END AS BabyfacePerformance,

    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS StorytellingLongTerm,
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS EmotionalRange,
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS Adaptability,
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS RivalryChemistry,
    CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER) AS CreativeInput,

    -- MoralAlignment corrélé avec Alignment
    CASE
        WHEN w.Alignment = 'Tweener' THEN CAST((lw.story + 20 + (ABS(RANDOM()) % 11 - 5)) AS INTEGER)
        ELSE CAST((lw.story + (ABS(RANDOM()) % 21 - 10)) AS INTEGER)
    END AS MoralAlignment

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

-- Nettoyer (même principe)
-- ... (10 UPDATE similaires)
```

### Étape 7: Génération Spécialisations par défaut

```sql
-- Ajouter 1 spécialisation primaire basée sur les attributs InRing
INSERT INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT
    w.Id AS WorkerId,

    -- Déterminer spécialisation dominante
    CASE
        WHEN wir.Striking >= wir.Grappling AND wir.Striking >= wir.HighFlying AND wir.Striking >= wir.Powerhouse THEN 'Brawler'
        WHEN wir.Grappling >= wir.Striking AND wir.Grappling >= wir.HighFlying AND wir.Grappling >= wir.Powerhouse THEN 'Technical'
        WHEN wir.HighFlying >= wir.Striking AND wir.HighFlying >= wir.Grappling AND wir.HighFlying >= wir.Powerhouse THEN 'HighFlyer'
        WHEN wir.Powerhouse >= wir.Striking AND wir.Powerhouse >= wir.Grappling AND wir.Powerhouse >= wir.HighFlying THEN 'Power'
        ELSE 'AllRounder'
    END AS Specialization,

    1 AS Level  -- Primary

FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId;
```

### Étape 8: Détacher ancienne DB

```sql
-- Détacher BAKI1.1.db
DETACH DATABASE legacy;
```

---

## 🔧 AMÉLIORATION ALGORITHMIQUE

### Problème avec Variation Aléatoire Pure
- Tous les attributs sont similaires → manque de personnalité
- Pas de corrélations réalistes (ex: Striking vs Grappling)

### Solution: Profils Typés

```sql
-- Créer des profils basés sur analyse
-- Exemple: Striker = Striking +20%, Grappling -15%, HighFlying -10%
-- Exemple: High-Flyer = HighFlying +25%, Powerhouse -20%, Safety -10%

-- Détection automatique du profil
WITH WorkerProfiles AS (
    SELECT
        w.Id,
        lw.in_ring,
        lw.entertainment,
        lw.story,
        -- Analyse nom pour détecter style (heuristique)
        CASE
            WHEN w.Name LIKE '%Rey%' OR w.Name LIKE '%Ricochet%' THEN 'HighFlyer'
            WHEN w.Name LIKE '%Lesnar%' OR w.Name LIKE '%Lashley%' THEN 'Powerhouse'
            WHEN w.Name LIKE '%Angle%' OR w.Name LIKE '%Gable%' THEN 'Technical'
            ELSE 'Balanced'
        END AS Profile
    FROM Workers w
    INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom)
)

-- Appliquer profil aux attributs
-- ... (logique complexe pour chaque profil)
```

---

## 📊 VALIDATION POST-IMPORT

### Tests SQL

```sql
-- 1. Vérifier nombre de workers importés
SELECT COUNT(*) AS TotalWorkers FROM Workers;
SELECT COUNT(*) AS OldWorkers FROM legacy.workers;

-- 2. Vérifier que tous ont des attributs
SELECT COUNT(*) FROM Workers w
LEFT JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
WHERE wir.WorkerId IS NULL;
-- Résultat attendu: 0

-- 3. Vérifier moyennes cohérentes
SELECT
    w.Name,
    w.CompanyId,
    wir.InRingAvg,
    wea.EntertainmentAvg,
    wsa.StoryAvg,
    (wir.InRingAvg + wea.EntertainmentAvg + wsa.StoryAvg) / 3 AS OverallRating
FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
INNER JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
ORDER BY OverallRating DESC
LIMIT 20;

-- 4. Comparer avec anciennes valeurs
SELECT
    w.Name,
    lw.in_ring AS OldInRing,
    wir.InRingAvg AS NewInRingAvg,
    (wir.InRingAvg - lw.in_ring) AS Difference
FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom)
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
WHERE ABS(wir.InRingAvg - lw.in_ring) > 15  -- Différence > 15 points
ORDER BY ABS(wir.InRingAvg - lw.in_ring) DESC;
```

---

## 🚀 AUTOMATISATION COMPLÈTE

### Script C# d'Importation

```csharp
public class WorkerImporter
{
    private readonly SqliteConnectionFactory _factory;
    private readonly IWorkerAttributesRepository _attributesRepo;

    public async Task ImportFromLegacyDb(string legacyDbPath)
    {
        using var connection = _factory.OuvrirConnexion();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Attacher ancienne DB
            var attachCmd = connection.CreateCommand();
            attachCmd.CommandText = $"ATTACH DATABASE '{legacyDbPath}' AS legacy";
            attachCmd.ExecuteNonQuery();

            // 2. Import workers
            var importWorkersCmd = connection.CreateCommand();
            importWorkersCmd.CommandText = @"
                INSERT INTO Workers (Name, CompanyId, ...)
                SELECT nom || ' ' || prenom, company_id, ...
                FROM legacy.workers";
            var workersImported = importWorkersCmd.ExecuteNonQuery();

            Console.WriteLine($"Imported {workersImported} workers");

            // 3. Générer attributs pour chaque worker
            var workers = await GetAllWorkers(connection);
            foreach (var worker in workers)
            {
                await GenerateAttributes(worker, connection);
            }

            // 4. Détacher
            var detachCmd = connection.CreateCommand();
            detachCmd.CommandText = "DETACH DATABASE legacy";
            detachCmd.ExecuteNonQuery();

            transaction.Commit();
            Console.WriteLine("Import completed successfully!");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Import failed: {ex.Message}");
            throw;
        }
    }

    private async Task GenerateAttributes(Worker worker, SqliteConnection conn)
    {
        // Logique de génération intelligente des 30 attributs
        // basée sur in_ring, entertainment, story de l'ancien système
    }
}
```

---

## 📅 PLANNING EXÉCUTION

### Jour 1 (3h)
- ✅ Analyser structure BAKI1.1.db
- ✅ Créer script d'importation SQL
- ✅ Créer algorithme génération attributs
- ✅ Tests unitaires sur subset (10 workers)

### Jour 2 (2h)
- Import complet de tous les workers
- Validation des données
- Tests avec ProfileView UI
- Corrections si nécessaire

### Jour 3 (1h)
- Documentation processus
- Script automatisé pour futurs imports
- Backup et archivage

---

## ⚠️ POINTS D'ATTENTION

### Risques
1. **Perte de données**: Backup obligatoire avant import
2. **Attributs trop homogènes**: Utiliser profils typés
3. **Performances**: Import par batch si > 1000 workers
4. **Encoding**: Vérifier UTF-8 pour noms spéciaux

### Recommandations
- Tester d'abord sur 10 workers
- Comparer visuellement dans ProfileView
- Ajuster algorithme si nécessaire
- Garder BAKI1.1.db intact (read-only)

---

## 📦 LIVRABLES

1. ✅ **Plan d'importation** (ce document)
2. ⏳ **Script SQL complet** (WorkersImport.sql)
3. ⏳ **Script C# automatisé** (WorkerImporter.cs)
4. ⏳ **Tests de validation** (ImportValidationTests.cs)
5. ⏳ **Documentation utilisateur** (IMPORT_GUIDE.md)

---

**Status**: ✅ Plan validé
**Prochaine étape**: Créer WorkersImport.sql complet
**Estimation**: 6h de développement total

---

**Chef de Projet**: Claude
**Date de création**: 2026-01-08
**Version**: 1.0

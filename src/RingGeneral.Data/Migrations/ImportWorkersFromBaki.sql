-- ============================================================================
-- IMPORT WORKERS - BAKI1.1.db vers Ring General
-- Version: 2.0 (Corrected)
-- Date: 2026-01-08
-- Description: Import workers from BAKI1.1.db with attribute generation
-- ============================================================================

-- Attacher BAKI1.1.db
ATTACH DATABASE 'BAKI1.1.db' AS baki;

BEGIN TRANSACTION;

-- ============================================================================
-- ÉTAPE 1: IMPORT WORKERS
-- ============================================================================

INSERT INTO Workers (
    worker_id,
    Name,
    Gender,
    Age,
    Height,
    Weight,

    -- Career
    Experience,
    DebutYear,

    -- Legacy aggregated attributes (from BAKI)
    in_ring,
    entertainment,
    story,

    -- Stats
    popularite,
    Popularity,
    fatigue,
    blessure,
    momentum,
    morale
)
SELECT
    CAST(w.workerID AS TEXT) AS worker_id,
    w.name AS Name,

    -- Gender
    CASE
        WHEN w.gender = 'Male' THEN 'M'
        WHEN w.gender = 'Female' THEN 'F'
        ELSE 'Other'
    END AS Gender,

    -- Age (calculate from birthDate or use random 25-45)
    COALESCE(
        CAST((julianday('now') - julianday(w.birthDate)) / 365.25 AS INTEGER),
        25 + (ABS(RANDOM()) % 21)
    ) AS Age,

    -- Height (parse from text, default 180)
    COALESCE(CAST(REPLACE(w.height, ' cm', '') AS INTEGER), 180) AS Height,

    -- Weight (parse from text, default 90)
    COALESCE(CAST(REPLACE(w.weight, ' kg', '') AS INTEGER), 90) AS Weight,

    -- Experience (calculate from debutDate or use random 0-20)
    COALESCE(
        CAST((julianday('now') - julianday(w.debutDate)) / 365.25 AS INTEGER),
        ABS(RANDOM()) % 21
    ) AS Experience,

    -- DebutYear
    COALESCE(
        CAST(substr(w.debutDate, 1, 4) AS INTEGER),
        2025 - (ABS(RANDOM()) % 21)
    ) AS DebutYear,

    -- Legacy aggregated attributes (0-100 scale)
    CAST(COALESCE(w.wrestlingSkill, 50) AS INTEGER) AS in_ring,
    CAST(COALESCE(w.entertainment, 50) AS INTEGER) AS entertainment,
    CAST(COALESCE(w.psychology, 50) AS INTEGER) AS story, -- Use psychology as story

    -- Stats
    CAST(COALESCE(w.northAmericaPop, 50) AS INTEGER) AS popularite,
    CAST(COALESCE(w.northAmericaPop, 50) AS INTEGER) AS Popularity,
    CAST(COALESCE(w.fatigue, 0) AS INTEGER) AS fatigue,
    COALESCE(w.injuryType, 'Aucune') AS blessure,
    50 AS momentum, -- Default
    60 AS morale    -- Default

FROM baki.workers w
WHERE w.status = 'Active'
  OR w.status = 'Inactive';

-- ============================================================================
-- ÉTAPE 2: GÉNÉRATION ATTRIBUTS IN-RING (10 attributes, 0-100 scale)
-- ============================================================================

INSERT INTO WorkerInRingAttributes (
    WorkerId,
    Striking,
    Grappling,
    HighFlying,
    Powerhouse,
    Timing,
    Selling,
    Psychology,
    Stamina,
    Safety,
    HardcoreBrawl
)
SELECT
    w.Id AS WorkerId,

    -- Striking (base: wrestlingSkill ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER) < 80 THEN 5 ELSE 0 END
    )) AS Striking,

    -- Grappling (base: wrestlingSkill ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Grappling,

    -- HighFlying (inversely correlated with weight)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        - CASE
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) > 110 THEN 25
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) > 100 THEN 15
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) < 75 THEN -20
            ELSE 0
          END
    )) AS HighFlying,

    -- Powerhouse (correlated with weight)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) > 110 THEN 25
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) > 100 THEN 15
            WHEN COALESCE(CAST(REPLACE(bw.weight, ' kg', '') AS INTEGER), 90) < 75 THEN -15
            ELSE 0
          END
    )) AS Powerhouse,

    -- Timing (correlated with experience)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 15 THEN 20
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 10 THEN 12
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 5 THEN 5
            ELSE 0
          END
    )) AS Timing,

    -- Selling (use BAKI greatSeller/badSeller)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.greatSeller, 0) = 1 THEN 30 ELSE 0 END
        - CASE WHEN COALESCE(bw.badSeller, 0) = 1 THEN 30 ELSE 0 END
    )) AS Selling,

    -- Psychology (from BAKI psychology attribute)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Psychology,

    -- Stamina (from BAKI stamina)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.stamina, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Stamina,

    -- Safety (from BAKI safety)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.safety, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Safety,

    -- HardcoreBrawl (use BAKI lovesHardcore/hatesHardcore)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.wrestlingSkill, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.lovesHardcore, 0) = 1 THEN 35 ELSE 0 END
        - CASE WHEN COALESCE(bw.hatesHardcore, 0) = 1 THEN 35 ELSE 0 END
    )) AS HardcoreBrawl

FROM Workers w
INNER JOIN baki.workers bw ON CAST(bw.workerID AS TEXT) = w.worker_id;

-- ============================================================================
-- ÉTAPE 3: GÉNÉRATION ATTRIBUTS ENTERTAINMENT (10 attributes, 0-100 scale)
-- ============================================================================

INSERT INTO WorkerEntertainmentAttributes (
    WorkerId,
    Charisma,
    MicWork,
    Acting,
    CrowdConnection,
    StarPower,
    Improvisation,
    Entrance,
    SexAppeal,
    MerchandiseAppeal,
    CrossoverPotential
)
SELECT
    w.Id AS WorkerId,

    -- Charisma (base: entertainment ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Charisma,

    -- MicWork (base: entertainment ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS MicWork,

    -- Acting (base: entertainment ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.canDoAngles, 0) = 1 THEN 15 ELSE 0 END
    )) AS Acting,

    -- CrowdConnection (base: entertainment ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS CrowdConnection,

    -- StarPower (from BAKI starPower)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.starPower, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS StarPower,

    -- Improvisation
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Improvisation,

    -- Entrance
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS Entrance,

    -- SexAppeal (from BAKI sexAppeal)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.sexAppeal, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS SexAppeal,

    -- MerchandiseAppeal (correlated with starPower + popularity)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.starPower, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN CAST(COALESCE(bw.northAmericaPop, 50) AS INTEGER) > 75 THEN 15 ELSE 0 END
    )) AS MerchandiseAppeal,

    -- CrossoverPotential (celebrity status)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.starPower, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.isCelebrity, 0) = 1 THEN 30 ELSE 0 END
    )) AS CrossoverPotential

FROM Workers w
INNER JOIN baki.workers bw ON CAST(bw.workerID AS TEXT) = w.worker_id;

-- ============================================================================
-- ÉTAPE 4: GÉNÉRATION ATTRIBUTS STORY (10 attributes, 0-100 scale)
-- ============================================================================

INSERT INTO WorkerStoryAttributes (
    WorkerId,
    CharacterDepth,
    Consistency,
    HeelPerformance,
    BabyfacePerformance,
    StorytellingLongTerm,
    EmotionalRange,
    Adaptability,
    RivalryChemistry,
    CreativeInput,
    MoralAlignment
)
SELECT
    w.Id AS WorkerId,

    -- CharacterDepth (base: psychology ±12)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS CharacterDepth,

    -- Consistency (correlated with experience)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 15 THEN 20
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 10 THEN 12
            ELSE 0
          END
    )) AS Consistency,

    -- HeelPerformance
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.intimidation, 50) > 70 THEN 15 ELSE 0 END
    )) AS HeelPerformance,

    -- BabyfacePerformance
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.respectful, 0) = 1 THEN 15 ELSE 0 END
    )) AS BabyfacePerformance,

    -- StorytellingLongTerm (correlated with experience)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 12 THEN 18
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 7 THEN 10
            ELSE 0
          END
    )) AS StorytellingLongTerm,

    -- EmotionalRange
    MAX(0, MIN(100,
        CAST(COALESCE(bw.entertainment, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS EmotionalRange,

    -- Adaptability (use quickLearner)
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.quickLearner, 0) = 1 THEN 20 ELSE 0 END
        - CASE WHEN COALESCE(bw.slowLearner, 0) = 1 THEN 20 ELSE 0 END
    )) AS Adaptability,

    -- RivalryChemistry
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
    )) AS RivalryChemistry,

    -- CreativeInput
    MAX(0, MIN(100,
        CAST(COALESCE(bw.psychology, 50) AS INTEGER)
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN COALESCE(bw.outspoken, 0) = 1 THEN 15 ELSE 0 END
    )) AS CreativeInput,

    -- MoralAlignment (50 = neutral, >50 = face, <50 = heel)
    50 + (ABS(RANDOM()) % 51 - 25) AS MoralAlignment

FROM Workers w
INNER JOIN baki.workers bw ON CAST(bw.workerID AS TEXT) = w.worker_id;

-- ============================================================================
-- ÉTAPE 5: GÉNÉRATION ATTRIBUTS MENTAUX (10 attributes, 0-20 scale)
-- ============================================================================

INSERT INTO WorkerMentalAttributes (
    WorkerId,
    Ambition,
    Loyauté,
    Professionnalisme,
    Pression,
    Tempérament,
    Égoïsme,
    Détermination,
    Adaptabilité,
    Influence,
    Sportivité,
    IsRevealed,
    ScoutingLevel,
    LastUpdated
)
SELECT
    w.Id AS WorkerId,

    -- Ambition (use BAKI drive + potential)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN bw.drive = 'High' THEN 5 WHEN bw.drive = 'Low' THEN -3 ELSE 0 END
        + CASE WHEN CAST(COALESCE(bw.potential, 0) AS INTEGER) > 80 THEN 4 ELSE 0 END
    )) AS Ambition,

    -- Loyauté (use BAKI lifer + experience)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.lifer, 0) = 1 THEN 6 ELSE 0 END
        + CASE
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 15 THEN 4
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 10 THEN 2
            ELSE 0
          END
    )) AS Loyauté,

    -- Professionnalisme (use BAKI respectful + safety)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.respectful, 0) = 1 THEN 5 ELSE 0 END
        + CASE WHEN CAST(COALESCE(bw.safety, 50) AS INTEGER) > 80 THEN 4 ELSE 0 END
    )) AS Professionnalisme,

    -- Pression (use BAKI anxious inversely + maximumEffort)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        - CASE WHEN COALESCE(bw.anxious, 0) = 1 THEN 5 ELSE 0 END
        + CASE WHEN COALESCE(bw.maximumEffort, 0) = 1 THEN 5 ELSE 0 END
    )) AS Pression,

    -- Tempérament (use BAKI angry inversely + explosive inversely)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        - CASE WHEN COALESCE(bw.angry, 0) = 1 THEN 6 ELSE 0 END
        - CASE WHEN COALESCE(bw.explosive, 0) = 1 THEN 4 ELSE 0 END
    )) AS Tempérament,

    -- Égoïsme (use BAKI selfish + narcissist)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.selfish, 0) = 1 THEN 7 ELSE 0 END
        + CASE WHEN COALESCE(bw.narcissist, 0) = 1 THEN 6 ELSE 0 END
    )) AS Égoïsme,

    -- Détermination (use BAKI maximumEffort + tough)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.maximumEffort, 0) = 1 THEN 5 ELSE 0 END
        + CASE WHEN COALESCE(bw.tough, 0) = 1 THEN 5 ELSE 0 END
        - CASE WHEN COALESCE(bw.minimumEffort, 0) = 1 THEN 8 ELSE 0 END
    )) AS Détermination,

    -- Adaptabilité (use BAKI quickLearner)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.quickLearner, 0) = 1 THEN 6 ELSE 0 END
        - CASE WHEN COALESCE(bw.slowLearner, 0) = 1 THEN 6 ELSE 0 END
        - CASE WHEN COALESCE(bw.oldSchool, 0) = 1 THEN 3 ELSE 0 END
    )) AS Adaptabilité,

    -- Influence (use BAKI leader + experience + popularity)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.leader, 0) = 1 THEN 6 ELSE 0 END
        + CASE
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 15 THEN 5
            WHEN COALESCE(CAST((julianday('now') - julianday(bw.debutDate)) / 365.25 AS INTEGER), 0) > 10 THEN 3
            ELSE 0
          END
        - CASE WHEN COALESCE(bw.loner, 0) = 1 THEN 7 ELSE 0 END
    )) AS Influence,

    -- Sportivité (use BAKI respectful + safety inversely with selfish)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN COALESCE(bw.respectful, 0) = 1 THEN 6 ELSE 0 END
        + CASE WHEN CAST(COALESCE(bw.safety, 50) AS INTEGER) > 80 THEN 4 ELSE 0 END
        - CASE WHEN COALESCE(bw.selfish, 0) = 1 THEN 5 ELSE 0 END
    )) AS Sportivité,

    -- Metadata
    0 AS IsRevealed,  -- Hidden by default
    0 AS ScoutingLevel,  -- Not scouted yet
    CURRENT_TIMESTAMP AS LastUpdated

FROM Workers w
INNER JOIN baki.workers bw ON CAST(bw.workerID AS TEXT) = w.worker_id;

COMMIT;

-- ============================================================================
-- VALIDATION QUERIES
-- ============================================================================

SELECT '========================================' AS '';
SELECT 'Workers importés:|' || COUNT(*) AS '' FROM Workers;
SELECT 'Workers sans InRing:|' || COUNT(*) AS ''
FROM Workers w
LEFT JOIN WorkerInRingAttributes a ON w.Id = a.WorkerId
WHERE a.WorkerId IS NULL;
SELECT 'Workers sans Entertainment:|' || COUNT(*) AS ''
FROM Workers w
LEFT JOIN WorkerEntertainmentAttributes a ON w.Id = a.WorkerId
WHERE a.WorkerId IS NULL;
SELECT 'Workers sans Story:|' || COUNT(*) AS ''
FROM Workers w
LEFT JOIN WorkerStoryAttributes a ON w.Id = a.WorkerId
WHERE a.WorkerId IS NULL;
SELECT 'Workers sans Mental:|' || COUNT(*) AS ''
FROM Workers w
LEFT JOIN WorkerMentalAttributes a ON w.Id = a.WorkerId
WHERE a.WorkerId IS NULL;
SELECT '✅ Import terminé avec succès !' AS '';

-- Detach BAKI database
DETACH DATABASE baki;

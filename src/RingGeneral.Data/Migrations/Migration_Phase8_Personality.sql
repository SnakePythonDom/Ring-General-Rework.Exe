-- ============================================================
-- MIGRATION PHASE 8: SYSTÈME PERSONNALITÉS
-- Date: 2026-01-08
-- Description: Ajout 10 attributs mentaux + profils personnalité
-- Author: Claude
-- Version: 1.0
-- ============================================================

BEGIN TRANSACTION;

-- ===== 1. NOUVELLE TABLE ATTRIBUTS MENTAUX =====

CREATE TABLE IF NOT EXISTS WorkerMentalAttributes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL UNIQUE,

    -- 10 Attributs Mentaux (0-20 scale)
    Ambition INTEGER NOT NULL DEFAULT 10 CHECK(Ambition BETWEEN 0 AND 20),
    Loyauté INTEGER NOT NULL DEFAULT 10 CHECK(Loyauté BETWEEN 0 AND 20),
    Professionnalisme INTEGER NOT NULL DEFAULT 10 CHECK(Professionnalisme BETWEEN 0 AND 20),
    Pression INTEGER NOT NULL DEFAULT 10 CHECK(Pression BETWEEN 0 AND 20),
    Tempérament INTEGER NOT NULL DEFAULT 10 CHECK(Tempérament BETWEEN 0 AND 20),
    Égoïsme INTEGER NOT NULL DEFAULT 10 CHECK(Égoïsme BETWEEN 0 AND 20),
    Détermination INTEGER NOT NULL DEFAULT 10 CHECK(Détermination BETWEEN 0 AND 20),
    Adaptabilité INTEGER NOT NULL DEFAULT 10 CHECK(Adaptabilité BETWEEN 0 AND 20),
    Influence INTEGER NOT NULL DEFAULT 10 CHECK(Influence BETWEEN 0 AND 20),
    Sportivité INTEGER NOT NULL DEFAULT 10 CHECK(Sportivité BETWEEN 0 AND 20),

    -- Metadata for scouting system
    IsRevealed BOOLEAN NOT NULL DEFAULT 0,
    ScoutingLevel INTEGER NOT NULL DEFAULT 0 CHECK(ScoutingLevel BETWEEN 0 AND 2),
    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Foreign Key
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_mental_worker ON WorkerMentalAttributes(WorkerId);
CREATE INDEX IF NOT EXISTS idx_mental_revealed ON WorkerMentalAttributes(IsRevealed);

-- ===== 2. EXTENSION TABLE WORKERS =====

-- Add personality profile column (TEXT for flexibility)
ALTER TABLE Workers ADD COLUMN PersonalityProfile TEXT DEFAULT NULL;

-- Timestamp when profile was last detected
ALTER TABLE Workers ADD COLUMN PersonalityProfileDetectedAt TEXT DEFAULT NULL;

-- ===== 3. GÉNÉRATION ATTRIBUTS MENTAUX POUR WORKERS EXISTANTS =====

-- Algorithm: Generate mental attributes based on existing performance attributes,
-- experience, age, popularity, and push level with intelligent correlations.

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

    -- Ambition: Correlated with Popularity + PushLevel + Charisma
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN w.Popularity > 80 THEN 5
            WHEN w.Popularity > 60 THEN 3
            WHEN w.Popularity > 40 THEN 1
            ELSE 0
          END
        + CASE
            WHEN w.PushLevel = 'MainEvent' THEN 5
            WHEN w.PushLevel = 'UpperMid' THEN 3
            WHEN w.PushLevel = 'MidCard' THEN 1
            ELSE 0
          END
        + CASE
            WHEN wea.Charisma > 80 THEN 2
            ELSE 0
          END
    )) AS Ambition,

    -- Loyauté: Correlated with Experience + Age + Momentum stability
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN w.Experience >= 15 THEN 5
            WHEN w.Experience >= 10 THEN 3
            WHEN w.Experience >= 5 THEN 1
            ELSE 0
          END
        + CASE
            WHEN w.Age >= 38 THEN 3
            WHEN w.Age >= 33 THEN 2
            WHEN w.Age >= 28 THEN 1
            ELSE 0
          END
        + CASE
            WHEN w.Momentum > 70 AND w.Momentum < 90 THEN 2
            ELSE 0
          END
    )) AS Loyauté,

    -- Professionnalisme: Correlated with Safety + Experience + Psychology
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN wir.Safety >= 85 THEN 6
            WHEN wir.Safety >= 70 THEN 4
            WHEN wir.Safety >= 55 THEN 2
            ELSE 0
          END
        + CASE
            WHEN w.Experience >= 12 THEN 4
            WHEN w.Experience >= 7 THEN 2
            ELSE 0
          END
        + CASE
            WHEN wir.Psychology >= 75 THEN 2
            ELSE 0
          END
    )) AS Professionnalisme,

    -- Pression: Correlated with Timing + Psychology + Charisma
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN wir.Timing >= 85 THEN 6
            WHEN wir.Timing >= 70 THEN 4
            WHEN wir.Timing >= 55 THEN 2
            ELSE 0
          END
        + CASE
            WHEN wir.Psychology >= 80 THEN 4
            WHEN wir.Psychology >= 65 THEN 2
            ELSE 0
          END
        + CASE
            WHEN wea.Charisma >= 75 THEN 2
            ELSE 0
          END
    )) AS Pression,

    -- Tempérament: Random (uncorrelated - personality trait)
    -- Slight bonus for older workers (more experienced = calmer)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 21 - 10)
        + CASE
            WHEN w.Age >= 35 THEN 2
            ELSE 0
          END
    )) AS Tempérament,

    -- Égoïsme: Correlated with Popularity + PushLevel + StarPower
    -- Success breeds ego
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN w.Popularity > 85 THEN 6
            WHEN w.Popularity > 70 THEN 4
            WHEN w.Popularity > 55 THEN 2
            ELSE 0
          END
        + CASE
            WHEN w.PushLevel = 'MainEvent' THEN 5
            WHEN w.PushLevel = 'UpperMid' THEN 2
            ELSE 0
          END
        + CASE
            WHEN wea.StarPower > 85 THEN 3
            ELSE 0
          END
        -- Young workers can be more egotistical
        - CASE
            WHEN w.Age >= 35 THEN 2
            ELSE 0
          END
    )) AS Égoïsme,

    -- Détermination: Correlated with Stamina + Experience + BabyfacePerf
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN wir.Stamina >= 85 THEN 5
            WHEN wir.Stamina >= 70 THEN 3
            WHEN wir.Stamina >= 55 THEN 1
            ELSE 0
          END
        + CASE
            WHEN w.Experience >= 10 THEN 4
            WHEN w.Experience >= 6 THEN 2
            ELSE 0
          END
        + CASE
            WHEN wea.BabyfacePerformance >= 75 THEN 2
            ELSE 0
          END
    )) AS Détermination,

    -- Adaptabilité: Correlated with Experience + balanced averages + Versatility
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN w.Experience >= 15 THEN 6
            WHEN w.Experience >= 10 THEN 4
            WHEN w.Experience >= 6 THEN 2
            ELSE 0
          END
        -- Bonus if InRing and Entertainment are balanced (versatile performer)
        + CASE
            WHEN ABS(wir.InRingAvg - wea.EntertainmentAvg) < 10 THEN 3
            WHEN ABS(wir.InRingAvg - wea.EntertainmentAvg) < 20 THEN 1
            ELSE 0
          END
        + CASE
            WHEN wea.Versatility >= 75 THEN 3
            ELSE 0
          END
    )) AS Adaptabilité,

    -- Influence: Correlated with Experience + Popularity + PushLevel + HeelPerf
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN w.Experience >= 18 THEN 7
            WHEN w.Experience >= 13 THEN 5
            WHEN w.Experience >= 8 THEN 3
            ELSE 0
          END
        + CASE
            WHEN w.Popularity > 80 THEN 5
            WHEN w.Popularity > 65 THEN 3
            ELSE 0
          END
        + CASE
            WHEN w.PushLevel = 'MainEvent' THEN 4
            WHEN w.PushLevel = 'UpperMid' THEN 2
            ELSE 0
          END
        + CASE
            WHEN wea.HeelPerformance >= 75 THEN 2
            ELSE 0
          END
    )) AS Influence,

    -- Sportivité: Correlated with Safety + Psychology + BabyfacePerf
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE
            WHEN wir.Safety >= 90 THEN 6
            WHEN wir.Safety >= 75 THEN 4
            WHEN wir.Safety >= 60 THEN 2
            ELSE 0
          END
        + CASE
            WHEN wir.Psychology >= 80 THEN 3
            WHEN wir.Psychology >= 65 THEN 1
            ELSE 0
          END
        + CASE
            WHEN wea.BabyfacePerformance >= 75 THEN 3
            ELSE 0
          END
        -- Veterans tend to be more sportsmanlike
        + CASE
            WHEN w.Experience >= 15 THEN 2
            ELSE 0
          END
    )) AS Sportivité,

    -- Metadata
    0 AS IsRevealed,  -- Hidden by default
    0 AS ScoutingLevel,  -- Not scouted yet
    CURRENT_TIMESTAMP AS LastUpdated

FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
WHERE NOT EXISTS (
    SELECT 1 FROM WorkerMentalAttributes wma WHERE wma.WorkerId = w.Id
);

-- ===== 4. VALIDATION QUERIES =====

-- Count workers with mental attributes
SELECT
    'Workers with Mental Attributes' AS Check_Name,
    COUNT(*) AS Count
FROM WorkerMentalAttributes;

-- Verify all workers have mental attributes
SELECT
    'Workers Missing Mental Attributes' AS Check_Name,
    COUNT(*) AS Count
FROM Workers w
LEFT JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
WHERE wma.Id IS NULL;

-- Distribution check (should average around 10-12 for each attribute)
SELECT
    'Ambition' AS Attribut,
    ROUND(AVG(Ambition), 1) AS Moyenne,
    MIN(Ambition) AS Min,
    MAX(Ambition) AS Max
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Loyauté', ROUND(AVG(Loyauté), 1), MIN(Loyauté), MAX(Loyauté)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Professionnalisme', ROUND(AVG(Professionnalisme), 1), MIN(Professionnalisme), MAX(Professionnalisme)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Pression', ROUND(AVG(Pression), 1), MIN(Pression), MAX(Pression)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Tempérament', ROUND(AVG(Tempérament), 1), MIN(Tempérament), MAX(Tempérament)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Égoïsme', ROUND(AVG(Égoïsme), 1), MIN(Égoïsme), MAX(Égoïsme)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Détermination', ROUND(AVG(Détermination), 1), MIN(Détermination), MAX(Détermination)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Adaptabilité', ROUND(AVG(Adaptabilité), 1), MIN(Adaptabilité), MAX(Adaptabilité)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Influence', ROUND(AVG(Influence), 1), MIN(Influence), MAX(Influence)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Sportivité', ROUND(AVG(Sportivité), 1), MIN(Sportivité), MAX(Sportivité)
FROM WorkerMentalAttributes;

-- Top 10 by Professionnalisme
SELECT
    'Top 10 Professionnalisme' AS Report,
    w.Name,
    wma.Professionnalisme,
    wma.Sportivité,
    wma.Loyauté
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY wma.Professionnalisme DESC, wma.Sportivité DESC
LIMIT 10;

-- Top 10 Égoïstes (potential problems)
SELECT
    'Top 10 Égoïstes' AS Report,
    w.Name,
    wma.Égoïsme,
    wma.Sportivité,
    wma.Tempérament
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY wma.Égoïsme DESC, wma.Sportivité ASC
LIMIT 10;

-- Potential Elite Workers (High Pro + High Pressure)
SELECT
    'Potential Elite Workers' AS Report,
    w.Name,
    ROUND((wma.Professionnalisme + wma.Sportivité + wma.Loyauté) / 3.0, 1) AS ProfScore,
    ROUND((wma.Pression + wma.Détermination) / 2.0, 1) AS PressureScore,
    wma.Égoïsme
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
WHERE wma.Professionnalisme >= 15
  AND wma.Pression >= 13
ORDER BY ProfScore DESC, PressureScore DESC
LIMIT 15;

-- Potential Problem Workers (Red Flags)
SELECT
    'Potential Problem Workers' AS Report,
    w.Name,
    wma.Professionnalisme AS Pro,
    wma.Tempérament AS Temp,
    wma.Égoïsme AS Ego,
    wma.Sportivité AS Sport,
    CASE
        WHEN wma.Professionnalisme <= 5 AND wma.Détermination <= 5 THEN 'Poids Mort'
        WHEN wma.Tempérament <= 5 AND wma.Pression <= 5 THEN 'Instable Chronique'
        WHEN wma.Égoïsme >= 17 AND wma.Tempérament <= 6 THEN 'Diva'
        WHEN wma.Sportivité <= 5 AND wma.Égoïsme >= 15 THEN 'Saboteur Potentiel'
        ELSE 'Surveillance'
    END AS Flag
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
WHERE wma.Professionnalisme <= 7
   OR wma.Tempérament <= 6
   OR (wma.Égoïsme >= 15 AND wma.Sportivité <= 6)
ORDER BY wma.Professionnalisme ASC, wma.Tempérament ASC
LIMIT 20;

COMMIT;

-- ===== SUCCESS MESSAGE =====
SELECT '✅ Migration Phase 8 completed successfully!' AS Status;
SELECT 'Mental attributes generated for ' || COUNT(*) || ' workers' AS Summary
FROM WorkerMentalAttributes;

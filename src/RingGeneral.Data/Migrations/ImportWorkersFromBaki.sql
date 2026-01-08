-- ============================================================================
-- IMPORT WORKERS - BAKI1.1.db vers nouveau système
-- Version: 1.0
-- Date: 2026-01-08
-- Description: Import complet des workers avec génération 30 attributs
-- ============================================================================

-- ============================================================================
-- ÉTAPE 1: BACKUP ET PRÉPARATION
-- ============================================================================

-- IMPORTANT: Faire un backup de la base actuelle avant d'exécuter ce script !
-- cp ring_general.db ring_general_backup_$(date +%Y%m%d_%H%M%S).db

-- ============================================================================
-- ÉTAPE 2: ATTACHER BAKI1.1.DB
-- ============================================================================

-- Attacher l'ancienne base de données en mode lecture seule
-- MODIFIER LE CHEMIN CI-DESSOUS SELON VOTRE INSTALLATION
ATTACH DATABASE 'BAKI1.1.db' AS legacy;

-- Vérifier que la connexion fonctionne
SELECT COUNT(*) AS WorkersCount FROM legacy.workers;

-- ============================================================================
-- ÉTAPE 3: IMPORT WORKERS (table principale)
-- ============================================================================

-- Import des workers avec mapping intelligent
INSERT INTO Workers (
    Name,
    RealName,
    Gender,
    Age,
    DateOfBirth,
    Height,
    Weight,

    -- Géographie (par défaut, sera enrichi manuellement)
    BirthCity,
    BirthCountry,
    ResidenceCity,
    ResidenceState,
    ResidenceCountry,

    -- Physique
    PhotoPath,
    Handedness,
    FightingStance,

    -- Gimmick & Push
    CurrentGimmick,
    Alignment,
    PushLevel,
    TvRole,
    BookingIntent,

    -- Career
    Experience,
    IsActive,
    IsInjured
)
SELECT
    -- Combinaison nom + prénom
    nom || ' ' || prenom AS Name,

    -- RealName (NULL par défaut, à enrichir manuellement)
    NULL AS RealName,

    -- Gender (déduction basique, à affiner)
    'Male' AS Gender,

    -- Age (estimation: 25-45 ans selon popularité)
    CASE
        WHEN popularite > 80 THEN 30 + (ABS(RANDOM()) % 10)  -- 30-39 ans (stars établies)
        WHEN popularite > 60 THEN 25 + (ABS(RANDOM()) % 15)  -- 25-39 ans (rising stars)
        WHEN popularite > 40 THEN 27 + (ABS(RANDOM()) % 13)  -- 27-39 ans (mid-carders)
        ELSE 24 + (ABS(RANDOM()) % 16)  -- 24-39 ans (jeunes/jobbers)
    END AS Age,

    -- DateOfBirth (NULL, sera calculé depuis Age si nécessaire)
    NULL AS DateOfBirth,

    -- Height (180cm par défaut, variation ±15cm)
    180 + (ABS(RANDOM()) % 31 - 15) AS Height,

    -- Weight (90kg par défaut, variation ±20kg)
    90 + (ABS(RANDOM()) % 41 - 20) AS Weight,

    -- Géographie par défaut (NULL, à enrichir)
    NULL AS BirthCity,
    NULL AS BirthCountry,
    NULL AS ResidenceCity,
    NULL AS ResidenceState,
    NULL AS ResidenceCountry,

    -- PhotoPath (NULL par défaut)
    NULL AS PhotoPath,

    -- Handedness (90% droitiers, 10% gauchers)
    CASE
        WHEN (ABS(RANDOM()) % 10) = 0 THEN 'Left'
        ELSE 'Right'
    END AS Handedness,

    -- FightingStance (85% Orthodox, 10% Southpaw, 5% Switch)
    CASE
        WHEN (ABS(RANDOM()) % 20) = 0 THEN 'Switch'
        WHEN (ABS(RANDOM()) % 10) = 0 THEN 'Southpaw'
        ELSE 'Orthodox'
    END AS FightingStance,

    -- CurrentGimmick (NULL par défaut, à enrichir)
    NULL AS CurrentGimmick,

    -- Alignment (déduction basée sur popularité)
    CASE
        WHEN popularite > 70 THEN 'Face'     -- Très populaire = Face
        WHEN popularite < 35 THEN 'Heel'     -- Impopulaire = Heel
        WHEN popularite BETWEEN 45 AND 55 THEN 'Tweener'  -- Neutre
        WHEN (ABS(RANDOM()) % 2) = 0 THEN 'Face'
        ELSE 'Heel'
    END AS Alignment,

    -- PushLevel (déduction depuis role_tv)
    CASE role_tv
        WHEN 'Main Event' THEN 'MainEvent'
        WHEN 'Upper Mid-Card' THEN 'UpperMid'
        WHEN 'Mid-Card' THEN 'MidCard'
        WHEN 'Lower Mid-Card' THEN 'LowerMid'
        WHEN 'Jobber' THEN 'Jobber'
        WHEN 'Enhancement Talent' THEN 'Jobber'
        ELSE 'MidCard'
    END AS PushLevel,

    -- TvRole (conversion role_tv en 0-100)
    CASE role_tv
        WHEN 'Main Event' THEN 85 + (ABS(RANDOM()) % 16)  -- 85-100
        WHEN 'Upper Mid-Card' THEN 70 + (ABS(RANDOM()) % 16)  -- 70-85
        WHEN 'Mid-Card' THEN 45 + (ABS(RANDOM()) % 21)  -- 45-65
        WHEN 'Lower Mid-Card' THEN 25 + (ABS(RANDOM()) % 21)  -- 25-45
        WHEN 'Jobber' THEN 10 + (ABS(RANDOM()) % 16)  -- 10-25
        ELSE 50
    END AS TvRole,

    -- BookingIntent (NULL par défaut)
    NULL AS BookingIntent,

    -- Experience (estimation depuis popularité + in_ring)
    CASE
        WHEN (in_ring + entertainment + story) / 3 > 80 THEN 10 + (ABS(RANDOM()) % 11)  -- 10-20 ans
        WHEN (in_ring + entertainment + story) / 3 > 60 THEN 5 + (ABS(RANDOM()) % 11)   -- 5-15 ans
        ELSE 1 + (ABS(RANDOM()) % 8)  -- 1-8 ans
    END AS Experience,

    -- IsActive (tous actifs par défaut)
    1 AS IsActive,

    -- IsInjured (basé sur blessure != 'Aucune')
    CASE
        WHEN blessure = 'Aucune' OR blessure IS NULL OR blessure = '' THEN 0
        ELSE 1
    END AS IsInjured

FROM legacy.workers
WHERE worker_id IS NOT NULL AND worker_id != '';

-- Afficher nombre de workers importés
SELECT COUNT(*) AS WorkersImported FROM Workers;

-- ============================================================================
-- ÉTAPE 4: GÉNÉRATION ATTRIBUTS IN-RING (10 attributs)
-- ============================================================================

-- Algorithme: Utiliser in_ring comme base, ajouter variation intelligente
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

    -- Striking (base + variation ±12)
    MAX(0, MIN(100, lw.in_ring + (ABS(RANDOM()) % 25 - 12))) AS Striking,

    -- Grappling (base + variation ±12)
    MAX(0, MIN(100, lw.in_ring + (ABS(RANDOM()) % 25 - 12))) AS Grappling,

    -- HighFlying (bonus si jeune âge, malus si lourd)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Age < 30 THEN 10 ELSE -5 END
        + CASE WHEN w.Weight > 110 THEN -15 ELSE 0 END
    )) AS HighFlying,

    -- Powerhouse (bonus si lourd, malus si léger)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Weight > 100 THEN 15 ELSE 0 END
        + CASE WHEN w.Weight < 80 THEN -10 ELSE 0 END
    )) AS Powerhouse,

    -- Timing (corrélé avec experience)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 10 THEN 10 ELSE 0 END
    )) AS Timing,

    -- Selling (base + variation)
    MAX(0, MIN(100, lw.in_ring + (ABS(RANDOM()) % 25 - 12))) AS Selling,

    -- Psychology (corrélé avec experience + age)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 8 THEN 12 ELSE 0 END
        + CASE WHEN w.Age > 35 THEN 8 ELSE 0 END
    )) AS Psychology,

    -- Stamina (malus si âgé, bonus si jeune)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Age < 28 THEN 10 ELSE 0 END
        + CASE WHEN w.Age > 38 THEN -12 ELSE 0 END
    )) AS Stamina,

    -- Safety (corrélé avec experience)
    MAX(0, MIN(100,
        lw.in_ring
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 7 THEN 15 ELSE 0 END
    )) AS Safety,

    -- HardcoreBrawl (variation large)
    MAX(0, MIN(100, lw.in_ring + (ABS(RANDOM()) % 31 - 15))) AS HardcoreBrawl

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

-- Afficher stats
SELECT COUNT(*) AS InRingAttributesCreated FROM WorkerInRingAttributes;

-- ============================================================================
-- ÉTAPE 5: GÉNÉRATION ATTRIBUTS ENTERTAINMENT (10 attributs)
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

    -- Charisma (corrélé avec popularité)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN lw.popularite > 75 THEN 15 ELSE 0 END
    )) AS Charisma,

    -- MicWork (base + variation)
    MAX(0, MIN(100, lw.entertainment + (ABS(RANDOM()) % 25 - 12))) AS MicWork,

    -- Acting (base + variation)
    MAX(0, MIN(100, lw.entertainment + (ABS(RANDOM()) % 25 - 12))) AS Acting,

    -- CrowdConnection (fortement corrélé avec popularité)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN lw.popularite > 80 THEN 20 ELSE 0 END
        + CASE WHEN lw.popularite < 30 THEN -15 ELSE 0 END
    )) AS CrowdConnection,

    -- StarPower (corrélé avec popularité + momentum)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN lw.popularite > 75 THEN 12 ELSE 0 END
        + CASE WHEN lw.momentum > 70 THEN 10 ELSE 0 END
    )) AS StarPower,

    -- Improvisation (corrélé avec experience)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 8 THEN 10 ELSE 0 END
    )) AS Improvisation,

    -- Entrance (base + variation)
    MAX(0, MIN(100, lw.entertainment + (ABS(RANDOM()) % 25 - 12))) AS Entrance,

    -- SexAppeal (bonus si jeune)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Age < 32 THEN 10 ELSE 0 END
    )) AS SexAppeal,

    -- MerchandiseAppeal (corrélé avec popularité)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN lw.popularite > 70 THEN 18 ELSE 0 END
    )) AS MerchandiseAppeal,

    -- CrossoverPotential (corrélé avec charisma estimé)
    MAX(0, MIN(100,
        lw.entertainment
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN lw.entertainment > 75 THEN 12 ELSE 0 END
    )) AS CrossoverPotential

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

SELECT COUNT(*) AS EntertainmentAttributesCreated FROM WorkerEntertainmentAttributes;

-- ============================================================================
-- ÉTAPE 6: GÉNÉRATION ATTRIBUTS STORY (10 attributs)
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

    -- CharacterDepth (corrélé avec experience)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 9 THEN 15 ELSE 0 END
    )) AS CharacterDepth,

    -- Consistency (corrélé avec experience + age)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 7 THEN 12 ELSE 0 END
        + CASE WHEN w.Age > 32 THEN 8 ELSE 0 END
    )) AS Consistency,

    -- HeelPerformance (bonus si Heel)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Alignment = 'Heel' THEN 20 ELSE 0 END
    )) AS HeelPerformance,

    -- BabyfacePerformance (bonus si Face)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Alignment = 'Face' THEN 20 ELSE 0 END
    )) AS BabyfacePerformance,

    -- StorytellingLongTerm (corrélé avec experience)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 8 THEN 15 ELSE 0 END
    )) AS StorytellingLongTerm,

    -- EmotionalRange (base + variation)
    MAX(0, MIN(100, lw.story + (ABS(RANDOM()) % 25 - 12))) AS EmotionalRange,

    -- Adaptability (corrélé avec experience)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 6 THEN 12 ELSE 0 END
    )) AS Adaptability,

    -- RivalryChemistry (base + variation)
    MAX(0, MIN(100, lw.story + (ABS(RANDOM()) % 25 - 12))) AS RivalryChemistry,

    -- CreativeInput (corrélé avec experience + popularité)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Experience > 10 THEN 15 ELSE 0 END
        + CASE WHEN lw.popularite > 75 THEN 10 ELSE 0 END
    )) AS CreativeInput,

    -- MoralAlignment (bonus si Tweener)
    MAX(0, MIN(100,
        lw.story
        + (ABS(RANDOM()) % 25 - 12)
        + CASE WHEN w.Alignment = 'Tweener' THEN 25 ELSE 0 END
    )) AS MoralAlignment

FROM Workers w
INNER JOIN legacy.workers lw ON w.Name = (lw.nom || ' ' || lw.prenom);

SELECT COUNT(*) AS StoryAttributesCreated FROM WorkerStoryAttributes;

-- ============================================================================
-- ÉTAPE 7: GÉNÉRATION SPÉCIALISATIONS
-- ============================================================================

-- Ajouter 1 spécialisation primaire basée sur attributs dominants
INSERT INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT
    w.Id AS WorkerId,

    -- Déterminer spécialisation dominante
    CASE
        -- Brawler: Striking + HardcoreBrawl dominants
        WHEN (wir.Striking + wir.HardcoreBrawl) > (wir.Grappling + wir.HighFlying + wir.Powerhouse) THEN 'Brawler'

        -- Technical: Grappling dominant
        WHEN wir.Grappling >= wir.HighFlying AND wir.Grappling >= wir.Powerhouse AND wir.Grappling >= wir.Striking THEN 'Technical'

        -- HighFlyer: HighFlying dominant
        WHEN wir.HighFlying >= wir.Powerhouse AND wir.HighFlying >= wir.Grappling AND wir.HighFlying >= wir.Striking THEN 'HighFlyer'

        -- Power: Powerhouse dominant
        WHEN wir.Powerhouse >= wir.Grappling AND wir.Powerhouse >= wir.HighFlying AND wir.Powerhouse >= wir.Striking THEN 'Power'

        -- Showman: Entertainment élevé
        WHEN wea.EntertainmentAvg > wir.InRingAvg + 10 THEN 'Showman'

        -- AllRounder par défaut
        ELSE 'AllRounder'
    END AS Specialization,

    1 AS Level  -- Primary

FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId;

SELECT COUNT(*) AS SpecializationsCreated FROM WorkerSpecializations;

-- ============================================================================
-- ÉTAPE 8: DÉTACHER BAKI1.1.DB
-- ============================================================================

DETACH DATABASE legacy;

-- ============================================================================
-- ÉTAPE 9: VALIDATION POST-IMPORT
-- ============================================================================

-- 1. Vérifier nombre total de workers
SELECT 'Workers importés:' AS Info, COUNT(*) AS Count FROM Workers;

-- 2. Vérifier que tous ont des attributs
SELECT 'Workers sans InRing:' AS Info, COUNT(*) AS Count
FROM Workers w LEFT JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
WHERE wir.WorkerId IS NULL;

SELECT 'Workers sans Entertainment:' AS Info, COUNT(*) AS Count
FROM Workers w LEFT JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
WHERE wea.WorkerId IS NULL;

SELECT 'Workers sans Story:' AS Info, COUNT(*) AS Count
FROM Workers w LEFT JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
WHERE wsa.WorkerId IS NULL;

-- 3. Afficher top 10 workers par Overall Rating
SELECT
    w.Name,
    wir.InRingAvg,
    wea.EntertainmentAvg,
    wsa.StoryAvg,
    (wir.InRingAvg + wea.EntertainmentAvg + wsa.StoryAvg) / 3 AS OverallRating,
    w.PushLevel,
    w.Alignment
FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
INNER JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
ORDER BY OverallRating DESC
LIMIT 10;

-- 4. Statistiques par PushLevel
SELECT
    w.PushLevel,
    COUNT(*) AS Count,
    AVG((wir.InRingAvg + wea.EntertainmentAvg + wsa.StoryAvg) / 3) AS AvgOverall
FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
INNER JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
GROUP BY w.PushLevel
ORDER BY AvgOverall DESC;

-- ============================================================================
-- TERMINÉ !
-- ============================================================================

-- L'import est terminé. Les workers sont prêts à être utilisés avec ProfileView.
-- Vous pouvez maintenant:
-- 1. Ouvrir ProfileView dans l'application
-- 2. Enrichir manuellement les données (géographie, photos, etc.)
-- 3. Ajuster les attributs si nécessaire

SELECT '✅ Import terminé avec succès !' AS Status;

-- Migration 003 : Refonte des Attributs de Performance
-- Ajout de 30 attributs granulaires pour remplacer InRing, Entertainment, Story
-- Date : 2026-01-07

-- ============================================================================
-- ÉTAPE 1 : AJOUT DES 30 NOUVELLES COLONNES
-- ============================================================================

-- IN-RING (10 attributs)
ALTER TABLE Workers ADD COLUMN Striking INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Grappling INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN HighFlying INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Powerhouse INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Timing INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Selling INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Psychology INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Stamina INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Safety INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN HardcoreBrawl INTEGER DEFAULT 50;

-- ENTERTAINMENT (10 attributs)
ALTER TABLE Workers ADD COLUMN Charisma INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN MicWork INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Acting INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN CrowdConnection INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN StarPower INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Improvisation INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Entrance INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN SexAppeal INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN MerchandiseAppeal INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN CrossoverPotential INTEGER DEFAULT 50;

-- STORY (10 attributs)
ALTER TABLE Workers ADD COLUMN CharacterDepth INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Consistency INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN HeelPerformance INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN BabyfacePerformance INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN StorytellingLongTerm INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN EmotionalRange INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN Adaptability INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN RivalryChemistry INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN CreativeInput INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN MoralAlignment INTEGER DEFAULT 50;

-- ============================================================================
-- ÉTAPE 2 : MIGRATION DES DONNÉES EXISTANTES
-- ============================================================================

-- Stratégie : Répartir le score global sur les 10 attributs de chaque catégorie
-- avec une variance aléatoire pour créer de la diversité

-- IN-RING : Basé sur le score InRing existant
UPDATE Workers
SET
    Striking = CAST(InRing + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    Grappling = CAST(InRing + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    HighFlying = CAST(InRing + (ABS(RANDOM()) % 30 - 15) AS INTEGER), -- Plus de variance (spécialisation)
    Powerhouse = CAST(InRing + (ABS(RANDOM()) % 30 - 15) AS INTEGER),
    Timing = CAST(InRing + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    Selling = CAST(InRing + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    Psychology = CAST(InRing + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    Stamina = CAST(InRing + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    Safety = CAST(InRing + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    HardcoreBrawl = CAST(InRing + (ABS(RANDOM()) % 30 - 15) AS INTEGER)
WHERE InRing > 0;

-- ENTERTAINMENT : Basé sur le score Entertainment existant
UPDATE Workers
SET
    Charisma = CAST(Entertainment + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    MicWork = CAST(Entertainment + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    Acting = CAST(Entertainment + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    CrowdConnection = CAST(Entertainment + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    StarPower = CAST(Entertainment + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    Improvisation = CAST(Entertainment + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    Entrance = CAST(Entertainment + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    SexAppeal = CAST(Entertainment + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    MerchandiseAppeal = CAST(Entertainment + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    CrossoverPotential = CAST(Entertainment + (ABS(RANDOM()) % 25 - 12) AS INTEGER)
WHERE Entertainment > 0;

-- STORY : Basé sur le score Story existant
UPDATE Workers
SET
    CharacterDepth = CAST(Story + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    Consistency = CAST(Story + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    HeelPerformance = CAST(Story + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    BabyfacePerformance = CAST(Story + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    StorytellingLongTerm = CAST(Story + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    EmotionalRange = CAST(Story + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    Adaptability = CAST(Story + (ABS(RANDOM()) % 15 - 7) AS INTEGER),
    RivalryChemistry = CAST(Story + (ABS(RANDOM()) % 10 - 5) AS INTEGER),
    CreativeInput = CAST(Story + (ABS(RANDOM()) % 20 - 10) AS INTEGER),
    MoralAlignment = CAST(50 + (ABS(RANDOM()) % 40 - 20) AS INTEGER) -- Neutre par défaut avec variance
WHERE Story > 0;

-- ============================================================================
-- ÉTAPE 3 : CONTRAINDRE LES VALEURS ENTRE 1 ET 100
-- ============================================================================

-- IN-RING
UPDATE Workers
SET
    Striking = MIN(100, MAX(1, Striking)),
    Grappling = MIN(100, MAX(1, Grappling)),
    HighFlying = MIN(100, MAX(1, HighFlying)),
    Powerhouse = MIN(100, MAX(1, Powerhouse)),
    Timing = MIN(100, MAX(1, Timing)),
    Selling = MIN(100, MAX(1, Selling)),
    Psychology = MIN(100, MAX(1, Psychology)),
    Stamina = MIN(100, MAX(1, Stamina)),
    Safety = MIN(100, MAX(1, Safety)),
    HardcoreBrawl = MIN(100, MAX(1, HardcoreBrawl));

-- ENTERTAINMENT
UPDATE Workers
SET
    Charisma = MIN(100, MAX(1, Charisma)),
    MicWork = MIN(100, MAX(1, MicWork)),
    Acting = MIN(100, MAX(1, Acting)),
    CrowdConnection = MIN(100, MAX(1, CrowdConnection)),
    StarPower = MIN(100, MAX(1, StarPower)),
    Improvisation = MIN(100, MAX(1, Improvisation)),
    Entrance = MIN(100, MAX(1, Entrance)),
    SexAppeal = MIN(100, MAX(1, SexAppeal)),
    MerchandiseAppeal = MIN(100, MAX(1, MerchandiseAppeal)),
    CrossoverPotential = MIN(100, MAX(1, CrossoverPotential));

-- STORY
UPDATE Workers
SET
    CharacterDepth = MIN(100, MAX(1, CharacterDepth)),
    Consistency = MIN(100, MAX(1, Consistency)),
    HeelPerformance = MIN(100, MAX(1, HeelPerformance)),
    BabyfacePerformance = MIN(100, MAX(1, BabyfacePerformance)),
    StorytellingLongTerm = MIN(100, MAX(1, StorytellingLongTerm)),
    EmotionalRange = MIN(100, MAX(1, EmotionalRange)),
    Adaptability = MIN(100, MAX(1, Adaptability)),
    RivalryChemistry = MIN(100, MAX(1, RivalryChemistry)),
    CreativeInput = MIN(100, MAX(1, CreativeInput)),
    MoralAlignment = MIN(100, MAX(1, MoralAlignment));

-- ============================================================================
-- ÉTAPE 4 : CRÉATION D'INDEX POUR OPTIMISER LES PERFORMANCES
-- ============================================================================

-- Index sur les attributs les plus utilisés pour les requêtes de tri/filtrage
CREATE INDEX IF NOT EXISTS idx_workers_striking ON Workers(Striking);
CREATE INDEX IF NOT EXISTS idx_workers_charisma ON Workers(Charisma);
CREATE INDEX IF NOT EXISTS idx_workers_character_depth ON Workers(CharacterDepth);

-- Index composites pour les moyennes (utilisés dans les calculs de simulation)
CREATE INDEX IF NOT EXISTS idx_workers_inring_avg ON Workers(Striking, Grappling, Timing);
CREATE INDEX IF NOT EXISTS idx_workers_entertainment_avg ON Workers(Charisma, MicWork, CrowdConnection);
CREATE INDEX IF NOT EXISTS idx_workers_story_avg ON Workers(CharacterDepth, Consistency, StorytellingLongTerm);

-- ============================================================================
-- NOTE : Les anciennes colonnes InRing, Entertainment, Story sont CONSERVÉES
-- pour compatibilité ascendante. Elles peuvent être supprimées dans une
-- migration future si nécessaire.
-- ============================================================================

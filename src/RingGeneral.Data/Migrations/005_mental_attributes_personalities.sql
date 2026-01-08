-- ============================================================================
-- Migration 005: Mental Attributes & Personalities System
-- Date: 2026-01-08
-- Description: Implements hidden mental attributes and visible personality labels
-- ============================================================================

-- Table des attributs mentaux (cachés, JAMAIS affichés au joueur)
CREATE TABLE IF NOT EXISTS MentalAttributes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL, -- 'Worker', 'Staff', 'Trainee'

    -- 10 Attributs Mentaux (0-20, cachés)
    Professionalism INTEGER DEFAULT 10 CHECK(Professionalism BETWEEN 0 AND 20),
    Ambition INTEGER DEFAULT 10 CHECK(Ambition BETWEEN 0 AND 20),
    Loyalty INTEGER DEFAULT 10 CHECK(Loyalty BETWEEN 0 AND 20),
    Ego INTEGER DEFAULT 10 CHECK(Ego BETWEEN 0 AND 20),
    Resilience INTEGER DEFAULT 10 CHECK(Resilience BETWEEN 0 AND 20),
    Adaptability INTEGER DEFAULT 10 CHECK(Adaptability BETWEEN 0 AND 20),
    Creativity INTEGER DEFAULT 10 CHECK(Creativity BETWEEN 0 AND 20),
    WorkEthic INTEGER DEFAULT 10 CHECK(WorkEthic BETWEEN 0 AND 20),
    SocialSkills INTEGER DEFAULT 10 CHECK(SocialSkills BETWEEN 0 AND 20),
    Temperament INTEGER DEFAULT 10 CHECK(Temperament BETWEEN 0 AND 20),

    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(EntityId, EntityType)
);

-- Table des personnalités visibles (labels FM-style)
CREATE TABLE IF NOT EXISTS Personalities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,

    -- Label de personnalité (FM-like, visible au joueur)
    PersonalityLabel TEXT NOT NULL DEFAULT 'Balanced',

    -- Traits secondaires (optionnel, max 2)
    SecondaryTrait1 TEXT,
    SecondaryTrait2 TEXT,

    -- Evolution tracking
    PreviousLabel TEXT,
    LabelChangedAt TEXT,

    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(EntityId, EntityType)
);

-- Table d'historique des changements de personnalité
CREATE TABLE IF NOT EXISTS PersonalityHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    OldLabel TEXT NOT NULL,
    NewLabel TEXT NOT NULL,
    ChangeReason TEXT, -- 'Success', 'Failure', 'Trauma', 'Growth'
    ChangedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Index pour performances
CREATE INDEX IF NOT EXISTS idx_mental_entity ON MentalAttributes(EntityId, EntityType);
CREATE INDEX IF NOT EXISTS idx_personality_entity ON Personalities(EntityId, EntityType);
CREATE INDEX IF NOT EXISTS idx_personality_history_entity ON PersonalityHistory(EntityId, EntityType);
CREATE INDEX IF NOT EXISTS idx_personality_history_date ON PersonalityHistory(ChangedAt DESC);

-- ============================================================================
-- Rollback Script (à exécuter manuellement si besoin)
-- ============================================================================
-- DROP TABLE IF EXISTS PersonalityHistory;
-- DROP TABLE IF EXISTS Personalities;
-- DROP TABLE IF EXISTS MentalAttributes;
-- DROP INDEX IF EXISTS idx_mental_entity;
-- DROP INDEX IF EXISTS idx_personality_entity;
-- DROP INDEX IF EXISTS idx_personality_history_entity;
-- DROP INDEX IF EXISTS idx_personality_history_date;

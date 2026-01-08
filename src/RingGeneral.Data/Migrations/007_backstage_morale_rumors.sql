-- Migration 007: Backstage Morale & Rumors System
-- Date: 2026-01-08

CREATE TABLE IF NOT EXISTS BackstageMorale (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL, -- Worker, Staff
    CompanyId TEXT NOT NULL,
    MoraleScore INTEGER DEFAULT 70 CHECK(MoraleScore BETWEEN 0 AND 100),
    PreviousMoraleScore INTEGER DEFAULT 70,
    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(EntityId, CompanyId)
);

CREATE TABLE IF NOT EXISTS CompanyMorale (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL UNIQUE,
    GlobalMoraleScore INTEGER DEFAULT 70 CHECK(GlobalMoraleScore BETWEEN 0 AND 100),
    WorkersMoraleAvg INTEGER DEFAULT 70,
    StaffMoraleAvg INTEGER DEFAULT 70,
    Trend TEXT DEFAULT 'Stable', -- Improving, Stable, Declining
    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Rumors (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    RumorType TEXT NOT NULL, -- Nepotism, UnfairPush, PayDisparity, Favoritism
    RumorText TEXT NOT NULL,
    Stage TEXT DEFAULT 'Emerging', -- Emerging, Growing, Widespread, Resolved, Ignored
    Severity INTEGER DEFAULT 1 CHECK(Severity BETWEEN 1 AND 5),
    AmplificationScore INTEGER DEFAULT 10 CHECK(AmplificationScore BETWEEN 0 AND 100),
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_morale_entity ON BackstageMorale(EntityId, CompanyId);
CREATE INDEX idx_morale_company ON CompanyMorale(CompanyId);
CREATE INDEX idx_rumors_company ON Rumors(CompanyId, Stage);

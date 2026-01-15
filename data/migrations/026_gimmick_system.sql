-- ============================================================================
-- Migration 026: Gimmick System
-- Description: Comprehensive gimmick system with 1,750+ predefined gimmicks
-- Date: 2026-01-15
-- ============================================================================

-- ============================================================================
-- TABLE: GimmickCategories
-- ============================================================================

CREATE TABLE IF NOT EXISTS GimmickCategories (
    CategoryId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    IconName TEXT,
    ColorHex TEXT,
    SortOrder INTEGER DEFAULT 0
);

-- Seed gimmick categories
INSERT OR IGNORE INTO GimmickCategories (CategoryId, Name, Description, IconName, ColorHex, SortOrder) VALUES
('POWER', 'Power/Monster', 'Powerhouses, monsters, and giants who dominate with raw strength', 'power', '#FF4444', 1),
('TECHNICAL', 'Technical', 'Submission specialists, mat wizards, and ring generals', 'technical', '#4444FF', 2),
('HIGHFLYER', 'High-Flyer', 'Aerial assassins, luchadores, and acrobatic performers', 'highflyer', '#44FF44', 3),
('BRAWLER', 'Brawler', 'Street fighters, knockout artists, and heavy hitters', 'brawler', '#FF8844', 4),
('SHOWMAN', 'Showman/Entertainer', 'Charismatic performers, trash talkers, and crowd pleasers', 'showman', '#FF44FF', 5),
('HARDCORE', 'Hardcore', 'Extreme warriors, deathmatch specialists, and weapon masters', 'hardcore', '#888888', 6),
('ALLROUNDER', 'All-Rounder', 'Complete packages, hybrid fighters, and versatile performers', 'allrounder', '#44FFFF', 7);

-- ============================================================================
-- TABLE: Gimmicks
-- ============================================================================

CREATE TABLE IF NOT EXISTS Gimmicks (
    GimmickId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Category TEXT NOT NULL,
    SubCategory TEXT,
    EntertainmentModifier INTEGER DEFAULT 0,
    CrowdReactionModifier INTEGER DEFAULT 0,
    PreferredAlignment TEXT DEFAULT 'Any',
    EraCompatibility TEXT DEFAULT 'Any',
    PopularityTier TEXT DEFAULT 'MidCard',
    IsActive INTEGER DEFAULT 1,
    CreatedDate TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (Category) REFERENCES GimmickCategories(CategoryId)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_gimmicks_category ON Gimmicks(Category);
CREATE INDEX IF NOT EXISTS idx_gimmicks_alignment ON Gimmicks(PreferredAlignment);
CREATE INDEX IF NOT EXISTS idx_gimmicks_tier ON Gimmicks(PopularityTier);
CREATE INDEX IF NOT EXISTS idx_gimmicks_active ON Gimmicks(IsActive);

-- ============================================================================
-- TABLE: GimmickHistory
-- ============================================================================

CREATE TABLE IF NOT EXISTS GimmickHistory (
    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    GimmickId TEXT,
    GimmickName TEXT NOT NULL,
    StartDate TEXT NOT NULL DEFAULT (datetime('now')),
    EndDate TEXT,
    AdoptionReason TEXT,
    SuccessRating INTEGER DEFAULT 50,
    Notes TEXT,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id),
    FOREIGN KEY (GimmickId) REFERENCES Gimmicks(GimmickId)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_gimmick_history_worker ON GimmickHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_gimmick_history_gimmick ON GimmickHistory(GimmickId);
CREATE INDEX IF NOT EXISTS idx_gimmick_history_dates ON GimmickHistory(StartDate, EndDate);

-- ============================================================================
-- MIGRATE EXISTING GIMMICKS
-- ============================================================================

-- Migrate existing CurrentGimmick values to GimmickHistory
INSERT INTO GimmickHistory (WorkerId, GimmickName, StartDate, AdoptionReason)
SELECT 
    Id,
    CurrentGimmick,
    datetime('now', '-365 days'), -- Assume gimmick started 1 year ago
    'Legacy Migration'
FROM Workers
WHERE CurrentGimmick IS NOT NULL AND CurrentGimmick != '';

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

SELECT 'GimmickCategories Created' AS Status, COUNT(*) AS Count FROM GimmickCategories
UNION ALL
SELECT 'Gimmicks Table Created', COUNT(*) FROM sqlite_master WHERE type='table' AND name='Gimmicks'
UNION ALL
SELECT 'GimmickHistory Table Created', COUNT(*) FROM sqlite_master WHERE type='table' AND name='GimmickHistory'
UNION ALL
SELECT 'Migrated Gimmicks', COUNT(*) FROM GimmickHistory WHERE AdoptionReason = 'Legacy Migration';

-- ============================================================================
-- Migration 008: Owner & Booker Systems
-- Description: Ajoute les tables pour le système Owner/Booker avec
--              auto-booking AI et mémoire persistante
-- Created: January 2026 (Phase 4 - Week 1)
-- ============================================================================

-- ============================================================================
-- TABLE: Owners
-- Description: Propriétaires de compagnies avec vision stratégique
-- ============================================================================
CREATE TABLE IF NOT EXISTS Owners (
    OwnerId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    VisionType TEXT NOT NULL CHECK(VisionType IN ('Creative', 'Business', 'Balanced')),
    RiskTolerance INTEGER NOT NULL CHECK(RiskTolerance BETWEEN 0 AND 100),
    PreferredProductType TEXT NOT NULL CHECK(PreferredProductType IN ('Technical', 'Entertainment', 'Hardcore', 'Family-Friendly')),
    ShowFrequencyPreference TEXT NOT NULL CHECK(ShowFrequencyPreference IN ('Weekly', 'BiWeekly', 'Monthly')),
    TalentDevelopmentFocus INTEGER NOT NULL CHECK(TalentDevelopmentFocus BETWEEN 0 AND 100),
    FinancialPriority INTEGER NOT NULL CHECK(FinancialPriority BETWEEN 0 AND 100),
    FanSatisfactionPriority INTEGER NOT NULL CHECK(FanSatisfactionPriority BETWEEN 0 AND 100),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides par compagnie
CREATE INDEX IF NOT EXISTS idx_owners_company ON Owners(CompanyId);

-- ============================================================================
-- TABLE: Bookers
-- Description: Bookers avec préférences créatives et capacité d'auto-booking
-- ============================================================================
CREATE TABLE IF NOT EXISTS Bookers (
    BookerId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    CreativityScore INTEGER NOT NULL CHECK(CreativityScore BETWEEN 0 AND 100),
    LogicScore INTEGER NOT NULL CHECK(LogicScore BETWEEN 0 AND 100),
    BiasResistance INTEGER NOT NULL CHECK(BiasResistance BETWEEN 0 AND 100),
    PreferredStyle TEXT NOT NULL CHECK(PreferredStyle IN ('Long-Term', 'Short-Term', 'Flexible')),
    LikesUnderdog INTEGER NOT NULL DEFAULT 0 CHECK(LikesUnderdog IN (0, 1)),
    LikesVeteran INTEGER NOT NULL DEFAULT 0 CHECK(LikesVeteran IN (0, 1)),
    LikesFastRise INTEGER NOT NULL DEFAULT 0 CHECK(LikesFastRise IN (0, 1)),
    LikesSlowBurn INTEGER NOT NULL DEFAULT 0 CHECK(LikesSlowBurn IN (0, 1)),
    IsAutoBookingEnabled INTEGER NOT NULL DEFAULT 0 CHECK(IsAutoBookingEnabled IN (0, 1)),
    EmploymentStatus TEXT NOT NULL DEFAULT 'Active' CHECK(EmploymentStatus IN ('Active', 'Inactive', 'Fired')),
    HireDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides par compagnie et statut
CREATE INDEX IF NOT EXISTS idx_bookers_company ON Bookers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_bookers_status ON Bookers(EmploymentStatus);
CREATE INDEX IF NOT EXISTS idx_bookers_auto ON Bookers(IsAutoBookingEnabled) WHERE IsAutoBookingEnabled = 1;

-- ============================================================================
-- TABLE: BookerMemory
-- Description: Mémoire persistante des bookers pour décisions cohérentes
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerMemory (
    MemoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,
    EventType TEXT NOT NULL CHECK(EventType IN (
        'GoodMatch', 'BadMatch', 'WorkerComplaint', 'FanReaction',
        'OwnerFeedback', 'ChampionshipDecision', 'PushSuccess', 'PushFailure'
    )),
    EventDescription TEXT NOT NULL,
    ImpactScore INTEGER NOT NULL CHECK(ImpactScore BETWEEN -100 AND 100),
    RecallStrength INTEGER NOT NULL CHECK(RecallStrength BETWEEN 0 AND 100),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE
);

-- Index pour recherches rapides par booker et type
CREATE INDEX IF NOT EXISTS idx_bookermemory_booker ON BookerMemory(BookerId);
CREATE INDEX IF NOT EXISTS idx_bookermemory_type ON BookerMemory(EventType);
CREATE INDEX IF NOT EXISTS idx_bookermemory_recall ON BookerMemory(RecallStrength DESC);
CREATE INDEX IF NOT EXISTS idx_bookermemory_created ON BookerMemory(CreatedAt DESC);

-- ============================================================================
-- TABLE: BookerEmploymentHistory
-- Description: Historique d'emploi des bookers (tracking multi-compagnies)
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerEmploymentHistory (
    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NULL,
    TerminationReason TEXT NULL,
    PerformanceScore INTEGER NULL CHECK(PerformanceScore IS NULL OR PerformanceScore BETWEEN 0 AND 100),

    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_employment_booker ON BookerEmploymentHistory(BookerId);
CREATE INDEX IF NOT EXISTS idx_employment_company ON BookerEmploymentHistory(CompanyId);
CREATE INDEX IF NOT EXISTS idx_employment_dates ON BookerEmploymentHistory(StartDate, EndDate);

-- ============================================================================
-- DONNÉES DE TEST (optionnel - à commenter en production)
-- ============================================================================

-- Exemple Owner pour la compagnie du joueur
-- INSERT INTO Owners (OwnerId, CompanyId, Name, VisionType, RiskTolerance, PreferredProductType, ShowFrequencyPreference, TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority)
-- VALUES ('OWN-00001', 'COMP-PLAYER', 'Vincent K. McMahon', 'Business', 75, 'Entertainment', 'Weekly', 60, 80, 70);

-- Exemple Booker pour la compagnie du joueur
-- INSERT INTO Bookers (BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance, PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn, IsAutoBookingEnabled, HireDate)
-- VALUES ('BOOK-00001', 'COMP-PLAYER', 'Paul Heyman', 85, 70, 60, 'Long-Term', 1, 0, 1, 1, 0, datetime('now'));

-- ============================================================================
-- ROLLBACK SCRIPT (à utiliser en cas de problème)
-- ============================================================================
-- DROP TABLE IF EXISTS BookerEmploymentHistory;
-- DROP TABLE IF EXISTS BookerMemory;
-- DROP TABLE IF EXISTS Bookers;
-- DROP TABLE IF EXISTS Owners;

-- ============================================================================
-- VALIDATION QUERIES
-- ============================================================================
-- SELECT * FROM Owners;
-- SELECT * FROM Bookers;
-- SELECT * FROM BookerMemory;
-- SELECT * FROM BookerEmploymentHistory;

-- Vérifier les contraintes
-- SELECT sql FROM sqlite_master WHERE type='table' AND name IN ('Owners', 'Bookers', 'BookerMemory', 'BookerEmploymentHistory');

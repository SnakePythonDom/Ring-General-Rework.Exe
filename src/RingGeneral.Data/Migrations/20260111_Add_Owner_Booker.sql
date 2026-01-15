-- ============================================================================
-- MIGRATION: 20260111_Add_Owner_Booker.sql
-- Description: Add Owner, Booker, BookerMemory, and BookerEmploymentHistory tables.
--              Also fix potential missing column in YouthStructures.
-- ============================================================================

BEGIN TRANSACTION;

-- ============================================================================
-- 1. Owners Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS Owners (
    OwnerId TEXT PRIMARY KEY,
    CompanyId TEXT,
    Name TEXT NOT NULL,
    VisionType TEXT,
    RiskTolerance INTEGER DEFAULT 50,
    PreferredProductType TEXT,
    ShowFrequencyPreference TEXT,
    TalentDevelopmentFocus INTEGER DEFAULT 50,
    FinancialPriority INTEGER DEFAULT 50,
    FanSatisfactionPriority INTEGER DEFAULT 50,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES companies(company_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_owners_company ON Owners(CompanyId);

-- ============================================================================
-- 2. Bookers Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS Bookers (
    BookerId TEXT PRIMARY KEY,
    CompanyId TEXT,
    Name TEXT NOT NULL,
    CreativityScore INTEGER DEFAULT 50,
    LogicScore INTEGER DEFAULT 50,
    BiasResistance INTEGER DEFAULT 50,
    PreferredStyle TEXT,
    LikesUnderdog INTEGER DEFAULT 0,
    LikesVeteran INTEGER DEFAULT 0,
    LikesFastRise INTEGER DEFAULT 0,
    LikesSlowBurn INTEGER DEFAULT 0,
    IsAutoBookingEnabled INTEGER DEFAULT 0,
    EmploymentStatus TEXT DEFAULT 'Active',
    HireDate TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES companies(company_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_bookers_company ON Bookers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_bookers_status ON Bookers(EmploymentStatus);

-- ============================================================================
-- 3. BookerMemory
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerMemory (
    MemoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,
    EventType TEXT NOT NULL,
    EventDescription TEXT,
    ImpactScore INTEGER NOT NULL,
    RecallStrength INTEGER NOT NULL,
    MatchType TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_booker_memory_booker ON BookerMemory(BookerId);

-- ============================================================================
-- 4. BookerEmploymentHistory
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerEmploymentHistory (
    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT,
    TerminationReason TEXT,
    PerformanceScore INTEGER,
    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_booker_history_booker ON BookerEmploymentHistory(BookerId);

-- ============================================================================
-- 5. Fix YouthStructures (Type column)
-- ============================================================================
-- Note: SQLite does not support IF NOT EXISTS for ADD COLUMN.
-- This command will fail if 'type' already exists.
-- Users should ignore error if column exists.
-- ALTER TABLE YouthStructures ADD COLUMN type TEXT DEFAULT 'Academy'; 
-- (Commented out to prevent script failure in strict transaction modes, uncomment if needed)

COMMIT;

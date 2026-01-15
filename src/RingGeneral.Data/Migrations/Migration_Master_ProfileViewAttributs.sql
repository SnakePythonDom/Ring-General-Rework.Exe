-- ============================================================================
-- Migration Master: ProfileView + Performance Attributes Rework
-- Version: 1.0
-- Date: 2026-01-07
-- Description: Complete database schema for 30 attributes + ProfileView
-- ============================================================================

-- ============================================================================
-- SECTION 1: PERFORMANCE ATTRIBUTES TABLES (3 tables)
-- ============================================================================

-- Table 1: Worker In-Ring Attributes (10 attributes)
CREATE TABLE IF NOT EXISTS WorkerInRingAttributes (
    WorkerId INTEGER PRIMARY KEY,

    -- Core Attributes (0-100 scale)
    Striking INTEGER DEFAULT 50 CHECK(Striking >= 0 AND Striking <= 100),
    Grappling INTEGER DEFAULT 50 CHECK(Grappling >= 0 AND Grappling <= 100),
    HighFlying INTEGER DEFAULT 50 CHECK(HighFlying >= 0 AND HighFlying <= 100),
    Powerhouse INTEGER DEFAULT 50 CHECK(Powerhouse >= 0 AND Powerhouse <= 100),
    Timing INTEGER DEFAULT 50 CHECK(Timing >= 0 AND Timing <= 100),
    Selling INTEGER DEFAULT 50 CHECK(Selling >= 0 AND Selling <= 100),
    Psychology INTEGER DEFAULT 50 CHECK(Psychology >= 0 AND Psychology <= 100),
    Stamina INTEGER DEFAULT 50 CHECK(Stamina >= 0 AND Stamina <= 100),
    Safety INTEGER DEFAULT 50 CHECK(Safety >= 0 AND Safety <= 100),
    HardcoreBrawl INTEGER DEFAULT 50 CHECK(HardcoreBrawl >= 0 AND HardcoreBrawl <= 100),

    -- Calculated Average (GENERATED COLUMN)
    InRingAvg INTEGER GENERATED ALWAYS AS (
        (Striking + Grappling + HighFlying + Powerhouse + Timing +
         Selling + Psychology + Stamina + Safety + HardcoreBrawl) / 10
    ) STORED,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

-- Table 2: Worker Entertainment Attributes (10 attributes)
CREATE TABLE IF NOT EXISTS WorkerEntertainmentAttributes (
    WorkerId INTEGER PRIMARY KEY,

    -- Core Attributes (0-100 scale)
    Charisma INTEGER DEFAULT 50 CHECK(Charisma >= 0 AND Charisma <= 100),
    MicWork INTEGER DEFAULT 50 CHECK(MicWork >= 0 AND MicWork <= 100),
    Acting INTEGER DEFAULT 50 CHECK(Acting >= 0 AND Acting <= 100),
    CrowdConnection INTEGER DEFAULT 50 CHECK(CrowdConnection >= 0 AND CrowdConnection <= 100),
    StarPower INTEGER DEFAULT 50 CHECK(StarPower >= 0 AND StarPower <= 100),
    Improvisation INTEGER DEFAULT 50 CHECK(Improvisation >= 0 AND Improvisation <= 100),
    Entrance INTEGER DEFAULT 50 CHECK(Entrance >= 0 AND Entrance <= 100),
    SexAppeal INTEGER DEFAULT 50 CHECK(SexAppeal >= 0 AND SexAppeal <= 100),
    MerchandiseAppeal INTEGER DEFAULT 50 CHECK(MerchandiseAppeal >= 0 AND MerchandiseAppeal <= 100),
    CrossoverPotential INTEGER DEFAULT 50 CHECK(CrossoverPotential >= 0 AND CrossoverPotential <= 100),

    -- Calculated Average (GENERATED COLUMN)
    EntertainmentAvg INTEGER GENERATED ALWAYS AS (
        (Charisma + MicWork + Acting + CrowdConnection + StarPower +
         Improvisation + Entrance + SexAppeal + MerchandiseAppeal + CrossoverPotential) / 10
    ) STORED,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

-- Table 3: Worker Story Attributes (10 attributes)
CREATE TABLE IF NOT EXISTS WorkerStoryAttributes (
    WorkerId INTEGER PRIMARY KEY,

    -- Core Attributes (0-100 scale)
    CharacterDepth INTEGER DEFAULT 50 CHECK(CharacterDepth >= 0 AND CharacterDepth <= 100),
    Consistency INTEGER DEFAULT 50 CHECK(Consistency >= 0 AND Consistency <= 100),
    HeelPerformance INTEGER DEFAULT 50 CHECK(HeelPerformance >= 0 AND HeelPerformance <= 100),
    BabyfacePerformance INTEGER DEFAULT 50 CHECK(BabyfacePerformance >= 0 AND BabyfacePerformance <= 100),
    StorytellingLongTerm INTEGER DEFAULT 50 CHECK(StorytellingLongTerm >= 0 AND StorytellingLongTerm <= 100),
    EmotionalRange INTEGER DEFAULT 50 CHECK(EmotionalRange >= 0 AND EmotionalRange <= 100),
    Adaptability INTEGER DEFAULT 50 CHECK(Adaptability >= 0 AND Adaptability <= 100),
    RivalryChemistry INTEGER DEFAULT 50 CHECK(RivalryChemistry >= 0 AND RivalryChemistry <= 100),
    CreativeInput INTEGER DEFAULT 50 CHECK(CreativeInput >= 0 AND CreativeInput <= 100),
    MoralAlignment INTEGER DEFAULT 50 CHECK(MoralAlignment >= 0 AND MoralAlignment <= 100),

    -- Calculated Average (GENERATED COLUMN)
    StoryAvg INTEGER GENERATED ALWAYS AS (
        (CharacterDepth + Consistency + HeelPerformance + BabyfacePerformance +
         StorytellingLongTerm + EmotionalRange + Adaptability + RivalryChemistry +
         CreativeInput + MoralAlignment) / 10
    ) STORED,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

-- ============================================================================
-- SECTION 2: PROFILEVIEW TABLES (7 tables)
-- ============================================================================

-- Table 4: Worker Specializations
CREATE TABLE IF NOT EXISTS WorkerSpecializations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    Specialization TEXT NOT NULL CHECK(Specialization IN (
        'Brawler', 'Technical', 'HighFlyer', 'Power',
        'Hardcore', 'Submission', 'Showman', 'AllRounder'
    )),
    Level INTEGER DEFAULT 1 CHECK(Level >= 1 AND Level <= 3), -- 1=Primary, 2=Secondary, 3=Tertiary

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE,
    UNIQUE(WorkerId, Specialization)
);

-- Table 5: Worker Relations (1-to-1 relations between workers)
CREATE TABLE IF NOT EXISTS WorkerRelations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId1 INTEGER NOT NULL,
    WorkerId2 INTEGER NOT NULL,
    RelationType TEXT NOT NULL CHECK(RelationType IN ('Amitie', 'Couple', 'Fraternite', 'Rivalite')),
    RelationStrength INTEGER DEFAULT 50 CHECK(RelationStrength >= 0 AND RelationStrength <= 100),
    Notes TEXT,
    IsPublic INTEGER DEFAULT 1, -- 1 = Kayfabe visible, 0 = Backstage only
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (WorkerId1) REFERENCES Workers(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkerId2) REFERENCES Workers(Id) ON DELETE CASCADE,
    CHECK(WorkerId1 < WorkerId2), -- Prevent duplicate reverse relations
    UNIQUE(WorkerId1, WorkerId2)
);

-- Table 6: Factions (Tag Teams, Trios, Factions)
CREATE TABLE IF NOT EXISTS Factions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    FactionType TEXT NOT NULL CHECK(FactionType IN ('TagTeam', 'Trio', 'Faction')),
    LeaderId INTEGER, -- Optional leader
    Status TEXT DEFAULT 'Active' CHECK(Status IN ('Active', 'Inactive', 'Disbanded')),
    CreatedWeek INTEGER NOT NULL,
    CreatedYear INTEGER NOT NULL,
    DisbandedWeek INTEGER,
    DisbandedYear INTEGER,

    FOREIGN KEY (LeaderId) REFERENCES Workers(Id) ON DELETE SET NULL
);

-- Table 7: Faction Members
CREATE TABLE IF NOT EXISTS FactionMembers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FactionId INTEGER NOT NULL,
    WorkerId INTEGER NOT NULL,
    JoinedWeek INTEGER NOT NULL,
    JoinedYear INTEGER NOT NULL,
    LeftWeek INTEGER,
    LeftYear INTEGER,

    FOREIGN KEY (FactionId) REFERENCES Factions(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE,
    UNIQUE(FactionId, WorkerId)
);

-- Table 8: Worker Notes
CREATE TABLE IF NOT EXISTS WorkerNotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    Category TEXT DEFAULT 'Other' CHECK(Category IN ('BookingIdeas', 'Personal', 'Injury', 'Other')),
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    ModifiedDate TEXT,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

-- Table 9: Contract History
CREATE TABLE IF NOT EXISTS ContractHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    WeeklySalary REAL NOT NULL CHECK(WeeklySalary >= 0),
    SigningBonus REAL DEFAULT 0 CHECK(SigningBonus >= 0),
    ContractType TEXT DEFAULT 'Exclusive' CHECK(ContractType IN ('Exclusive', 'PerAppearance', 'Developmental')),
    Status TEXT DEFAULT 'Active' CHECK(Status IN ('Active', 'Expired', 'Terminated')),

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

-- Table 10: Match History (if not already exists)
CREATE TABLE IF NOT EXISTS MatchHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    ShowId INTEGER,
    MatchDate TEXT NOT NULL,
    MatchType TEXT,
    OpponentId INTEGER,
    Result TEXT CHECK(Result IN ('Win', 'Loss', 'Draw', 'NoContest')),
    Rating INTEGER CHECK(Rating >= 0 AND Rating <= 100),
    Duration INTEGER, -- in minutes

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ShowId) REFERENCES Shows(Id) ON DELETE SET NULL,
    FOREIGN KEY (OpponentId) REFERENCES Workers(Id) ON DELETE SET NULL
);

-- Table 11: Title Reigns (if not already exists)
CREATE TABLE IF NOT EXISTS TitleReigns (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    TitleId INTEGER NOT NULL,
    WonDate TEXT NOT NULL,
    WonShowId INTEGER,
    LostDate TEXT,
    LostShowId INTEGER,
    DaysHeld INTEGER,
    ReignNumber INTEGER DEFAULT 1,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE,
    FOREIGN KEY (TitleId) REFERENCES Titles(Id) ON DELETE CASCADE,
    FOREIGN KEY (WonShowId) REFERENCES Shows(Id) ON DELETE SET NULL,
    FOREIGN KEY (LostShowId) REFERENCES Shows(Id) ON DELETE SET NULL
);

-- ============================================================================
-- SECTION 3: ALTER WORKERS TABLE (13 new columns)
-- ============================================================================

-- Geography columns
ALTER TABLE Workers ADD COLUMN BirthCity TEXT;
ALTER TABLE Workers ADD COLUMN BirthCountry TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCity TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceState TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCountry TEXT;

-- Physical attributes
ALTER TABLE Workers ADD COLUMN PhotoPath TEXT;
ALTER TABLE Workers ADD COLUMN Handedness TEXT DEFAULT 'Right' CHECK(Handedness IN ('Right', 'Left', 'Ambidextrous'));
ALTER TABLE Workers ADD COLUMN FightingStance TEXT DEFAULT 'Orthodox' CHECK(FightingStance IN ('Orthodox', 'Southpaw', 'Switch'));

-- Gimmick & Push columns
ALTER TABLE Workers ADD COLUMN CurrentGimmick TEXT;
ALTER TABLE Workers ADD COLUMN Alignment TEXT DEFAULT 'Face' CHECK(Alignment IN ('Face', 'Heel', 'Tweener'));
ALTER TABLE Workers ADD COLUMN PushLevel TEXT DEFAULT 'MidCard' CHECK(PushLevel IN ('MainEvent', 'UpperMid', 'MidCard', 'LowerMid', 'Jobber'));
ALTER TABLE Workers ADD COLUMN TvRole INTEGER DEFAULT 50 CHECK(TvRole >= 0 AND TvRole <= 100);
ALTER TABLE Workers ADD COLUMN BookingIntent TEXT;

-- ============================================================================
-- SECTION 4: INDEXES FOR PERFORMANCE
-- ============================================================================

-- Attributes tables (already have PRIMARY KEY on WorkerId)
CREATE INDEX IF NOT EXISTS idx_inring_avg ON WorkerInRingAttributes(InRingAvg);
CREATE INDEX IF NOT EXISTS idx_entertainment_avg ON WorkerEntertainmentAttributes(EntertainmentAvg);
CREATE INDEX IF NOT EXISTS idx_story_avg ON WorkerStoryAttributes(StoryAvg);

-- Specializations
CREATE INDEX IF NOT EXISTS idx_specializations_worker ON WorkerSpecializations(WorkerId);
CREATE INDEX IF NOT EXISTS idx_specializations_type ON WorkerSpecializations(Specialization);

-- Relations
CREATE INDEX IF NOT EXISTS idx_relations_worker1 ON WorkerRelations(WorkerId1);
CREATE INDEX IF NOT EXISTS idx_relations_worker2 ON WorkerRelations(WorkerId2);
CREATE INDEX IF NOT EXISTS idx_relations_type ON WorkerRelations(RelationType);

-- Factions
CREATE INDEX IF NOT EXISTS idx_factions_status ON Factions(Status);
CREATE INDEX IF NOT EXISTS idx_factions_leader ON Factions(LeaderId);

-- Faction Members
CREATE INDEX IF NOT EXISTS idx_faction_members_faction ON FactionMembers(FactionId);
CREATE INDEX IF NOT EXISTS idx_faction_members_worker ON FactionMembers(WorkerId);

-- Notes
CREATE INDEX IF NOT EXISTS idx_notes_worker ON WorkerNotes(WorkerId);
CREATE INDEX IF NOT EXISTS idx_notes_category ON WorkerNotes(Category);
CREATE INDEX IF NOT EXISTS idx_notes_created ON WorkerNotes(CreatedDate);

-- Contract History
CREATE INDEX IF NOT EXISTS idx_contract_history_worker ON ContractHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_contract_history_status ON ContractHistory(Status);

-- Match History
CREATE INDEX IF NOT EXISTS idx_match_history_worker ON MatchHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_match_history_show ON MatchHistory(ShowId);
CREATE INDEX IF NOT EXISTS idx_match_history_date ON MatchHistory(MatchDate);

-- Title Reigns
CREATE INDEX IF NOT EXISTS idx_title_reigns_worker ON TitleReigns(WorkerId);
CREATE INDEX IF NOT EXISTS idx_title_reigns_title ON TitleReigns(TitleId);

-- ============================================================================
-- SECTION 5: MIGRATION VALIDATION QUERIES
-- ============================================================================

-- Verify all tables were created
SELECT
    'WorkerInRingAttributes' AS TableName,
    COUNT(*) AS Exists
FROM sqlite_master
WHERE type='table' AND name='WorkerInRingAttributes'
UNION ALL
SELECT 'WorkerEntertainmentAttributes', COUNT(*) FROM sqlite_master WHERE type='table' AND name='WorkerEntertainmentAttributes'
UNION ALL
SELECT 'WorkerStoryAttributes', COUNT(*) FROM sqlite_master WHERE type='table' AND name='WorkerStoryAttributes'
UNION ALL
SELECT 'WorkerSpecializations', COUNT(*) FROM sqlite_master WHERE type='table' AND name='WorkerSpecializations'
UNION ALL
SELECT 'WorkerRelations', COUNT(*) FROM sqlite_master WHERE type='table' AND name='WorkerRelations'
UNION ALL
SELECT 'Factions', COUNT(*) FROM sqlite_master WHERE type='table' AND name='Factions'
UNION ALL
SELECT 'FactionMembers', COUNT(*) FROM sqlite_master WHERE type='table' AND name='FactionMembers'
UNION ALL
SELECT 'WorkerNotes', COUNT(*) FROM sqlite_master WHERE type='table' AND name='WorkerNotes'
UNION ALL
SELECT 'ContractHistory', COUNT(*) FROM sqlite_master WHERE type='table' AND name='ContractHistory'
UNION ALL
SELECT 'MatchHistory', COUNT(*) FROM sqlite_master WHERE type='table' AND name='MatchHistory'
UNION ALL
SELECT 'TitleReigns', COUNT(*) FROM sqlite_master WHERE type='table' AND name='TitleReigns';

-- ============================================================================
-- ROLLBACK SCRIPT (In case of issues)
-- ============================================================================
-- Uncomment the following lines to rollback this migration:
/*
DROP TABLE IF EXISTS TitleReigns;
DROP TABLE IF EXISTS MatchHistory;
DROP TABLE IF EXISTS ContractHistory;
DROP TABLE IF EXISTS WorkerNotes;
DROP TABLE IF EXISTS FactionMembers;
DROP TABLE IF EXISTS Factions;
DROP TABLE IF EXISTS WorkerRelations;
DROP TABLE IF EXISTS WorkerSpecializations;
DROP TABLE IF EXISTS WorkerStoryAttributes;
DROP TABLE IF EXISTS WorkerEntertainmentAttributes;
DROP TABLE IF EXISTS WorkerInRingAttributes;

-- Note: Cannot easily rollback ALTER TABLE statements in SQLite
-- Would need to recreate Workers table from scratch
*/

-- ============================================================================
-- END OF MIGRATION
-- ============================================================================

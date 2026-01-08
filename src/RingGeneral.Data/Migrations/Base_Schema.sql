-- ============================================================================
-- BASE_SCHEMA.SQL - Ring General Complete Database Schema
-- Version: 1.0.0
-- Date: 2026-01-08
-- Description: Unified database schema consolidating all tables and migrations
-- ============================================================================
--
-- This schema includes:
-- - Core game tables (companies, workers, shows, etc.)
-- - Performance attributes system (30 attributes across 3 categories)
-- - Mental attributes & personality system (10 mental attributes)
-- - ProfileView support tables (relations, factions, notes, etc.)
-- - Contract & negotiation system
-- - Youth development system
-- - Scouting & medical systems
-- - Finance tracking
-- - Backstage incidents
--
-- Note: This schema creates an EMPTY database structure.
-- Use separate scripts for:
-- - Initial data seeding (seed_data.sql)
-- - Importing from BAKI1.1.db (ImportWorkersFromBaki.sql)
-- ============================================================================

PRAGMA foreign_keys = ON;

BEGIN TRANSACTION;

-- ============================================================================
-- SECTION 1: CORE GAME TABLES
-- ============================================================================

-- Table: SaveGames (game save management)
CREATE TABLE IF NOT EXISTS SaveGames (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveName TEXT NOT NULL,
    CompanyId TEXT,
    CurrentWeek INTEGER NOT NULL DEFAULT 1,
    CurrentYear INTEGER NOT NULL DEFAULT 2025,
    IsActive INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastPlayedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_savegames_active ON SaveGames(IsActive);

-- Table: Countries (region grouping)
CREATE TABLE IF NOT EXISTS Countries (
    CountryId TEXT PRIMARY KEY,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL
);

-- Table: Regions (geographic regions)
CREATE TABLE IF NOT EXISTS Regions (
    RegionId TEXT PRIMARY KEY,
    CountryId TEXT NOT NULL,
    Name TEXT NOT NULL,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)
);

-- Seed default countries and regions for UI selections
INSERT OR IGNORE INTO Countries (CountryId, Code, Name) VALUES
    ('COUNTRY_USA', 'USA', 'United States'),
    ('COUNTRY_CAN', 'CAN', 'Canada'),
    ('COUNTRY_MEX', 'MEX', 'Mexico'),
    ('COUNTRY_JPN', 'JPN', 'Japan'),
    ('COUNTRY_UK', 'UK', 'United Kingdom'),
    ('COUNTRY_EUR', 'EUR', 'Europe'),
    ('COUNTRY_WLD', 'WLD', 'World');

INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name) VALUES
    ('REGION_USA', 'COUNTRY_USA', 'USA'),
    ('REGION_CAN', 'COUNTRY_CAN', 'Canada'),
    ('REGION_MEX', 'COUNTRY_MEX', 'Mexico'),
    ('REGION_JPN', 'COUNTRY_JPN', 'Japan'),
    ('REGION_UK', 'COUNTRY_UK', 'UK'),
    ('REGION_EUR', 'COUNTRY_EUR', 'Europe'),
    ('REGION_GLOBAL', 'COUNTRY_WLD', 'Global');

-- Table: companies
CREATE TABLE IF NOT EXISTS companies (
    company_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    region TEXT NOT NULL,
    prestige INTEGER NOT NULL,
    tresorerie REAL NOT NULL,
    audience_moyenne INTEGER NOT NULL,
    reach INTEGER NOT NULL
);

-- Table: Workers (main worker/wrestler table)
CREATE TABLE IF NOT EXISTS Workers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Legacy compatibility (TEXT id still used in many places)
    worker_id TEXT UNIQUE,

    -- Basic Info
    Name TEXT NOT NULL,
    FirstName TEXT,
    LastName TEXT,
    Gender TEXT DEFAULT 'M' CHECK(Gender IN ('M', 'F', 'Other')),
    BirthDate TEXT,
    Age INTEGER,

    -- Physical Attributes
    Height INTEGER, -- in cm
    Weight INTEGER, -- in kg
    PhotoPath TEXT,
    Handedness TEXT DEFAULT 'Right' CHECK(Handedness IN ('Right', 'Left', 'Ambidextrous')),
    FightingStance TEXT DEFAULT 'Orthodox' CHECK(FightingStance IN ('Orthodox', 'Southpaw', 'Switch')),

    -- Geography
    BirthCity TEXT,
    BirthCountry TEXT,
    ResidenceCity TEXT,
    ResidenceState TEXT,
    ResidenceCountry TEXT,

    -- Career Info
    Experience INTEGER DEFAULT 0, -- years of experience
    DebutYear INTEGER,
    company_id TEXT,

    -- Legacy aggregated attributes (0-100 scale) - deprecated, use detailed attributes instead
    in_ring INTEGER NOT NULL DEFAULT 50,
    entertainment INTEGER NOT NULL DEFAULT 50,
    story INTEGER NOT NULL DEFAULT 50,

    -- Game Stats
    popularite INTEGER NOT NULL DEFAULT 50,
    Popularity INTEGER DEFAULT 50, -- PascalCase alias
    fatigue INTEGER NOT NULL DEFAULT 0,
    blessure TEXT NOT NULL DEFAULT 'Aucune',
    momentum INTEGER NOT NULL DEFAULT 50,
    morale INTEGER NOT NULL DEFAULT 60,

    -- Gimmick & Push
    CurrentGimmick TEXT,
    Alignment TEXT DEFAULT 'Face' CHECK(Alignment IN ('Face', 'Heel', 'Tweener')),
    PushLevel TEXT DEFAULT 'MidCard' CHECK(PushLevel IN ('MainEvent', 'UpperMid', 'MidCard', 'LowerMid', 'Jobber')),
    role_tv TEXT NOT NULL DEFAULT 'MidCard',
    TvRole INTEGER DEFAULT 50 CHECK(TvRole >= 0 AND TvRole <= 100),
    BookingIntent TEXT,

    -- Personality Profile (Phase 8)
    PersonalityProfile TEXT DEFAULT NULL,
    PersonalityProfileDetectedAt TEXT DEFAULT NULL,

    -- Metadata
    IsRetired INTEGER DEFAULT 0,
    RetiredDate TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT
);

CREATE INDEX IF NOT EXISTS idx_workers_company ON Workers(company_id);
CREATE INDEX IF NOT EXISTS idx_workers_popularite ON Workers(popularite);
CREATE INDEX IF NOT EXISTS idx_workers_popularity ON Workers(Popularity);
CREATE INDEX IF NOT EXISTS idx_workers_alignment ON Workers(Alignment);
CREATE INDEX IF NOT EXISTS idx_workers_pushlevel ON Workers(PushLevel);
CREATE INDEX IF NOT EXISTS idx_workers_personality ON Workers(PersonalityProfile);

-- Note: Legacy snake_case table 'workers' is no longer created
-- SQLite is case-insensitive for table names, so 'Workers' == 'workers'
-- The PascalCase Workers table above handles all functionality

-- ============================================================================
-- SECTION 2: PERFORMANCE ATTRIBUTES SYSTEM (30 attributes)
-- ============================================================================

-- Table: WorkerInRingAttributes (10 in-ring attributes, 0-100 scale)
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

CREATE INDEX IF NOT EXISTS idx_inring_avg ON WorkerInRingAttributes(InRingAvg);

-- Table: WorkerEntertainmentAttributes (10 entertainment attributes, 0-100 scale)
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

CREATE INDEX IF NOT EXISTS idx_entertainment_avg ON WorkerEntertainmentAttributes(EntertainmentAvg);

-- Table: WorkerStoryAttributes (10 story/character attributes, 0-100 scale)
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

CREATE INDEX IF NOT EXISTS idx_story_avg ON WorkerStoryAttributes(StoryAvg);

-- ============================================================================
-- SECTION 3: MENTAL ATTRIBUTES & PERSONALITY SYSTEM (Phase 8)
-- ============================================================================

-- Table: WorkerMentalAttributes (10 mental attributes, 0-20 scale)
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
CREATE INDEX IF NOT EXISTS idx_mental_professionnalisme ON WorkerMentalAttributes(Professionnalisme);
CREATE INDEX IF NOT EXISTS idx_mental_egoisme ON WorkerMentalAttributes(Égoïsme);

-- ============================================================================
-- SECTION 4: PROFILEVIEW SUPPORT TABLES
-- ============================================================================

-- Table: WorkerSpecializations (Primary/Secondary/Tertiary wrestling styles)
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

CREATE INDEX IF NOT EXISTS idx_specializations_worker ON WorkerSpecializations(WorkerId);
CREATE INDEX IF NOT EXISTS idx_specializations_type ON WorkerSpecializations(Specialization);

-- Table: WorkerRelations (1-to-1 relations between workers)
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

CREATE INDEX IF NOT EXISTS idx_relations_worker1 ON WorkerRelations(WorkerId1);
CREATE INDEX IF NOT EXISTS idx_relations_worker2 ON WorkerRelations(WorkerId2);
CREATE INDEX IF NOT EXISTS idx_relations_type ON WorkerRelations(RelationType);

-- Table: Factions (Tag Teams, Trios, Factions)
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

CREATE INDEX IF NOT EXISTS idx_factions_status ON Factions(Status);
CREATE INDEX IF NOT EXISTS idx_factions_leader ON Factions(LeaderId);

-- Table: FactionMembers
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

CREATE INDEX IF NOT EXISTS idx_faction_members_faction ON FactionMembers(FactionId);
CREATE INDEX IF NOT EXISTS idx_faction_members_worker ON FactionMembers(WorkerId);

-- Table: WorkerNotes (user notes on workers)
CREATE TABLE IF NOT EXISTS WorkerNotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    Category TEXT DEFAULT 'Other' CHECK(Category IN ('BookingIdeas', 'Personal', 'Injury', 'Other')),
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    ModifiedDate TEXT,

    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_notes_worker ON WorkerNotes(WorkerId);
CREATE INDEX IF NOT EXISTS idx_notes_category ON WorkerNotes(Category);
CREATE INDEX IF NOT EXISTS idx_notes_created ON WorkerNotes(CreatedDate);

-- ============================================================================
-- SECTION 5: CONTRACTS & NEGOTIATIONS
-- ============================================================================

-- Table: contracts (active contracts)
CREATE TABLE IF NOT EXISTS contracts (
    contract_id TEXT PRIMARY KEY,
    worker_id TEXT NOT NULL,
    company_id TEXT NOT NULL,
    type TEXT NOT NULL DEFAULT 'exclusif',
    debut_semaine INTEGER NOT NULL DEFAULT 1,
    fin_semaine INTEGER NOT NULL,
    salaire REAL NOT NULL DEFAULT 0,
    bonus_show REAL NOT NULL DEFAULT 0,
    buyout REAL NOT NULL DEFAULT 0,
    non_compete_weeks INTEGER NOT NULL DEFAULT 0,
    auto_renew INTEGER NOT NULL DEFAULT 0,
    exclusif INTEGER NOT NULL DEFAULT 1,
    statut TEXT NOT NULL DEFAULT 'actif',
    created_week INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX IF NOT EXISTS idx_contracts_enddate ON contracts(fin_semaine);
CREATE INDEX IF NOT EXISTS idx_contracts_company ON contracts(company_id);
CREATE INDEX IF NOT EXISTS idx_contracts_worker ON contracts(worker_id);
CREATE INDEX IF NOT EXISTS idx_contracts_company_end ON contracts(company_id, fin_semaine);

-- Table: contract_offers (negotiation offers)
CREATE TABLE IF NOT EXISTS contract_offers (
    offer_id TEXT PRIMARY KEY,
    negotiation_id TEXT NOT NULL,
    worker_id TEXT NOT NULL,
    company_id TEXT NOT NULL,
    type TEXT NOT NULL,
    debut_semaine INTEGER NOT NULL,
    fin_semaine INTEGER NOT NULL,
    salaire REAL NOT NULL,
    bonus_show REAL NOT NULL,
    buyout REAL NOT NULL,
    non_compete_weeks INTEGER NOT NULL,
    auto_renew INTEGER NOT NULL,
    exclusif INTEGER NOT NULL,
    statut TEXT NOT NULL,
    created_week INTEGER NOT NULL,
    expiration_week INTEGER NOT NULL,
    parent_offer_id TEXT,
    est_ia INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_contract_offers_company ON contract_offers(company_id);
CREATE INDEX IF NOT EXISTS idx_contract_offers_worker ON contract_offers(worker_id);
CREATE INDEX IF NOT EXISTS idx_contract_offers_expiration ON contract_offers(expiration_week);

-- Table: contract_clauses
CREATE TABLE IF NOT EXISTS contract_clauses (
    clause_id INTEGER PRIMARY KEY AUTOINCREMENT,
    contract_id TEXT,
    offer_id TEXT,
    type TEXT NOT NULL,
    valeur TEXT NOT NULL
);

-- Table: negotiation_state
CREATE TABLE IF NOT EXISTS negotiation_state (
    negotiation_id TEXT PRIMARY KEY,
    worker_id TEXT NOT NULL,
    company_id TEXT NOT NULL,
    statut TEXT NOT NULL DEFAULT 'en_cours',
    last_offer_id TEXT,
    updated_week INTEGER NOT NULL DEFAULT 1
);

-- Table: ContractHistory (historical contract records)
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

CREATE INDEX IF NOT EXISTS idx_contract_history_worker ON ContractHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_contract_history_status ON ContractHistory(Status);

-- ============================================================================
-- SECTION 6: TITLES & CHAMPIONSHIPS
-- ============================================================================

-- Table: titles
CREATE TABLE IF NOT EXISTS titles (
    title_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    prestige INTEGER NOT NULL,
    detenteur_id TEXT,
    company_id TEXT
);

CREATE INDEX IF NOT EXISTS idx_titles_company ON titles(company_id);

-- Table: TitleReigns (championship reigns history)
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
    FOREIGN KEY (TitleId) REFERENCES titles(title_id) ON DELETE CASCADE,
    FOREIGN KEY (WonShowId) REFERENCES shows(show_id) ON DELETE SET NULL,
    FOREIGN KEY (LostShowId) REFERENCES shows(show_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_title_reigns_worker ON TitleReigns(WorkerId);
CREATE INDEX IF NOT EXISTS idx_title_reigns_title ON TitleReigns(TitleId);

-- ============================================================================
-- SECTION 7: SHOWS & SEGMENTS
-- ============================================================================

-- Table: shows
CREATE TABLE IF NOT EXISTS shows (
    show_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    region TEXT NOT NULL,
    duree INTEGER NOT NULL,
    compagnie_id TEXT NOT NULL,
    tv_deal_id TEXT,
    lieu TEXT NOT NULL DEFAULT '',
    diffusion TEXT NOT NULL DEFAULT ''
);

-- Table: tv_deals
CREATE TABLE IF NOT EXISTS tv_deals (
    tv_deal_id TEXT PRIMARY KEY,
    company_id TEXT NOT NULL,
    network_name TEXT NOT NULL,
    reach_bonus INTEGER NOT NULL DEFAULT 0,
    audience_cap INTEGER NOT NULL DEFAULT 100,
    audience_min INTEGER NOT NULL DEFAULT 0,
    base_revenue REAL NOT NULL DEFAULT 0,
    revenue_per_point REAL NOT NULL DEFAULT 0,
    penalty REAL NOT NULL DEFAULT 0,
    constraints TEXT NOT NULL DEFAULT ''
);

-- Table: segments (legacy)
CREATE TABLE IF NOT EXISTS segments (
    segment_id TEXT PRIMARY KEY,
    show_id TEXT NOT NULL,
    ordre INTEGER NOT NULL,
    type TEXT NOT NULL,
    duree INTEGER NOT NULL,
    participants_json TEXT NOT NULL,
    storyline_id TEXT,
    title_id TEXT,
    main_event INTEGER NOT NULL,
    intensite INTEGER NOT NULL,
    vainqueur_id TEXT,
    perdant_id TEXT
);

-- Table: ShowSegments (new)
CREATE TABLE IF NOT EXISTS ShowSegments (
    ShowSegmentId TEXT PRIMARY KEY,
    ShowId TEXT NOT NULL,
    OrderIndex INTEGER NOT NULL,
    SegmentType TEXT NOT NULL,
    DurationMinutes INTEGER NOT NULL,
    StorylineId TEXT,
    TitleId TEXT,
    IsMainEvent INTEGER NOT NULL DEFAULT 0,
    Intensity INTEGER NOT NULL DEFAULT 50,
    WinnerWorkerId TEXT,
    LoserWorkerId TEXT
);

-- Table: SegmentParticipants
CREATE TABLE IF NOT EXISTS SegmentParticipants (
    ShowSegmentId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Role TEXT,
    PRIMARY KEY (ShowSegmentId, WorkerId)
);

-- Table: SegmentSettings
CREATE TABLE IF NOT EXISTS SegmentSettings (
    ShowSegmentId TEXT NOT NULL,
    SettingKey TEXT NOT NULL,
    SettingValue TEXT NOT NULL,
    PRIMARY KEY (ShowSegmentId, SettingKey)
);

-- Table: match_types (library of available match types)
CREATE TABLE IF NOT EXISTS match_types (
    match_type_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    description TEXT,
    actif INTEGER NOT NULL,
    ordre INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_match_types_actif ON match_types(actif);

-- Table: segment_templates
CREATE TABLE IF NOT EXISTS segment_templates (
    template_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    type_segment TEXT NOT NULL,
    duree INTEGER NOT NULL,
    main_event INTEGER NOT NULL,
    intensite INTEGER NOT NULL,
    match_type_id TEXT
);

CREATE INDEX IF NOT EXISTS idx_segment_templates_type ON segment_templates(type_segment);

-- ============================================================================
-- SECTION 8: MATCH & SHOW HISTORY
-- ============================================================================

-- Table: MatchHistory
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
    FOREIGN KEY (ShowId) REFERENCES shows(show_id) ON DELETE SET NULL,
    FOREIGN KEY (OpponentId) REFERENCES Workers(Id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_match_history_worker ON MatchHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_match_history_show ON MatchHistory(ShowId);
CREATE INDEX IF NOT EXISTS idx_match_history_date ON MatchHistory(MatchDate);

-- Table: show_history
CREATE TABLE IF NOT EXISTS show_history (
    show_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    note INTEGER NOT NULL,
    audience INTEGER NOT NULL,
    resume TEXT NOT NULL
);

-- Table: audience_history
CREATE TABLE IF NOT EXISTS audience_history (
    show_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    audience INTEGER NOT NULL,
    reach INTEGER NOT NULL,
    show_score INTEGER NOT NULL,
    stars INTEGER NOT NULL,
    saturation INTEGER NOT NULL
);

-- Table: segment_history
CREATE TABLE IF NOT EXISTS segment_history (
    segment_id TEXT NOT NULL,
    show_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    note INTEGER NOT NULL,
    resume TEXT NOT NULL,
    details_json TEXT NOT NULL
);

-- ============================================================================
-- SECTION 9: STORYLINES
-- ============================================================================

-- Table: storylines
CREATE TABLE IF NOT EXISTS storylines (
    storyline_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    heat INTEGER NOT NULL
);

-- Table: storyline_participants
CREATE TABLE IF NOT EXISTS storyline_participants (
    storyline_id TEXT NOT NULL,
    worker_id TEXT NOT NULL,
    role TEXT NOT NULL DEFAULT 'principal'
);

-- ============================================================================
-- SECTION 10: BACKSTAGE & MORALE
-- ============================================================================

-- Table: backstage_incidents
CREATE TABLE IF NOT EXISTS backstage_incidents (
    incident_id TEXT PRIMARY KEY,
    company_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    type_id TEXT NOT NULL,
    titre TEXT NOT NULL,
    description TEXT NOT NULL,
    gravite INTEGER NOT NULL,
    workers_json TEXT NOT NULL
);

-- Table: disciplinary_actions
CREATE TABLE IF NOT EXISTS disciplinary_actions (
    action_id TEXT PRIMARY KEY,
    company_id TEXT NOT NULL,
    worker_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    type_id TEXT NOT NULL,
    gravite INTEGER NOT NULL,
    morale_delta INTEGER NOT NULL,
    notes TEXT NOT NULL,
    incident_id TEXT
);

-- Table: morale_history
CREATE TABLE IF NOT EXISTS morale_history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    worker_id TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    morale_avant INTEGER NOT NULL,
    morale_apres INTEGER NOT NULL,
    delta INTEGER NOT NULL,
    raison TEXT NOT NULL,
    incident_id TEXT,
    action_id TEXT
);

-- ============================================================================
-- SECTION 11: MEDICAL SYSTEM
-- ============================================================================

-- Table: Injuries
CREATE TABLE IF NOT EXISTS Injuries (
    InjuryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    Type TEXT NOT NULL,
    Severity INTEGER NOT NULL,
    StartDate INTEGER NOT NULL,
    EndDate INTEGER,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Notes TEXT
);

-- Table: MedicalNotes
CREATE TABLE IF NOT EXISTS MedicalNotes (
    MedicalNoteId INTEGER PRIMARY KEY AUTOINCREMENT,
    InjuryId INTEGER,
    WorkerId TEXT NOT NULL,
    Note TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table: RecoveryPlans
CREATE TABLE IF NOT EXISTS RecoveryPlans (
    RecoveryPlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    InjuryId INTEGER NOT NULL,
    WorkerId TEXT NOT NULL,
    StartDate INTEGER NOT NULL,
    TargetDate INTEGER NOT NULL,
    RecommendedRestWeeks INTEGER NOT NULL,
    RiskLevel TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'EN_COURS',
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- SECTION 12: YOUTH DEVELOPMENT
-- ============================================================================

-- Table: youth_structures
CREATE TABLE IF NOT EXISTS youth_structures (
    youth_id TEXT PRIMARY KEY,
    company_id TEXT NOT NULL,
    nom TEXT NOT NULL,
    type TEXT NOT NULL,
    region TEXT NOT NULL,
    budget_annuel INTEGER NOT NULL,
    capacite_max INTEGER NOT NULL,
    niveau_equipements INTEGER NOT NULL,
    qualite_coaching INTEGER NOT NULL,
    philosophie TEXT NOT NULL,
    actif INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_youth_company ON youth_structures(company_id);
CREATE INDEX IF NOT EXISTS idx_youth_region ON youth_structures(region);

-- Table: youth_trainees
CREATE TABLE IF NOT EXISTS youth_trainees (
    worker_id TEXT NOT NULL,
    youth_id TEXT NOT NULL,
    statut TEXT NOT NULL,
    semaine_inscription INTEGER,
    semaine_graduation INTEGER,
    PRIMARY KEY (worker_id, youth_id)
);

CREATE INDEX IF NOT EXISTS idx_youth_trainees_youth ON youth_trainees(youth_id);

-- Table: youth_programs
CREATE TABLE IF NOT EXISTS youth_programs (
    program_id TEXT PRIMARY KEY,
    youth_id TEXT NOT NULL,
    nom TEXT NOT NULL,
    duree_semaines INTEGER NOT NULL,
    focus TEXT
);

CREATE INDEX IF NOT EXISTS idx_youth_programs_youth ON youth_programs(youth_id);

-- Table: youth_staff_assignments
CREATE TABLE IF NOT EXISTS youth_staff_assignments (
    assignment_id INTEGER PRIMARY KEY AUTOINCREMENT,
    youth_id TEXT NOT NULL,
    worker_id TEXT NOT NULL,
    role TEXT NOT NULL,
    semaine_debut INTEGER
);

CREATE INDEX IF NOT EXISTS idx_youth_staff_youth ON youth_staff_assignments(youth_id);

-- Table: youth_generation_state
CREATE TABLE IF NOT EXISTS youth_generation_state (
    youth_id TEXT PRIMARY KEY,
    derniere_generation_semaine INTEGER
);

-- ============================================================================
-- SECTION 13: WORKER GENERATION & ATTRIBUTES
-- ============================================================================

-- Table: worker_attributes (legacy attribute storage)
CREATE TABLE IF NOT EXISTS worker_attributes (
    worker_id TEXT NOT NULL,
    attribut_id TEXT NOT NULL,
    valeur INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_worker_attributes_worker ON worker_attributes(worker_id);

-- Table: worker_generation_counters
CREATE TABLE IF NOT EXISTS worker_generation_counters (
    annee INTEGER NOT NULL,
    scope_type TEXT NOT NULL,
    scope_id TEXT NOT NULL,
    worker_type TEXT NOT NULL,
    count INTEGER NOT NULL,
    PRIMARY KEY (annee, scope_type, scope_id, worker_type)
);

-- Table: worker_generation_events
CREATE TABLE IF NOT EXISTS worker_generation_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    worker_id TEXT NOT NULL,
    worker_type TEXT NOT NULL,
    semaine INTEGER NOT NULL,
    youth_id TEXT,
    region TEXT NOT NULL,
    company_id TEXT
);

CREATE INDEX IF NOT EXISTS idx_generation_events_semaine ON worker_generation_events(semaine);

-- ============================================================================
-- SECTION 14: SCOUTING SYSTEM
-- ============================================================================

-- Table: scout_reports
CREATE TABLE IF NOT EXISTS scout_reports (
    report_id TEXT PRIMARY KEY,
    worker_id TEXT NOT NULL,
    nom TEXT NOT NULL,
    region TEXT,
    potentiel INTEGER NOT NULL DEFAULT 0,
    in_ring INTEGER NOT NULL DEFAULT 0,
    entertainment INTEGER NOT NULL DEFAULT 0,
    story INTEGER NOT NULL DEFAULT 0,
    resume TEXT,
    notes TEXT,
    semaine INTEGER NOT NULL,
    source TEXT
);

-- Table: scout_missions
CREATE TABLE IF NOT EXISTS scout_missions (
    mission_id TEXT PRIMARY KEY,
    titre TEXT NOT NULL,
    region TEXT,
    focus TEXT,
    progression INTEGER NOT NULL DEFAULT 0,
    objectif INTEGER NOT NULL DEFAULT 100,
    statut TEXT NOT NULL DEFAULT 'active',
    semaine_debut INTEGER NOT NULL,
    semaine_maj INTEGER NOT NULL
);

-- ============================================================================
-- SECTION 15: FINANCE TRACKING
-- ============================================================================

-- Table: finances (legacy)
CREATE TABLE IF NOT EXISTS finances (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    show_id TEXT NOT NULL,
    type TEXT NOT NULL,
    montant REAL NOT NULL,
    libelle TEXT NOT NULL,
    semaine INTEGER NOT NULL
);

-- Table: FinanceTransactions (new)
CREATE TABLE IF NOT EXISTS FinanceTransactions (
    FinanceTransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    ShowId TEXT,
    Date TEXT,
    Week INTEGER,
    Category TEXT NOT NULL,
    Amount REAL NOT NULL,
    Description TEXT
);

-- Table: CompanyBalanceSnapshots
CREATE TABLE IF NOT EXISTS CompanyBalanceSnapshots (
    CompanyBalanceSnapshotId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Balance REAL NOT NULL,
    Revenues REAL NOT NULL,
    Expenses REAL NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- SECTION 16: MISC TABLES
-- ============================================================================

-- Table: chimies (worker chemistry)
CREATE TABLE IF NOT EXISTS chimies (
    worker_a TEXT NOT NULL,
    worker_b TEXT NOT NULL,
    valeur INTEGER NOT NULL
);

-- Table: InboxItems (in-game messages)
CREATE TABLE IF NOT EXISTS InboxItems (
    InboxItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    Week INTEGER NOT NULL
);

-- Table: popularity_regionale (regional popularity tracking)
CREATE TABLE IF NOT EXISTS popularity_regionale (
    entity_type TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    region TEXT NOT NULL,
    valeur INTEGER NOT NULL,
    UNIQUE(entity_type, entity_id, region)
);

-- ============================================================================
-- SECTION 17: GAME SETTINGS
-- ============================================================================

-- Table: game_settings
CREATE TABLE IF NOT EXISTS game_settings (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    youth_generation_mode TEXT NOT NULL,
    world_generation_mode TEXT NOT NULL,
    semaine_pivot_annuelle INTEGER
);

-- Table: ui_table_settings (UI state persistence)
CREATE TABLE IF NOT EXISTS ui_table_settings (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    recherche TEXT,
    filtre_type TEXT,
    filtre_statut TEXT,
    colonnes_visibles TEXT,
    colonnes_ordre TEXT,
    tri_colonnes TEXT
);

COMMIT;

-- ============================================================================
-- VALIDATION QUERIES
-- ============================================================================

-- Count all tables created
SELECT
    '✅ Base Schema Created' AS Status,
    COUNT(*) AS TableCount
FROM sqlite_master
WHERE type='table';

-- List all created tables
SELECT
    name AS TableName,
    type AS Type
FROM sqlite_master
WHERE type='table'
ORDER BY name;

-- ============================================================================
-- END OF BASE SCHEMA
-- ============================================================================
--
-- Next Steps:
-- 1. Run seed_data.sql to populate initial data (companies, match types, etc.)
-- 2. Run ImportWorkersFromBaki.sql to import workers from BAKI1.1.db
-- 3. Mental attributes will be auto-generated during BAKI import
-- ============================================================================

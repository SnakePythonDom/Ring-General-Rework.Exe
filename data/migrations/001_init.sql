PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS SchemaVersion (
    Version INTEGER PRIMARY KEY,
    AppliedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Countries (
    CountryId TEXT PRIMARY KEY,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Regions (
    RegionId TEXT PRIMARY KEY,
    CountryId TEXT NOT NULL,
    Name TEXT NOT NULL,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)
);

CREATE TABLE IF NOT EXISTS Companies (
    CompanyId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CountryId TEXT,
    RegionId TEXT NOT NULL,
    Prestige INTEGER NOT NULL DEFAULT 0,
    Treasury REAL NOT NULL DEFAULT 0,
    AverageAudience INTEGER NOT NULL DEFAULT 0,
    Reach INTEGER NOT NULL DEFAULT 0,
    SimLevel INTEGER NOT NULL DEFAULT 0,
    LastSimulatedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS CompanyCustomization (
    CompanyCustomizationId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL UNIQUE,
    PrimaryColor TEXT,
    SecondaryColor TEXT,
    LogoPath TEXT,
    Notes TEXT,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS NetworkRelations (
    NetworkRelationId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    TargetCompanyId TEXT NOT NULL,
    RelationType TEXT NOT NULL,
    RelationValue INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (TargetCompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS Workers (
    WorkerId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    FirstName TEXT,
    LastName TEXT,
    RingName TEXT,
    CompanyId TEXT,
    Nationality TEXT NOT NULL,
    Gender TEXT,
    BirthDate TEXT,
    InRing INTEGER NOT NULL DEFAULT 0,
    Entertainment INTEGER NOT NULL DEFAULT 0,
    Story INTEGER NOT NULL DEFAULT 0,
    Popularity INTEGER NOT NULL DEFAULT 0,
    Fatigue INTEGER NOT NULL DEFAULT 0,
    InjuryStatus TEXT NOT NULL DEFAULT 'AUCUNE',
    Momentum INTEGER NOT NULL DEFAULT 0,
    RoleTv TEXT NOT NULL DEFAULT 'NONE',
    SimLevel INTEGER NOT NULL DEFAULT 0,
    LastSimulatedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS WorkerAttributes (
    WorkerId TEXT PRIMARY KEY,
    InRing INTEGER NOT NULL DEFAULT 0,
    Entertainment INTEGER NOT NULL DEFAULT 0,
    Story INTEGER NOT NULL DEFAULT 0,
    Popularity INTEGER NOT NULL DEFAULT 0,
    Stamina INTEGER NOT NULL DEFAULT 0,
    Charisma INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS WorkerPopularityByRegion (
    WorkerId TEXT NOT NULL,
    RegionId TEXT NOT NULL,
    Popularity INTEGER NOT NULL,
    LastUpdatedAt TEXT,
    PRIMARY KEY (WorkerId, RegionId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS Contracts (
    ContractId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    StartDate INTEGER,
    EndDate INTEGER NOT NULL,
    Salary REAL NOT NULL DEFAULT 0,
    IsExclusive INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS Titles (
    TitleId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Prestige INTEGER NOT NULL DEFAULT 0,
    HolderWorkerId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (HolderWorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS TitleReigns (
    TitleReignId INTEGER PRIMARY KEY AUTOINCREMENT,
    TitleId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    StartDate INTEGER NOT NULL,
    EndDate INTEGER,
    IsCurrent INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS Storylines (
    StorylineId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Heat INTEGER NOT NULL DEFAULT 0,
    SimLevel INTEGER NOT NULL DEFAULT 0,
    LastSimulatedAt TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS StorylineParticipants (
    StorylineId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Role TEXT,
    PRIMARY KEY (StorylineId, WorkerId),
    FOREIGN KEY (StorylineId) REFERENCES Storylines(StorylineId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS Shows (
    ShowId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Week INTEGER NOT NULL,
    RegionId TEXT NOT NULL,
    DurationMinutes INTEGER NOT NULL,
    Date TEXT,
    VenueId TEXT,
    TvDealId TEXT,
    SimLevel INTEGER NOT NULL DEFAULT 0,
    LastSimulatedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS ShowSegments (
    ShowSegmentId TEXT PRIMARY KEY,
    ShowId TEXT NOT NULL,
    OrderIndex INTEGER NOT NULL,
    SegmentType TEXT NOT NULL,
    DurationMinutes INTEGER NOT NULL,
    StorylineId TEXT,
    TitleId TEXT,
    IsMainEvent INTEGER NOT NULL DEFAULT 0,
    Intensity INTEGER NOT NULL DEFAULT 0,
    WinnerWorkerId TEXT,
    LoserWorkerId TEXT,
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId),
    FOREIGN KEY (StorylineId) REFERENCES Storylines(StorylineId),
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (WinnerWorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (LoserWorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS SegmentParticipants (
    ShowSegmentId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Role TEXT,
    PRIMARY KEY (ShowSegmentId, WorkerId),
    FOREIGN KEY (ShowSegmentId) REFERENCES ShowSegments(ShowSegmentId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS SegmentResults (
    SegmentResultId INTEGER PRIMARY KEY AUTOINCREMENT,
    ShowSegmentId TEXT NOT NULL,
    ShowId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Note INTEGER NOT NULL,
    Summary TEXT NOT NULL,
    Details TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShowSegmentId) REFERENCES ShowSegments(ShowSegmentId),
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId)
);

CREATE TABLE IF NOT EXISTS Injuries (
    InjuryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    Type TEXT NOT NULL,
    Severity INTEGER NOT NULL DEFAULT 0,
    StartDate INTEGER NOT NULL,
    EndDate INTEGER,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Notes TEXT,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS Fatigue (
    WorkerId TEXT PRIMARY KEY,
    Value INTEGER NOT NULL DEFAULT 0,
    LastUpdatedAt TEXT,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS FinanceTransactions (
    FinanceTransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    ShowId TEXT,
    Date TEXT,
    Week INTEGER,
    Category TEXT NOT NULL,
    Amount REAL NOT NULL,
    Description TEXT,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId)
);

CREATE TABLE IF NOT EXISTS TVDeals (
    TvDealId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    NetworkName TEXT NOT NULL,
    StartDate INTEGER,
    EndDate INTEGER,
    AudienceCap INTEGER NOT NULL DEFAULT 0,
    Revenue REAL NOT NULL DEFAULT 0,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS YouthStructures (
    YouthStructureId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Capacity INTEGER NOT NULL DEFAULT 0,
    CountryId TEXT,
    RegionId TEXT,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS YouthTrainees (
    YouthTraineeId TEXT PRIMARY KEY,
    YouthStructureId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    StartDate INTEGER NOT NULL,
    EndDate INTEGER,
    Status TEXT NOT NULL,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS Calendars (
    CalendarId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Date TEXT NOT NULL,
    Type TEXT NOT NULL,
    ReferenceId TEXT,
    Notes TEXT,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS Venues (
    VenueId TEXT PRIMARY KEY,
    CountryId TEXT,
    RegionId TEXT,
    Name TEXT NOT NULL,
    City TEXT,
    Capacity INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS InboxItems (
    InboxItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    Week INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Chimies (
    WorkerA TEXT NOT NULL,
    WorkerB TEXT NOT NULL,
    Value INTEGER NOT NULL,
    PRIMARY KEY (WorkerA, WorkerB),
    FOREIGN KEY (WorkerA) REFERENCES Workers(WorkerId),
    FOREIGN KEY (WorkerB) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS ShowHistory (
    ShowHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    ShowId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Note INTEGER NOT NULL,
    Audience INTEGER NOT NULL,
    Summary TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId)
);

CREATE INDEX IF NOT EXISTS idx_workers_name ON Workers(Name);
CREATE INDEX IF NOT EXISTS idx_workers_company ON Workers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_workers_nationality ON Workers(Nationality);
CREATE INDEX IF NOT EXISTS idx_contracts_company_end ON Contracts(CompanyId, EndDate);
CREATE INDEX IF NOT EXISTS idx_worker_popularity_region ON WorkerPopularityByRegion(WorkerId, RegionId);
CREATE INDEX IF NOT EXISTS idx_shows_company_date ON Shows(CompanyId, Date);
CREATE INDEX IF NOT EXISTS idx_titles_company ON Titles(CompanyId);

-- Migration 004: Owner & Booker Governance System
-- Crée les tables pour la gouvernance des compagnies (Owner + Booker + Memory)

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: Owners
-- Propriétaires de compagnie avec vision stratégique
-- ============================================================================
CREATE TABLE IF NOT EXISTS Owners (
    OwnerId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL CHECK(length(Name) >= 2 AND length(Name) <= 200),

    -- Vision stratégique
    VisionType TEXT NOT NULL CHECK(VisionType IN ('Creative', 'Business', 'Balanced')),
    RiskTolerance INTEGER NOT NULL CHECK(RiskTolerance >= 0 AND RiskTolerance <= 100),

    -- Préférences de produit
    PreferredProductType TEXT NOT NULL CHECK(PreferredProductType IN ('Technical', 'Entertainment', 'Hardcore', 'Family-Friendly')),
    ShowFrequencyPreference TEXT NOT NULL CHECK(ShowFrequencyPreference IN ('Weekly', 'BiWeekly', 'Monthly')),

    -- Priorités stratégiques (0-100)
    TalentDevelopmentFocus INTEGER NOT NULL CHECK(TalentDevelopmentFocus >= 0 AND TalentDevelopmentFocus <= 100),
    FinancialPriority INTEGER NOT NULL CHECK(FinancialPriority >= 0 AND FinancialPriority <= 100),
    FanSatisfactionPriority INTEGER NOT NULL CHECK(FanSatisfactionPriority >= 0 AND FanSatisfactionPriority <= 100),

    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherche rapide par compagnie
CREATE INDEX IF NOT EXISTS idx_owners_company ON Owners(CompanyId);

-- ============================================================================
-- TABLE: Bookers
-- Bookers avec préférences créatives et historique d'emploi
-- ============================================================================
CREATE TABLE IF NOT EXISTS Bookers (
    BookerId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL CHECK(length(Name) >= 2 AND length(Name) <= 200),

    -- Scores de compétence (0-100)
    CreativityScore INTEGER NOT NULL CHECK(CreativityScore >= 0 AND CreativityScore <= 100),
    LogicScore INTEGER NOT NULL CHECK(LogicScore >= 0 AND LogicScore <= 100),
    BiasResistance INTEGER NOT NULL CHECK(BiasResistance >= 0 AND BiasResistance <= 100),

    -- Préférences de booking
    PreferredStyle TEXT NOT NULL CHECK(PreferredStyle IN ('Long-Term', 'Short-Term', 'Flexible')),
    LikesUnderdog INTEGER NOT NULL DEFAULT 0 CHECK(LikesUnderdog IN (0, 1)),
    LikesVeteran INTEGER NOT NULL DEFAULT 0 CHECK(LikesVeteran IN (0, 1)),
    LikesFastRise INTEGER NOT NULL DEFAULT 0 CHECK(LikesFastRise IN (0, 1)),
    LikesSlowBurn INTEGER NOT NULL DEFAULT 0 CHECK(LikesSlowBurn IN (0, 1)),

    -- Configuration
    IsAutoBookingEnabled INTEGER NOT NULL DEFAULT 0 CHECK(IsAutoBookingEnabled IN (0, 1)),
    EmploymentStatus TEXT NOT NULL CHECK(EmploymentStatus IN ('Active', 'Inactive', 'Fired')),

    -- Dates
    HireDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherche rapide
CREATE INDEX IF NOT EXISTS idx_bookers_company ON Bookers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_bookers_status ON Bookers(EmploymentStatus);

-- ============================================================================
-- TABLE: BookerMemory
-- Système de mémoire du Booker (événements marquants)
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerMemory (
    MemoryId TEXT PRIMARY KEY,
    BookerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    WorkerId TEXT,  -- Worker concerné (nullable si événement global)

    -- Type d'événement
    EventType TEXT NOT NULL CHECK(EventType IN (
        'GoodMatch', 'BadMatch', 'InjuryDuringMatch', 'WorkerComplaint',
        'ContractNegotiation', 'TitleChange', 'FeudSuccess', 'FeudFailure',
        'BehavioralIssue', 'PositiveFeedback', 'NegativeFeedback', 'Other'
    )),

    -- Impact et rappel
    ImpactScore INTEGER NOT NULL CHECK(ImpactScore >= -100 AND ImpactScore <= 100),
    RecallStrength INTEGER NOT NULL DEFAULT 100 CHECK(RecallStrength >= 0 AND RecallStrength <= 100),

    -- Détails
    Description TEXT,
    MatchId TEXT,  -- Référence optionnelle à un match
    ShowId TEXT,   -- Référence optionnelle à un show

    EventDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour requêtes fréquentes
CREATE INDEX IF NOT EXISTS idx_booker_memory_booker ON BookerMemory(BookerId);
CREATE INDEX IF NOT EXISTS idx_booker_memory_worker ON BookerMemory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_booker_memory_event_type ON BookerMemory(EventType);
CREATE INDEX IF NOT EXISTS idx_booker_memory_date ON BookerMemory(EventDate DESC);

-- ============================================================================
-- TABLE: BookerEmploymentHistory
-- Historique d'emploi des Bookers (tracking multi-compagnies)
-- ============================================================================
CREATE TABLE IF NOT EXISTS BookerEmploymentHistory (
    HistoryId TEXT PRIMARY KEY,
    BookerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,

    -- Période d'emploi
    StartDate TEXT NOT NULL,
    EndDate TEXT,  -- NULL si emploi actuel

    -- Raison de départ
    ReasonForLeaving TEXT CHECK(ReasonForLeaving IN (
        'Fired', 'Resigned', 'BetterOffer', 'Retired', 'Still-Employed'
    )),

    -- Performance pendant l'emploi
    PerformanceScore INTEGER CHECK(PerformanceScore >= 0 AND PerformanceScore <= 100),
    ShowsBooked INTEGER NOT NULL DEFAULT 0,
    SuccessfulStorylines INTEGER NOT NULL DEFAULT 0,
    FailedStorylines INTEGER NOT NULL DEFAULT 0,

    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour historique
CREATE INDEX IF NOT EXISTS idx_booker_employment_booker ON BookerEmploymentHistory(BookerId);
CREATE INDEX IF NOT EXISTS idx_booker_employment_company ON BookerEmploymentHistory(CompanyId);
CREATE INDEX IF NOT EXISTS idx_booker_employment_current ON BookerEmploymentHistory(EndDate) WHERE EndDate IS NULL;

-- ============================================================================
-- Données de référence (exemples de profils types)
-- ============================================================================

-- Ces données ne sont PAS insérées automatiquement pour éviter les conflits
-- Elles doivent être créées lors de la création d'une nouvelle compagnie

-- Exemple de requête de création d'Owner :
-- INSERT INTO Owners (OwnerId, CompanyId, Name, VisionType, RiskTolerance,
--                     PreferredProductType, ShowFrequencyPreference,
--                     TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority)
-- VALUES ('OWN_' || hex(randomblob(16)), 'COMP_ID', 'Nom Owner', 'Balanced', 50,
--         'Entertainment', 'Weekly', 50, 50, 50);

-- Exemple de requête de création de Booker :
-- INSERT INTO Bookers (BookerId, CompanyId, Name, CreativityScore, LogicScore,
--                      BiasResistance, PreferredStyle, EmploymentStatus, HireDate)
-- VALUES ('BOOK_' || hex(randomblob(16)), 'COMP_ID', 'Nom Booker', 60, 70,
--         60, 'Flexible', 'Active', datetime('now'));

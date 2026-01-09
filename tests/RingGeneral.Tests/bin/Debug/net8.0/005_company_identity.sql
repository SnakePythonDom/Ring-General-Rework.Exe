-- Migration 005: Company Identity & Era System
-- Ajoute les champs d'identité et contexte historique aux compagnies

PRAGMA foreign_keys = ON;

-- ============================================================================
-- EXTENSION DE LA TABLE COMPANIES
-- Ajoute les champs pour l'identité de la compagnie
-- ============================================================================

-- Année de fondation (ex: 1996, 2010, 2024)
ALTER TABLE Companies ADD COLUMN FoundedYear INTEGER DEFAULT 2024 CHECK(FoundedYear >= 1950 AND FoundedYear <= 2100);

-- Taille de la compagnie (impact sur le reach, les budgets, etc.)
ALTER TABLE Companies ADD COLUMN CompanySize TEXT DEFAULT 'Local' CHECK(CompanySize IN (
    'Local',      -- Promotion locale (1 ville/région)
    'Regional',   -- Plusieurs régions
    'National',   -- Échelle nationale
    'International', -- Multi-pays
    'Global'      -- Mondiale (WWE, AEW level)
));

-- Era actuelle (timeline narrative de la compagnie)
ALTER TABLE Companies ADD COLUMN CurrentEra TEXT DEFAULT 'Foundation Era' CHECK(length(CurrentEra) <= 100);

-- Référence au style de catch dominant (FK vers CatchStyles créée en migration 006)
ALTER TABLE Companies ADD COLUMN CatchStyleId TEXT;

-- Indicateur si la compagnie est contrôlée par le joueur
ALTER TABLE Companies ADD COLUMN IsPlayerControlled INTEGER DEFAULT 0 CHECK(IsPlayerControlled IN (0, 1));

-- Budget annuel (burn rate mensuel)
ALTER TABLE Companies ADD COLUMN MonthlyBurnRate REAL DEFAULT 0.0 CHECK(MonthlyBurnRate >= 0);

-- ============================================================================
-- NOUVELLE TABLE: CompanyEras
-- Historique des eras de la compagnie (similaire aux Attitude Era, Ruthless Aggression, etc.)
-- ============================================================================

CREATE TABLE IF NOT EXISTS CompanyEras (
    EraId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    EraName TEXT NOT NULL CHECK(length(EraName) >= 2 AND length(EraName) <= 100),

    -- Période
    StartWeek INTEGER NOT NULL,
    EndWeek INTEGER,  -- NULL si era actuelle

    -- Caractéristiques de l'era
    Description TEXT,
    KeyWorkers TEXT,  -- JSON array des workers emblématiques
    MajorStorylines TEXT,  -- JSON array des storylines majeures

    -- Statistiques de l'era
    AverageRating REAL DEFAULT 0.0,
    PeakAudience INTEGER DEFAULT 0,
    TitlesDefended INTEGER DEFAULT 0,

    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_company_eras_company ON CompanyEras(CompanyId);
CREATE INDEX IF NOT EXISTS idx_company_eras_current ON CompanyEras(EndWeek) WHERE EndWeek IS NULL;

-- ============================================================================
-- NOUVELLE TABLE: CompanyMilestones
-- Jalons importants de l'histoire de la compagnie
-- ============================================================================

CREATE TABLE IF NOT EXISTS CompanyMilestones (
    MilestoneId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,

    -- Type de jalon
    MilestoneType TEXT NOT NULL CHECK(MilestoneType IN (
        'FirstShow', 'FirstTitle', 'First100kAudience', 'First1MAudience',
        'FirstPPV', 'FirstTVDeal', 'CompanyMerger', 'CompanyAcquisition',
        'LegendaryMatch', 'HistoricMoment', 'Championship', 'Custom'
    )),

    Title TEXT NOT NULL CHECK(length(Title) >= 3 AND length(Title) <= 200),
    Description TEXT,

    -- Contexte
    Week INTEGER NOT NULL,
    EventDate TEXT NOT NULL,
    WorkersInvolved TEXT,  -- JSON array des workers impliqués
    ShowId TEXT,  -- Référence optionnelle à un show

    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_company_milestones_company ON CompanyMilestones(CompanyId);
CREATE INDEX IF NOT EXISTS idx_company_milestones_type ON CompanyMilestones(MilestoneType);
CREATE INDEX IF NOT EXISTS idx_company_milestones_date ON CompanyMilestones(EventDate DESC);

-- ============================================================================
-- MIGRATION DES DONNÉES EXISTANTES
-- Applique les valeurs par défaut aux compagnies existantes
-- ============================================================================

-- Toutes les compagnies existantes reçoivent les valeurs par défaut
UPDATE Companies
SET
    FoundedYear = 2024,
    CompanySize = 'Local',
    CurrentEra = 'Foundation Era',
    IsPlayerControlled = 0,
    MonthlyBurnRate = 0.0
WHERE FoundedYear IS NULL;

-- Si une compagnie a déjà du prestige/reach, ajuster sa taille
UPDATE Companies
SET CompanySize = CASE
    WHEN Reach >= 10000000 THEN 'Global'
    WHEN Reach >= 1000000 THEN 'International'
    WHEN Reach >= 100000 THEN 'National'
    WHEN Reach >= 10000 THEN 'Regional'
    ELSE 'Local'
END
WHERE CompanySize = 'Local' AND Reach > 0;

-- ============================================================================
-- VUES UTILITAIRES
-- ============================================================================

-- Vue combinant Company + Owner + Booker (pour le Company Hub)
CREATE VIEW IF NOT EXISTS vw_CompanyGovernance AS
SELECT
    c.CompanyId,
    c.Name AS CompanyName,
    c.CountryId,
    c.RegionId,
    c.FoundedYear,
    c.CompanySize,
    c.CurrentEra,
    c.CatchStyleId,
    c.Prestige,
    c.Treasury,
    c.AverageAudience,
    c.Reach,
    c.IsPlayerControlled,
    c.MonthlyBurnRate,

    -- Owner info
    o.OwnerId,
    o.Name AS OwnerName,
    o.VisionType,
    o.RiskTolerance,
    o.PreferredProductType,
    o.ShowFrequencyPreference,
    o.TalentDevelopmentFocus,
    o.FinancialPriority,
    o.FanSatisfactionPriority,

    -- Booker info
    b.BookerId,
    b.Name AS BookerName,
    b.CreativityScore,
    b.LogicScore,
    b.BiasResistance,
    b.PreferredStyle AS BookerPreferredStyle,
    b.EmploymentStatus,
    b.IsAutoBookingEnabled,
    b.HireDate

FROM Companies c
LEFT JOIN Owners o ON c.CompanyId = o.CompanyId
LEFT JOIN Bookers b ON c.CompanyId = b.CompanyId AND b.EmploymentStatus = 'Active';

-- Vue des compagnies rivales (toutes sauf celle du joueur)
CREATE VIEW IF NOT EXISTS vw_RivalCompanies AS
SELECT
    CompanyId,
    Name,
    CountryId,
    RegionId,
    FoundedYear,
    CompanySize,
    CurrentEra,
    Prestige,
    Reach,
    AverageAudience
FROM Companies
WHERE IsPlayerControlled = 0
ORDER BY Prestige DESC, Reach DESC;

-- ============================================================================
-- TRIGGERS
-- ============================================================================

-- Trigger : Créer automatiquement une première Era lors de la création d'une Company
CREATE TRIGGER IF NOT EXISTS trg_create_initial_era
AFTER INSERT ON Companies
BEGIN
    INSERT INTO CompanyEras (
        EraId,
        CompanyId,
        EraName,
        StartWeek,
        Description
    )
    VALUES (
        'ERA_' || hex(randomblob(16)),
        NEW.CompanyId,
        'Foundation Era',
        1,
        'L''ère fondatrice de ' || NEW.Name
    );
END;

-- Trigger : Créer un milestone "FirstShow" lors du premier show
-- (sera implémenté dans le code applicatif)

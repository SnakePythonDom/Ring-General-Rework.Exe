-- populate_test_data.sql
PRAGMA foreign_keys = OFF;

-- =============================================================================
-- PART 0: ENSURE TABLES EXIST
-- =============================================================================

-- Base Schema (Migration 001 extracts)
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
    FoundedYear INTEGER DEFAULT 2024,
    IsPlayerControlled INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
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

CREATE TABLE IF NOT EXISTS FinanceTransactions (
    FinanceTransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    ShowId TEXT,
    Date TEXT,
    Week INTEGER,
    Category TEXT NOT NULL,
    Amount REAL NOT NULL,
    Description TEXT,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
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

-- Staff Tables (Migration 010 extracts)
CREATE TABLE IF NOT EXISTS StaffMembers (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    BrandId TEXT NULL,
    Name TEXT NOT NULL,
    Role TEXT NOT NULL,
    Department TEXT NOT NULL,
    ExpertiseLevel TEXT NOT NULL,
    YearsOfExperience INTEGER DEFAULT 0,
    SkillScore INTEGER NOT NULL,
    PersonalityScore INTEGER DEFAULT 50,
    AnnualSalary REAL DEFAULT 0,
    HireDate TEXT NOT NULL,
    ContractEndDate TEXT NULL,
    EmploymentStatus TEXT DEFAULT 'Active',
    IsActive INTEGER DEFAULT 1,
    Notes TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS CreativeStaff (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    BookerId TEXT NULL,
    CreativityScore INTEGER,
    ConsistencyScore INTEGER,
    PreferredStyle TEXT,
    WorkerBias TEXT,
    LongTermStorylinePreference INTEGER,
    CreativeRiskTolerance INTEGER,
    BookerCompatibilityScore INTEGER,
    GimmickPreferences TEXT,
    CanRuinStorylines INTEGER,
    ProposedStorylines TEXT,
    ProposalAcceptanceRate INTEGER,
    Specialty TEXT,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId)
);

CREATE TABLE IF NOT EXISTS StructuralStaff (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    EfficiencyScore INTEGER,
    ProactivityScore INTEGER,
    ExpertiseDomain TEXT,
    GlobalImpactAreas TEXT,
    InjuryRecoveryBonus INTEGER,
    InjuryPreventionScore INTEGER,
    CrisisManagementScore INTEGER,
    ReputationBonus INTEGER,
    DealNegotiationScore INTEGER,
    CostReductionBonus INTEGER,
    TalentDiscoveryScore INTEGER,
    IndustryNetworkScore INTEGER,
    MoraleBonus INTEGER,
    ConflictResolutionScore INTEGER,
    LitigationManagementScore INTEGER,
    ContractNegotiationScore INTEGER,
    SuccessfulInterventions INTEGER,
    TotalInterventions INTEGER,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId)
);

CREATE TABLE IF NOT EXISTS Trainers (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    InfrastructureId TEXT,
    TrainingSpecialization TEXT,
    TrainingEfficiency INTEGER,
    ProgressionBonus INTEGER,
    YouthDevelopmentScore INTEGER,
    WrestlingExperience INTEGER,
    WrestlingStyle TEXT,
    Reputation INTEGER,
    CurrentStudents INTEGER,
    MaxStudentCapacity INTEGER,
    GraduatedStudents INTEGER,
    FailedStudents INTEGER,
    TeachingSpecialty TEXT,
    CanDevelopStars INTEGER,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId)
);

-- =============================================================================
-- PART 1: SEED BASE DATA (IF EMPTY)
-- =============================================================================

INSERT OR IGNORE INTO Countries (CountryId, Code, Name) VALUES ('COUNTRY_USA', 'USA', 'United States');
INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name) VALUES ('REGION_USA_EAST', 'COUNTRY_USA', 'USA East');

INSERT OR IGNORE INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, FoundedYear, IsPlayerControlled)
VALUES ('COMP_WWE', 'World Wrestling Entertainment', 'COUNTRY_USA', 'REGION_USA_EAST', 95, 10000000.0, 2024, 1);

-- Insert 20 Workers
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_CENA', 'John Cena', 'COMP_WWE', 'USA', 85, 92, 88, 95, 20, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_ORTON', 'Randy Orton', 'COMP_WWE', 'USA', 88, 85, 86, 92, 15, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_PUNK', 'CM Punk', 'COMP_WWE', 'USA', 90, 88, 90, 88, 25, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_ROCK', 'The Rock', 'COMP_WWE', 'USA', 82, 95, 92, 98, 10, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_AUSTIN', 'Stone Cold Steve Austin', 'COMP_WWE', 'USA', 86, 90, 89, 96, 12, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_TAKER', 'The Undertaker', 'COMP_WWE', 'USA', 88, 87, 91, 94, 30, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_HHH', 'Triple H', 'COMP_WWE', 'USA', 87, 86, 88, 90, 22, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_HBK', 'Shawn Michaels', 'COMP_WWE', 'USA', 92, 88, 87, 91, 18, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_ANGLE', 'Kurt Angle', 'COMP_WWE', 'USA', 95, 82, 85, 87, 28, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_EDGE', 'Edge', 'COMP_WWE', 'USA', 86, 84, 88, 86, 20, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_JERICHO', 'Chris Jericho', 'COMP_WWE', 'USA', 88, 87, 89, 85, 15, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_BENOIT', 'Chris Benoit', 'COMP_WWE', 'USA', 96, 78, 82, 84, 35, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_EDDIE', 'Eddie Guerrero', 'COMP_WWE', 'USA', 91, 86, 87, 85, 12, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_REY', 'Rey Mysterio', 'COMP_WWE', 'USA', 89, 82, 80, 83, 10, 'Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_KANE', 'Kane', 'COMP_WWE', 'USA', 82, 80, 84, 82, 25, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_SHOW', 'Big Show', 'COMP_WWE', 'USA', 78, 76, 79, 80, 20, 'Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_BATISTA', 'Batista', 'COMP_WWE', 'USA', 80, 82, 81, 84, 15, 'Upper Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_LESNAR', 'Brock Lesnar', 'COMP_WWE', 'USA', 88, 79, 83, 89, 5, 'Main Eventer');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_RVD', 'Rob Van Dam', 'COMP_WWE', 'USA', 88, 84, 79, 82, 10, 'Midcard');
INSERT OR IGNORE INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv) VALUES ('W_BOOKER', 'Booker T', 'COMP_WWE', 'USA', 84, 83, 82, 81, 15, 'Midcard');

-- Ensure Attributes
INSERT OR IGNORE INTO WorkerAttributes (WorkerId, InRing, Entertainment, Story, Popularity, Stamina, Charisma) SELECT WorkerId, InRing, Entertainment, Story, Popularity, 80, 85 FROM Workers;

-- =============================================================================
-- PART 2: MEDICAL DATA
-- =============================================================================

-- 1. Modifie 5 Workers existants pour leur mettre un InjuryStatus à 'BLESSÉ' et une Fatigue à 80.
UPDATE Workers SET InjuryStatus = 'BLESSÉ', Fatigue = 80 WHERE WorkerId IN ('W_CENA', 'W_ORTON', 'W_PUNK', 'W_ROCK', 'W_AUSTIN');

-- 2. Ajoute dans la table Injuries des entrées pour ces Workers (Type : 'Genou', 'Commotion', 'Fatigue intense')
DELETE FROM Injuries WHERE WorkerId IN ('W_CENA', 'W_ORTON', 'W_PUNK', 'W_ROCK', 'W_AUSTIN');
INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes) VALUES ('W_CENA', 'Genou', 50, 20500, 20515, 1, 'Déchirure ligamentaire');
INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes) VALUES ('W_ORTON', 'Commotion', 40, 20502, 20520, 1, 'Choc lors d''un match');
INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes) VALUES ('W_PUNK', 'Fatigue intense', 30, 20504, 20510, 1, 'Besoin de repos');
INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes) VALUES ('W_ROCK', 'Genou', 60, 20498, 20530, 1, 'Problème ménisque');
INSERT INTO Injuries (WorkerId, Type, Severity, StartDate, EndDate, IsActive, Notes) VALUES ('W_AUSTIN', 'Commotion', 45, 20501, 20525, 1, 'Symptômes persistants');

-- =============================================================================
-- PART 3: FINANCIAL DATA
-- =============================================================================

-- Insère 20 lignes dans FinanceTransactions
DELETE FROM FinanceTransactions;

-- 10 Revenus (2025-12-12 to 2026-01-11)
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-12', 49, 'Vente billets', 25000.0, 'Live Event NYC');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-15', 50, 'Merchandising', 12000.0, 'Vente T-shirts');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-20', 50, 'Droits TV', 50000.0, 'Paiement mensuel Network');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-24', 51, 'Vente billets', 15000.0, 'Holiday Special');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-26', 51, 'Merchandising', 8000.0, 'Vente en ligne');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-30', 52, 'Droits TV', 45000.0, 'Bonus fin d''année');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-02', 1, 'Vente billets', 30000.0, 'New Year Smash');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-05', 1, 'Merchandising', 10000.0, 'Tournée Europe');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-08', 2, 'Vente billets', 20000.0, 'Weekly Show');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-10', 2, 'Merchandising', 7000.0, 'Produits dérivés');

-- 10 Dépenses
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-13', 49, 'Salaires', -15000.0, 'Paie staff');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-16', 50, 'Location salle', -5000.0, 'Madison Square Garden');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-21', 50, 'Marketing', -3000.0, 'Campagne Social Media');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-25', 51, 'Location salle', -4000.0, 'Chicago Arena');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-27', 51, 'Salaires', -12000.0, 'Bonus Noël Workers');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2025-12-31', 52, 'Marketing', -6000.0, 'Promotion Royal Rumble');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-03', 1, 'Location salle', -5500.0, 'Boston Garden');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-06', 1, 'Salaires', -14000.0, 'Paie mensuelle');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-09', 2, 'Marketing', -2000.0, 'Ads Google');
INSERT INTO FinanceTransactions (CompanyId, Date, Week, Category, Amount, Description) VALUES ('COMP_WWE', '2026-01-11', 2, 'Location salle', -4500.0, 'Miami Center');

-- =============================================================================
-- PART 4: STAFF & CONTRACTS
-- =============================================================================

-- 1. Crée 6 entrées dans la table Staff Members (2 Créatifs, 2 Entraîneurs, 2 Administratifs/Structural).
DELETE FROM StaffMembers;
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore) VALUES ('S_STEPH', 'COMP_WWE', 'Stephanie McMahon', 'LeadWriter', 'Creative', 'Expert', '2024-01-01', 90);
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore) VALUES ('S_HEYMAN', 'COMP_WWE', 'Paul Heyman', 'CreativeWriter', 'Creative', 'Legend', '2024-01-01', 95);
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore, BrandId) VALUES ('S_REGAL', 'COMP_WWE', 'William Regal', 'HeadTrainer', 'Training', 'Expert', '2024-01-01', 92, 'B_NXT_DUMMY');
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore, BrandId) VALUES ('S_MICHAELS', 'COMP_WWE', 'Shawn Michaels', 'WrestlingTrainer', 'Training', 'Legend', '2024-01-01', 94, 'B_NXT_DUMMY');
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore) VALUES ('S_NICK', 'COMP_WWE', 'Nick Khan', 'FinancialDirector', 'Structural', 'Expert', '2024-01-01', 98);
INSERT INTO StaffMembers (StaffId, CompanyId, Name, Role, Department, ExpertiseLevel, HireDate, SkillScore) VALUES ('S_PRICHARD', 'COMP_WWE', 'Bruce Prichard', 'PRManager', 'Structural', 'Senior', '2024-01-01', 85);

-- 2. Génère des Contracts pour ces membres de staff et pour 10 Workers, avec des dates d'expiration proches (5-10 jours).
-- Current date is 2026-01-11. 5-10 days: 2026-01-16 to 2026-01-21.
DELETE FROM Contracts;
-- Staff Contracts (expiration 2026-01-16 to 2026-01-21)
-- EndDate in days since start of some epoch? In the schema it says INTEGER for EndDate in some tables, but TEXT in others.
-- Wait, in Migration 001, Contracts.EndDate is INTEGER.
-- Let's assume days since 1970-01-01. 
-- 2026-01-11 is approx 20464 days.
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_STEPH', 'COMP_WWE', 20000, 20469, 5000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_HEYMAN', 'COMP_WWE', 20000, 20470, 4500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_REGAL', 'COMP_WWE', 20000, 20471, 3500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_MICHAELS', 'COMP_WWE', 20000, 20472, 4000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_NICK', 'COMP_WWE', 20000, 20473, 6000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('S_PRICHARD', 'COMP_WWE', 20000, 20474, 3000.0);

-- 10 Workers Contracts (W_TAKER to W_BATISTA)
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_TAKER', 'COMP_WWE', 20000, 20469, 10000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_HHH', 'COMP_WWE', 20000, 20470, 12000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_HBK', 'COMP_WWE', 20000, 20471, 11000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_ANGLE', 'COMP_WWE', 20000, 20472, 9500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_EDGE', 'COMP_WWE', 20000, 20473, 8500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_JERICHO', 'COMP_WWE', 20000, 20474, 8000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_BENOIT', 'COMP_WWE', 20000, 20469, 8500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_EDDIE', 'COMP_WWE', 20000, 20470, 9000.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_REY', 'COMP_WWE', 20000, 20471, 7500.0);
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary) VALUES ('W_BATISTA', 'COMP_WWE', 20000, 20472, 9000.0);

-- =============================================================================
-- PART 5: ATTRIBUTES
-- =============================================================================

-- Déjà fait dans le seed part 1, mais on s'assure pour tous les workers existants
INSERT OR IGNORE INTO WorkerAttributes (WorkerId, InRing, Entertainment, Story, Popularity, Stamina, Charisma)
SELECT WorkerId, InRing, Entertainment, Story, Popularity, 75, 80 FROM Workers;

PRAGMA foreign_keys = ON;

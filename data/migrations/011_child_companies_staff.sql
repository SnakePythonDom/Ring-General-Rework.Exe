-- Migration: 011_child_companies_staff.sql
-- Description: Ajout du système de Child Companies et partage de staff
-- Date: 2026-01-08
-- Auteur: Claude (Lead Software Architect)

PRAGMA foreign_keys = ON;

-- =============================================================================
-- TABLES PRINCIPALES
-- =============================================================================

-- Table Child Companies
CREATE TABLE IF NOT EXISTS ChildCompanies (
    ChildCompanyId TEXT PRIMARY KEY,
    ParentCompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    RegionId TEXT,
    Level TEXT NOT NULL CHECK(Level IN ('Development', 'Official', 'Advanced')),
    MonthlyBudget REAL NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (ParentCompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

-- Table Assignations Staff
CREATE TABLE IF NOT EXISTS ChildCompanyStaffAssignments (
    AssignmentId TEXT PRIMARY KEY,
    StaffId TEXT NOT NULL,
    ChildCompanyId TEXT NOT NULL,
    AssignmentType TEXT NOT NULL CHECK(AssignmentType IN ('PartTime', 'TemporarySupport', 'DedicatedRotation')),
    TimePercentage REAL NOT NULL CHECK(TimePercentage BETWEEN 0.1 AND 1.0),
    StartDate TEXT NOT NULL,
    EndDate TEXT,
    MissionObjective TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (ChildCompanyId) REFERENCES ChildCompanies(ChildCompanyId) ON DELETE CASCADE
);

-- Table Planning Hebdomadaire
CREATE TABLE IF NOT EXISTS StaffSharingSchedules (
    ScheduleId TEXT PRIMARY KEY,
    StaffId TEXT NOT NULL,
    WeekNumber INTEGER NOT NULL,
    MondayLocation TEXT,
    TuesdayLocation TEXT,
    WednesdayLocation TEXT,
    ThursdayLocation TEXT,
    FridayLocation TEXT,
    SaturdayLocation TEXT,
    SundayLocation TEXT,
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE
);

-- Table Impacts Progression
CREATE TABLE IF NOT EXISTS StaffProgressionImpacts (
    ImpactId INTEGER PRIMARY KEY AUTOINCREMENT,
    StaffId TEXT NOT NULL,
    YouthStructureId TEXT NOT NULL,
    InRingBonus REAL NOT NULL DEFAULT 0,
    EntertainmentBonus REAL NOT NULL DEFAULT 0,
    StoryBonus REAL NOT NULL DEFAULT 0,
    MentalBonus REAL NOT NULL DEFAULT 0,
    CompatibilityScore REAL NOT NULL CHECK(CompatibilityScore BETWEEN 0.0 AND 1.0),
    FatigueModifier REAL NOT NULL CHECK(FatigueModifier BETWEEN 0.0 AND 1.0),
    CalculatedAt TEXT NOT NULL,
    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId) ON DELETE CASCADE
);

-- =============================================================================
-- EXTENSIONS TABLES EXISTANTES
-- =============================================================================

-- Extensions StaffMembers
ALTER TABLE StaffMembers ADD COLUMN CanBeShared INTEGER DEFAULT 1;
ALTER TABLE StaffMembers ADD COLUMN MobilityRating TEXT DEFAULT 'MEDIUM';
ALTER TABLE StaffMembers ADD COLUMN SharingPreferences TEXT; -- JSON
ALTER TABLE StaffMembers ADD COLUMN ChildSpecializations TEXT; -- JSON

-- =============================================================================
-- INDEXES POUR PERFORMANCES
-- =============================================================================

-- Indexes Child Companies
CREATE INDEX IF NOT EXISTS idx_child_companies_parent ON ChildCompanies(ParentCompanyId);
CREATE INDEX IF NOT EXISTS idx_child_companies_region ON ChildCompanies(RegionId);

-- Indexes Assignations Staff
CREATE INDEX IF NOT EXISTS idx_child_staff_assignments_staff ON ChildCompanyStaffAssignments(StaffId);
CREATE INDEX IF NOT EXISTS idx_child_staff_assignments_child ON ChildCompanyStaffAssignments(ChildCompanyId);
CREATE INDEX IF NOT EXISTS idx_child_staff_assignments_dates ON ChildCompanyStaffAssignments(StartDate, EndDate);

-- Indexes Planning
CREATE INDEX IF NOT EXISTS idx_staff_schedules_staff ON StaffSharingSchedules(StaffId, WeekNumber);

-- Indexes Impacts
CREATE INDEX IF NOT EXISTS idx_staff_impacts_staff ON StaffProgressionImpacts(StaffId);
CREATE INDEX IF NOT EXISTS idx_staff_impacts_youth ON StaffProgressionImpacts(YouthStructureId);
CREATE INDEX IF NOT EXISTS idx_staff_impacts_calculated ON StaffProgressionImpacts(CalculatedAt);

-- =============================================================================
-- DONNÉES DE TEST (À SUPPRIMER EN PRODUCTION)
-- =============================================================================

-- Insertion d'une Child Company exemple
INSERT OR IGNORE INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget)
SELECT 'CC001', CompanyId, 'Elite Wrestling Academy', 'REG001', 'Development', 15000.0
FROM Companies
WHERE Name LIKE '%Test%' OR Name LIKE '%Demo%'
LIMIT 1;

-- =============================================================================
-- VÉRIFICATIONS POST-MIGRATION
-- =============================================================================

-- Vérifier que les tables ont été créées
SELECT name FROM sqlite_master WHERE type='table' AND name IN (
    'ChildCompanies',
    'ChildCompanyStaffAssignments',
    'StaffSharingSchedules',
    'StaffProgressionImpacts'
);

-- Vérifier que les colonnes ont été ajoutées
PRAGMA table_info(StaffMembers);
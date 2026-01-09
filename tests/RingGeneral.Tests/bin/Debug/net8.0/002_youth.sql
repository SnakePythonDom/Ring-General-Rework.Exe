PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS YouthStructures (
    YouthStructureId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    RegionId TEXT,
    Type TEXT NOT NULL,
    BudgetAnnuel REAL NOT NULL DEFAULT 0,
    CapaciteMax INTEGER NOT NULL DEFAULT 0,
    NiveauEquipements INTEGER NOT NULL DEFAULT 1,
    QualiteCoaching INTEGER NOT NULL DEFAULT 10,
    Philosophie TEXT NOT NULL DEFAULT 'HYBRIDE',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

CREATE TABLE IF NOT EXISTS YouthTrainees (
    YouthTraineeId INTEGER PRIMARY KEY AUTOINCREMENT,
    YouthStructureId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'EN_FORMATION',
    WeekStart INTEGER,
    WeekGraduation INTEGER,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS YouthPrograms (
    ProgramId TEXT PRIMARY KEY,
    YouthStructureId TEXT NOT NULL,
    Name TEXT NOT NULL,
    DurationWeeks INTEGER NOT NULL,
    FocusAttributes TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId)
);

CREATE TABLE IF NOT EXISTS YouthStaffAssignments (
    YouthStaffAssignmentId INTEGER PRIMARY KEY AUTOINCREMENT,
    YouthStructureId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Role TEXT NOT NULL,
    StartWeek INTEGER,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_youth_structures_company ON YouthStructures(CompanyId);
CREATE INDEX IF NOT EXISTS idx_youth_trainees_structure ON YouthTrainees(YouthStructureId);
CREATE INDEX IF NOT EXISTS idx_youth_programs_structure ON YouthPrograms(YouthStructureId);
CREATE INDEX IF NOT EXISTS idx_youth_staff_structure ON YouthStaffAssignments(YouthStructureId);

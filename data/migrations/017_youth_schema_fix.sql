PRAGMA foreign_keys = ON;

ALTER TABLE YouthStructures ADD COLUMN Type TEXT NOT NULL DEFAULT 'ACADEMY';
ALTER TABLE YouthStructures ADD COLUMN BudgetAnnuel INTEGER NOT NULL DEFAULT 0;
ALTER TABLE YouthStructures ADD COLUMN CapaciteMax INTEGER NOT NULL DEFAULT 0;
ALTER TABLE YouthStructures ADD COLUMN NiveauEquipements INTEGER NOT NULL DEFAULT 1;
ALTER TABLE YouthStructures ADD COLUMN QualiteCoaching INTEGER NOT NULL DEFAULT 10;
ALTER TABLE YouthStructures ADD COLUMN Philosophie TEXT NOT NULL DEFAULT 'HYBRIDE';
ALTER TABLE YouthStructures ADD COLUMN Actif INTEGER NOT NULL DEFAULT 1;

ALTER TABLE YouthTrainees ADD COLUMN ProgramId TEXT;
ALTER TABLE YouthTrainees ADD COLUMN GraduationDate INTEGER;

CREATE TABLE IF NOT EXISTS YouthPrograms (
    ProgramId TEXT PRIMARY KEY,
    YouthStructureId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type TEXT NOT NULL,
    DurationWeeks INTEGER NOT NULL,
    FocusAttributes TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId)
);

CREATE TABLE IF NOT EXISTS YouthStaffAssignments (
    YouthStaffAssignmentId TEXT PRIMARY KEY,
    YouthStructureId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Role TEXT NOT NULL,
    StartDate INTEGER NOT NULL,
    EndDate INTEGER,
    FOREIGN KEY (YouthStructureId) REFERENCES YouthStructures(YouthStructureId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

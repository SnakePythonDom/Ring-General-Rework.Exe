PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Injuries (
    InjuryId TEXT PRIMARY KEY,
    WorkerId TEXT NOT NULL,
    InjuryType TEXT NOT NULL,
    Severity TEXT NOT NULL,
    Status TEXT NOT NULL,
    StartWeek INTEGER NOT NULL,
    EndWeek INTEGER,
    DurationWeeks INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS MedicalNotes (
    MedicalNoteId TEXT PRIMARY KEY,
    WorkerId TEXT NOT NULL,
    InjuryId TEXT,
    NoteType TEXT NOT NULL,
    Content TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Author TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (InjuryId) REFERENCES Injuries(InjuryId)
);

CREATE TABLE IF NOT EXISTS RecoveryPlans (
    RecoveryPlanId TEXT PRIMARY KEY,
    InjuryId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Status TEXT NOT NULL,
    StartWeek INTEGER NOT NULL,
    EndWeek INTEGER,
    DurationWeeks INTEGER NOT NULL,
    RecommendedRestWeeks INTEGER NOT NULL,
    Restrictions TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (InjuryId) REFERENCES Injuries(InjuryId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_injuries_worker ON Injuries(WorkerId);
CREATE INDEX IF NOT EXISTS idx_injuries_status ON Injuries(Status);
CREATE INDEX IF NOT EXISTS idx_medical_notes_worker ON MedicalNotes(WorkerId);
CREATE INDEX IF NOT EXISTS idx_recovery_plans_worker ON RecoveryPlans(WorkerId);

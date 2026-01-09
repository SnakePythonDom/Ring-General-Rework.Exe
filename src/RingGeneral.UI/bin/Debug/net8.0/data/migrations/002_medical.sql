PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS MedicalNotes (
    MedicalNoteId INTEGER PRIMARY KEY AUTOINCREMENT,
    InjuryId INTEGER,
    WorkerId TEXT NOT NULL,
    Note TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (InjuryId) REFERENCES Injuries(InjuryId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS RecoveryPlans (
    RecoveryPlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    InjuryId INTEGER NOT NULL,
    WorkerId TEXT NOT NULL,
    StartDate INTEGER NOT NULL,
    TargetDate INTEGER NOT NULL,
    RecommendedRestWeeks INTEGER NOT NULL,
    RiskLevel TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'EN_COURS',
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (InjuryId) REFERENCES Injuries(InjuryId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_recovery_plans_worker ON RecoveryPlans(WorkerId);
CREATE INDEX IF NOT EXISTS idx_medical_notes_worker ON MedicalNotes(WorkerId);

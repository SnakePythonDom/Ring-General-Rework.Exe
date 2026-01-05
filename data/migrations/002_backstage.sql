PRAGMA foreign_keys = ON;

ALTER TABLE Workers ADD COLUMN Morale INTEGER NOT NULL DEFAULT 60;

CREATE TABLE IF NOT EXISTS BackstageIncidents (
    BackstageIncidentId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    TypeId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    Severity INTEGER NOT NULL,
    WorkersJson TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS DisciplinaryActions (
    DisciplinaryActionId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    TypeId TEXT NOT NULL,
    Severity INTEGER NOT NULL,
    MoraleDelta INTEGER NOT NULL,
    Notes TEXT NOT NULL,
    IncidentId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (IncidentId) REFERENCES BackstageIncidents(BackstageIncidentId)
);

CREATE TABLE IF NOT EXISTS MoraleHistory (
    MoraleHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    MoraleBefore INTEGER NOT NULL,
    MoraleAfter INTEGER NOT NULL,
    Delta INTEGER NOT NULL,
    Reason TEXT NOT NULL,
    IncidentId TEXT,
    ActionId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (IncidentId) REFERENCES BackstageIncidents(BackstageIncidentId),
    FOREIGN KEY (ActionId) REFERENCES DisciplinaryActions(DisciplinaryActionId)
);

CREATE INDEX IF NOT EXISTS idx_backstage_incidents_company_week ON BackstageIncidents(CompanyId, Week);
CREATE INDEX IF NOT EXISTS idx_disciplinary_actions_worker_week ON DisciplinaryActions(WorkerId, Week);
CREATE INDEX IF NOT EXISTS idx_morale_history_worker_week ON MoraleHistory(WorkerId, Week);

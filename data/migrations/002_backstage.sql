PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS BackstageIncidents (
    IncidentId TEXT PRIMARY KEY,
    WorkerId TEXT NOT NULL,
    IncidentType TEXT NOT NULL,
    Description TEXT NOT NULL,
    Severity INTEGER NOT NULL,
    Week INTEGER NOT NULL,
    Status TEXT NOT NULL DEFAULT 'OPEN',
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS MoraleHistory (
    MoraleHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Delta INTEGER NOT NULL,
    Value INTEGER NOT NULL,
    Reason TEXT NOT NULL,
    IncidentId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (IncidentId) REFERENCES BackstageIncidents(IncidentId)
);

CREATE TABLE IF NOT EXISTS DisciplinaryActions (
    ActionId TEXT PRIMARY KEY,
    IncidentId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    ActionType TEXT NOT NULL,
    MoraleDelta INTEGER NOT NULL,
    Week INTEGER NOT NULL,
    Notes TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IncidentId) REFERENCES BackstageIncidents(IncidentId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_backstage_incidents_worker ON BackstageIncidents(WorkerId);
CREATE INDEX IF NOT EXISTS idx_backstage_incidents_week ON BackstageIncidents(Week);
CREATE INDEX IF NOT EXISTS idx_morale_history_worker ON MoraleHistory(WorkerId);
CREATE INDEX IF NOT EXISTS idx_morale_history_week ON MoraleHistory(Week);
CREATE INDEX IF NOT EXISTS idx_disciplinary_actions_incident ON DisciplinaryActions(IncidentId);

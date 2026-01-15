PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS ScoutReports (
    ScoutReportId TEXT PRIMARY KEY,
    WorkerId TEXT NOT NULL,
    WorkerName TEXT NOT NULL,
    RegionId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Note INTEGER NOT NULL,
    Strengths TEXT NOT NULL,
    Weaknesses TEXT NOT NULL,
    Recommendation TEXT NOT NULL,
    Summary TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS Shortlists (
    WorkerId TEXT PRIMARY KEY,
    WorkerName TEXT NOT NULL,
    Note INTEGER NOT NULL,
    Notes TEXT NOT NULL,
    AddedWeek INTEGER NOT NULL,
    ScoutReportId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (ScoutReportId) REFERENCES ScoutReports(ScoutReportId)
);

CREATE TABLE IF NOT EXISTS ScoutMissions (
    ScoutMissionId TEXT PRIMARY KEY,
    RegionId TEXT NOT NULL,
    StartWeek INTEGER NOT NULL,
    DurationWeeks INTEGER NOT NULL,
    Progression INTEGER NOT NULL,
    Status TEXT NOT NULL,
    ScoutReportId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ScoutReportId) REFERENCES ScoutReports(ScoutReportId)
);

CREATE INDEX IF NOT EXISTS idx_scout_reports_worker ON ScoutReports(WorkerId);
CREATE INDEX IF NOT EXISTS idx_scout_reports_week ON ScoutReports(Week);
CREATE INDEX IF NOT EXISTS idx_shortlists_note ON Shortlists(Note);
CREATE INDEX IF NOT EXISTS idx_scout_missions_status ON ScoutMissions(Status);

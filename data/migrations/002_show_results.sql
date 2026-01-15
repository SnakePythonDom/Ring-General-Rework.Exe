PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS ShowResults (
    ShowResultId INTEGER PRIMARY KEY AUTOINCREMENT,
    ShowId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Note INTEGER NOT NULL,
    Audience INTEGER NOT NULL,
    Billetterie REAL NOT NULL,
    Merch REAL NOT NULL,
    Tv REAL NOT NULL,
    Summary TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId)
);

CREATE INDEX IF NOT EXISTS idx_show_results_show_week ON ShowResults(ShowId, Week);

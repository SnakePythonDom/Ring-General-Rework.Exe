PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS TitleMatches (
    TitleMatchId INTEGER PRIMARY KEY AUTOINCREMENT,
    TitleId TEXT NOT NULL,
    ShowId TEXT,
    Week INTEGER NOT NULL,
    ChampionId TEXT,
    ChallengerId TEXT NOT NULL,
    WinnerId TEXT NOT NULL,
    IsTitleChange INTEGER NOT NULL DEFAULT 0,
    PrestigeDelta INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId),
    FOREIGN KEY (ChampionId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (ChallengerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (WinnerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS ContenderRankings (
    ContenderRankingId INTEGER PRIMARY KEY AUTOINCREMENT,
    TitleId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Rank INTEGER NOT NULL,
    Score REAL NOT NULL,
    Reason TEXT,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (TitleId, WorkerId),
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_titlematches_title_week ON TitleMatches(TitleId, Week);
CREATE INDEX IF NOT EXISTS idx_contenders_title_rank ON ContenderRankings(TitleId, Rank);

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS TitleMatches (
    TitleMatchId INTEGER PRIMARY KEY AUTOINCREMENT,
    TitleId TEXT NOT NULL,
    ChampionWorkerId TEXT NOT NULL,
    ChallengerWorkerId TEXT NOT NULL,
    WinnerWorkerId TEXT NOT NULL,
    LoserWorkerId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    ShowId TEXT,
    SegmentId TEXT,
    IsTitleChange INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (ChampionWorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (ChallengerWorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (WinnerWorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (LoserWorkerId) REFERENCES Workers(WorkerId)
);

CREATE TABLE IF NOT EXISTS ContenderRankings (
    ContenderRankingId INTEGER PRIMARY KEY AUTOINCREMENT,
    TitleId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    Rank INTEGER NOT NULL,
    Score INTEGER NOT NULL DEFAULT 0,
    Week INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
);

CREATE INDEX IF NOT EXISTS idx_titlematches_title ON TitleMatches(TitleId, Week);
CREATE INDEX IF NOT EXISTS idx_titlematches_workers ON TitleMatches(ChampionWorkerId, ChallengerWorkerId);
CREATE INDEX IF NOT EXISTS idx_contenders_title_rank ON ContenderRankings(TitleId, Rank);

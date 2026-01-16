-- Migration to add TitleReigns table for history tracking
CREATE TABLE IF NOT EXISTS TitleReigns (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    TitleId TEXT NOT NULL,
    WonDate TEXT NOT NULL,
    WonShowId TEXT,
    LostDate TEXT,
    LostShowId TEXT,
    DaysHeld INTEGER DEFAULT 0,
    ReignNumber INTEGER DEFAULT 1,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (TitleId) REFERENCES Titles(TitleId),
    FOREIGN KEY (WonShowId) REFERENCES Shows(ShowId),
    FOREIGN KEY (LostShowId) REFERENCES Shows(ShowId)
);

CREATE INDEX IF NOT EXISTS IDX_TitleReigns_WorkerId ON TitleReigns(WorkerId);
CREATE INDEX IF NOT EXISTS IDX_TitleReigns_TitleId ON TitleReigns(TitleId);

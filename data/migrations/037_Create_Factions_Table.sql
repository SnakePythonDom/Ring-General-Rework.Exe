-- Create Factions Tables

CREATE TABLE IF NOT EXISTS Factions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    LeaderId TEXT,
    FactionType INTEGER NOT NULL, -- 0: TagTeam, 1: Trio, 2: Faction
    Status INTEGER NOT NULL,      -- 0: Active, 1: Inactive, 2: Disbanded
    CreatedWeek INTEGER NOT NULL,
    CreatedYear INTEGER NOT NULL,
    DisbandedWeek INTEGER,
    DisbandedYear INTEGER,
    FOREIGN KEY(LeaderId) REFERENCES Workers(Id)
);

CREATE TABLE IF NOT EXISTS FactionMembers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FactionId INTEGER NOT NULL,
    WorkerId TEXT NOT NULL,
    JoinedWeek INTEGER NOT NULL,
    JoinedYear INTEGER NOT NULL,
    LeftWeek INTEGER,
    LeftYear INTEGER,
    FOREIGN KEY(FactionId) REFERENCES Factions(Id),
    FOREIGN KEY(WorkerId) REFERENCES Workers(Id)
);

-- Index for performance
CREATE INDEX IF NOT EXISTS IX_FactionMembers_Worker ON FactionMembers(WorkerId);
CREATE INDEX IF NOT EXISTS IX_FactionMembers_Faction ON FactionMembers(FactionId);

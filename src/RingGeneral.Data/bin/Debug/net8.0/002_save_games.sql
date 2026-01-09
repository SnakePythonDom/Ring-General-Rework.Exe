-- Table pour gérer les sauvegardes de jeu
CREATE TABLE IF NOT EXISTS SaveGames (
    SaveGameId INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveName TEXT NOT NULL,
    PlayerCompanyId TEXT NOT NULL,
    CurrentWeek INTEGER NOT NULL DEFAULT 1,
    CurrentDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastPlayedAt TEXT,
    FOREIGN KEY (PlayerCompanyId) REFERENCES Companies(CompanyId)
);

-- Table pour stocker l'état du jeu
CREATE TABLE IF NOT EXISTS GameState (
    GameStateId INTEGER PRIMARY KEY CHECK (GameStateId = 1),
    CurrentSaveGameId INTEGER,
    CurrentWeek INTEGER NOT NULL DEFAULT 1,
    CurrentDate TEXT,
    LastUpdatedAt TEXT,
    FOREIGN KEY (CurrentSaveGameId) REFERENCES SaveGames(SaveGameId)
);

-- Insérer un état de jeu par défaut
INSERT OR IGNORE INTO GameState (GameStateId, CurrentWeek) VALUES (1, 1);

-- Index pour performance
CREATE INDEX IF NOT EXISTS idx_savegames_active ON SaveGames(IsActive);
CREATE INDEX IF NOT EXISTS idx_savegames_company ON SaveGames(PlayerCompanyId);

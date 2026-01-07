PRAGMA foreign_keys = ON;

-- Table pour l'état de la partie en cours
CREATE TABLE IF NOT EXISTS GameState (
    GameStateId INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveName TEXT NOT NULL,
    PlayerCompanyId TEXT NOT NULL,
    CurrentWeek INTEGER NOT NULL DEFAULT 1,
    CurrentDate TEXT NOT NULL DEFAULT '2024-01-01',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PlayerCompanyId) REFERENCES Companies(CompanyId)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_gamestate_active
    ON GameState(IsActive) WHERE IsActive = 1;

CREATE INDEX IF NOT EXISTS idx_gamestate_company
    ON GameState(PlayerCompanyId);

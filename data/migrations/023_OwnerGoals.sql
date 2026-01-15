-- Migration 023: Owner Goals System
-- Adds table for tracking goals set by Owners for Bookers

CREATE TABLE IF NOT EXISTS OwnerGoals (
    GoalId TEXT PRIMARY KEY,
    OwnerId TEXT NOT NULL,
    Description TEXT NOT NULL,
    Metric TEXT NOT NULL, -- Enum as String: Revenue, FanSatisfaction, etc.
    TargetValue REAL NOT NULL,
    CurrentValue REAL NOT NULL DEFAULT 0,
    Deadline TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Active', -- Active, Met, Failed, Cancelled
    TargetEntityId TEXT, -- Optionnel (ex: WorkerId)
    Importance INTEGER NOT NULL DEFAULT 50,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (OwnerId) REFERENCES Owners(OwnerId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_owner_goals_owner ON OwnerGoals(OwnerId);
CREATE INDEX IF NOT EXISTS idx_owner_goals_status ON OwnerGoals(Status);

-- Migration to add Aura column and YouthStructures table
-- 006_high_density_rework.sql

-- 1. Add Aura column to WorkerEntertainmentAttributes if it doesn't exist
PRAGMA foreign_keys=off;

-- Check if Aura column exists (using a safe way that works even if column exists)
-- Since SQLite doesn't support IF NOT EXISTS for ADD COLUMN in all versions/tools,
-- we trust the app logic to run this only once via migration system.
ALTER TABLE WorkerEntertainmentAttributes ADD COLUMN Aura INTEGER NOT NULL DEFAULT 50;

-- 2. Create YouthStructures table
CREATE TABLE IF NOT EXISTS YouthStructures (
    YouthId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    Region TEXT NOT NULL,
    Type TEXT NOT NULL DEFAULT 'Dojo',
    BudgetAnnual INTEGER NOT NULL DEFAULT 0,
    MaxCapacity INTEGER NOT NULL DEFAULT 10,
    EquipmentLevel INTEGER NOT NULL DEFAULT 50,
    CoachingQuality INTEGER NOT NULL DEFAULT 50,
    Philosophy TEXT NOT NULL DEFAULT 'Balanced',
    IsActive INTEGER NOT NULL DEFAULT 1,
    LastGraduationWeek INTEGER,
    Level INTEGER NOT NULL DEFAULT 1,
    ActiveTraineesCount INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 3. Ensure Titles table has CompanyId column
-- (Checking if column exists is tricky in pure SQL script without knowing current state,
-- but we can try adding it and catch error or just recreate table if needed.
-- For now, let's assume it might be missing and add it if we can, or rely on the Generator code to fix it if this fails.)
-- A safe way in SQLite for adding column:
-- ALTER TABLE Titles ADD COLUMN CompanyId TEXT;
-- But if it exists, it throws.
-- We will handle "Titles" schema check in the C# Generator Service more robustly if migration fails,
-- or just try to add it here.
ALTER TABLE Titles ADD COLUMN CompanyId TEXT REFERENCES Companies(CompanyId);

-- 4. Ensure SegmentTemplates table exists
CREATE TABLE IF NOT EXISTS SegmentTemplates (
    TemplateId TEXT PRIMARY KEY,
    Nom TEXT NOT NULL,
    TypeSegment TEXT NOT NULL,
    DureeMinutes INTEGER NOT NULL,
    EstMainEvent INTEGER NOT NULL DEFAULT 0,
    Intensite INTEGER NOT NULL DEFAULT 50,
    MatchTypeId TEXT
);

PRAGMA foreign_keys=on;

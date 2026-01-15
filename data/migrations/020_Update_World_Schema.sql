-- Add columns for World Data
ALTER TABLE Countries ADD COLUMN Continent TEXT;
ALTER TABLE Countries ADD COLUMN WrestlingImportance INTEGER DEFAULT 0;

ALTER TABLE Regions ADD COLUMN WrestlingImportance INTEGER DEFAULT 0;

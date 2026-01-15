-- Migration to add Alumni capabilities to Workers table
ALTER TABLE Workers ADD COLUMN DepartureDate TEXT;
ALTER TABLE Workers ADD COLUMN DepartureReason TEXT;
ALTER TABLE Workers ADD COLUMN IsHallOfFame INTEGER DEFAULT 0;
ALTER TABLE Workers ADD COLUMN LegacyScore INTEGER DEFAULT 0;

-- Optional: Create index for faster alumni queries
CREATE INDEX IF NOT EXISTS IDX_Workers_DepartureDate ON Workers(DepartureDate);

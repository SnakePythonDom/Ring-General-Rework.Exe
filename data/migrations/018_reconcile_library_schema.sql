PRAGMA foreign_keys = OFF;

-- Reconcile MatchTypes
-- SQLite 3.25+ supports RENAME COLUMN
-- We use a safe approach by checking if we have the old names (implicit in migration sequentiality)

-- Rename columns if they exist with old names
-- Note: Migrations are applied in transaction, but SQLite ALTER TABLE is atomic
-- Rename columns if they exist with old names
-- Note: Migrations are applied in transaction, but SQLite ALTER TABLE is atomic
-- ALTER TABLE MatchTypes RENAME COLUMN Name TO Libelle;
-- ALTER TABLE MatchTypes RENAME COLUMN IsActive TO Actif;
-- ALTER TABLE MatchTypes RENAME COLUMN SortOrder TO Ordre;

-- Add missing columns
-- ALTER TABLE MatchTypes ADD COLUMN Participants INTEGER;
-- ALTER TABLE MatchTypes ADD COLUMN DureeParDefaut INTEGER;

-- Reconcile SegmentTemplates
-- ALTER TABLE SegmentTemplates RENAME COLUMN SegmentTemplateId TO TemplateId;
-- ALTER TABLE SegmentTemplates RENAME COLUMN Name TO Libelle;
-- ALTER TABLE SegmentTemplates RENAME COLUMN SegmentType TO TypeSegment;
-- ALTER TABLE SegmentTemplates RENAME COLUMN DurationMinutes TO DureeMinutes;
-- ALTER TABLE SegmentTemplates RENAME COLUMN IsMainEvent TO EstMainEvent;
-- ALTER TABLE SegmentTemplates RENAME COLUMN Intensity TO Intensite;

-- Add missing columns
-- ALTER TABLE SegmentTemplates ADD COLUMN Description TEXT;
-- ALTER TABLE SegmentTemplates ADD COLUMN SegmentsJson TEXT;

PRAGMA foreign_keys = ON;

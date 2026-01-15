-- Migration 024: Nepotism Columns for WorkerRelations
-- Adds tracking for hidden biases and familial/mentor ties

-- Check if column exists before adding to prevent errors
-- SQLite doesn't support 'IF NOT EXISTS' in ALTER TABLE, so we handle it gracefully in C# if needed
-- or just assume the migration runner handles success/failure.

ALTER TABLE WorkerRelations ADD COLUMN IsHidden INTEGER NOT NULL DEFAULT 0;
ALTER TABLE WorkerRelations ADD COLUMN BiasStrength INTEGER NOT NULL DEFAULT 0;
ALTER TABLE WorkerRelations ADD COLUMN OriginEvent TEXT;
ALTER TABLE WorkerRelations ADD COLUMN LastImpact TEXT;

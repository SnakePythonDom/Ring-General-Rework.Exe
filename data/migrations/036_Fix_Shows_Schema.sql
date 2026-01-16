-- ============================================================================
-- Migration 036: Fix Shows Table Schema
-- Description: Adds missing columns to the 'shows' table
-- ============================================================================

BEGIN TRANSACTION;

-- Add Date column if not exists (SQLite doesn't support IF NOT EXISTS in ALTER)
-- So we use a generic statement and the python script handles the error.
ALTER TABLE shows ADD COLUMN Date TEXT;

COMMIT;

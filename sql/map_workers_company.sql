-- ============================================================================
-- Map workers without a company to the Free Agent company
-- ============================================================================

PRAGMA foreign_keys = ON;

UPDATE Workers
SET CompanyId = '0'
WHERE CompanyId IS NULL;

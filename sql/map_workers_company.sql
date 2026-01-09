-- SQLite syntax - Do not validate as T-SQL
-- ============================================================================
-- Map workers without a company to the Free Agent company
-- ============================================================================



UPDATE Workers
SET CompanyId = '0'
WHERE CompanyId IS NULL;

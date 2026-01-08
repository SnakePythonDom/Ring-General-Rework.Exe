-- ============================================================================
-- Validation checks for imported data (fail fast on issues)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- Foreign key violations
CREATE TEMP TABLE fk_violations AS SELECT * FROM pragma_foreign_key_check;
SELECT * FROM fk_violations LIMIT 50;
CREATE TEMP TABLE fk_check(x INTEGER CHECK (x = 0));
INSERT INTO fk_check SELECT COUNT(*) FROM fk_violations;

-- Companies missing mappings
SELECT CompanyId, Name, CountryId, RegionId, CatchStyleId
FROM Companies
WHERE CountryId IS NULL OR RegionId IS NULL OR CatchStyleId IS NULL
LIMIT 50;
CREATE TEMP TABLE company_null_check(x INTEGER CHECK (x = 0));
INSERT INTO company_null_check
SELECT COUNT(*) FROM Companies
WHERE CountryId IS NULL OR RegionId IS NULL OR CatchStyleId IS NULL;

-- Workers missing nationality
SELECT WorkerId, Name, Nationality
FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = ''
LIMIT 50;
CREATE TEMP TABLE worker_null_check(x INTEGER CHECK (x = 0));
INSERT INTO worker_null_check
SELECT COUNT(*) FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = '';

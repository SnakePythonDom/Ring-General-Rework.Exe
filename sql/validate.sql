-- ============================================================
-- Validation (SQLite compatible)
-- ============================================================

PRAGMA foreign_keys = ON;

-- ------------------------------------------------------------
-- Foreign key integrity
-- ------------------------------------------------------------
CREATE TEMP TABLE fk_violations AS
SELECT * FROM pragma_foreign_key_check;

SELECT * FROM fk_violations LIMIT 50;

CREATE TEMP TABLE fk_check(x INTEGER CHECK (x = 0));
INSERT INTO fk_check
SELECT COUNT(*) FROM fk_violations;

-- ------------------------------------------------------------
-- Companies: required fields (ignore placeholder Free Agent CompanyId = 0)
-- ------------------------------------------------------------
SELECT CompanyId, Name, CountryId, RegionId, CatchStyleId
FROM Companies
WHERE (CompanyId <> 0 AND CompanyId <> '0')
  AND (
        CountryId IS NULL OR TRIM(CountryId) = ''
     OR RegionId IS NULL OR TRIM(RegionId) = ''
     OR CatchStyleId IS NULL OR TRIM(CatchStyleId) = ''
  )
LIMIT 50;

CREATE TEMP TABLE company_null_check(x INTEGER CHECK (x = 0));
INSERT INTO company_null_check
SELECT COUNT(*)
FROM Companies
WHERE (CompanyId <> 0 AND CompanyId <> '0')
  AND (
        CountryId IS NULL OR TRIM(CountryId) = ''
     OR RegionId IS NULL OR TRIM(RegionId) = ''
     OR CatchStyleId IS NULL OR TRIM(CatchStyleId) = ''
  );

-- ------------------------------------------------------------
-- Workers: required nationality
-- ------------------------------------------------------------
SELECT WorkerId, Name, Nationality
FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = ''
LIMIT 50;

CREATE TEMP TABLE worker_null_check(x INTEGER CHECK (x = 0));
INSERT INTO worker_null_check
SELECT COUNT(*)
FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = '';

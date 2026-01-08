-- ============================================================================
-- Validation checks for imported data (fail fast on issues)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- Foreign key violations
CREATE TEMP TABLE fk_violations AS SELECT * FROM pragma_foreign_key_check;
SELECT * FROM fk_violations LIMIT 50;
CREATE TEMP TABLE fk_check(x INTEGER CHECK (x = 0));
INSERT INTO fk_check SELECT COUNT(*) FROM fk_violations;

-- Companies missing country mappings (report first, then fail fast)
WITH company_country_violations AS (
    SELECT
        c.CompanyId,
        c.LegacyPromotionId,
        c.Name,
        c.CountryId,
        lc.RawCountry
    FROM Companies c
    LEFT JOIN Countries co ON co.CountryId = c.CountryId
    LEFT JOIN LegacyCompanies lc
        ON lc.LegacyPromotionId = c.LegacyPromotionId
    WHERE c.CountryId IS NULL
        OR TRIM(c.CountryId) = ''
        OR co.CountryId IS NULL
)
SELECT * FROM company_country_violations LIMIT 50;
CREATE TEMP TABLE company_country_check(x INTEGER CHECK (x = 0));
INSERT INTO company_country_check
SELECT COUNT(*) FROM company_country_violations;

-- Companies missing other required mappings
SELECT CompanyId, Name, CountryId, RegionId, CatchStyleId
FROM Companies
WHERE RegionId IS NULL OR CatchStyleId IS NULL
LIMIT 50;
CREATE TEMP TABLE company_null_check(x INTEGER CHECK (x = 0));
INSERT INTO company_null_check
SELECT COUNT(*) FROM Companies
WHERE RegionId IS NULL OR CatchStyleId IS NULL;

-- Workers missing nationality
SELECT WorkerId, Name, Nationality
FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = ''
LIMIT 50;
CREATE TEMP TABLE worker_null_check(x INTEGER CHECK (x = 0));
INSERT INTO worker_null_check
SELECT COUNT(*) FROM Workers
WHERE Nationality IS NULL OR TRIM(Nationality) = '';

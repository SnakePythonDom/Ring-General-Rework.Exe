-- sql/map_companies.sql corrected version (SQLite compatible)

PRAGMA foreign_keys = ON;

DELETE FROM Companies;

WITH
normalized_companies AS (
    SELECT
        lc.*,
        LOWER(
            TRIM(
                REPLACE(
                    REPLACE(COALESCE(lc.country, ''), '  ', ' '),
                    '  ', ' '
                )
            )
        ) AS RawCountryNorm
    FROM LegacyCompanies lc
),
linked_workers AS (
    SELECT promotionID, workerID FROM legacy.contracts WHERE promotionID IS NOT NULL
    UNION ALL
    SELECT promotionID, workerID FROM legacy.contractoffers WHERE promotionID IS NOT NULL
),
worker_countries AS (
    SELECT
        lw.promotionID,
        LOWER(
            TRIM(
                REPLACE(
                    REPLACE(COALESCE(legacy_country.name, ''), '  ', ' '),
                    '  ', ' '
                )
            )
        ) AS WorkerCountryNorm
    FROM linked_workers lw
    JOIN legacy.workers w ON w.workerID = lw.workerID
    LEFT JOIN legacy.countries legacy_country
        ON legacy_country.countryID = COALESCE(w.basedIn, w.homeCountry, w.nationality)
),
country_votes AS (
    SELECT promotionID, WorkerCountryNorm AS norm, COUNT(*) AS cnt
    FROM worker_countries
    WHERE WorkerCountryNorm IS NOT NULL AND WorkerCountryNorm <> ''
    GROUP BY promotionID, WorkerCountryNorm
),
country_pick AS (
    SELECT
        promotionID,
        norm,
        ROW_NUMBER() OVER (PARTITION BY promotionID ORDER BY cnt DESC, norm ASC) AS rn
    FROM country_votes
),
mapped AS (
    SELECT
        nc.companyID AS LegacyCompanyID,
        nc.name AS CompanyName,
        COALESCE(ca.country_id, c.id, ca2.country_id, c2.id) AS CountryID
    FROM normalized_companies nc
    LEFT JOIN country_aliases ca ON ca.alias_norm = nc.RawCountryNorm
    LEFT JOIN countries c ON c.name_norm = nc.RawCountryNorm
    LEFT JOIN country_pick cp ON cp.promotionID = nc.companyID AND cp.rn = 1
    LEFT JOIN country_aliases ca2 ON ca2.alias_norm = cp.norm
    LEFT JOIN countries c2 ON c2.name_norm = cp.norm
)
INSERT INTO Companies (legacy_id, name, country_id)
SELECT
    LegacyCompanyID,
    CompanyName,
    COALESCE(CountryID, (SELECT id FROM countries WHERE code = 'COUNTRY_DEFAULT' LIMIT 1))
FROM mapped;

INSERT OR IGNORE INTO Companies (legacy_id, name, country_id, region_id, is_active)
VALUES ('0', 'Free Agent', 'COUNTRY_DEFAULT', 'REGION_DEFAULT', 0);

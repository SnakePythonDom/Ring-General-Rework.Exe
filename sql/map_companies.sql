-- ============================================================================
-- Map legacy company data into Ring General Companies table
-- ============================================================================

PRAGMA foreign_keys = ON;

DELETE FROM Companies;

WITH
normalized_companies AS (
    SELECT
        lc.*,
        LOWER(
            TRIM(
                REPLACE(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            REPLACE(
                                                REPLACE(
                                                    REPLACE(
                                                        REPLACE(
                                                            REPLACE(
                                                                REPLACE(
                                                                    REPLACE(
                                                                        REPLACE(COALESCE(lc.RawCountry, ''), '.', ' '),
                                                                        ',',
                                                                        ' '
                                                                    ),
                                                                    '-',
                                                                    ' '
                                                                ),
                                                                '''',
                                                                ' '
                                                            ),
                                                            '/',
                                                            ' '
                                                        ),
                                                        '0',
                                                        ' '
                                                    ),
                                                    '1',
                                                    ' '
                                                ),
                                                '2',
                                                ' '
                                            ),
                                            '3',
                                            ' '
                                        ),
                                        '4',
                                        ' '
                                    ),
                                    '5',
                                    ' '
                                ),
                                '6',
                                ' '
                            ),
                            '7',
                            ' '
                        ),
                        '8',
                        ' '
                    ),
                    '9',
                    ' '
                ),
                    '  ',
                    ' '
                ),
                '  ',
                ' '
            )
        ) AS RawCountryNorm
    FROM LegacyCompanies lc
),
linked_workers AS (
    SELECT promotionID, workerID
    FROM legacy.contracts
    WHERE promotionID IS NOT NULL
    UNION ALL
    SELECT promotionID, workerID
    FROM legacy.contractoffers
    WHERE promotionID IS NOT NULL
),
worker_countries AS (
    SELECT
        lw.promotionID,
        LOWER(
            TRIM(
                REPLACE(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            REPLACE(
                                                REPLACE(
                                                    REPLACE(
                                                        REPLACE(
                                                            REPLACE(
                                                                REPLACE(
                                                                    REPLACE(
                                                                        REPLACE(
                                                                            COALESCE(
                                                                                NULLIF(TRIM(w.basedInCountry), ''),
                                                                                NULLIF(TRIM(w.birthPlaceCountry), ''),
                                                                                legacy_country.countryName,
                                                                                ''
                                                                            ),
                                                                            '.',
                                                                            ' '
                                                                        ),
                                                                        ',',
                                                                        ' '
                                                                    ),
                                                                    '-',
                                                                    ' '
                                                                ),
                                                                '''',
                                                                ' '
                                                            ),
                                                            '/',
                                                            ' '
                                                        ),
                                                        '0',
                                                        ' '
                                                    ),
                                                    '1',
                                                    ' '
                                                ),
                                                '2',
                                                ' '
                                            ),
                                            '3',
                                            ' '
                                        ),
                                        '4',
                                        ' '
                                    ),
                                    '5',
                                    ' '
                                ),
                                '6',
                                ' '
                            ),
                            '7',
                            ' '
                        ),
                        '8',
                        ' '
                    ),
                    '9',
                    ' '
                ),
                    '  ',
                    ' '
                ),
                '  ',
                ' '
            )
        ) AS WorkerCountryNorm
    FROM linked_workers lw
    JOIN legacy.workers w ON w.workerID = lw.workerID
    LEFT JOIN legacy.countries legacy_country
        ON legacy_country.countryID = COALESCE(w.basedInCountry, w.birthPlaceCountry)
),
ranked_worker_countries AS (
    SELECT
        promotionID,
        WorkerCountryNorm,
        COUNT(*) AS CountryCount,
        ROW_NUMBER() OVER (
            PARTITION BY promotionID
            ORDER BY COUNT(*) DESC, WorkerCountryNorm ASC
        ) AS CountryRank
    FROM worker_countries
    WHERE WorkerCountryNorm IS NOT NULL AND TRIM(WorkerCountryNorm) != ''
    GROUP BY promotionID, WorkerCountryNorm
),
inferred_country AS (
    SELECT promotionID, WorkerCountryNorm
    FROM ranked_worker_countries
    WHERE CountryRank = 1
),
resolved_country AS (
    SELECT
        nc.LegacyPromotionId,
        nc.Name,
        nc.Prestige,
        nc.Treasury,
        nc.AverageAudience,
        nc.Reach,
        nc.FoundedYear,
        nc.StyleName,
        nc.RawCountry,
        CASE
            -- Prefer explicit promotion country; infer from linked workers only when empty.
            WHEN nc.RawCountryNorm IS NOT NULL AND TRIM(nc.RawCountryNorm) != ''
                THEN nc.RawCountryNorm
            ELSE inferred.WorkerCountryNorm
        END AS ResolvedCountryNorm
    FROM normalized_companies nc
    LEFT JOIN inferred_country inferred
        ON inferred.promotionID = nc.LegacyPromotionId
)
INSERT INTO Companies (
    CompanyId,
    LegacyPromotionId,
    Name,
    CountryId,
    RegionId,
    Prestige,
    Treasury,
    AverageAudience,
    Reach,
    SimLevel,
    LastSimulatedAt,
    CreatedAt,
    FoundedYear,
    CompanySize,
    CurrentEra,
    CatchStyleId,
    IsPlayerControlled,
    MonthlyBurnRate
)
SELECT
    'COMP_' || rc.LegacyPromotionId,
    rc.LegacyPromotionId,
    rc.Name,
    COALESCE(alias.CountryId, 'COUNTRY_DEFAULT'),
    COALESCE(
        region.RegionId,
        (
            SELECT r2.RegionId
            FROM Regions r2
            WHERE r2.CountryId = COALESCE(alias.CountryId, 'COUNTRY_DEFAULT')
            ORDER BY r2.Name
            LIMIT 1
        ),
        'REGION_DEFAULT'
    ),
    CAST(COALESCE(rc.Prestige, 50) AS INTEGER),
    COALESCE(rc.Treasury, 0),
    COALESCE(rc.AverageAudience, 0),
    COALESCE(rc.Reach, 0),
    0,
    NULL,
    CURRENT_TIMESTAMP,
    COALESCE(rc.FoundedYear, 2024),
    'Local',
    'Foundation Era',
    CASE
        WHEN rc.StyleName IS NULL THEN 'STYLE_HYBRID'
        WHEN LOWER(TRIM(rc.StyleName)) LIKE '%pure%' THEN 'STYLE_PURE_WRESTLING'
        WHEN LOWER(TRIM(rc.StyleName)) LIKE '%sports%' THEN 'STYLE_SPORTS_ENTERTAINMENT'
        WHEN LOWER(TRIM(rc.StyleName)) LIKE '%strong%' THEN 'STYLE_STRONG_STYLE'
        WHEN LOWER(TRIM(rc.StyleName)) LIKE '%lucha%' THEN 'STYLE_LUCHA_LIBRE'
        WHEN LOWER(TRIM(rc.StyleName)) LIKE '%hardcore%' THEN 'STYLE_HARDCORE'
        ELSE 'STYLE_HYBRID'
    END,
    0,
    0.0
FROM resolved_country rc
LEFT JOIN CountryAliases alias
    ON alias.AliasNorm = rc.ResolvedCountryNorm
LEFT JOIN Regions region
    ON region.CountryId = COALESCE(alias.CountryId, 'COUNTRY_DEFAULT');

INSERT OR IGNORE INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige)
VALUES ('0', 'Free Agent', 'COUNTRY_DEFAULT', 'REGION_DEFAULT', 0);

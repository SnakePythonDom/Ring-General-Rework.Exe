-- ============================================================================
-- Map legacy company data into Ring General Companies table
-- ============================================================================

PRAGMA foreign_keys = ON;

DELETE FROM Companies;

INSERT INTO Companies (
    CompanyId,
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
    'COMP_' || LegacyPromotionId,
    Name,
    COALESCE(country.CountryId, 'COUNTRY_DEFAULT'),
    COALESCE(
        region.RegionId,
        (
            SELECT r2.RegionId
            FROM Regions r2
            WHERE r2.CountryId = COALESCE(country.CountryId, 'COUNTRY_DEFAULT')
            ORDER BY r2.Name
            LIMIT 1
        ),
        'REGION_DEFAULT'
    ),
    CAST(COALESCE(Prestige, 50) AS INTEGER),
    COALESCE(Treasury, 0),
    COALESCE(AverageAudience, 0),
    COALESCE(Reach, 0),
    0,
    NULL,
    CURRENT_TIMESTAMP,
    COALESCE(FoundedYear, 2024),
    'Local',
    'Foundation Era',
    CASE
        WHEN StyleName IS NULL THEN 'STYLE_HYBRID'
        WHEN LOWER(TRIM(StyleName)) LIKE '%pure%' THEN 'STYLE_PURE_WRESTLING'
        WHEN LOWER(TRIM(StyleName)) LIKE '%sports%' THEN 'STYLE_SPORTS_ENTERTAINMENT'
        WHEN LOWER(TRIM(StyleName)) LIKE '%strong%' THEN 'STYLE_STRONG_STYLE'
        WHEN LOWER(TRIM(StyleName)) LIKE '%lucha%' THEN 'STYLE_LUCHA_LIBRE'
        WHEN LOWER(TRIM(StyleName)) LIKE '%hardcore%' THEN 'STYLE_HARDCORE'
        ELSE 'STYLE_HYBRID'
    END,
    0,
    0.0
FROM LegacyCompanies lc
LEFT JOIN Countries country ON LOWER(country.Name) = LOWER(lc.CountryName)
LEFT JOIN Regions region ON LOWER(region.Name) = LOWER(lc.RegionName)
    AND region.CountryId = country.CountryId;

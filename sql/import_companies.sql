-- ============================================================================
-- Import companies from legacy BAKI database into staging table
-- ============================================================================

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS LegacyCompanies;
CREATE TABLE LegacyCompanies (
    LegacyPromotionId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    CountryName TEXT,
    RegionName TEXT,
    Prestige REAL,
    Treasury REAL,
    StyleName TEXT,
    FoundedYear INTEGER,
    AverageAudience INTEGER,
    Reach INTEGER
);

INSERT INTO LegacyCompanies (
    LegacyPromotionId,
    Name,
    CountryName,
    RegionName,
    Prestige,
    Treasury,
    StyleName,
    FoundedYear,
    AverageAudience,
    Reach
)
SELECT
    p.promotionID,
    p.fullName,
    c.countryName,
    r.regionName,
    p.prestige,
    p.money,
    p.style,
    p.founded,
    CAST(ROUND((
        COALESCE(p.northAmericaPop, 0)
        + COALESCE(p.southAmericaPop, 0)
        + COALESCE(p.oceaniaPop, 0)
        + COALESCE(p.asiaPop, 0)
        + COALESCE(p.europePop, 0)
        + COALESCE(p.africaPop, 0)
    ) / 6.0, 0) AS INTEGER),
    CAST(ROUND((
        COALESCE(p.northAmericaPop, 0)
        + COALESCE(p.southAmericaPop, 0)
        + COALESCE(p.oceaniaPop, 0)
        + COALESCE(p.asiaPop, 0)
        + COALESCE(p.europePop, 0)
        + COALESCE(p.africaPop, 0)
    ) * 1000.0, 0) AS INTEGER)
FROM baki.promotions p
LEFT JOIN baki.countries c ON c.countryID = p.basedInCountry
LEFT JOIN baki.regions r ON r.regionID = p.basedInRegion
WHERE p.fullName IS NOT NULL
  AND p.fullName != '';

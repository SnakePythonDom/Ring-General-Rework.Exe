-- ============================================================================
-- Import promotions from legacy BAKI database into a staging table
-- ============================================================================

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS LegacyCompanies;
CREATE TABLE LegacyCompanies (
    LegacyPromotionId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    RawCountry TEXT,
    Prestige REAL,
    Treasury REAL,
    AverageAudience INTEGER,
    Reach INTEGER,
    FoundedYear INTEGER,
    StyleName TEXT
);

INSERT INTO LegacyCompanies (
    LegacyPromotionId,
    Name,
    RawCountry,
    Prestige,
    Treasury,
    AverageAudience,
    Reach,
    FoundedYear,
    StyleName
)
SELECT
    p.promotionID,
    p.fullName,
    COALESCE(legacy_country.countryName, NULLIF(TRIM(p.basedIn), '')),
    p.prestige,
    p.money,
    NULL,
    NULL,
    p.founded,
    p.style
FROM legacy.promotions p
LEFT JOIN legacy.countries legacy_country
    ON legacy_country.countryID = p.basedInCountry;

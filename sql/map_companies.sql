-- SQLite syntax - Do not validate as T-SQL
-- Database is already attached by init.sh script

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
    -- On prùfùre le vrai pays, sinon fallback sur basedIn (continent)
    COALESCE(legacy_country.countryName, NULLIF(TRIM(p.basedIn), '')),
    p.prestige,
    p.money,
    0,
    0,
    p.founded,
    p.style
FROM legacy.promotions p
LEFT JOIN legacy.countries legacy_country
    ON legacy_country.countryID = p.basedInCountry;

-- Note: Database detachment is handled by init.sh script

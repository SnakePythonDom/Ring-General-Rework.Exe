-- ============================================================================
-- Import workers from legacy BAKI database into staging table
-- ============================================================================

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS LegacyWorkers;
CREATE TABLE LegacyWorkers (
    LegacyWorkerId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Gender TEXT,
    BirthDate TEXT,
    InRing INTEGER,
    Entertainment INTEGER,
    Story INTEGER,
    Popularity INTEGER,
    Fatigue INTEGER,
    InjuryStatus TEXT,
    Momentum INTEGER,
    RoleTv TEXT,
    CompanyLegacyId INTEGER,
    CountryName TEXT,
    RegionName TEXT
);

INSERT INTO LegacyWorkers (
    LegacyWorkerId,
    Name,
    Gender,
    BirthDate,
    InRing,
    Entertainment,
    Story,
    Popularity,
    Fatigue,
    InjuryStatus,
    Momentum,
    RoleTv,
    CompanyLegacyId,
    CountryName,
    RegionName
)
SELECT
    w.workerID,
    w.name,
    w.gender,
    w.birthDate,
    CAST(COALESCE(w.wrestlingSkill, 50) AS INTEGER),
    CAST(COALESCE(w.entertainment, 50) AS INTEGER),
    CAST(COALESCE(w.psychology, 50) AS INTEGER),
    CAST(ROUND((
        COALESCE(w.northAmericaPop, 0)
        + COALESCE(w.southAmericaPop, 0)
        + COALESCE(w.oceaniaPop, 0)
        + COALESCE(w.asiaPop, 0)
        + COALESCE(w.europePop, 0)
        + COALESCE(w.africaPop, 0)
    ) / 6.0, 0) AS INTEGER),
    CAST(COALESCE(w.fatigue, 0) AS INTEGER),
    'AUCUNE',
    50,
    'NONE',
    latest_contract.promotionID,
    country.countryName,
    region.regionName
FROM baki.workers w
LEFT JOIN (
    SELECT c1.workerID, c1.promotionID
    FROM baki.contracts c1
    WHERE c1.finalised = 1
      AND c1.expired = 0
      AND c1.contractStarted = 1
      AND c1.contractID = (
          SELECT MAX(c2.contractID)
          FROM baki.contracts c2
          WHERE c2.workerID = c1.workerID
            AND c2.finalised = 1
            AND c2.expired = 0
            AND c2.contractStarted = 1
      )
) AS latest_contract ON latest_contract.workerID = w.workerID
LEFT JOIN baki.countries country ON country.countryID = COALESCE(w.basedInCountry, w.birthPlaceCountry)
LEFT JOIN baki.regions region ON region.regionID = COALESCE(w.basedInRegion, w.birthPlaceRegion)
WHERE w.name IS NOT NULL
  AND w.name != '';

-- ============================================================================
-- Map legacy worker data into Ring General Workers table
-- ============================================================================

PRAGMA foreign_keys = ON;

DELETE FROM Workers;

INSERT INTO Workers (
    WorkerId,
    Name,
    FirstName,
    LastName,
    RingName,
    CompanyId,
    Nationality,
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
    SimLevel,
    LastSimulatedAt,
    CreatedAt
)
SELECT
    CAST(lw.LegacyWorkerId AS TEXT),
    lw.Name,
    NULL,
    NULL,
    lw.Name,
    CASE
        WHEN companies.CompanyId IS NOT NULL THEN companies.CompanyId
        ELSE NULL
    END,
    COALESCE(country.Code, 'WLD'),
    CASE
        WHEN LOWER(lw.Gender) = 'male' THEN 'M'
        WHEN LOWER(lw.Gender) = 'female' THEN 'F'
        ELSE 'Other'
    END,
    lw.BirthDate,
    COALESCE(lw.InRing, 50),
    COALESCE(lw.Entertainment, 50),
    COALESCE(lw.Story, 50),
    COALESCE(lw.Popularity, 50),
    COALESCE(lw.Fatigue, 0),
    COALESCE(lw.InjuryStatus, 'AUCUNE'),
    COALESCE(lw.Momentum, 50),
    COALESCE(lw.RoleTv, 'NONE'),
    0,
    NULL,
    CURRENT_TIMESTAMP
FROM LegacyWorkers lw
LEFT JOIN Countries country ON LOWER(country.Name) = LOWER(lw.CountryName)
LEFT JOIN Companies companies ON companies.CompanyId = 'COMP_' || lw.CompanyLegacyId;

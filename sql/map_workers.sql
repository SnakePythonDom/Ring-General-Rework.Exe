-- SQLite syntax - Do not validate as T-SQL
-- 1. On s'assure que le pays par défaut existe
INSERT INTO Countries (CountryId, Code, Name)
SELECT 'COUNTRY_DEFAULT', 'WLD', 'World'
WHERE NOT EXISTS (SELECT 1 FROM Countries WHERE CountryId = 'COUNTRY_DEFAULT');

DELETE FROM Workers;

-- 2. Insertion directe (sans le bloc WITH qui pose problème)
INSERT INTO Workers (
    WorkerId, Name, FirstName, LastName, RingName, CompanyId, CountryId,
    Gender, BirthDate, InRing, Entertainment, Story, Popularity,
    Fatigue, InjuryStatus, Momentum, RoleTv, SimLevel, LastSimulatedAt, CreatedAt
)
SELECT
    'WRK_' || lw.LegacyWorkerId,
    lw.Name,
    NULL,
    NULL,
    lw.Name,
    COALESCE('COMP_' || lw.CompanyLegacyId, '0'),
    -- Résolution du pays : on cherche d'abord dans les alias, sinon les fallbacks manuels, sinon défaut
    COALESCE(
        alias.CountryId, 
        CASE 
            WHEN LOWER(lw.CountryName) LIKE '%north america%' THEN 'COUNTRY_USA'
            WHEN LOWER(lw.CountryName) LIKE '%asia%'          THEN 'COUNTRY_JAPAN'
            WHEN LOWER(lw.CountryName) LIKE '%europe%'        THEN 'COUNTRY_UK'
            WHEN LOWER(lw.CountryName) LIKE '%mexico%'        THEN 'COUNTRY_MEXICO'
            ELSE NULL 
        END, 
        'COUNTRY_DEFAULT'
    ),
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
LEFT JOIN CountryAliases alias ON alias.AliasNorm = LOWER(TRIM(lw.CountryName));
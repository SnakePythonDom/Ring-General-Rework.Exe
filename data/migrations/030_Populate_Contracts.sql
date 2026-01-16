-- =================================================================================
-- Migration: 030_Populate_Contracts.sql
-- Description: Assign workers to companies and create contracts
-- Règle d'Or: Salaire ∝ Popularité
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. ASSIGNER WORKERS AUX COMPANIES (80% signés)
-- =================================================================================

-- Assigner les TOP workers (Popularity > 60) aux compagnies majeures de leur pays
UPDATE Workers
SET CompanyId = (
    SELECT c.CompanyId 
    FROM Companies c 
    WHERE c.CountryId = Workers.Nationality
    AND c.Prestige > 60
    ORDER BY c.Prestige DESC
    LIMIT 1
)
WHERE Popularity > 60
AND CompanyId IS NULL
AND EXISTS (SELECT 1 FROM Companies c WHERE c.CountryId = Workers.Nationality);

-- Assigner les workers MID (Popularity 35-60) aux compagnies moyennes/indies
UPDATE Workers
SET CompanyId = (
    SELECT c.CompanyId 
    FROM Companies c 
    WHERE c.CountryId = Workers.Nationality
    ORDER BY RANDOM()
    LIMIT 1
)
WHERE Popularity BETWEEN 35 AND 60
AND CompanyId IS NULL
AND abs(random() % 100) < 85 -- 85% de cette tranche sont signés
AND EXISTS (SELECT 1 FROM Companies c WHERE c.CountryId = Workers.Nationality);

-- Assigner les LOWER workers (Popularity < 35) aux indies ou laisser free agent
UPDATE Workers
SET CompanyId = (
    SELECT c.CompanyId 
    FROM Companies c 
    WHERE c.CountryId = Workers.Nationality
    AND c.Prestige < 50 -- Indies
    ORDER BY RANDOM()
    LIMIT 1
)
WHERE Popularity < 35
AND CompanyId IS NULL
AND abs(random() % 100) < 60 -- 60% de cette tranche sont signés
AND EXISTS (SELECT 1 FROM Companies c WHERE c.CountryId = Workers.Nationality AND c.Prestige < 50);

-- =================================================================================
-- 2. CRÉER CONTRATS (Pour tous les workers assignés)
-- =================================================================================

INSERT OR IGNORE INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary, IsExclusive, Status, PayFrequency, StartWeek)
SELECT 
    w.WorkerId,
    w.CompanyId,
    1 as StartDate,
    -- Durée: Stars = plus longs contrats
    CASE 
        WHEN w.Popularity > 75 THEN 104 + abs(random() % 52) 
        WHEN w.Popularity > 50 THEN 52 + abs(random() % 52)  
        ELSE 26 + abs(random() % 52)                         
    END as EndDate,
    -- SALAIRE
    CASE 
        WHEN w.Popularity >= 85 THEN 150000 + (w.Popularity * 2000) + abs(random() % 100000)
        WHEN w.Popularity >= 75 THEN 80000 + (w.Popularity * 1200) + abs(random() % 80000)
        WHEN w.Popularity >= 60 THEN 40000 + (w.Popularity * 700) + abs(random() % 50000)
        WHEN w.Popularity >= 45 THEN 15000 + (w.Popularity * 400) + abs(random() % 25000)
        WHEN w.Popularity >= 30 THEN 8000 + (w.Popularity * 200) + abs(random() % 12000)
        ELSE 3000 + (w.Popularity * 100) + abs(random() % 5000)
    END as Salary,
    CASE WHEN w.Popularity > 70 THEN 1 WHEN abs(random() % 100) < 55 THEN 1 ELSE 0 END as IsExclusive,
    'actif' as Status,
    'Hebdomadaire' as PayFrequency,
    1 as StartWeek
FROM Workers w
WHERE w.CompanyId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM Contracts c WHERE c.WorkerId = w.WorkerId);

COMMIT;

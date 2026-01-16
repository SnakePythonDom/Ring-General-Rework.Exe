-- =================================================================================
-- Migration: 031_Populate_Titles.sql
-- Description: Create titles for each company and assign champions
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. WORLD CHAMPIONSHIPS (Prestige > 60)
-- =================================================================================

INSERT OR IGNORE INTO Titles (TitleId, CompanyId, Name, Prestige, HolderWorkerId)
SELECT 
    'TITLE_' || c.CompanyId || '_WORLD' as TitleId,
    c.CompanyId,
    c.Name || ' World Championship' as Name,
    88 + abs(random() % 10) as Prestige, -- 88-98
    (
        SELECT w.WorkerId 
        FROM Workers w 
        WHERE w.CompanyId = c.CompanyId 
        ORDER BY w.Popularity DESC, w.InRing DESC
        LIMIT 1
    ) as HolderWorkerId
FROM Companies c
WHERE c.Prestige > 60;

-- =================================================================================
-- 2. SECONDARY TITLES (Prestige > 50)
-- =================================================================================

INSERT OR IGNORE INTO Titles (TitleId, CompanyId, Name, Prestige, HolderWorkerId)
SELECT 
    'TITLE_' || c.CompanyId || '_SEC' as TitleId,
    c.CompanyId,
    CASE 
        WHEN c.CountryId = 'USA' THEN c.Name || ' Intercontinental Championship'
        WHEN c.CountryId = 'JPN' THEN c.Name || ' NEVER Openweight Championship'
        WHEN c.CountryId = 'MEX' THEN c.Name || ' Nacional Championship'
        WHEN c.CountryId = 'GBR' THEN c.Name || ' British Championship'
        ELSE c.Name || ' Continental Championship'
    END as Name,
    70 + abs(random() % 15) as Prestige, -- 70-85
    (
        SELECT w.WorkerId 
        FROM Workers w 
        WHERE w.CompanyId = c.CompanyId 
        AND w.WorkerId NOT IN (SELECT HolderWorkerId FROM Titles WHERE HolderWorkerId IS NOT NULL)
        ORDER BY w.Popularity DESC, w.InRing DESC
        LIMIT 1 OFFSET 1 -- Prend le 2ème top worker
    ) as HolderWorkerId
FROM Companies c
WHERE c.Prestige > 50;

-- =================================================================================
-- 3. TAG TEAM CHAMPIONSHIPS (Prestige > 55)
-- =================================================================================

INSERT OR IGNORE INTO Titles (TitleId, CompanyId, Name, Prestige, HolderWorkerId)
SELECT 
    'TITLE_' || c.CompanyId || '_TAG' as TitleId,
    c.CompanyId,
    c.Name || ' Tag Team Championship' as Name,
    60 + abs(random() % 18) as Prestige, -- 60-78
    NULL as HolderWorkerId -- Tag teams gérés séparément
FROM Companies c
WHERE c.Prestige > 55;

-- =================================================================================
-- 4. TITLE REIGNS (Règnes actuels + Historique)
-- =================================================================================

-- Règnes actuels
INSERT OR IGNORE INTO TitleReigns (TitleId, WorkerId, StartDate, EndDate, IsCurrent)
SELECT 
    t.TitleId,
    t.HolderWorkerId,
    1 as StartDate, -- Début de la simulation
    NULL as EndDate,
    1 as IsCurrent
FROM Titles t
WHERE t.HolderWorkerId IS NOT NULL;

-- Règnes passés (2-3 par titre majeur)
INSERT OR IGNORE INTO TitleReigns (TitleId, WorkerId, StartDate, EndDate, IsCurrent)
SELECT 
    t.TitleId,
    w.WorkerId,
    -200 - abs(random() % 150) as StartDate, -- Il y a 200-350 semaines
    -50 - abs(random() % 100) as EndDate,    -- Terminé il y a 50-150 semaines
    0 as IsCurrent
FROM Titles t
JOIN Workers w ON w.CompanyId = t.CompanyId
WHERE t.HolderWorkerId IS NOT NULL
AND w.Popularity > 50
AND w.WorkerId != t.HolderWorkerId
ORDER BY RANDOM()
LIMIT (SELECT COUNT(*) * 2 FROM Titles WHERE HolderWorkerId IS NOT NULL);

COMMIT;

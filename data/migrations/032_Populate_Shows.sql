-- =================================================================================
-- Migration: 032_Populate_Shows.sql
-- Description: Create weekly shows and monthly PPVs (Schema Français)
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. SHOWS HEBDOMADAIRES (52 semaines pour compagnies majeures)
-- =================================================================================

INSERT OR IGNORE INTO shows (show_id, compagnie_id, nom, semaine, region, duree, Date, tv_deal_id, lieu)
SELECT 
    'SHOW_' || c.CompanyId || '_W' || weeks.WeekNum as show_id,
    c.CompanyId as compagnie_id,
    c.Name || ' Weekly' as nom,
    weeks.WeekNum as semaine,
    c.RegionId as region,
    CASE 
        WHEN c.Prestige > 80 THEN 120 
        WHEN c.Prestige > 60 THEN 90  
        ELSE 60                        
    END as duree,
    date('now', '+' || ((weeks.WeekNum - 1) * 7) || ' days') as Date,
    (SELECT tv.TvDealId FROM TVDeals tv WHERE tv.CompanyId = c.CompanyId LIMIT 1) as tv_deal_id,
    COALESCE(
        (SELECT v.Name FROM Venues v WHERE v.RegionId = c.RegionId ORDER BY RANDOM() LIMIT 1),
        'Compagnie Venue'
    ) as lieu
FROM Companies c
CROSS JOIN (
    WITH RECURSIVE generate_weeks(WeekNum) AS (
        SELECT 1
        UNION ALL
        SELECT WeekNum + 1 FROM generate_weeks WHERE WeekNum < 52
    )
    SELECT WeekNum FROM generate_weeks
) weeks
WHERE c.Prestige > 55;

-- =================================================================================
-- 2. PPV MENSUELS (12 par an pour compagnies top)
-- =================================================================================

INSERT OR IGNORE INTO shows (show_id, compagnie_id, nom, semaine, region, duree, Date, lieu)
SELECT 
    'SHOW_' || c.CompanyId || '_PPV_M' || months.MonthNum as show_id,
    c.CompanyId as compagnie_id,
    c.Name || ' ' || CASE months.MonthNum
        WHEN 1 THEN 'New Year Revolution'
        WHEN 2 THEN 'Valentines Vengeance'
        WHEN 3 THEN 'Spring Brawl'
        WHEN 4 THEN 'April Assault'
        WHEN 5 THEN 'May Mayhem'
        WHEN 6 THEN 'Summer Showdown'
        WHEN 7 THEN 'July Judgement'
        WHEN 8 THEN 'August Armageddon'
        WHEN 9 THEN 'September Storm'
        WHEN 10 THEN 'October Onslaught'
        WHEN 11 THEN 'November Nightmare'
        ELSE 'December Destruction'
    END as nom,
    months.MonthNum * 4 as semaine,
    c.RegionId as region,
    CASE 
        WHEN c.Prestige > 80 THEN 240 
        WHEN c.Prestige > 65 THEN 180 
        ELSE 150                       
    END as duree,
    date('now', '+' || ((months.MonthNum * 4 - 1) * 7) || ' days') as Date,
    COALESCE(
        (SELECT v.Name FROM Venues v WHERE v.RegionId = c.RegionId ORDER BY v.Capacity DESC LIMIT 1),
        'Major Arena'
    ) as lieu
FROM Companies c
CROSS JOIN (
    WITH RECURSIVE generate_months(MonthNum) AS (
        SELECT 1
        UNION ALL
        SELECT MonthNum + 1 FROM generate_months WHERE MonthNum < 12
    )
    SELECT MonthNum FROM generate_months
) months
WHERE c.Prestige > 65;

COMMIT;


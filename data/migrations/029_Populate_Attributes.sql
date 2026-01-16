-- =================================================================================
-- Migration: 029_Populate_Attributes.sql
-- Description: Populate WorkerAttributes and WorkerMentalAttributes
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. WORKER ATTRIBUTES (InRing, Entertainment, Popularity, etc.)
-- =================================================================================

INSERT OR IGNORE INTO WorkerAttributes (WorkerId, InRing, Entertainment, Story, Popularity, Stamina, Charisma)
SELECT 
    w.WorkerId,
    w.InRing,
    w.Entertainment,
    w.Story,
    w.Popularity,
    -- Stamina: 45-95
    45 + abs(random() % 50) as Stamina,
    -- Charisma: 40-95
    40 + abs(random() % 55) as Charisma
FROM Workers w
WHERE NOT EXISTS (SELECT 1 FROM WorkerAttributes wa WHERE wa.WorkerId = w.WorkerId);

-- =================================================================================
-- 2. MENTAL ATTRIBUTES (Schema Français)
-- =================================================================================

INSERT OR IGNORE INTO WorkerMentalAttributes (
    WorkerId, 
    Ambition, 
    Loyauté, 
    Professionnalisme, 
    Pression, 
    Tempérament, 
    Égoïsme, 
    Détermination, 
    Adaptabilité, 
    Influence, 
    Sportivité
)
SELECT 
    w.WorkerId,
    20 + abs(random() % 70) as Ambition,
    30 + abs(random() % 65) as Loyauté,
    40 + abs(random() % 55) as Professionnalisme,
    30 + abs(random() % 60) as Pression,
    40 + abs(random() % 50) as Tempérament,
    10 + abs(random() % 60) as Égoïsme,
    45 + abs(random() % 50) as Détermination,
    40 + abs(random() % 55) as Adaptabilité,
    10 + abs(random() % 70) as Influence,
    35 + abs(random() % 60) as Sportivité
FROM Workers w
WHERE NOT EXISTS (SELECT 1 FROM WorkerMentalAttributes ma WHERE ma.WorkerId = w.WorkerId);

COMMIT;


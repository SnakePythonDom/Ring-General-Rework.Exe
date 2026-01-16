-- =================================================================================
-- Migration: 033_Populate_Staff.sql
-- Description: Generate staff (Bookers, Coaches, Agents, Referees) for companies
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. BOOKERS (1-2 par compagnie majeure)
-- =================================================================================

INSERT OR IGNORE INTO Workers (
    WorkerId, Name, FirstName, LastName, RingName,
    CompanyId, Nationality, Gender, BirthDate,
    InRing, Entertainment, Story, Popularity,
    Fatigue, Momentum, RoleTv, SimLevel, WorkerType
)
SELECT 
    'STAFF_BOOKER_' || c.CompanyId || '_' || substr(hex(randomblob(3)), 1, 4) as WorkerId,
    CASE abs(random() % 10)
        WHEN 0 THEN 'Paul Heyman Jr.'
        WHEN 1 THEN 'Tony Schiavone II'
        WHEN 2 THEN 'Gedo Tanaka'
        WHEN 3 THEN 'Dutch Mantell III'
        WHEN 4 THEN 'Jim Cornette Jr.'
        WHEN 5 THEN 'Dusty Rhodes Legacy'
        WHEN 6 THEN 'Pat Patterson II'
        WHEN 7 THEN 'Arn Anderson Jr.'
        WHEN 8 THEN 'Michael Hayes III'
        ELSE 'Road Dogg Legacy'
    END as Name,
    'Booker' as FirstName,
    c.Name as LastName,
    'Head Booker' as RingName,
    c.CompanyId,
    COALESCE(c.CountryId, 'USA') as Nationality,
    'Male' as Gender,
    date('now', '-' || (38 + abs(random() % 22)) || ' years') as BirthDate, -- 38-60 ans
    30 + abs(random() % 30) as InRing, -- Anciens catcheurs: 30-60
    70 + abs(random() % 25) as Entertainment, -- Bons créateurs: 70-95
    75 + abs(random() % 20) as Story, -- Experts storylines: 75-95
    40 + abs(random() % 30) as Popularity, -- Connus des fans hardcore
    0 as Fatigue,
    0 as Momentum,
    'NONE' as RoleTv,
    1 as SimLevel,
    'STAFF' as WorkerType
FROM Companies c
WHERE c.Prestige > 45;

-- =================================================================================
-- 2. COACHES (2-4 par compagnie avec Youth Structure)
-- =================================================================================

INSERT OR IGNORE INTO Workers (
    WorkerId, Name, FirstName, LastName, RingName,
    CompanyId, Nationality, Gender, BirthDate,
    InRing, Entertainment, Story, Popularity,
    Fatigue, Momentum, RoleTv, SimLevel, WorkerType
)
SELECT 
    'STAFF_COACH_' || ys.YouthStructureId || '_' || nums.n as WorkerId,
    CASE abs(random() % 10)
        WHEN 0 THEN 'Matt Bloom Jr.'
        WHEN 1 THEN 'Terry Taylor III'
        WHEN 2 THEN 'Norman Smiley II'
        WHEN 3 THEN 'Scotty 2 Hotty Jr.'
        WHEN 4 THEN 'Steve Keirn Legacy'
        WHEN 5 THEN 'Dr. Tom Prichard II'
        WHEN 6 THEN 'Billy Gunn Jr.'
        WHEN 7 THEN 'Road Dogg Jr.'
        WHEN 8 THEN 'Sara Del Rey II'
        ELSE 'Lance Storm Jr.'
    END as Name,
    'Coach' as FirstName,
    ys.Name as LastName,
    'Head Coach' as RingName,
    ys.CompanyId,
    COALESCE(ys.CountryId, 'USA') as Nationality,
    CASE WHEN abs(random() % 100) < 80 THEN 'Male' ELSE 'Female' END as Gender,
    date('now', '-' || (35 + abs(random() % 20)) || ' years') as BirthDate, -- 35-55 ans
    60 + abs(random() % 30) as InRing, -- Bons techniciens: 60-90
    40 + abs(random() % 30) as Entertainment, -- Variable: 40-70
    45 + abs(random() % 30) as Story, -- 45-75
    25 + abs(random() % 25) as Popularity, -- 25-50
    0 as Fatigue,
    0 as Momentum,
    'NONE' as RoleTv,
    1 as SimLevel,
    'STAFF' as WorkerType
FROM YouthStructures ys
CROSS JOIN (SELECT 1 as n UNION SELECT 2 UNION SELECT 3) nums; -- 3 coaches par structure

-- =================================================================================
-- 3. AGENTS / PRODUCERS (1-3 par compagnie majeure)
-- =================================================================================

INSERT OR IGNORE INTO Workers (
    WorkerId, Name, FirstName, LastName, RingName,
    CompanyId, Nationality, Gender, BirthDate,
    InRing, Entertainment, Story, Popularity,
    Fatigue, Momentum, RoleTv, SimLevel, WorkerType
)
SELECT 
    'STAFF_AGENT_' || c.CompanyId || '_' || nums.n as WorkerId,
    CASE abs(random() % 8)
        WHEN 0 THEN 'Michael Hayes Jr.'
        WHEN 1 THEN 'Jamie Noble II'
        WHEN 2 THEN 'Fit Finlay Jr.'
        WHEN 3 THEN 'Dean Malenko II'
        WHEN 4 THEN 'Tyson Kidd Jr.'
        WHEN 5 THEN 'Shane Helms II'
        WHEN 6 THEN 'Adam Pearce Jr.'
        ELSE 'Shane McMahon Jr.'
    END as Name,
    'Agent' as FirstName,
    c.Name as LastName,
    'Match Producer' as RingName,
    c.CompanyId,
    COALESCE(c.CountryId, 'USA') as Nationality,
    'Male' as Gender,
    date('now', '-' || (40 + abs(random() % 18)) || ' years') as BirthDate, -- 40-58 ans
    55 + abs(random() % 35) as InRing, -- Anciens bon catcheurs: 55-90
    45 + abs(random() % 30) as Entertainment, -- 45-75
    60 + abs(random() % 30) as Story, -- Bons en structure match: 60-90
    35 + abs(random() % 30) as Popularity, -- 35-65
    0 as Fatigue,
    0 as Momentum,
    'NONE' as RoleTv,
    1 as SimLevel,
    'STAFF' as WorkerType
FROM Companies c
CROSS JOIN (SELECT 1 as n UNION SELECT 2) nums -- 2 agents par company
WHERE c.Prestige > 55;

-- =================================================================================
-- 4. REFEREES (2-4 par compagnie)
-- =================================================================================

INSERT OR IGNORE INTO Workers (
    WorkerId, Name, FirstName, LastName, RingName,
    CompanyId, Nationality, Gender, BirthDate,
    InRing, Entertainment, Story, Popularity,
    Fatigue, Momentum, RoleTv, SimLevel, WorkerType
)
SELECT 
    'STAFF_REF_' || c.CompanyId || '_' || nums.n as WorkerId,
    CASE abs(random() % 8)
        WHEN 0 THEN 'Charles Robinson III'
        WHEN 1 THEN 'Mike Chioda Jr.'
        WHEN 2 THEN 'Nick Patrick II'
        WHEN 3 THEN 'Earl Hebner Jr.'
        WHEN 4 THEN 'John Cone II'
        WHEN 5 THEN 'Aubrey Edwards II'
        WHEN 6 THEN 'Red Shoes Jr.'
        ELSE 'Rick Knox II'
    END as Name,
    'Referee' as FirstName,
    c.Name as LastName,
    'Senior Referee' as RingName,
    c.CompanyId,
    COALESCE(c.CountryId, 'USA') as Nationality,
    CASE WHEN abs(random() % 100) < 85 THEN 'Male' ELSE 'Female' END as Gender,
    date('now', '-' || (30 + abs(random() % 25)) || ' years') as BirthDate, -- 30-55 ans
    35 + abs(random() % 25) as InRing, -- Compétents physiquement: 35-60
    45 + abs(random() % 30) as Entertainment, -- 45-75
    40 + abs(random() % 25) as Story, -- 40-65
    20 + abs(random() % 30) as Popularity, -- 20-50 (quelques refs célèbres)
    0 as Fatigue,
    0 as Momentum,
    'NONE' as RoleTv,
    1 as SimLevel,
    'STAFF' as WorkerType
FROM Companies c
CROSS JOIN (SELECT 1 as n UNION SELECT 2 UNION SELECT 3) nums -- 3 refs par company
WHERE c.Prestige > 40;

COMMIT;


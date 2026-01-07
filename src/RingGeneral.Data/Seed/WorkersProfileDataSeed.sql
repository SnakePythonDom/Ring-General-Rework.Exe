-- ============================================================================
-- Seed Data: Workers Profile Data (Specializations, Geography, Relations, Factions)
-- Version: 1.0
-- Date: 2026-01-07
-- Description: Complete ProfileView seed data
-- ============================================================================

-- ============================================================================
-- SECTION 1: WORKER SPECIALIZATIONS
-- ============================================================================

-- John Cena: Brawler + Power
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Brawler', 1 FROM Workers WHERE Name = 'John Cena';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Power', 2 FROM Workers WHERE Name = 'John Cena';

-- Randy Orton: Technical + Brawler
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Technical', 1 FROM Workers WHERE Name = 'Randy Orton';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Brawler', 2 FROM Workers WHERE Name = 'Randy Orton';

-- The Rock: Showman + Power
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Showman', 1 FROM Workers WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson' LIMIT 1;

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Power', 2 FROM Workers WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson' LIMIT 1;

-- AJ Styles: High-Flyer + Technical
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'HighFlyer', 1 FROM Workers WHERE Name = 'AJ Styles';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Technical', 2 FROM Workers WHERE Name = 'AJ Styles';

-- CM Punk: Technical + Showman
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Technical', 1 FROM Workers WHERE Name = 'CM Punk';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Showman', 2 FROM Workers WHERE Name = 'CM Punk';

-- Roman Reigns: Power + Brawler
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Power', 1 FROM Workers WHERE Name = 'Roman Reigns';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Brawler', 2 FROM Workers WHERE Name = 'Roman Reigns';

-- Seth Rollins: AllRounder + HighFlyer
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'AllRounder', 1 FROM Workers WHERE Name = 'Seth Rollins';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'HighFlyer', 2 FROM Workers WHERE Name = 'Seth Rollins';

-- Dean Ambrose: Hardcore + Brawler
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Hardcore', 1 FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%' LIMIT 1;

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Brawler', 2 FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%' LIMIT 1;

-- Brock Lesnar: Power (pure)
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Power', 1 FROM Workers WHERE Name = 'Brock Lesnar';

-- Finn Balor: HighFlyer + Technical
INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'HighFlyer', 1 FROM Workers WHERE Name = 'Finn Balor';

INSERT OR IGNORE INTO WorkerSpecializations (WorkerId, Specialization, Level)
SELECT Id, 'Technical', 2 FROM Workers WHERE Name = 'Finn Balor';

-- ============================================================================
-- SECTION 2: GEOGRAPHY & PERSONAL INFO
-- ============================================================================

-- John Cena
UPDATE Workers
SET
    BirthCity = 'West Newbury',
    BirthCountry = 'USA',
    ResidenceCity = 'Tampa',
    ResidenceState = 'Florida',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Prototype / Doctor of Thuganomics / Never Give Up',
    Alignment = 'Face',
    PushLevel = 'MainEvent',
    TvRole = 95,
    BookingIntent = 'Top babyface, franchise player. Book strong in main events.'
WHERE Name = 'John Cena';

-- Randy Orton
UPDATE Workers
SET
    BirthCity = 'Knoxville',
    BirthCountry = 'USA',
    ResidenceCity = 'St. Charles',
    ResidenceState = 'Missouri',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Viper / Legend Killer / Apex Predator',
    Alignment = 'Heel',
    PushLevel = 'MainEvent',
    TvRole = 92,
    BookingIntent = 'Heel authority figure. Methodical villain. RKO out of nowhere.'
WHERE Name = 'Randy Orton';

-- The Rock
UPDATE Workers
SET
    BirthCity = 'Hayward',
    BirthCountry = 'USA',
    ResidenceCity = 'Los Angeles',
    ResidenceState = 'California',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Great One / People''s Champion',
    Alignment = 'Face',
    PushLevel = 'MainEvent',
    TvRole = 98,
    BookingIntent = 'Special attraction. Part-time megastar. Huge drawing power.'
WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson';

-- AJ Styles
UPDATE Workers
SET
    BirthCity = 'Jacksonville',
    BirthCountry = 'USA',
    ResidenceCity = 'Gainesville',
    ResidenceState = 'Georgia',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Phenomenal One',
    Alignment = 'Tweener',
    PushLevel = 'UpperMid',
    TvRole = 88,
    BookingIntent = 'Workrate machine. Can work with anyone. Reliable upper card.'
WHERE Name = 'AJ Styles';

-- CM Punk
UPDATE Workers
SET
    BirthCity = 'Chicago',
    BirthCountry = 'USA',
    ResidenceCity = 'Chicago',
    ResidenceState = 'Illinois',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Best in the World / Straight Edge Savior',
    Alignment = 'Tweener',
    PushLevel = 'MainEvent',
    TvRole = 90,
    BookingIntent = 'Anti-establishment rebel. Shoots from the hip. Unpredictable.'
WHERE Name = 'CM Punk';

-- Roman Reigns
UPDATE Workers
SET
    BirthCity = 'Pensacola',
    BirthCountry = 'USA',
    ResidenceCity = 'Tampa',
    ResidenceState = 'Florida',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Tribal Chief / Head of the Table',
    Alignment = 'Heel',
    PushLevel = 'MainEvent',
    TvRole = 98,
    BookingIntent = 'Dominant heel champion. Long reign. Acknowledge him.'
WHERE Name = 'Roman Reigns';

-- Seth Rollins
UPDATE Workers
SET
    BirthCity = 'Buffalo',
    BirthCountry = 'USA',
    ResidenceCity = 'Davenport',
    ResidenceState = 'Iowa',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Architect / The Visionary',
    Alignment = 'Face',
    PushLevel = 'MainEvent',
    TvRole = 90,
    BookingIntent = 'Versatile top guy. Can do heel or face. Workhorse.'
WHERE Name = 'Seth Rollins';

-- Dean Ambrose / Jon Moxley
UPDATE Workers
SET
    BirthCity = 'Cincinnati',
    BirthCountry = 'USA',
    ResidenceCity = 'Las Vegas',
    ResidenceState = 'Nevada',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Lunatic Fringe / The Purveyor of Violence',
    Alignment = 'Tweener',
    PushLevel = 'UpperMid',
    TvRole = 82,
    BookingIntent = 'Chaotic wildcard. Hardcore matches. Unpredictable storytelling.'
WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%';

-- Brock Lesnar
UPDATE Workers
SET
    BirthCity = 'Webster',
    BirthCountry = 'USA',
    ResidenceCity = 'Maryfield',
    ResidenceState = 'Saskatchewan',
    ResidenceCountry = 'Canada',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Beast Incarnate',
    Alignment = 'Heel',
    PushLevel = 'MainEvent',
    TvRole = 95,
    BookingIntent = 'Special attraction monster. Destroy everyone. Limited dates.'
WHERE Name = 'Brock Lesnar';

-- Finn Balor
UPDATE Workers
SET
    BirthCity = 'Bray',
    BirthCountry = 'Ireland',
    ResidenceCity = 'Orlando',
    ResidenceState = 'Florida',
    ResidenceCountry = 'USA',
    Handedness = 'Right',
    FightingStance = 'Orthodox',
    CurrentGimmick = 'The Demon / Prince',
    Alignment = 'Face',
    PushLevel = 'UpperMid',
    TvRole = 85,
    BookingIntent = 'Strong mid-card face. Demon gimmick for big matches.'
WHERE Name = 'Finn Balor';

-- ============================================================================
-- SECTION 3: WORKER RELATIONS
-- ============================================================================

-- John Cena & Randy Orton: Amitié / Rivalry over the years
INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Rivalite',
    95,
    'Legendary rivals. Have fought countless times. Great chemistry.',
    1
FROM Workers w1, Workers w2
WHERE w1.Name = 'John Cena' AND w2.Name = 'Randy Orton';

-- The Rock & John Cena: Rivalry
INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Rivalite',
    98,
    'Once in a Lifetime (x2). Dream match. Incredible chemistry.',
    1
FROM Workers w1, Workers w2
WHERE (w1.Name = 'John Cena' AND (w2.Name LIKE '%Rock%' OR w2.Name = 'Dwayne Johnson'))
   OR ((w1.Name LIKE '%Rock%' OR w1.Name = 'Dwayne Johnson') AND w2.Name = 'John Cena');

-- Seth Rollins & Roman Reigns & Dean Ambrose: Fraternité (The Shield)
INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Fraternite',
    90,
    'Shield brothers. Unbreakable bond despite betrayals.',
    1
FROM Workers w1, Workers w2
WHERE (w1.Name = 'Seth Rollins' AND w2.Name = 'Roman Reigns')
   OR (w1.Name = 'Roman Reigns' AND w2.Name = 'Seth Rollins');

INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Fraternite',
    88,
    'Shield brothers. Wild card partnership.',
    1
FROM Workers w1, Workers w2
WHERE (w1.Name = 'Seth Rollins' AND (w2.Name LIKE '%Ambrose%' OR w2.Name LIKE '%Moxley%'))
   OR ((w1.Name LIKE '%Ambrose%' OR w1.Name LIKE '%Moxley%') AND w2.Name = 'Seth Rollins');

INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Fraternite',
    85,
    'Shield brothers. Contrasting styles.',
    1
FROM Workers w1, Workers w2
WHERE (w1.Name = 'Roman Reigns' AND (w2.Name LIKE '%Ambrose%' OR w2.Name LIKE '%Moxley%'))
   OR ((w1.Name LIKE '%Ambrose%' OR w1.Name LIKE '%Moxley%') AND w2.Name = 'Roman Reigns');

-- AJ Styles & John Cena: Amitié (Mutual respect)
INSERT OR IGNORE INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic)
SELECT
    CASE WHEN w1.Id < w2.Id THEN w1.Id ELSE w2.Id END,
    CASE WHEN w1.Id < w2.Id THEN w2.Id ELSE w1.Id END,
    'Amitie',
    82,
    'Mutual professional respect. Great match chemistry.',
    0
FROM Workers w1, Workers w2
WHERE (w1.Name = 'John Cena' AND w2.Name = 'AJ Styles')
   OR (w1.Name = 'AJ Styles' AND w2.Name = 'John Cena');

-- ============================================================================
-- SECTION 4: FACTIONS
-- ============================================================================

-- The Shield (Trio - Most famous version)
INSERT OR IGNORE INTO Factions (Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear)
SELECT
    'The Shield',
    'Trio',
    (SELECT Id FROM Workers WHERE Name = 'Roman Reigns' LIMIT 1),
    'Disbanded',
    45,
    2012
WHERE NOT EXISTS (SELECT 1 FROM Factions WHERE Name = 'The Shield');

-- Add Shield members
INSERT OR IGNORE INTO FactionMembers (FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear)
SELECT
    (SELECT Id FROM Factions WHERE Name = 'The Shield' LIMIT 1),
    Id,
    45,
    2012,
    20,
    2014
FROM Workers WHERE Name = 'Roman Reigns';

INSERT OR IGNORE INTO FactionMembers (FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear)
SELECT
    (SELECT Id FROM Factions WHERE Name = 'The Shield' LIMIT 1),
    Id,
    45,
    2012,
    20,
    2014
FROM Workers WHERE Name = 'Seth Rollins';

INSERT OR IGNORE INTO FactionMembers (FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear)
SELECT
    (SELECT Id FROM Factions WHERE Name = 'The Shield' LIMIT 1),
    Id,
    45,
    2012,
    20,
    2014
FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%';

-- Evolution (Faction - 4 members example, if you have Triple H, Batista, Ric Flair)
-- Commented out as we may not have all members
/*
INSERT OR IGNORE INTO Factions (Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear)
SELECT
    'Evolution',
    'Faction',
    (SELECT Id FROM Workers WHERE Name = 'Triple H' LIMIT 1),
    'Disbanded',
    5,
    2003;
*/

-- ============================================================================
-- SECTION 5: WORKER NOTES (Examples)
-- ============================================================================

-- John Cena booking notes
INSERT OR IGNORE INTO WorkerNotes (WorkerId, Text, Category, CreatedDate)
SELECT
    Id,
    'Never loses clean. Only major PPV defeats. Booking directive from management.',
    'BookingIdeas',
    datetime('now', '-30 days')
FROM Workers WHERE Name = 'John Cena';

INSERT OR IGNORE INTO WorkerNotes (WorkerId, Text, Category, CreatedDate)
SELECT
    Id,
    'Consider heel turn storyline for freshness. Audience getting tired of super-Cena.',
    'BookingIdeas',
    datetime('now', '-15 days')
FROM Workers WHERE Name = 'John Cena';

-- CM Punk notes
INSERT OR IGNORE INTO WorkerNotes (WorkerId, Text, Category, CreatedDate)
SELECT
    Id,
    'Watch for contract status. May walk if not happy with creative.',
    'Personal',
    datetime('now', '-7 days')
FROM Workers WHERE Name = 'CM Punk';

-- Brock Lesnar notes
INSERT OR IGNORE INTO WorkerNotes (WorkerId, Text, Category, CreatedDate)
SELECT
    Id,
    'Limited schedule. Only big 4 PPVs. Expensive but huge draw.',
    'BookingIdeas',
    datetime('now', '-20 days')
FROM Workers WHERE Name = 'Brock Lesnar';

-- ============================================================================
-- SECTION 6: VERIFICATION QUERIES
-- ============================================================================

SELECT 'Specializations Count' AS Metric, COUNT(*) AS Count FROM WorkerSpecializations
UNION ALL
SELECT 'Workers with Geography', COUNT(*) FROM Workers WHERE BirthCity IS NOT NULL
UNION ALL
SELECT 'Relations Count', COUNT(*) FROM WorkerRelations
UNION ALL
SELECT 'Factions Count', COUNT(*) FROM Factions
UNION ALL
SELECT 'Faction Members Count', COUNT(*) FROM FactionMembers
UNION ALL
SELECT 'Worker Notes Count', COUNT(*) FROM WorkerNotes;

-- Show example profile data
SELECT
    w.Name,
    w.BirthCity || ', ' || w.BirthCountry AS Birthplace,
    w.ResidenceCity || ', ' || w.ResidenceState || ', ' || w.ResidenceCountry AS Residence,
    w.CurrentGimmick,
    w.Alignment,
    w.PushLevel,
    GROUP_CONCAT(ws.Specialization, ', ') AS Specializations
FROM Workers w
LEFT JOIN WorkerSpecializations ws ON w.Id = ws.WorkerId
WHERE w.Name IN ('John Cena', 'Randy Orton', 'AJ Styles')
GROUP BY w.Id
ORDER BY w.Name;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================

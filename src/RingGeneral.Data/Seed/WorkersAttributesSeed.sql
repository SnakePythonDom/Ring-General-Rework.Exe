-- ============================================================================
-- Seed Data: Workers Performance Attributes (30 attributes per worker)
-- Version: 1.0
-- Date: 2026-01-07
-- Description: Insert 30 performance attributes for existing workers
-- ============================================================================

-- ============================================================================
-- JOHN CENA (Worker ID will be determined dynamically)
-- Main Eventer, Brawler/Powerhouse style
-- ============================================================================

-- In-Ring Attributes (Average: 82)
INSERT OR REPLACE INTO WorkerInRingAttributes (
    WorkerId, Striking, Grappling, HighFlying, Powerhouse,
    Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl
)
SELECT
    Id,
    75, -- Striking
    78, -- Grappling
    45, -- HighFlying (not his style)
    90, -- Powerhouse (signature strength)
    85, -- Timing
    82, -- Selling
    88, -- Psychology
    85, -- Stamina
    94, -- Safety (very safe worker)
    80  -- HardcoreBrawl
FROM Workers WHERE Name = 'John Cena';

-- Entertainment Attributes (Average: 88)
INSERT OR REPLACE INTO WorkerEntertainmentAttributes (
    WorkerId, Charisma, MicWork, Acting, CrowdConnection,
    StarPower, Improvisation, Entrance, SexAppeal,
    MerchandiseAppeal, CrossoverPotential
)
SELECT
    Id,
    92, -- Charisma (natural leader)
    95, -- MicWork (legendary promos)
    88, -- Acting
    98, -- CrowdConnection (polarizing but effective)
    95, -- StarPower (top guy)
    90, -- Improvisation
    92, -- Entrance (iconic)
    85, -- SexAppeal
    96, -- MerchandiseAppeal (top seller)
    94  -- CrossoverPotential (movies, TV)
FROM Workers WHERE Name = 'John Cena';

-- Story Attributes (Average: 80)
INSERT OR REPLACE INTO WorkerStoryAttributes (
    WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance,
    StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry,
    CreativeInput, MoralAlignment
)
SELECT
    Id,
    84, -- CharacterDepth
    90, -- Consistency (character never wavers)
    80, -- HeelPerformance (did heel runs)
    95, -- BabyfacePerformance (iconic face)
    88, -- StorytellingLongTerm
    85, -- EmotionalRange
    75, -- Adaptability
    82, -- RivalryChemistry
    78, -- CreativeInput
    72  -- MoralAlignment (pure babyface, less tweener)
FROM Workers WHERE Name = 'John Cena';

-- ============================================================================
-- RANDY ORTON
-- Upper Card, Technical/Brawler style
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 82, 88, 65, 75, 92, 85, 90, 80, 88, 70 FROM Workers WHERE Name = 'Randy Orton';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 88, 82, 90, 85, 92, 75, 88, 90, 80, 70 FROM Workers WHERE Name = 'Randy Orton';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 88, 85, 95, 70, 85, 88, 82, 90, 75, 85 FROM Workers WHERE Name = 'Randy Orton';

-- ============================================================================
-- THE ROCK
-- Main Eventer, Charismatic Powerhouse
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 78, 75, 40, 88, 90, 88, 92, 82, 85, 75 FROM Workers WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson' LIMIT 1;

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 98, 98, 95, 98, 98, 95, 95, 95, 95, 98 FROM Workers WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson' LIMIT 1;

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 92, 88, 95, 95, 90, 95, 90, 95, 88, 88 FROM Workers WHERE Name LIKE '%Rock%' OR Name = 'Dwayne Johnson' LIMIT 1;

-- ============================================================================
-- AJ STYLES
-- Upper Card, High-Flyer/Technical
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 88, 92, 95, 65, 95, 90, 92, 90, 92, 65 FROM Workers WHERE Name = 'AJ Styles';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 85, 80, 75, 90, 88, 85, 82, 78, 85, 65 FROM Workers WHERE Name = 'AJ Styles';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 82, 90, 88, 85, 88, 80, 88, 92, 80, 75 FROM Workers WHERE Name = 'AJ Styles';

-- ============================================================================
-- CM PUNK
-- Main Event, Technical/Showman
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 85, 88, 70, 60, 90, 85, 95, 85, 80, 75 FROM Workers WHERE Name = 'CM Punk';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 95, 98, 90, 95, 92, 92, 88, 80, 90, 75 FROM Workers WHERE Name = 'CM Punk';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 95, 85, 95, 90, 95, 92, 85, 95, 95, 90 FROM Workers WHERE Name = 'CM Punk';

-- ============================================================================
-- ROMAN REIGNS
-- Main Event, Powerhouse/Brawler
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 82, 78, 55, 92, 85, 82, 88, 88, 90, 80 FROM Workers WHERE Name = 'Roman Reigns';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 90, 85, 88, 88, 95, 75, 92, 92, 88, 80 FROM Workers WHERE Name = 'Roman Reigns';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 90, 92, 95, 75, 92, 85, 85, 88, 80, 88 FROM Workers WHERE Name = 'Roman Reigns';

-- ============================================================================
-- SETH ROLLINS
-- Main Event, All-Rounder
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 85, 88, 90, 70, 92, 88, 90, 90, 88, 75 FROM Workers WHERE Name = 'Seth Rollins';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 88, 85, 85, 90, 90, 88, 90, 85, 85, 70 FROM Workers WHERE Name = 'Seth Rollins';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 85, 88, 92, 88, 88, 88, 92, 90, 85, 85 FROM Workers WHERE Name = 'Seth Rollins';

-- ============================================================================
-- DEAN AMBROSE / JON MOXLEY
-- Upper Card, Hardcore/Brawler
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 80, 75, 65, 72, 85, 88, 88, 85, 75, 92 FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%' LIMIT 1;

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 90, 90, 92, 92, 85, 95, 80, 78, 82, 72 FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%' LIMIT 1;

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 92, 80, 88, 88, 85, 90, 88, 90, 90, 88 FROM Workers WHERE Name LIKE '%Ambrose%' OR Name LIKE '%Moxley%' LIMIT 1;

-- ============================================================================
-- BROCK LESNAR
-- Main Event, Pure Powerhouse
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 90, 85, 30, 98, 80, 70, 85, 75, 85, 85 FROM Workers WHERE Name = 'Brock Lesnar';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 88, 65, 75, 85, 95, 60, 95, 88, 85, 90 FROM Workers WHERE Name = 'Brock Lesnar';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 75, 88, 90, 70, 80, 65, 70, 85, 60, 75 FROM Workers WHERE Name = 'Brock Lesnar';

-- ============================================================================
-- FINN BALOR
-- Upper Mid, High-Flyer/Technical
-- ============================================================================

INSERT OR REPLACE INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
SELECT Id, 82, 88, 92, 58, 92, 88, 88, 88, 90, 65 FROM Workers WHERE Name = 'Finn Balor';

INSERT OR REPLACE INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
SELECT Id, 85, 75, 80, 88, 85, 80, 92, 88, 88, 70 FROM Workers WHERE Name = 'Finn Balor';

INSERT OR REPLACE INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
SELECT Id, 88, 85, 85, 90, 82, 85, 85, 85, 78, 80 FROM Workers WHERE Name = 'Finn Balor';

-- ============================================================================
-- DEFAULT VALUES FOR REMAINING WORKERS
-- This will set baseline attributes for all workers not explicitly seeded
-- ============================================================================

-- In-Ring Attributes (Baseline: 60-70)
INSERT OR IGNORE INTO WorkerInRingAttributes (
    WorkerId, Striking, Grappling, HighFlying, Powerhouse,
    Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl
)
SELECT
    Id,
    65 + (Id % 20), -- Some variation based on ID
    65 + ((Id * 3) % 20),
    60 + ((Id * 5) % 25),
    65 + ((Id * 7) % 20),
    65 + ((Id * 11) % 20),
    65 + ((Id * 13) % 20),
    65 + ((Id * 17) % 20),
    65 + ((Id * 19) % 20),
    70 + ((Id * 23) % 15), -- Safety higher baseline
    60 + ((Id * 29) % 25)
FROM Workers
WHERE Id NOT IN (
    SELECT WorkerId FROM WorkerInRingAttributes
);

-- Entertainment Attributes (Baseline: 60-70)
INSERT OR IGNORE INTO WorkerEntertainmentAttributes (
    WorkerId, Charisma, MicWork, Acting, CrowdConnection,
    StarPower, Improvisation, Entrance, SexAppeal,
    MerchandiseAppeal, CrossoverPotential
)
SELECT
    Id,
    65 + (Id % 20),
    65 + ((Id * 2) % 20),
    60 + ((Id * 4) % 25),
    65 + ((Id * 6) % 20),
    60 + ((Id * 8) % 25),
    65 + ((Id * 10) % 20),
    65 + ((Id * 12) % 20),
    60 + ((Id * 14) % 25),
    60 + ((Id * 16) % 25),
    55 + ((Id * 18) % 25)
FROM Workers
WHERE Id NOT IN (
    SELECT WorkerId FROM WorkerEntertainmentAttributes
);

-- Story Attributes (Baseline: 60-70)
INSERT OR IGNORE INTO WorkerStoryAttributes (
    WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance,
    StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry,
    CreativeInput, MoralAlignment
)
SELECT
    Id,
    65 + (Id % 20),
    65 + ((Id * 3) % 20),
    65 + ((Id * 5) % 20),
    65 + ((Id * 7) % 20),
    65 + ((Id * 9) % 20),
    60 + ((Id * 11) % 25),
    65 + ((Id * 13) % 20),
    65 + ((Id * 15) % 20),
    60 + ((Id * 17) % 25),
    65 + ((Id * 19) % 20)
FROM Workers
WHERE Id NOT IN (
    SELECT WorkerId FROM WorkerStoryAttributes
);

-- ============================================================================
-- VERIFICATION QUERY
-- ============================================================================

SELECT
    'Total Workers' AS Metric,
    COUNT(*) AS Count
FROM Workers
UNION ALL
SELECT
    'Workers with In-Ring Attributes',
    COUNT(*)
FROM WorkerInRingAttributes
UNION ALL
SELECT
    'Workers with Entertainment Attributes',
    COUNT(*)
FROM WorkerEntertainmentAttributes
UNION ALL
SELECT
    'Workers with Story Attributes',
    COUNT(*)
FROM WorkerStoryAttributes;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================

-- =================================================================================
-- Migration: 026_Populate_Venues.sql
-- Description: Populate Venues table with iconic and procedural data
-- =================================================================================

BEGIN TRANSACTION;

-- 1. Venues Iconiques (Manuelles)
-- =================================================================================

-- USA
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) VALUES
('VEN_MSG', 'USA', 'USA_NY', 'Madison Square Garden', 'New York', 20789),
('VEN_BARCLAYS', 'USA', 'USA_NY', 'Barclays Center', 'Brooklyn', 19000),
('VEN_UNITED_CENTER', 'USA', 'USA_IL', 'United Center', 'Chicago', 23500),
('VEN_ALLSTATE', 'USA', 'USA_IL', 'Allstate Arena', 'Rosemont', 18500),
('VEN_STAPLES', 'USA', 'USA_CA', 'Crypto.com Arena', 'Los Angeles', 19060),
('VEN_FORUM', 'USA', 'USA_CA', 'Kia Forum', 'Inglewood', 17505),
('VEN_DAILYS_PLACE', 'USA', 'USA_FL', 'Daily''s Place', 'Jacksonville', 5500),
('VEN_AMWAY', 'USA', 'USA_FL', 'Amway Center', 'Orlando', 18846),
('VEN_ALAMODOME', 'USA', 'USA_TX', 'Alamodome', 'San Antonio', 64000),
('VEN_ATT_STADIUM', 'USA', 'USA_TX', 'AT&T Stadium', 'Arlington', 80000),
('VEN_WELLS_FARGO', 'USA', 'USA_PA', 'Wells Fargo Center', 'Philadelphia', 19500),
('VEN_2300_ARENA', 'USA', 'USA_PA', '2300 Arena', 'Philadelphia', 1300), -- ECW Arena
('VEN_TD_GARDEN', 'USA', 'USA_MA', 'TD Garden', 'Boston', 19580);

-- Japon
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) VALUES
('VEN_TOKYO_DOME', 'JPN', 'JPN_TO', 'Tokyo Dome', 'Tokyo', 55000),
('VEN_KORAKUEN', 'JPN', 'JPN_TO', 'Korakuen Hall', 'Tokyo', 2005),
('VEN_RYOGOKU', 'JPN', 'JPN_TO', 'Ryogoku Kokugikan', 'Tokyo', 11098),
('VEN_BUDOKAN', 'JPN', 'JPN_TO', 'Nippon Budokan', 'Tokyo', 14471),
('VEN_OSAKA_JO', 'JPN', 'JPN_OS', 'Osaka-Jo Hall', 'Osaka', 16000),
('VEN_EDION', 'JPN', 'JPN_OS', 'Edion Arena Osaka', 'Osaka', 8000),
('VEN_YOKOHAMA', 'JPN', 'JPN_KN', 'Yokohama Arena', 'Yokohama', 17000),
('VEN_BUNTAI', 'JPN', 'JPN_KN', 'Yokohama Buntai', 'Yokohama', 5000);

-- Mexique
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) VALUES
('VEN_ARENA_MX', 'MEX', 'MEX_CMX', 'Arena México', 'Ciudad de México', 16500),
('VEN_ARENA_COLISEO', 'MEX', 'MEX_CMX', 'Arena Coliseo', 'Ciudad de México', 5500),
('VEN_ARENA_GDL', 'MEX', 'MEX_JAL', 'Arena Guadalajara', 'Guadalajara', 17000),
('VEN_ARENA_MONTERREY', 'MEX', 'MEX_NLE', 'Arena Monterrey', 'Monterrey', 17599);

-- UK / Europe
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) VALUES
('VEN_WEMBLEY', 'GBR', 'GBR_LDN', 'Wembley Stadium', 'London', 90000),
('VEN_O2_ARENA', 'GBR', 'GBR_LDN', 'The O2 Arena', 'London', 20000),
('VEN_YORK_HALL', 'GBR', 'GBR_LDN', 'York Hall', 'London', 1200),
('VEN_HYDRO', 'GBR', 'GBR_SCT', 'OVO Hydro', 'Glasgow', 14300),
('VEN_PRINCIPALITY', 'GBR', 'GBR_WAL', 'Principality Stadium', 'Cardiff', 74500),
('VEN_ACCOR', 'FRA', 'FRA_IDF', 'Accor Arena', 'Paris', 20300),
('VEN_LANXESS', 'DEU', 'DEU_NW', 'Lanxess Arena', 'Cologne', 18000),
('VEN_TURBINENHALLE', 'DEU', 'DEU_NW', 'Turbinenhalle', 'Oberhausen', 3000); -- wXw home

-- Canada
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) VALUES
('VEN_SCOTIABANK', 'CAN', 'CAN_ON', 'Scotiabank Arena', 'Toronto', 19800),
('VEN_BELL_CENTRE', 'CAN', 'CAN_QC', 'Centre Bell', 'Montréal', 21273);

-- 2. Génération Procédurale (Basée sur les régions)
-- =================================================================================

-- Générer ~2-3 venues par région importante (> 20 importance)
-- Utilisation de randomblob pour ID unique et random() pour variations

INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) 
SELECT 
    'VEN_' || r.RegionId || '_' || substr(hex(randomblob(4)), 1, 6) as VenueId,
    r.CountryId,
    r.RegionId,
    -- Nom généré
    r.Name || CASE abs(random() % 10)
        WHEN 0 THEN ' Grand Arena'
        WHEN 1 THEN ' Coliseum'
        WHEN 2 THEN ' Sports Hall'
        WHEN 3 THEN ' Civic Center'
        WHEN 4 THEN ' Dome'
        WHEN 5 THEN ' Stadium'
        WHEN 6 THEN ' Event Center'
        WHEN 7 THEN ' Memorial Hall'
        WHEN 8 THEN ' Plaza'
        ELSE ' Garden'
    END as Name,
    -- Ville supposée être le nom de la région pour simplification
    r.Name as City,
    -- Capacité basée sur importance de la région + variation aléatoire
    CASE 
        WHEN r.WrestlingImportance > 80 THEN 15000 + abs(random() % 35000) -- 15k - 50k
        WHEN r.WrestlingImportance > 60 THEN 5000 + abs(random() % 15000)  -- 5k - 20k
        WHEN r.WrestlingImportance > 40 THEN 2000 + abs(random() % 8000)   -- 2k - 10k
        ELSE 500 + abs(random() % 2500)                                     -- 500 - 3k
    END as Capacity
FROM Regions r
WHERE r.WrestlingImportance > 20
-- Exclure régions qui ont déjà une venue manuelle exacte (simplification: on accepte doublons de région, tant pis)
;

-- Ajouter une "Local Gym" pour les toutes petites régions (Importance <= 20)
INSERT INTO Venues (VenueId, CountryId, RegionId, Name, City, Capacity) 
SELECT 
    'VEN_' || r.RegionId || '_GYM',
    r.CountryId,
    r.RegionId,
    r.Name || ' Community Center',
    r.Name,
    100 + abs(random() % 400) -- 100-500 places
FROM Regions r
WHERE r.WrestlingImportance <= 20
AND abs(random() % 100) < 50; -- 50% de chance d'avoir une venue

COMMIT;

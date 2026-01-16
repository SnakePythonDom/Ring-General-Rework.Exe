INSERT INTO Venues (VenueId, Name, Region, Capacity, Cost, Prestige, Type) VALUES
-- 🏟️ US - TRI-STATE (Zone de départ classique)
('V_USA_NY_01', 'Queens Community Hall', 'USA_TriState', 150, 500, 5, 'SmallHall'),
('V_USA_PA_01', 'Allentown Rec Center', 'USA_TriState', 300, 800, 10, 'SmallHall'),
('V_USA_NJ_01', 'Asbury Park Convention Hall', 'USA_TriState', 3600, 15000, 55, 'LargeHall'),
('V_USA_NY_02', 'Hammerstein Ballroom', 'USA_TriState', 2500, 12000, 60, 'Ballroom'),
('V_USA_NY_MSG', 'Madison Square Garden', 'USA_TriState', 20000, 250000, 100, 'Arena'),

-- 🏟️ US - SOUTH EAST
('V_USA_NC_01', 'Donton Gymnasium', 'USA_SouthEast', 800, 2000, 20, 'Gymnasium'),
('V_USA_FL_01', 'Orlando Armory', 'USA_SouthEast', 1200, 3500, 30, 'Hall'),
('V_USA_GA_01', 'Center Stage Atlanta', 'USA_SouthEast', 700, 2500, 45, 'TVStudio'),

-- 🏟️ CANADA
('V_CAN_ON_01', 'Ted Reeve Arena', 'CAN_Ontario', 900, 2200, 25, 'IceRink'),
('V_CAN_QC_01', 'Centre Pierre-Charbonneau', 'CAN_Quebec', 1500, 4000, 35, 'Arena'),

-- 🏟️ JAPAN
('V_JPN_TK_01', 'Shinjuku FACE', 'JPN_Kanto', 600, 2000, 40, 'Hall'),
('V_JPN_TK_02', 'Korakuen Hall', 'JPN_Kanto', 2000, 8000, 85, 'LegendaryHall'),
('V_JPN_TK_DOME', 'Tokyo Dome', 'JPN_Kanto', 45000, 400000, 100, 'Stadium'),

-- 🏟️ MEXICO
('V_MEX_CM_01', 'Arena Naucalpan', 'MEX_Central', 2400, 5000, 50, 'Arena'),
('V_MEX_CM_02', 'Arena Mexico', 'MEX_Central', 16500, 100000, 95, 'Cathedral');

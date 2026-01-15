-- =================================================================================
-- 0. SCHEMA UPDATE (Moved from 020 to ensure columns exist before insert)
-- =================================================================================
ALTER TABLE Countries ADD COLUMN Continent TEXT;
ALTER TABLE Countries ADD COLUMN WrestlingImportance INTEGER DEFAULT 0;
ALTER TABLE Regions ADD COLUMN WrestlingImportance INTEGER DEFAULT 0;

-- =================================================================================
-- 1. NETTOYAGE (Optionnel : Décommentez si vous voulez vider les tables avant)
-- =================================================================================
-- DELETE FROM Regions;
-- DELETE FROM Countries;

-- =================================================================================
-- 2. PEUPLEMENT DES PAYS (TABLE: Countries)
-- Structure supposée : Id (ISO code), Name, Continent, WrestlingImportance (0-100)
-- =================================================================================

-- --- AMÉRIQUE DU NORD (Le marché principal) ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('USA', 'USA', 'États-Unis', 'North America', 100),
('CAN', 'CAN', 'Canada', 'North America', 85),
('MEX', 'MEX', 'Mexique', 'North America', 90),
('PUR', 'PUR', 'Porto Rico', 'North America', 60),
-- --- LES CARAÏBES (Hormis Porto Rico) ---
('DOM', 'DOM', 'République Dominicaine', 'North America', 50), -- Grosse histoire (Jack Veneno), rivalité avec PUR
('CUB', 'CUB', 'Cuba', 'North America', 20),                   -- Fort en lutte olympique, mais pro-wrestling limité
('JAM', 'JAM', 'Jamaïque', 'North America', 15),               -- Kingston (Kofi Kingston heritage)
('BHS', 'BHS', 'Bahamas', 'North America', 10),                -- Nassau (Lieu de vacances/Shows privés)
('BRB', 'BRB', 'Barbade', 'North America', 10),
('TTO', 'TTO', 'Trinité-et-Tobago', 'North America', 10),
('HTI', 'HTI', 'Haïti', 'North America', 10),
('PAN', 'PAN', 'Panama', 'North America', 35),                 -- GWE (Global Wrestling Evolution), scène active
('GTM', 'GTM', 'Guatemala', 'North America', 25),              -- Influence Lucha Libre mexicaine
('CRI', 'CRI', 'Costa Rica', 'North America', 25),             -- CWE
('SLV', 'SLV', 'Salvador', 'North America', 15),
('HND', 'HND', 'Honduras', 'North America', 10),
('NIC', 'NIC', 'Nicaragua', 'North America', 10),
('BLZ', 'BLZ', 'Belize', 'North America', 10),
('LCA', 'LCA', 'Sainte-Lucie', 'North America', 10),                 -- Tourisme de luxe
('ATG', 'ATG', 'Antigua-et-Barbuda', 'North America', 10),           -- Tourisme
('GRD', 'GRD', 'Grenade', 'North America', 10),
('KNA', 'KNA', 'Saint-Christophe-et-Niévès', 'North America', 10),    -- (St. Kitts & Nevis)
('VCT', 'VCT', 'Saint-Vincent-et-les-Grenadines', 'North America', 10),
('DMA', 'DMA', 'Dominique', 'North America', 10);                     -- (Dominica) - Ne pas confondre avec Rep. Dom.

-- --- ASIE (Le bastion du Strong Style) ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('JPN', 'JPN', 'Japon', 'Asia', 95),
('IND', 'IND', 'Inde', 'Asia', 10),      
('CHN', 'CHN', 'Chine', 'Asia', 20),
('SAU', 'SAU', 'Arabie Saoudite', 'Asia', 40), -- Pour les gros events type Crown Jewel
('KOR', 'KOR', 'Corée du Sud', 'Asia', 40),      -- WWA Legacy, marché accessible
('PRK', 'PRK', 'Corée du Nord', 'Asia', 55),       -- "Collision in Korea"
('TWN', 'TWN', 'Taïwan', 'Asia', 50),             -- Marché très ouvert au catch japonais/US
('MNG', 'MNG', 'Mongolie', 'Asia', 20),           -- Terre de lutte traditionnelle (Bökh) et Sumo
('SGP', 'SGP', 'Singapour', 'Asia', 35),          -- Hub logistique (SPW)
('PHL', 'PHL', 'Philippines', 'Asia', 35),        -- Scène Indy active (PWR, MWF)
('THA', 'THA', 'Thaïlande', 'Asia', 30),          -- Gatoh Move (Emi Sakura), destination touristique
('MYS', 'MYS', 'Malaisie', 'Asia', 25),           -- MYPW
('IDN', 'IDN', 'Indonésie', 'Asia', 20),          -- Immense population
('VNM', 'VNM', 'Vietnam', 'Asia', 15),            -- VPW (Vietnam Pro Wrestling)
('KHM', 'KHM', 'Cambodge', 'Asia', 15),
('LAO', 'LAO', 'Laos', 'Asia', 15),
('MMR', 'MMR', 'Birmanie (Myanmar)', 'Asia', 15),
('BRN', 'BRN', 'Brunei', 'Asia', 10),
('TLS', 'TLS', 'Timor oriental', 'Asia', 10),
('ARE', 'ARE', 'Émirats Arabes Unis', 'Asia', 55),-- Dubai/Abu Dhabi (Gros hub expat, WWE Live)
('QAT', 'QAT', 'Qatar', 'Asia', 45),              -- Riche, capable d'acheter des shows
('TUR', 'TUR', 'Turquie', 'Asia', 35),            -- TPW (Bilgehan Demir), pont Europe/Asie
('KWT', 'KWT', 'Koweït', 'Asia', 20),
('BHR', 'BHR', 'Bahreïn', 'Asia', 15),
('OMN', 'OMN', 'Oman', 'Asia', 10),
('JOR', 'JOR', 'Jordanie', 'Asia', 10),
('LBN', 'LBN', 'Liban', 'Asia', 10),
('IRQ', 'IRQ', 'Irak', 'Asia', 15),
('IRN', 'IRN', 'Iran', 'Asia', 15),               -- Immense culture de lutte olympique, catch pro difficile
('SYR', 'SYR', 'Syrie', 'Asia', 10),
('YEM', 'YEM', 'Yémen', 'Asia', 10),
('PSE', 'PSE', 'Palestine', 'Asia', 15),
('PAK', 'PAK', 'Pakistan', 'Asia', 25),           -- Ring of Pakistan, grosse fanbase
('BGD', 'BGD', 'Bangladesh', 'Asia', 10),
('LKA', 'LKA', 'Sri Lanka', 'Asia', 10),
('NPL', 'NPL', 'Népal', 'Asia', 10),              -- Great Gama legacy influence
('MDV', 'MDV', 'Maldives', 'Asia', 15),
('BTN', 'BTN', 'Bhoutan', 'Asia', 10),
('KAZ', 'KAZ', 'Kazakhstan', 'Asia', 20),         -- Sports de combat très populaires
('UZB', 'UZB', 'Ouzbékistan', 'Asia', 15),
('KGZ', 'KGZ', 'Kirghizistan', 'Asia', 10),
('TKM', 'TKM', 'Turkménistan', 'Asia', 10),
('TJK', 'TJK', 'Tadjikistan', 'Asia', 10),
('AFG', 'AFG', 'Afghanistan', 'Asia', 10);



-- --- EUROPE (L'expansion moderne) ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('GBR', 'GBR', 'Royaume-Uni', 'Europe', 85),
('IRL', 'IRL', 'Irlande', 'Europe', 65),
('DEU', 'DEU', 'Allemagne', 'Europe', 60), -- Marché wXw
('FRA', 'FRA', 'France', 'Europe', 55),    -- Marché en boom (Backlash)
('ESP', 'ESP', 'Espagne', 'Europe', 30),
('ITA', 'ITA', 'Italie', 'Europe', 30),
('POL', 'POL', 'Pologne', 'Europe', 45),       -- Scène Indy très forte (KPW, PpW)
('ROU', 'ROU', 'Roumanie', 'Europe', 30),      -- Romanian Pro Wrestling, marché émergent
('RUS', 'RUS', 'Russie', 'Europe', 40),        -- IWF, mais géopolitique compliquée
('NLD', 'NLD', 'Pays-Bas', 'Europe', 50),      -- Dutch Pro Wrestling (Malakai Black)
('BEL', 'BEL', 'Belgique', 'Europe', 45),      -- Flemish Wrestling Force
('CHE', 'CHE', 'Suisse', 'Europe', 55),        -- Claudio Castagnoli, très technique
('AUT', 'AUT', 'Autriche', 'Europe', 50),      -- WALTER/Gunther (CWA/EWA)
('PRT', 'PRT', 'Portugal', 'Europe', 35),      -- CTW, APW (Centre de formation)
('CZE', 'CZE', 'République Tchèque', 'Europe', 30), -- wXw y va souvent
('SWE', 'SWE', 'Suède', 'Europe', 35),         -- STHLM Wrestling
('HUN', 'HUN', 'Hongrie', 'Europe', 30),       -- HCW (Hungarian Championship Wrestling)
('NOR', 'NOR', 'Norvège', 'Europe', 25),       -- NWF
('DNK', 'DNK', 'Danemark', 'Europe', 30),      -- DPW (Dansk Pro Wrestling)
('FIN', 'FIN', 'Finlande', 'Europe', 25),      -- FCF Wrestling (Slammiversary)
('EST', 'EST', 'Estonie', 'Europe', 10),
('LVA', 'LVA', 'Lettonie', 'Europe', 10),
('LTU', 'LTU', 'Lituanie', 'Europe', 10),
('ISL', 'ISL', 'Islande', 'Europe', 15),       -- Très isolé
('GRC', 'GRC', 'Grèce', 'Europe', 25),         -- ZMAK (Pro-wrestling grec unique)
('HRV', 'HRV', 'Croatie', 'Europe', 20),       -- Prvi Hrvatski Kečeri
('SRB', 'SRB', 'Serbie', 'Europe', 15),
('BGR', 'BGR', 'Bulgarie', 'Europe', 20),      -- Rusev/Miro heritage
('SVN', 'SVN', 'Slovénie', 'Europe', 15),
('SVK', 'SVK', 'Slovaquie', 'Europe', 15),
('BIH', 'BIH', 'Bosnie-Herzégovine', 'Europe', 10),
('MKD', 'MKD', 'Macédoine du Nord', 'Europe', 10),
('ALB', 'ALB', 'Albanie', 'Europe', 10),
('MNE', 'MNE', 'Monténégro', 'Europe', 10),
('CYP', 'CYP', 'Chypre', 'Europe', 15),
('UKR', 'UKR', 'Ukraine', 'Europe', 25),       -- Scène active avant le conflit
('BLR', 'BLR', 'Biélorussie', 'Europe', 10),
('MDA', 'MDA', 'Moldavie', 'Europe', 10),
('MCO', 'MCO', 'Monaco', 'Europe', 30),        -- Riche, petit show de luxe
('LUX', 'LUX', 'Luxembourg', 'Europe', 25),
('MLT', 'MLT', 'Malte', 'Europe', 20),         -- PWM (Pro Wrestling Malta)
('AND', 'AND', 'Andorre', 'Europe', 10),
('SMR', 'SMR', 'Saint-Marin', 'Europe', 10),
('LIE', 'LIE', 'Liechtenstein', 'Europe', 10);

-- --- Amerique Sud ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('CHL', 'CHL', 'Chili', 'South America', 50),     -- Public le plus chaud du continent (Sami Zayn/Cody Rhodes adorent)
('BRA', 'BRA', 'Brésil', 'South America', 45),    -- Immense population, culture de combat (BWF)
('ARG', 'ARG', 'Argentine', 'South America', 35),
('PER', 'PER', 'Pérou', 'South America', 30),     -- LWA, Imperio (Gros shows à Lima)
('COL', 'COL', 'Colombie', 'South America', 25),  -- Scène émergente
('ECU', 'ECU', 'Équateur', 'South America', 25),  -- WAR (Wrestling Alliance Revolution)
('BOL', 'BOL', 'Bolivie', 'South America', 25),
('URY', 'URY', 'Uruguay', 'South America', 15),
('VEN', 'VEN', 'Venezuela', 'South America', 10), -- Crise économique, difficile d'organiser
('PRY', 'PRY', 'Paraguay', 'South America', 10),
('GUY', 'GUY', 'Guyana', 'South America', 15),     -- Anglophone (Culture Cricket)
('SUR', 'SUR', 'Suriname', 'South America', 15);   -- Néerlandophone

-- --- Afrique ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('ZAF', 'ZAF', 'Afrique du Sud', 'Africa', 60),
('NGA', 'NGA', 'Nigeria', 'Africa', 35),         -- Immense population, Omos/Apollo Crews heritage
('EGY', 'EGY', 'Égypte', 'Africa', 30),          -- Pont avec le Moyen-Orient, histoire riche
('GHA', 'GHA', 'Ghana', 'Africa', 25),           -- Kofi Kingston, popularité du catch US
('MAR', 'MAR', 'Maroc', 'Africa', 25),           -- Proche Europe, White Eagle Wrestling
('SEN', 'SEN', 'Sénégal', 'Africa', 30),
('DZA', 'DZA', 'Algérie', 'Africa', 20),
('TUN', 'TUN', 'Tunisie', 'Africa', 20),
('LBY', 'LBY', 'Libye', 'Africa', 15),
('KEN', 'KEN', 'Kenya', 'Africa', 20),           -- Nairobi est un hub majeur
('TZA', 'TZA', 'Tanzanie', 'Africa', 15),
('ETH', 'ETH', 'Éthiopie', 'Africa', 10),
('UGA', 'UGA', 'Ouganda', 'Africa', 10),
('RWA', 'RWA', 'Rwanda', 'Africa', 10),
('SYC', 'SYC', 'Seychelles', 'Africa', 10),      -- Tourisme
('MUS', 'MUS', 'Maurice', 'Africa', 10),
('CMR', 'CMR', 'Cameroun', 'Africa', 20),        -- Terre de combattants (Ngannou)
('CIV', 'CIV', 'Côte d''Ivoire', 'Africa', 20),  -- Hub francophone
('COD', 'COD', 'Rép. Dém. du Congo', 'Africa', 15), -- Catch Fétiche (Vaudou wrestling)
('COG', 'COG', 'Congo (Brazzaville)', 'Africa', 10),
('GAB', 'GAB', 'Gabon', 'Africa', 10),
('AGO', 'AGO', 'Angola', 'Africa', 10),
('ZWE', 'ZWE', 'Zimbabwe', 'Africa', 10),        -- Ancien marché correct, aujourd'hui dur
('NAM', 'NAM', 'Namibie', 'Africa', 15),
('BWA', 'BWA', 'Botswana', 'Africa', 15),
('ZMB', 'ZMB', 'Zambie', 'Africa', 15),
('MOZ', 'MOZ', 'Mozambique', 'Africa', 15),
('MDG', 'MDG', 'Madagascar', 'Africa', 15),
('MLI', 'MLI', 'Mali', 'Africa', 15),
('BFA', 'BFA', 'Burkina Faso', 'Africa', 15),
('GIN', 'GIN', 'Guinée', 'Africa', 15),
('SLE', 'SLE', 'Sierra Leone', 'Africa', 10),
('LBR', 'LBR', 'Liberia', 'Africa', 10),
('SDN', 'SDN', 'Soudan', 'Africa', 10),
('SSD', 'SSD', 'Soudan du Sud', 'Africa', 10),
('SOM', 'SOM', 'Somalie', 'Africa', 10),
('ERI', 'ERI', 'Érythrée', 'Africa', 10),
('DJI', 'DJI', 'Djibouti', 'Africa', 10),
('TCD', 'TCD', 'Tchad', 'Africa', 10),
('NER', 'NER', 'Niger', 'Africa', 10),
('MRT', 'MRT', 'Mauritanie', 'Africa', 10),
('GMB', 'GMB', 'Gambie', 'Africa', 10),
('GNB', 'GNB', 'Guinée-Bissau', 'Africa', 10),
('CPV', 'CPV', 'Cap-Vert', 'Africa', 15),
('STP', 'STP', 'Sao Tomé-et-Principe', 'Africa', 10),
('GNQ', 'GNQ', 'Guinée équatoriale', 'Africa', 10),
('CAF', 'CAF', 'Centrafrique', 'Africa', 10),
('BDI', 'BDI', 'Burundi', 'Africa', 10),
('MWI', 'MWI', 'Malawi', 'Africa', 10),
('LSO', 'LSO', 'Lesotho', 'Africa', 10),
('SWZ', 'SWZ', 'Eswatini', 'Africa', 10),
('COM', 'COM', 'Comores', 'Africa', 10);
-- --- OCÉANIE ---
INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance) VALUES 
('AUS', 'AUS', 'Australie', 'Oceania', 60),
('NZL', 'NZL', 'Nouvelle-Zélande', 'Oceania', 45),
('PNG', 'PNG', 'Papouasie-Nouvelle-Guinée', 'Oceania', 15), -- 10 Millions d'habitants, mais très pauvre
('SLB', 'SLB', 'Îles Salomon', 'Oceania', 5),
('VUT', 'VUT', 'Vanuatu', 'Oceania', 5),
('FSM', 'FSM', 'Micronésie (États fédérés)', 'Oceania', 5),
('KIR', 'KIR', 'Kiribati', 'Oceania', 15),
('MHL', 'MHL', 'Îles Marshall', 'Oceania', 15),
('NRU', 'NRU', 'Nauru', 'Oceania', 15),              -- Plus petite république du monde
('PLW', 'PLW', 'Palaos', 'Oceania', 15),
('TUV', 'TUV', 'Tuvalu', 'Oceania', 15),
('WSM', 'WSM', 'Samoa', 'Oceania', 75),          -- L'État indépendant (Samoa Joe)
('TON', 'TON', 'Tonga', 'Oceania', 55),          -- Haku/Meng, Tama Tonga, Tanga Loa
('FJI', 'FJI', 'Fidji', 'Oceania', 45),          -- Jimmy "Superfly" Snuka (Origine)
('ASM', 'ASM', 'Samoa américaines', 'Oceania', 60); -- Territoire US, mais traité à part pour l'héritage Anoa'i
-- =================================================================================
-- 3. PEUPLEMENT DES RÉGIONS (TABLE: Regions)
-- Ces IDs (ex: USA_NE) seront utilisés par vos Child Companies pour le "Geo-Locking"
-- =================================================================================

-- --------------------------------------------------------
-- -- 1. Amerique Nord
-- ---------------------------------------------------------------------------------
INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
('USA_NY', 'New York', 'USA', 100),       -- Le fief historique (MSG, WWE HQ)
('USA_CA', 'California', 'USA', 95),      -- Le marché Ouest (PWG, WrestleMania stars)
('USA_FL', 'Florida', 'USA', 95),         -- La Mecque de l'entrainement (NXT, AEW Daily's Place)
('USA_TX', 'Texas', 'USA', 90),           -- Terre des Von Erichs, Stone Cold, Undertaker
('USA_PA', 'Pennsylvania', 'USA', 90),    -- Philadelphie (ECW Legacy) + Pittsburgh (Angle)
('USA_MA', 'Massachusetts', 'USA', 85),   -- Boston (Public très chaud, Cena/Sasha Banks)
('USA_NJ', 'New Jersey', 'USA', 85),      -- Gros marché Indy (GCW, WrestlePro)
('USA_CT', 'Connecticut', 'USA', 80),     -- Stamford (Siège WWE)
('USA_RI', 'Rhode Island', 'USA', 65),
('USA_IL', 'Illinois', 'USA', 95),        -- Chicago est sans doute le meilleur public US (AEW, CM Punk)
('USA_OH', 'Ohio', 'USA', 85),            -- Cleveland/Cincinnati (Moxley, The Miz, Gargano)
('USA_MN', 'Minnesota', 'USA', 80),       -- AWA Legacy (Lesnar, Hennig, Verne Gagne)
('USA_MI', 'Michigan', 'USA', 80),        -- Detroit (Big Van Vader, Steiner Brothers)
('USA_MO', 'Missouri', 'USA', 75),        -- St. Louis (NWA Historic Hub)
('USA_IN', 'Indiana', 'USA', 70),         -- Wrestling traditionnel 
('USA_GA', 'Georgia', 'USA', 90),         -- Atlanta (Ancien fief WCW, Cody Rhodes)
('USA_NC', 'North Carolina', 'USA', 90),  -- Charlotte (Flair Country, Hardy Boyz)
('USA_TN', 'Tennessee', 'USA', 85),       -- Memphis/Nashville (Jarrett, Lawler, TNA)
('USA_AL', 'Alabama', 'USA', 70),         -- Territoire historique
('USA_KY', 'Kentucky', 'USA', 70),        -- OVW (Formation de Cena, Batista, Orton)
('USA_SC', 'South Carolina', 'USA', 65),
('USA_VA', 'Virginia', 'USA', 65),
('USA_NV', 'Nevada', 'USA', 85),          -- Las Vegas (Double or Nothing, Gambling themes)
('USA_AZ', 'Arizona', 'USA', 70),         -- Phoenix
('USA_WA', 'Washington', 'USA', 65),      -- Seattle (Bryan Danielson)
('USA_OR', 'Oregon', 'USA', 60),          -- Portland
('USA_CO', 'Colorado', 'USA', 60),        -- Denver
('USA_OK', 'Oklahoma', 'USA', 70),        -- Jim Ross, Jack Swagger
('USA_MD', 'Maryland', 'USA', 80),        -- Baltimore est une ville historique (ROH, WCW, WWE)
('USA_DC', 'District of Columbia', 'USA', 75), -- Washington DC (Capital One Arena)
('USA_WV', 'West Virginia', 'USA', 65),   -- Public très fidèle et "Old School"
('USA_DE', 'Delaware', 'USA', 50),        -- Petit marché (ROH y passait souvent)
('USA_LA', 'Louisiana', 'USA', 75),       -- New Orleans (Plusieurs WrestleManias, Mid-South)
('USA_MS', 'Mississippi', 'USA', 50),     -- Terre du "Million Dollar Man"
('USA_AR', 'Arkansas', 'USA', 55),        -- Little Rock (Mid-South history)
('USA_WI', 'Wisconsin', 'USA', 75),       -- Milwaukee/Green Bay (AWA Stronghold, Mr. Anderson)
('USA_IA', 'Iowa', 'USA', 65),            -- Capitale de la lutte amateur (Seth Rollins Academy)
('USA_KS', 'Kansas', 'USA', 55),          -- Wichita
('USA_NE', 'Nebraska', 'USA', 50),        -- Omaha
('USA_ND', 'North Dakota', 'USA', 30),
('USA_SD', 'South Dakota', 'USA', 30),
('USA_UT', 'Utah', 'USA', 60),            -- Salt Lake City (Public assez bruyant)
('USA_NM', 'New Mexico', 'USA', 50),      -- Albuquerque
('USA_ID', 'Idaho', 'USA', 40),           -- Boise
('USA_MT', 'Montana', 'USA', 30),
('USA_WY', 'Wyoming', 'USA', 25),         -- État le moins peuplé
('USA_NH', 'New Hampshire', 'USA', 55),   -- Triple H est de Nashua
('USA_ME', 'Maine', 'USA', 45),
('USA_VT', 'Vermont', 'USA', 40),
('USA_HI', 'Hawaii', 'USA', 65),          -- Héritage Maivia/Steamboat, mais dur d'accès
('USA_AK', 'Alaska', 'USA', 30),          -- Très difficile logistiquement
('CAN_ON', 'Ontario', 'CAN', 90),         -- Toronto (Edge, Christian, Trish)
('CAN_QC', 'Québec', 'CAN', 90),          -- Montréal (Zayn, Owens, Hart Foundation) - Public unique
('CAN_AB', 'Alberta', 'CAN', 85),         -- Calgary (Le Donjon des Hart, Stampede)
('CAN_BC', 'British Columbia', 'CAN', 70),-- Vancouver (ECCW)
('CAN_MB', 'Manitoba', 'CAN', 65),        -- Winnipeg (Jericho, Omega)
('CAN_NS', 'Nova Scotia', 'CAN', 50),     -- Maritimes
('MEX_CMX', 'Ciudad de México', 'MEX', 100), -- Le centre mondial (Arena Mexico)
('MEX_JAL', 'Jalisco', 'MEX', 90),           -- Guadalajara (Arena Coliseo)
('MEX_NLE', 'Nuevo León', 'MEX', 85),        -- Monterrey (Lucha moderne)
('MEX_PUE', 'Puebla', 'MEX', 80),            -- Gros marché traditionnel
('MEX_BCN', 'Baja California', 'MEX', 75),   -- Tijuana (Crash Lucha Libre, Mysterio)
('MEX_MEX', 'Estado de México', 'MEX', 70),  -- Naucalpan (IWRG)
('PUR_SJ', 'San Juan Metro', 'PUR', 70),     -- La capitale (Coliseo de Puerto Rico)
('PUR_PO', 'Ponce (South)', 'PUR', 60),      -- Le sud historique
('PUR_MY', 'Mayagüez (West)', 'PUR', 60),    -- La côte ouest
('DOM_SD', 'Santo Domingo', 'DOM', 50), -- Parque Eugenio María de Hostos (Légendaire)
('PAN_PC', 'Panama City', 'PAN', 35),
('GTM_GC', 'Guatemala City', 'GTM', 25),
('CRI_SJ', 'San José', 'CRI', 25),
('CUB_LH', 'La Havane', 'CUB', 20),
('JAM_KI', 'Kingston', 'JAM', 15),
('BHS_NA', 'Nassau', 'BHS', 10),
('BRB_BR', 'Bridgetown', 'BRB', 10),
('TTO_PS', 'Port of Spain', 'TTO', 10),
('HTI_PP', 'Port-au-Prince', 'HTI', 10),
('SLV_SS', 'San Salvador', 'SLV', 15),
('HND_TE', 'Tegucigalpa', 'HND', 10),
('NIC_MA', 'Managua', 'NIC', 10),
('BLZ_BC', 'Belize City', 'BLZ', 10),
('LCA_CA', 'Castries', 'LCA', 10),        -- Capitale / Port de croisière
('ATG_SJ', 'Saint John''s', 'ATG', 10),
('GRD_SG', 'Saint George''s', 'GRD', 10),
('KNA_BA', 'Basseterre', 'KNA', 10),
('VCT_KI', 'Kingstown', 'VCT', 10),
('DMA_RO', 'Roseau', 'DMA', 10);

-- 2. Asie
-- ---------------------------------------------------------------------------------
-- Note : Le Japon fonctionne par "Tournées". Tokyo est le QG, les autres sont des étapes.

INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
-- KANTO (Le Cœur du marché)
('JPN_TO', 'Tokyo', 'JPN', 100),          -- Korakuen Hall, Tokyo Dome, Ryogoku (Le centre du monde)
('JPN_KN', 'Kanagawa', 'JPN', 85),        -- Yokohama (Yokohama Arena, Buntai) - Très proche de Tokyo
('JPN_SA', 'Saitama', 'JPN', 70),         -- Saitama Super Arena
-- KANSAI (Le deuxième bastion)
('JPN_OS', 'Osaka', 'JPN', 90),           -- Osaka-Jo Hall, Edion Arena (Public très vocal/difficile)
('JPN_HY', 'Hyogo', 'JPN', 75),           -- Kobe (Dragon Gate Base)
('JPN_KY', 'Kyoto', 'JPN', 65),           -- Marché historique traditionnel
-- AUTRES HUB MAJEURS (Pour les tournées nationales)
('JPN_AI', 'Aichi', 'JPN', 80),           -- Nagoya (3ème marché clé)
('JPN_FU', 'Fukuoka', 'JPN', 75),         -- Le hub de l'île de Kyushu (Wrestling Dontaku)
('JPN_HO', 'Hokkaido', 'JPN', 70),        -- Sapporo (Gros shows en hiver/été)
('JPN_MI', 'Miyagi', 'JPN', 60),          -- Sendai (Hub du Nord/Tohoku)
('JPN_HI', 'Hiroshima', 'JPN', 55),       -- Hub de l'ouest (Chugoku)
('JPN_CH', 'Chiba', 'JPN', 75),           -- TRES IMPORTANT (2AW, Blue Justice), banlieue de Tokyo
('JPN_IB', 'Ibaraki', 'JPN', 50),
('JPN_TC', 'Tochigi', 'JPN', 50),
('JPN_GU', 'Gunma', 'JPN', 45),
('JPN_SH', 'Shizuoka', 'JPN', 65),        -- Dragon Gate y passe souvent, Mt Fuji
('JPN_NI', 'Niigata', 'JPN', 60),         -- Place forte historique (Giant Baba est de là-bas)
('JPN_NA', 'Nagano', 'JPN', 50),
('JPN_GI', 'Gifu', 'JPN', 45),
('JPN_IS', 'Ishikawa (Kanazawa)', 'JPN', 45),
('JPN_TOY', 'Toyama', 'JPN', 40),
('JPN_FI', 'Fukui', 'JPN', 35),
('JPN_YA', 'Yamanashi', 'JPN', 35),
('JPN_IW', 'Iwate (Morioka)', 'JPN', 55), -- Base de la Michinoku Pro / Great Sasuke
('JPN_AO', 'Aomori', 'JPN', 50),
('JPN_FK', 'Fukushima', 'JPN', 45),
('JPN_AK', 'Akita', 'JPN', 40),
('JPN_YM', 'Yamagata', 'JPN', 40),
('JPN_NR', 'Nara', 'JPN', 50),
('JPN_SI', 'Shiga', 'JPN', 45),
('JPN_WK', 'Wakayama', 'JPN', 40),
('JPN_OK', 'Okayama', 'JPN', 55),
('JPN_YG', 'Yamaguchi', 'JPN', 45),
('JPN_TT', 'Tottori', 'JPN', 30),         -- Préfecture la moins peuplée
('JPN_SM', 'Shimane', 'JPN', 30),
('JPN_KA', 'Kagawa', 'JPN', 45),
('JPN_EH', 'Ehime (Matsuyama)', 'JPN', 45),
('JPN_KO', 'Kochi', 'JPN', 40),
('JPN_TK', 'Tokushima', 'JPN', 35),
('JPN_KM', 'Kumamoto', 'JPN', 60),        -- Gros marché au sud
('JPN_KG', 'Kagoshima', 'JPN', 55),       -- Terre de Ibushi
('JPN_NG', 'Nagasaki', 'JPN', 50),
('JPN_OI', 'Oita', 'JPN', 45),
('JPN_MZ', 'Miyazaki', 'JPN', 40),
('JPN_SG', 'Saga', 'JPN', 35),
('JPN_OKI', 'Okinawa', 'JPN', 55),
('IND_DL', 'Delhi (NCR)', 'IND', 60),     -- La Capitale (Shows WWE Live)
('IND_MH', 'Maharashtra', 'IND', 55),     -- Mumbai (Capitale du divertissement/Bollywood)
('IND_PB', 'Punjab', 'IND', 70),          -- Terre de Lutte (Great Khali, Jinder Mahal) - Le plus gros intérêt
('IND_WB', 'West Bengal', 'IND', 40),     -- Kolkata
('IND_TG', 'Telangana', 'IND', 45),       -- Hyderabad (WWE Superstar Spectacle 2023)
('IND_KA', 'Karnataka', 'IND', 40),       -- Bangalore (Tech Hub)
('CHN_SH', 'Shanghai', 'CHN', 50),        -- Hub international principal (OWE y était basé)
('CHN_BJ', 'Beijing', 'CHN', 45),         -- Capitale politique
('CHN_GD', 'Guangdong', 'CHN', 40),       -- Guangzhou/Shenzhen (Proche Hong Kong)
('CHN_HK', 'Hong Kong', 'CHN', 65),       -- S.A.R. (Marché historiquement plus ouvert, Ho Ho Lun)
('CHN_MO', 'Macau', 'CHN', 40),           -- S.A.R. (Casinos/Tourisme)
-- L'EST RICHE (Voisins de Shanghai)
('CHN_JS', 'Jiangsu (Nanjing)', 'CHN', 45),   -- Très riche, grosse population
('CHN_ZJ', 'Zhejiang (Hangzhou)', 'CHN', 45), -- Tech hub (Alibaba), potentiel moderne
-- LE CENTRE & OUEST (Les géants démographiques)
('CHN_SC', 'Sichuan (Chengdu)', 'CHN', 40),   -- Immense marché, culture relax, Pandas
('CHN_HB', 'Hubei (Wuhan)', 'CHN', 35),       -- Hub central de transport
('CHN_SN', 'Shaanxi (Xi''an)', 'CHN', 30),    -- Chine historique
-- LE NORD (Autour de Beijing)
('CHN_TJ', 'Tianjin', 'CHN', 40),             -- Municipalité portuaire majeure
('CHN_HE', 'Hebei', 'CHN', 30),
('CHN_LN', 'Liaoning (Shenyang)', 'CHN', 30), -- Frontière Corée du Nord, industriel
('CHN_SD', 'Shandong (Qingdao)', 'CHN', 35),
('SAU_RI', 'Riyadh Region', 'SAU', 80),   -- Crown Jewel, King Abdullah Stadium
('SAU_MK', 'Makkah Region', 'SAU', 75),
('SAU_EP', 'Eastern Province (Dammam)', 'SAU', 50), -- Hub pétrolier, beaucoup d'expats américains
('SAU_MD', 'Madinah Region', 'SAU', 30),            -- Importance religieuse, moins de divertissement
('SAU_AS', 'Asir (Abha)', 'SAU', 20),               -- Montagnes au sud, tourisme local
('KOR_SE', 'Seoul Capital Area', 'KOR', 40),
('KOR_BU', 'Busan (Yeongnam)', 'KOR', 35),     -- 2ème ville, immense port, public chaud
('KOR_IN', 'Incheon', 'KOR', 30),              -- Hub aéroportuaire, collé à Seoul mais distinct
('KOR_DA', 'Daegu', 'KOR', 25),                -- Ville intérieure, conservatrice
('KOR_GW', 'Gwangju (Honam)', 'KOR', 20),      -- Sud-Ouest, riche histoire politique
('KOR_DJ', 'Daejeon', 'KOR', 20),              -- Centre technologique et nœud ferroviaire
('KOR_JE', 'Jeju Island', 'KOR', 15),         -- Île touristique (vols obligatoires)
('PRK_RY', 'Mount Paektu (Samjiyon)', 'PRK', 30),
('PRK_PY', 'Pyongyang', 'PRK', 55),            -- May Day Stadium (Plus grand stade du monde)
('PRK_HA', 'Hamhung', 'PRK', 15),              -- 2ème ville, centre industriel et chimique
('PRK_CH', 'Chongjin', 'PRK', 10),             -- "La ville de fer", industrie lourde au nord-est
('PRK_KA', 'Kaesong (DMZ)', 'PRK', 15),        -- Ancienne capitale, zone frontalière (tourisme politique)
('PRK_WO', 'Wonsan', 'PRK', 10),               -- Zone balnéaire développée pour le tourisme (Kalma)
('PRK_NA', 'Nampo', 'PRK', 10),                -- Port principal de Pyongyang
('TWN_TP', 'Taipei', 'TWN', 50),
('MNG_UB', 'Ulaanbaatar', 'MNG', 20),
('SGP_CI', 'Singapore City', 'SGP', 35),
('PHL_MN', 'Metro Manila', 'PHL', 35),
('THA_BK', 'Bangkok', 'THA', 30),
('MYS_KL', 'Kuala Lumpur', 'MYS', 25),
('IDN_JK', 'Jakarta', 'IDN', 20),
('VNM_HC', 'Ho Chi Minh City', 'VNM', 15),
('KHM_PH', 'Phnom Penh', 'KHM', 15),
('LAO_VI', 'Vientiane', 'LAO', 15),
('MMR_YA', 'Yangon', 'MMR', 15),
('BRN_BS', 'Bandar Seri Begawan', 'BRN', 10),
('TLS_DI', 'Dili', 'TLS', 10),
('ARE_DU', 'Dubai & Abu Dhabi', 'ARE', 55), -- Le hub du luxe et du tourisme
('QAT_DO', 'Doha', 'QAT', 45),
('TUR_IS', 'Istanbul', 'TUR', 35),          -- Europe/Asie mix
('KWT_KC', 'Kuwait City', 'KWT', 20),
('BHR_MA', 'Manama', 'BHR', 15),
('OMN_MU', 'Muscat', 'OMN', 10),
('JOR_AM', 'Amman', 'JOR', 10),
('LBN_BE', 'Beirut', 'LBN', 10),
('IRQ_BA', 'Baghdad', 'IRQ', 10),
('IRN_TE', 'Tehran', 'IRN', 15),
('SYR_DA', 'Damascus', 'SYR', 10),
('YEM_SA', 'Sanaa', 'YEM', 10),
('PSE_RA', 'Ramallah', 'PSE', 15),
('PAK_LA', 'Lahore', 'PAK', 25),            -- Cœur culturel et sportif
('BGD_DH', 'Dhaka', 'BGD', 10),
('LKA_CO', 'Colombo', 'LKA', 10),
('NPL_KM', 'Kathmandu', 'NPL', 10),
('MDV_MA', 'Malé', 'MDV', 15),
('BTN_TH', 'Thimphu', 'BTN', 10),
('KAZ_AL', 'Almaty', 'KAZ', 20),
('UZB_TA', 'Tashkent', 'UZB', 15),
('KGZ_BI', 'Bishkek', 'KGZ', 10),
('TKM_AS', 'Ashgabat', 'TKM', 15),
('TJK_DU', 'Dushanbe', 'TJK', 15),
('AFG_KA', 'Kabul', 'AFG', 10);


-- ---------------------------------------------------------------------------------
-- 3. Europe
-- ---------------------------------------------------------------------------------
INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
-- ANGLETERRE (Les bastions)
('GBR_LDN', 'London (Greater London)', 'GBR', 100), -- Wembley, O2 Arena, York Hall (RevPro)
('GBR_NW', 'North West (Manchester/Liverpool)', 'GBR', 90), -- Terre de catch très rude (PCW, WWE NXT UK)
('GBR_WM', 'West Midlands (Birmingham)', 'GBR', 80),  -- Ville centrale, public "smark"
('GBR_YH', 'Yorkshire (Leeds/Sheffield)', 'GBR', 75), -- Catch traditionnel
('GBR_SE', 'South East (Brighton/Kent)', 'GBR', 70),  -- Riptide Wrestling
('GBR_NE', 'North East (Newcastle)', 'GBR', 85),   -- HUGE Market (Neville/Pac), public très bruyant
('GBR_EE', 'East of England (Norwich)', 'GBR', 70),-- Terre des Knight (Famille de Saraya/Paige - WAW)
('GBR_EM', 'East Midlands (Nottingham)', 'GBR', 65),
('GBR_SW', 'South West (Bristol)', 'GBR', 60),     -- Pro Wrestling Chaos
('GBR_SCT', 'Scotland (Glasgow/Edinburgh)', 'GBR', 85), -- ICW (Insane Championship Wrestling), public fou
('GBR_WAL', 'Wales (Cardiff)', 'GBR', 75),             -- Clash at the Castle, grosse base de fans
('GBR_NIR', 'Northern Ireland (Belfast)', 'GBR', 60),   -- OTT a une présence ici
('IRL_LE', 'Leinster (Dublin)', 'IRL', 80),    -- OTT Wrestling (National Stadium), scène très chaude
('IRL_MU', 'Munster (Cork)', 'IRL', 50),       -- Deuxième marché
('IRL_CO', 'Connacht (Galway)', 'IRL', 40),
('IRL_UL', 'Ulster (Donegal/Cavan)', 'IRL', 35),   -- Zone frontalière, plus rurale
('DEU_NW', 'Nordrhein-Westfalen', 'DEU', 95),  -- Oberhausen (wXw Academy & Shows), Cologne. Le "Japon de l'Europe".
('DEU_BE', 'Berlin', 'DEU', 85),               -- Capitale, GWF (German Wrestling Federation)
('DEU_HH', 'Hamburg', 'DEU', 80),              -- Catch historique (Tournois de CWA des années 80/90)
('DEU_BY', 'Bayern (Munich)', 'DEU', 75),      -- Munich, gros marché au sud
('DEU_HE', 'Hessen (Frankfurt)', 'DEU', 70),   -- Hub financier, bons shows internationaux
('DEU_SN', 'Sachsen (Dresden/Leipzig)', 'DEU', 60),
('DEU_NI', 'Niedersachsen (Hannover)', 'DEU', 75), -- Historique: Catch tournaments (POW) légendaires
('DEU_BW', 'Baden-Württemberg (Stuttgart)', 'DEU', 70), -- Gros hub industriel (Porsche/Mercedes)
('DEU_RP', 'Rheinland-Pfalz', 'DEU', 50),
('DEU_TH', 'Thüringen', 'DEU', 40),                -- Ex-RDA
('DEU_BB', 'Brandenburg', 'DEU', 35),              -- Autour de Berlin
('DEU_SA', 'Sachsen-Anhalt', 'DEU', 30),
('DEU_SH', 'Schleswig-Holstein (Kiel)', 'DEU', 40), -- Frontière danoise, wXw passe parfois à Kiel
('DEU_MV', 'Mecklenburg-Vorpommern', 'DEU', 25),    -- Très rural, ex-RDA, peu de shows
('DEU_SL', 'Saarland (Saarbrücken)', 'DEU', 45),    -- Frontière française, carrefour intéressant
('DEU_HB', 'Bremen', 'DEU', 50),                    -- Ville-État historique, proche de Hambourg
('FRA_IDF', 'Île-de-France (Paris)', 'FRA', 95),       -- Accor Arena (Backlash), APC (Studio Jenny)
('FRA_HDF', 'Hauts-de-France (Lille)', 'FRA', 80),     -- Proche frontière belge/UK, culture catch populaire
('FRA_ARA', 'Auvergne-Rhône-Alpes (Lyon)', 'FRA', 75), -- Lyon est une place forte historique
('FRA_GES', 'Grand Est (Strasbourg)', 'FRA', 65),      -- Influence du catch allemand/européen
('FRA_PAC', 'Provence-Alpes-Côte d''Azur', 'FRA', 60), -- Nice/Marseille
('FRA_OCC', 'Occitanie (Toulouse/Montpellier)', 'FRA', 55),
('FRA_NAQ', 'Nouvelle-Aquitaine (Bordeaux)', 'FRA', 70), -- Immense région, public en demande
('FRA_BRE', 'Bretagne (Rennes)', 'FRA', 65),             -- Forte identité culturelle (Kouign Amann Catch !)
('FRA_PDL', 'Pays de la Loire (Nantes)', 'FRA', 60),     -- Zénith de Nantes (gros potentiel)
('FRA_NOR', 'Normandie (Rouen/Caen)', 'FRA', 55),
('FRA_BFC', 'Bourgogne-Franche-Comté', 'FRA', 45),
('FRA_CVL', 'Centre-Val de Loire', 'FRA', 40),
('FRA_COR', 'Corse', 'FRA', 30),
('ESP_MD', 'Comunidad de Madrid', 'ESP', 70),  -- La Triple W, public très fidèle
('ESP_CT', 'Catalunya (Barcelona)', 'ESP', 75),-- RIOT Wrestling, RCDE Stadium
('ESP_PV', 'País Vasco (Bilbao)', 'ESP', 50),
('ESP_AN', 'Andalucía (Sevilla)', 'ESP', 45),
('ESP_VC', 'Comunidad Valenciana', 'ESP', 60),     -- Tyris Wrestling (Valence est active)
('ESP_GA', 'Galicia', 'ESP', 45),
('ESP_CL', 'Castilla y León', 'ESP', 40),
('ESP_CN', 'Islas Canarias', 'ESP', 55),
('ESP_AS', 'Principado de Asturias', 'ESP', 55),    -- Gijón/Oviedo : Scène active (Pro Wrestling Euskadi parfois)
('ESP_IB', 'Islas Baleares (Mallorca)', 'ESP', 50), -- Tourisme de masse (Shows d'été pour touristes allemands/uk)
('ESP_AR', 'Aragón (Zaragoza)', 'ESP', 45),         -- Carrefour entre Madrid et Barcelone
('ESP_MU', 'Región de Murcia', 'ESP', 40),
('ESP_NA', 'Navarra (Pamplona)', 'ESP', 40),
('ESP_CM', 'Castilla-La Mancha', 'ESP', 35),        -- La grande région autour de Madrid (Don Quichotte)
('ESP_CB', 'Cantabria (Santander)', 'ESP', 35),
('ESP_EX', 'Extremadura', 'ESP', 30),               -- Frontière portugaise, très rural
('ESP_LR', 'La Rioja', 'ESP', 30),
('ITA_LB', 'Lombardia (Milano)', 'ITA', 70),   -- Hub économique et catch (ICW Italy)
('ITA_LZ', 'Lazio (Roma)', 'ITA', 65),         -- Capitale, shows occasionnels WWE
('ITA_TO', 'Toscana (Firenze)', 'ITA', 50),
('ITA_CP', 'Campania (Napoli)', 'ITA', 45),
('ITA_PM', 'Piemonte (Torino)', 'ITA', 65),        -- Ville industrielle, riche histoire sportive
('ITA_VE', 'Veneto (Venezia/Verona)', 'ITA', 60),  -- Wrestling Megastars
('ITA_ER', 'Emilia-Romagna (Bologna)', 'ITA', 55),
('ITA_SI', 'Sicilia (Palermo)', 'ITA', 40),        -- Grand public, mais logistique insulaire
('ITA_PU', 'Puglia (Bari)', 'ITA', 35),
('ITA_LI', 'Liguria (Genova)', 'ITA', 55),          -- Port historique, public ouvrier
('ITA_FV', 'Friuli-Venezia Giulia', 'ITA', 40),     -- Frontière Autriche/Slovénie
('ITA_TA', 'Trentino-Alto Adige', 'ITA', 35),       -- Les Alpes, influence germanique
('ITA_MA', 'Marche (Ancona)', 'ITA', 35),           -- Côte Adriatique
('ITA_UM', 'Umbria (Perugia)', 'ITA', 35),          -- Le cœur vert de l'Italie
('ITA_SA', 'Sardegna (Cagliari)', 'ITA', 35),       -- Île isolée logistiquement
('ITA_AB', 'Abruzzo', 'ITA', 30),
('ITA_CA', 'Calabria', 'ITA', 25),                  -- La pointe de la botte, pauvre économiquement
('ITA_BA', 'Basilicata', 'ITA', 20),
('ITA_MO', 'Molise', 'ITA', 15),                    -- La région "qui n'existe pas" (blague italienne), très petit marché
('ITA_VA', 'Valle d''Aosta', 'ITA', 15),
('POL_WA', 'Warsaw (Mazovia)', 'POL', 50),     -- Capitale (KPW Arena)
('POL_GD', 'Gdansk (Pomerania)', 'POL', 45),   -- Kombat Pro Wrestling (Grosse scène)
('POL_SI', 'Silesia (Katowice)', 'POL', 40),   -- Zone industrielle, public ouvrier
('POL_MA', 'Lesser Poland (Krakow)', 'POL', 45), -- Capitale culturelle, tourisme, public étudiant
('POL_WP', 'Greater Poland (Poznan)', 'POL', 40),-- Hub commercial, proche frontière allemande
('POL_DS', 'Lower Silesia (Wroclaw)', 'POL', 40),-- Ville très dynamique, grosse économie
('POL_EA', 'Eastern Poland (Lublin)', 'POL', 25),-- Zone plus rurale et traditionnelle
('ROU_BU', 'Bucharest', 'ROU', 35),            -- Capitale, Romanian Pro Wrestling
('ROU_CJ', 'Cluj-Napoca (Transylvania)', 'ROU', 30), -- Hub étudiant et culturel
('ROU_WE', 'West (Timisoara)', 'ROU', 30),       -- Proche Serbie/Hongrie, influence occidentale
('ROU_EA', 'Moldavia Region (Iasi)', 'ROU', 25), -- Grand centre universitaire à l'Est
('ROU_SO', 'South East (Constanta)', 'ROU', 25), -- La côte (Mer Noire), tourisme d'été
('RUS_MO', 'Moscow', 'RUS', 45),               -- IWF (Independent Wrestling Federation)
('RUS_SP', 'Saint Petersburg', 'RUS', 40),     -- NSW (Northern Storm Wrestling)
('RUS_UR', 'Urals/Siberia', 'RUS', 20),        -- Immense zone, logistique difficile
('RUS_SO', 'South (Krasnodar/Sochi)', 'RUS', 35),-- Zone olympique, climat chaud
('RUS_VO', 'Volga Region (Kazan)', 'RUS', 30),   -- Hub sportif majeur
('RUS_FE', 'Far East (Vladivostok)', 'RUS', 15), -- Très loin, mais porte vers le Japon
('RUS_CN', 'Central (Nizhny Novgorod)', 'RUS', 25),
('NLD_RA', 'Randstad (Amsterdam/Rotterdam)', 'NLD', 60), -- Dutch Pro Wrestling
('NLD_SO', 'South (Eindhoven)', 'NLD', 45),
('BEL_BR', 'Brussels & Wallonia', 'BEL', 45),  -- Francophone
('BEL_FL', 'Flanders (Antwerp)', 'BEL', 50),   -- Néerlandophone (Plus proche scène wXw/Dutch)
('BEL_BC', 'Brussels Capital Region', 'BEL', 55), -- Siège de l'UE, public international
('CHE_GE', 'German Switz. (Zurich)', 'CHE', 60), -- SCW, wXw influence
('CHE_FR', 'French Switz. (Geneva)', 'CHE', 45), -- Influence française
('CHE_ITA', 'Italian Switz. (Bellinzone)', 'CHE', 45), -- Influence Italliene
('AUT_VI', 'Vienna', 'AUT', 55),               -- EWA, Prater Catcheurs
('AUT_TY', 'Tyrol/West', 'AUT', 40),
('PRT_LI', 'Lisbon', 'PRT', 40),               -- CTW (Centro de Treinos de Wrestling)
('PRT_PO', 'Porto', 'PRT', 35),                -- APW
('PRT_AL', 'Algarve (Faro)', 'PRT', 30),          -- Zone touristique (Shows d'été)
('PRT_IS', 'Islands (Madeira/Azores)', 'PRT', 25),-- Cristiano Ronaldo land, logistique avion
('SWE_ST', 'Stockholm', 'SWE', 40),            -- STHLM Wrestling
('SWE_GO', 'Gothenburg/Malmo', 'SWE', 35),     -- GBG Wrestling
('CZE_PR', 'Prague', 'CZE', 35),               -- VCW
('CZE_MO', 'Moravia (Brno/Ostrava)', 'CZE', 25),
('NOR_OS', 'Oslo', 'NOR', 25),
('DNK_CP', 'Copenhagen', 'DNK', 30),           -- BODYSLAM! Wrestling
('FIN_HE', 'Helsinki', 'FIN', 25),
('ISL_RE', 'Reykjavik', 'ISL', 15),
('EST_TA', 'Tallinn', 'EST', 10),
('LVA_RI', 'Riga', 'LVA', 10),
('LTU_VI', 'Vilnius', 'LTU', 10),
('GRC_AT', 'Athens', 'GRC', 25),               -- ZMAK Base
('HRV_ZA', 'Zagreb', 'HRV', 20),
('SRB_BE', 'Belgrade', 'SRB', 15),
('BGR_SO', 'Sofia', 'BGR', 20),
('HUN_BU', 'Budapest', 'HUN', 30),             -- HCW Dojo
('SVN_LJ', 'Ljubljana', 'SVN', 15),
('SVK_BR', 'Bratislava', 'SVK', 15),
('BIH_SA', 'Sarajevo', 'BIH', 10),
('MKD_SK', 'Skopje', 'MKD', 10),
('ALB_TI', 'Tirana', 'ALB', 10),
('MNE_PO', 'Podgorica', 'MNE', 10),
('CYP_NI', 'Nicosia', 'CYP', 15),
('UKR_KI', 'Kyiv', 'UKR', 25),
('BLR_MI', 'Minsk', 'BLR', 10),
('MDA_CH', 'Chisinau', 'MDA', 10),
('MCO_MC', 'Monte Carlo', 'MCO', 30),
('LUX_LU', 'Luxembourg City', 'LUX', 25),
('MLT_VA', 'Valletta', 'MLT', 20),
('AND_AL', 'Andorra la Vella', 'AND', 10),
('SMR_SM', 'San Marino', 'SMR', 10),
('LIE_VA', 'Vaduz', 'LIE', 10);
-- --------------------------------------------------------
-- Amerique Sud
-- --------------------------------------------------------
-- =================================================================================
-- RÉGIONS DÉTAILLÉES - AMÉRIQUE DU SUD
-- =================================================================================

-- ---------------------------------------------------------------------------------
-- 1. CHILI (CHL) - Le bastion du Catch
-- ---------------------------------------------------------------------------------
INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
('CHL_RM', 'Santiago (Metropolitana)', 'CHL', 55), -- Le hub central (Movistar Arena)
('CHL_VA', 'Valparaíso & Coast', 'CHL', 45),       -- Villes portuaires, grosse activité
('CHL_BI', 'Biobío (Concepción)', 'CHL', 40),      -- Le sud industriel
('CHL_NO', 'Norte (Antofagasta)', 'CHL', 30),
('BRA_SP', 'São Paulo State', 'BRA', 50),          -- Le cœur économique et catch (BWF Base)
('BRA_RJ', 'Rio de Janeiro', 'BRA', 45),           -- Tourisme, MMA history
('BRA_SU', 'South (Porto Alegre)', 'BRA', 40),     -- Proche frontière Argentine/Uruguay
('BRA_MG', 'Minas Gerais (Belo Horizonte)', 'BRA', 35),
('BRA_NE', 'North East (Salvador/Recife)', 'BRA', 30), -- Immense population, culture différente
('BRA_AM', 'Amazonas (Manaus)', 'BRA', 20),
('ARG_BA', 'Buenos Aires (City & Metro)', 'ARG', 45), -- Luna Park Stadium (Légendaire)
('ARG_CB', 'Córdoba', 'ARG', 35),                  -- 2ème ville
('ARG_SF', 'Santa Fe (Rosario)', 'ARG', 30),       -- Ville de Messi, hub agricole
('ARG_MD', 'Mendoza (Cuyo)', 'ARG', 25),
('PER_LI', 'Lima Region', 'PER', 35),              -- La capitale concentre tout
('PER_CU', 'Cusco & South', 'PER', 25),
('COL_BO', 'Bogotá DC', 'COL', 30),
('COL_AN', 'Medellín (Antioquia)', 'COL', 30),     -- Ville très dynamique
('COL_VA', 'Cali (Valle del Cauca)', 'COL', 20),
('ECU_QU', 'Quito & Guayaquil', 'ECU', 25),        -- WAR Wrestling
('BOL_LP', 'La Paz (El Alto)', 'BOL', 30),         -- Fief des "Fighting Cholitas"
('URY_MO', 'Montevideo', 'URY', 15),
('VEN_CA', 'Caracas', 'VEN', 10),
('PRY_AS', 'Asunción', 'PRY', 10),
('GUY_GE', 'Georgetown', 'GUY', 15),
('SUR_PA', 'Paramaribo', 'SUR', 15);
 -- --------------------------------------------------------
-- Afrique
-- --------------------------------------------------------
INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
('ZAF_GT', 'Gauteng (Joburg/Pretoria)', 'ZAF', 65), -- Le cœur économique, Sun City
('ZAF_WC', 'Western Cape (Cape Town)', 'ZAF', 60),  -- Grand West Arena
('ZAF_KZ', 'KwaZulu-Natal (Durban)', 'ZAF', 45),
('ZAF_NW', 'North West (Sun City)', 'ZAF', 55),   -- TRES IMPORTANT : C'est là qu'est le Superbowl de Sun City (fief historique)
('ZAF_EC', 'Eastern Cape (Gqeberha)', 'ZAF', 40), -- Grosse tradition sportive (Rugby/Cricket), public passionné
('ZAF_FS', 'Free State (Bloemfontein)', 'ZAF', 35),-- Centre géographique, hub judiciaire
('ZAF_MP', 'Mpumalanga', 'ZAF', 30),              -- Tourisme (Kruger Park), shows dans les lodges
('ZAF_LI', 'Limpopo', 'ZAF', 25),                 -- Frontière nord
('ZAF_NC', 'Northern Cape', 'ZAF', 20),
('NGA_LA', 'Lagos', 'NGA', 40),           -- Ville monstre (20M habitants)
('NGA_FC', 'Abuja (Capital)', 'NGA', 30), -- Centre administratif
('NGA_PH', 'Port Harcourt', 'NGA', 25),
('NGA_KN', 'Kano (North)', 'NGA', 35),            -- La plus grande ville du Nord. Culture très différente du sud.
('NGA_OY', 'Oyo (Ibadan)', 'NGA', 35),            -- Immense ville proche de Lagos, berceau culturel Yoruba
('NGA_EN', 'Enugu (East)', 'NGA', 30),            -- Hub de l'Est (Igbo land), tradition de force
('NGA_KD', 'Kaduna', 'NGA', 25),                  -- Hub politique du Nord
('NGA_DE', 'Delta State (Warri)', 'NGA', 25),
('EGY_CA', 'Cairo & Giza', 'EGY', 35),    -- Cairo Stadium Indoor Halls
('EGY_AL', 'Alexandria', 'EGY', 25),      -- La côte méditerranéenne
('EGY_SS', 'South Sinai (Sharm El Sheikh)', 'EGY', 45), -- LE lieu pour les shows internationaux/touristiques (comme l'Arabie Saoudite)
('EGY_PS', 'Suez Canal (Port Said)', 'EGY', 30),  -- Zone industrielle riche
('EGY_LU', 'Luxor & Aswan', 'EGY', 25),           -- Cadre historique incroyable (Shows en plein air), mais logistique dure
('EGY_DT', 'Nile Delta (Mansoura)', 'EGY', 20),
('MAR_CS', 'Casablanca/Rabat', 'MAR', 30),-- Le hub économique
('MAR_MA', 'Marrakech', 'MAR', 25),      -- Tourisme événementiel
('MAR_TT', 'Tanger-Tétouan', 'MAR', 35),          -- Porte de l'Europe, économie en boom, public jeune
('MAR_SM', 'Souss-Massa (Agadir)', 'MAR', 30),    -- Hub touristique majeur du sud
('MAR_FM', 'Fès-Meknès', 'MAR', 25),              -- Cœur spirituel/historique
('MAR_OR', 'Oriental (Oujda)', 'MAR', 20),
('SEN_DA', 'Dakar', 'SEN', 35),         -- Arène nationale de lutte (Stade Léopold Sédar Senghor)
('SEN_TH', 'Thiès', 'SEN', 30),                   -- "La ville aux deux gares", gros bastion de lutte traditionnelle
('SEN_SL', 'Saint-Louis', 'SEN', 25),             -- Ville historique au nord
('SEN_ZI', 'Ziguinchor (Casamance)', 'SEN', 20), 
('DZA_AL', 'Alger', 'DZA', 20),
('TUN_TU', 'Tunis', 'TUN', 20),
('LBY_TR', 'Tripoli', 'LBY', 15),
('KEN_NA', 'Nairobi', 'KEN', 20),
('TZA_DA', 'Dar es Salaam', 'TZA', 15),
('ETH_AD', 'Addis Ababa', 'ETH', 10),
('UGA_KA', 'Kampala', 'UGA', 10),
('RWA_KI', 'Kigali', 'RWA', 10),
('SYC_VI', 'Victoria', 'SYC', 10),
('MUS_PL', 'Port Louis', 'MUS', 10), 
('CMR_DO', 'Douala/Yaoundé', 'CMR', 20),
('CIV_AB', 'Abidjan', 'CIV', 20),
('COD_KI', 'Kinshasa', 'COD', 15),        -- Catch Edingwe (Populaire localement)
('COG_BR', 'Brazzaville', 'COG', 10),
('GAB_LI', 'Libreville', 'GAB', 10),
('AGO_LU', 'Luanda', 'AGO', 10),
('GHA_AC', 'Accra', 'GHA', 25),
('ZWE_HA', 'Harare', 'ZWE', 10),
('NAM_WI', 'Windhoek', 'NAM', 15),
('BWA_GA', 'Gaborone', 'BWA', 15),
('ZMB_LU', 'Lusaka', 'ZMB', 15),
('MOZ_MA', 'Maputo', 'MOZ', 15),
('MDG_AN', 'Antananarivo', 'MDG', 15),
('MLI_BA', 'Bamako', 'MLI', 15),
('BFA_OU', 'Ouagadougou', 'BFA', 15),
('GIN_CO', 'Conakry', 'GIN', 15),
('SLE_FR', 'Freetown', 'SLE', 10),
('LBR_MO', 'Monrovia', 'LBR', 10),
('SDN_KH', 'Khartoum', 'SDN', 10),
('SSD_JU', 'Juba', 'SSD', 10),
('SOM_MO', 'Mogadishu', 'SOM', 10),
('ERI_AS', 'Asmara', 'ERI', 10),
('DJI_DJ', 'Djibouti City', 'DJI', 10),
('TCD_ND', 'N''Djamena', 'TCD', 10),
('NER_NI', 'Niamey', 'NER', 10),
('MRT_NK', 'Nouakchott', 'MRT', 10),
('GMB_BA', 'Banjul', 'GMB', 10),
('GNB_BI', 'Bissau', 'GNB', 10),
('CPV_PR', 'Praia', 'CPV', 15),
('STP_SA', 'Sao Tomé', 'STP', 10),
('GNQ_MA', 'Malabo', 'GNQ', 10),
('CAF_BA', 'Bangui', 'CAF', 10),
('BDI_GI', 'Gitega', 'BDI', 10),
('MWI_LI', 'Lilongwe', 'MWI', 10),
('LSO_MA', 'Maseru', 'LSO', 10),
('SWZ_MB', 'Mbabane', 'SWZ', 10),
('COM_MO', 'Moroni', 'COM', 10);
-- --------------------------------------------------------
-- Océanie
-- --------------------------------------------------------
INSERT INTO Regions (RegionId, Name, CountryId, WrestlingImportance) VALUES 
('AUS_NSW', 'New South Wales (Sydney)', 'AUS', 70), -- PWA Black Label, Robbie Eagles, The IIconics
('AUS_VIC', 'Victoria (Melbourne)', 'AUS', 70),     -- MCW (Melbourne City Wrestling), Buddy Matthews
('AUS_QLD', 'Queensland (Brisbane)', 'AUS', 60),
('AUS_SA', 'South Australia (Adelaide)', 'AUS', 55), -- Riot City Wrestling (RCW), Rhea Ripley y a débuté
('AUS_WA', 'Western Australia (Perth)', 'AUS', 50),  -- EPW (Explosive Pro Wrestling), TRÈS isolé (4h de vol de Sydney)
('AUS_TAS', 'Tasmania (Hobart)', 'AUS', 30),         -- Une île au sud, petit marché
('AUS_ACT', 'Canberra (Capital Territory)', 'AUS', 35),
('AUS_NT', 'Northern Territory (Darwin)', 'AUS', 20),
('NZL_SI', 'South Island (Christchurch)', 'NZL', 50),
('NZL_NI', 'North Island (Wellington)', 'NZL', 50),
('PNG_PM', 'Port Moresby', 'PNG', 15),
('SLB_HO', 'Honiara', 'SLB', 5),
('VUT_PV', 'Port Vila', 'VUT', 5),
('FSM_PA', 'Palikir', 'FSM', 5),
('KIR_TR', 'Tarawa', 'KIR', 15),
('MHL_MA', 'Majuro', 'MHL', 15),
('NRU_YA', 'Yaren', 'NRU', 15),
('PLW_NG', 'Ngerulmud', 'PLW', 15),
('WSM_AP', 'Apia', 'WSM', 75),
('TON_NU', 'Nuku''alofa', 'TON', 55), 
('FJI_SU', 'Suva', 'FJI', 45), 
('ASM_PP', 'Pago Pago', 'ASM', 60),
('TUV_FU', 'Funafuti', 'TUV', 15);
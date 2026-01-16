-- =================================================================================
-- Migration: 027_Populate_Companies.sql (V2)
-- Description: 1 Company per major Country + YouthStructures + ChildCompanies
-- =================================================================================

BEGIN TRANSACTION;

-- =================================================================================
-- 1. COMPAGNIES PRINCIPALES (1 par pays majeur, fictives mais inspirées du réel)
-- =================================================================================

-- USA (Inspiré WWE + AEW)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_USA', 'Global Wrestling Alliance', 'USA', 'USA_NY', 90, 50000000, 2500000, 95, 1, 'STYLE_SPORTS_ENTERTAINMENT');

-- Japon (Inspiré NJPW)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_JPN', 'King''s Road Pro Wrestling', 'JPN', 'JPN_TO', 85, 25000000, 800000, 80, 1, 'STYLE_STRONG_STYLE');

-- Mexique (Inspiré CMLL + AAA)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_MEX', 'Lucha Azteca Universal', 'MEX', 'MEX_CMX', 75, 15000000, 600000, 70, 1, 'STYLE_LUCHA_LIBRE');

-- Royaume-Uni (Inspiré Revolution Pro + Progress)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_GBR', 'British Empire Wrestling', 'GBR', 'GBR_LDN', 68, 8000000, 350000, 65, 1, 'STYLE_PURE_WRESTLING');

-- Canada (Inspiré Stampede + ECCW)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_CAN', 'Northern Storm Wrestling', 'CAN', 'CAN_AB', 65, 5000000, 200000, 55, 1, 'STYLE_HYBRID');

-- Allemagne (Inspiré wXw)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_DEU', 'Westside Xtreme Wrestling', 'DEU', 'DEU_NW', 60, 3000000, 120000, 50, 1, 'STYLE_PURE_WRESTLING');

-- France (Inspiré Catch Wrestling Organisation)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_FRA', 'Fédération Française de Catch', 'FRA', 'FRA_IDF', 55, 2000000, 80000, 45, 1, 'STYLE_HYBRID');

-- Australie (Inspiré MCW + PWA)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_AUS', 'Pacific Pro Wrestling', 'AUS', 'AUS_VIC', 58, 2500000, 100000, 50, 1, 'STYLE_INDIE');

-- Irlande (Inspiré OTT)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_IRL', 'Celtic Championship Wrestling', 'IRL', 'IRL_LE', 55, 1500000, 60000, 40, 1, 'STYLE_PURE_WRESTLING');

-- Arabie Saoudite (Nouveau marché émergent)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_SAU', 'Arabian Desert Wrestling', 'SAU', 'SAU_RI', 50, 20000000, 150000, 35, 1, 'STYLE_SPORTS_ENTERTAINMENT');

-- Chili (Inspiré Xplosion Lucha Libre)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_CHL', 'Andes Championship Wrestling', 'CHL', 'CHL_RM', 45, 800000, 40000, 30, 1, 'STYLE_LUCHA_LIBRE');

-- Afrique du Sud (Inspiré AWA South Africa)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_ZAF', 'African Wrestling Alliance', 'ZAF', 'ZAF_GT', 40, 500000, 25000, 25, 1, 'STYLE_INDIE');

-- Porto Rico (Inspiré WWC)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_PUR', 'Corazón de León Wrestling', 'PUR', 'PUR_SJ', 50, 1000000, 50000, 35, 1, 'STYLE_LUCHA_LIBRE');

-- Inde (Inspiré CWE India)
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
VALUES ('COMP_IND', 'Continental Wrestling Empire', 'IND', 'IND_PB', 35, 1500000, 100000, 30, 1, 'STYLE_SPORTS_ENTERTAINMENT');

-- =================================================================================
-- 2. YOUTH STRUCTURES (Centre de formation pour chaque compagnie majeure)
-- =================================================================================

INSERT INTO YouthStructures (YouthStructureId, CompanyId, Name, Capacity, CountryId, RegionId) VALUES
('YOUTH_USA', 'COMP_USA', 'GWA Performance Center', 50, 'USA', 'USA_FL'),
('YOUTH_JPN', 'COMP_JPN', 'KRPW Dojo', 30, 'JPN', 'JPN_TO'),
('YOUTH_MEX', 'COMP_MEX', 'Academia Azteca', 35, 'MEX', 'MEX_CMX'),
('YOUTH_GBR', 'COMP_GBR', 'British Wrestling Academy', 25, 'GBR', 'GBR_NW'),
('YOUTH_CAN', 'COMP_CAN', 'Hart Dungeon Legacy', 20, 'CAN', 'CAN_AB'),
('YOUTH_DEU', 'COMP_DEU', 'wXw Academy', 20, 'DEU', 'DEU_NW'),
('YOUTH_FRA', 'COMP_FRA', 'École de Catch de Paris', 15, 'FRA', 'FRA_IDF'),
('YOUTH_AUS', 'COMP_AUS', 'Pacific Training Centre', 15, 'AUS', 'AUS_VIC'),
('YOUTH_IRL', 'COMP_IRL', 'Celtic Dojo', 12, 'IRL', 'IRL_LE'),
('YOUTH_SAU', 'COMP_SAU', 'Saudi Sports Academy', 20, 'SAU', 'SAU_RI');

-- =================================================================================
-- 3. CHILD COMPANIES (Filiales de développement pour compagnies majeures)
-- =================================================================================

-- USA : 2 Child Companies (Développement + Marque Secondaire)
INSERT INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget) VALUES
('CC_USA_DEV', 'COMP_USA', 'GWA Evolve (Development)', 'USA_FL', 'Development', 150000),
('CC_USA_ADV', 'COMP_USA', 'GWA Velocity (Advanced)', 'USA_CA', 'Advanced', 300000);

-- Japon : 1 Child Company
INSERT INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget) VALUES
('CC_JPN_DEV', 'COMP_JPN', 'KRPW Young Lions', 'JPN_OS', 'Development', 80000);

-- Mexique : 1 Child Company
INSERT INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget) VALUES
('CC_MEX_DEV', 'COMP_MEX', 'Lucha Azteca Futura', 'MEX_JAL', 'Development', 50000);

-- UK : 1 Child Company
INSERT INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget) VALUES
('CC_GBR_DEV', 'COMP_GBR', 'BEW Rising Stars', 'GBR_SCT', 'Development', 40000);

-- Canada : 1 Child Company
INSERT INTO ChildCompanies (ChildCompanyId, ParentCompanyId, Name, RegionId, Level, MonthlyBudget) VALUES
('CC_CAN_DEV', 'COMP_CAN', 'Northern Lights Academy', 'CAN_BC', 'Development', 30000);

-- =================================================================================
-- 4. COMPANY CUSTOMIZATION
-- =================================================================================

INSERT INTO CompanyCustomization (CompanyId, PrimaryColor, SecondaryColor, Notes) VALUES
('COMP_USA', '#FF0000', '#000000', 'Global leader in Sports Entertainment'),
('COMP_JPN', '#FFFFFF', '#FFD700', 'Strong Style tradition since 1980'),
('COMP_MEX', '#006847', '#CE1126', 'Lucha Libre heritage'),
('COMP_GBR', '#00247D', '#CF142B', 'British wrestling technical excellence'),
('COMP_CAN', '#FF0000', '#FFFFFF', 'Calgary wrestling legacy'),
('COMP_DEU', '#000000', '#FFD700', 'European hardcore culture'),
('COMP_FRA', '#0055A4', '#EF4135', 'French catch renaissance'),
('COMP_AUS', '#00008B', '#FFD700', 'Pacific indie spirit'),
('COMP_IRL', '#169B62', '#FF883E', 'Celtic fighting tradition'),
('COMP_SAU', '#006C35', '#FFFFFF', 'New era of Arabian entertainment'),
('COMP_CHL', '#D52B1E', '#FFFFFF', 'South American passion'),
('COMP_ZAF', '#007749', '#FFB612', 'African wrestling pioneer'),
('COMP_PUR', '#FF0000', '#FFFFFF', 'Caribbean wrestling heart'),
('COMP_IND', '#FF9933', '#138808', 'Billion fan potential');

-- =================================================================================
-- 5. TV DEALS (Pour compagnies avec Prestige > 50)
-- =================================================================================

INSERT INTO TVDeals (TvDealId, CompanyId, NetworkName, StartDate, EndDate, AudienceCap, Revenue) VALUES
('TV_USA', 'COMP_USA', 'NBC Universal', 1, 260, 4000000, 250000),
('TV_JPN', 'COMP_JPN', 'TV Asahi', 1, 260, 1500000, 100000),
('TV_MEX', 'COMP_MEX', 'Televisa', 1, 260, 1000000, 60000),
('TV_GBR', 'COMP_GBR', 'ITV Sport', 1, 156, 500000, 40000),
('TV_CAN', 'COMP_CAN', 'TSN', 1, 156, 300000, 25000),
('TV_DEU', 'COMP_DEU', 'Sport1', 1, 156, 200000, 15000),
('TV_FRA', 'COMP_FRA', 'L''Equipe TV', 1, 104, 150000, 10000),
('TV_AUS', 'COMP_AUS', 'Fox Sports Australia', 1, 104, 180000, 12000),
('TV_IRL', 'COMP_IRL', 'TG4', 1, 104, 100000, 8000),
('TV_SAU', 'COMP_SAU', 'SSC Sports', 1, 156, 200000, 30000),
('TV_PUR', 'COMP_PUR', 'Telemundo Puerto Rico', 1, 104, 80000, 6000);

-- =================================================================================
-- 6. INDIES (Génération procédurale pour régions à haute importance)
-- =================================================================================

INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach, SimLevel, CatchStyleId)
SELECT 
    'COMP_INDIE_' || r.RegionId as CompanyId,
    r.Name || CASE abs(random() % 5)
        WHEN 0 THEN ' Wrestling'
        WHEN 1 THEN ' Pro Wrestling'
        WHEN 2 THEN ' Championship Wrestling'
        WHEN 3 THEN ' Combat Club'
        ELSE ' Wrestling Alliance'
    END as Name,
    r.CountryId,
    r.RegionId,
    20 + abs(random() % 25) as Prestige,
    50000 + abs(random() % 200000) as Treasury,
    500 + abs(random() % 5000) as AverageAudience,
    10 + abs(random() % 20) as Reach,
    1 as SimLevel,
    CASE abs(random() % 3)
        WHEN 0 THEN 'STYLE_INDIE'
        WHEN 1 THEN 'STYLE_HARDCORE'
        ELSE 'STYLE_PURE_WRESTLING'
    END as CatchStyleId
FROM Regions r
WHERE r.WrestlingImportance > 50
AND r.RegionId NOT IN (
    SELECT RegionId FROM Companies WHERE RegionId IS NOT NULL
)
AND abs(random() % 100) < 30;

COMMIT;

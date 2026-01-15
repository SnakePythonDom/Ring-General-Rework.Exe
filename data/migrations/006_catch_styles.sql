-- Migration 006: Catch Styles System (Product Style)
-- Définit l'ADN du style de catch de chaque compagnie

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: CatchStyles
-- Styles de catch prédéfinis (Pure Wrestling, Entertainment, Hardcore, etc.)
-- ============================================================================

CREATE TABLE IF NOT EXISTS CatchStyles (
    CatchStyleId TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE CHECK(length(Name) >= 3 AND length(Name) <= 50),
    Description TEXT,

    -- Caractéristiques du style (0-100)
    WrestlingPurity INTEGER NOT NULL DEFAULT 50 CHECK(WrestlingPurity >= 0 AND WrestlingPurity <= 100),
    EntertainmentFocus INTEGER NOT NULL DEFAULT 50 CHECK(EntertainmentFocus >= 0 AND EntertainmentFocus <= 100),
    HardcoreIntensity INTEGER NOT NULL DEFAULT 0 CHECK(HardcoreIntensity >= 0 AND HardcoreIntensity <= 100),
    LuchaInfluence INTEGER NOT NULL DEFAULT 0 CHECK(LuchaInfluence >= 0 AND LuchaInfluence <= 100),
    StrongStyleInfluence INTEGER NOT NULL DEFAULT 0 CHECK(StrongStyleInfluence >= 0 AND StrongStyleInfluence <= 100),

    -- Impact sur les attentes du public
    FanExpectationMatchQuality INTEGER NOT NULL DEFAULT 50 CHECK(FanExpectationMatchQuality >= 0 AND FanExpectationMatchQuality <= 100),
    FanExpectationStorylines INTEGER NOT NULL DEFAULT 50 CHECK(FanExpectationStorylines >= 0 AND FanExpectationStorylines <= 100),
    FanExpectationPromos INTEGER NOT NULL DEFAULT 50 CHECK(FanExpectationPromos >= 0 AND FanExpectationPromos <= 100),
    FanExpectationSpectacle INTEGER NOT NULL DEFAULT 50 CHECK(FanExpectationSpectacle >= 0 AND FanExpectationSpectacle <= 100),

    -- Modificateurs de rating
    MatchRatingMultiplier REAL NOT NULL DEFAULT 1.0 CHECK(MatchRatingMultiplier >= 0.5 AND MatchRatingMultiplier <= 2.0),
    PromoRatingMultiplier REAL NOT NULL DEFAULT 1.0 CHECK(PromoRatingMultiplier >= 0.5 AND PromoRatingMultiplier <= 2.0),

    -- Icône / Couleur pour l'UI
    IconName TEXT,
    AccentColor TEXT,  -- Hex color code

    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_catch_styles_active ON CatchStyles(IsActive);

-- ============================================================================
-- LIAISON COMPANIES <-> CATCHSTYLES
-- Ajoute la contrainte de clé étrangère (migration 005 a créé la colonne)
-- ============================================================================

-- Note: SQLite ne supporte pas ALTER TABLE ADD CONSTRAINT, donc on crée un index
-- et la validation sera faite au niveau applicatif
CREATE INDEX IF NOT EXISTS idx_companies_catch_style ON Companies(CatchStyleId);

-- ============================================================================
-- DONNÉES DE RÉFÉRENCE : STYLES PRÉDÉFINIS
-- ============================================================================

INSERT INTO CatchStyles (CatchStyleId, Name, Description, WrestlingPurity, EntertainmentFocus, HardcoreIntensity, LuchaInfluence, StrongStyleInfluence, FanExpectationMatchQuality, FanExpectationStorylines, FanExpectationPromos, FanExpectationSpectacle, MatchRatingMultiplier, PromoRatingMultiplier, IconName, AccentColor, IsActive) VALUES

-- Style 1: Pure Wrestling (Technical Focus)
('STYLE_PURE_WRESTLING',
 'Pure Wrestling',
 'Focus absolu sur la qualité technique des matchs. Workrate élevé, compétition sportive, peu de storylines extravagantes.',
 90,  -- WrestlingPurity
 20,  -- EntertainmentFocus
 0,   -- HardcoreIntensity
 10,  -- LuchaInfluence
 30,  -- StrongStyleInfluence
 90,  -- FanExpectationMatchQuality
 30,  -- FanExpectationStorylines
 20,  -- FanExpectationPromos
 20,  -- FanExpectationSpectacle
 1.3, -- MatchRatingMultiplier (les bons matchs sont encore mieux notés)
 0.7, -- PromoRatingMultiplier (les promos comptent moins)
 '🥋',
 '#4CAF50',
 1),

-- Style 2: Sports Entertainment
('STYLE_SPORTS_ENTERTAINMENT',
 'Sports Entertainment',
 'Équilibre entre wrestling et spectacle. Storylines complexes, gimmicks marquants, grande production.',
 50,  -- WrestlingPurity
 80,  -- EntertainmentFocus
 20,  -- HardcoreIntensity
 15,  -- LuchaInfluence
 10,  -- StrongStyleInfluence
 60,  -- FanExpectationMatchQuality
 80,  -- FanExpectationStorylines
 70,  -- FanExpectationPromos
 85,  -- FanExpectationSpectacle
 1.0, -- MatchRatingMultiplier
 1.2, -- PromoRatingMultiplier (les promos comptent plus)
 '🎭',
 '#2196F3',
 1),

-- Style 3: Hardcore/Extreme
('STYLE_HARDCORE',
 'Hardcore Wrestling',
 'Violence extrême, objets, sang, no-DQ. Public niche amateur de brutalité et spots dangereux.',
 30,  -- WrestlingPurity
 40,  -- EntertainmentFocus
 95,  -- HardcoreIntensity
 0,   -- LuchaInfluence
 20,  -- StrongStyleInfluence
 50,  -- FanExpectationMatchQuality
 40,  -- FanExpectationStorylines
 30,  -- FanExpectationPromos
 80,  -- FanExpectationSpectacle (spots spectaculaires)
 1.1, -- MatchRatingMultiplier
 0.8, -- PromoRatingMultiplier
 '💀',
 '#F44336',
 1),

-- Style 4: Lucha Libre
('STYLE_LUCHA_LIBRE',
 'Lucha Libre',
 'High-flying, rythme rapide, masques, trios. Tradition mexicaine avec acrobaties spectaculaires.',
 60,  -- WrestlingPurity
 60,  -- EntertainmentFocus
 10,  -- HardcoreIntensity
 95,  -- LuchaInfluence
 0,   -- StrongStyleInfluence
 75,  -- FanExpectationMatchQuality
 50,  -- FanExpectationStorylines
 40,  -- FanExpectationPromos
 80,  -- FanExpectationSpectacle
 1.2, -- MatchRatingMultiplier (high spots)
 0.9, -- PromoRatingMultiplier
 '🎪',
 '#FF9800',
 1),

-- Style 5: Strong Style (Puroresu)
('STYLE_STRONG_STYLE',
 'Strong Style',
 'Dureté japonaise, stiff strikes, fighting spirit. Présentation sérieuse et sportive.',
 85,  -- WrestlingPurity
 30,  -- EntertainmentFocus
 30,  -- HardcoreIntensity
 5,   -- LuchaInfluence
 95,  -- StrongStyleInfluence
 85,  -- FanExpectationMatchQuality
 40,  -- FanExpectationStorylines
 25,  -- FanExpectationPromos
 50,  -- FanExpectationSpectacle
 1.3, -- MatchRatingMultiplier
 0.7, -- PromoRatingMultiplier
 '⚔️',
 '#9C27B0',
 1),

-- Style 6: Hybrid/Balanced
('STYLE_HYBRID',
 'Hybrid Wrestling',
 'Mix équilibré de tous les styles. Adaptable selon les talents et le public.',
 60,  -- WrestlingPurity
 60,  -- EntertainmentFocus
 20,  -- HardcoreIntensity
 30,  -- LuchaInfluence
 30,  -- StrongStyleInfluence
 65,  -- FanExpectationMatchQuality
 65,  -- FanExpectationStorylines
 60,  -- FanExpectationPromos
 65,  -- FanExpectationSpectacle
 1.0, -- MatchRatingMultiplier
 1.0, -- PromoRatingMultiplier
 '🌐',
 '#607D8B',
 1),

-- Style 7: Family-Friendly
('STYLE_FAMILY_FRIENDLY',
 'Family-Friendly',
 'Contenu tous publics, heroes vs villains clairs, messages positifs. Accessible aux enfants.',
 50,  -- WrestlingPurity
 70,  -- EntertainmentFocus
 0,   -- HardcoreIntensity (aucune violence)
 20,  -- LuchaInfluence
 5,   -- StrongStyleInfluence
 55,  -- FanExpectationMatchQuality
 75,  -- FanExpectationStorylines (histoires simples)
 65,  -- FanExpectationPromos
 80,  -- FanExpectationSpectacle
 0.9, -- MatchRatingMultiplier
 1.1, -- PromoRatingMultiplier
 '👨‍👩‍👧‍👦',
 '#4CAF50',
 1),

-- Style 8: Indie Darling
('STYLE_INDIE',
 'Indie Wrestling',
 'Compétition pure, innovation, passion. Petit budget mais grande créativité.',
 75,  -- WrestlingPurity
 50,  -- EntertainmentFocus
 15,  -- HardcoreIntensity
 40,  -- LuchaInfluence
 40,  -- StrongStyleInfluence
 80,  -- FanExpectationMatchQuality
 55,  -- FanExpectationStorylines
 50,  -- FanExpectationPromos
 60,  -- FanExpectationSpectacle
 1.2, -- MatchRatingMultiplier
 0.9, -- PromoRatingMultiplier
 '💎',
 '#00BCD4',
 1);

-- ============================================================================
-- MIGRATION DES DONNÉES EXISTANTES
-- Attribue un style par défaut aux compagnies existantes
-- ============================================================================

UPDATE Companies
SET CatchStyleId = 'STYLE_HYBRID'
WHERE CatchStyleId IS NULL;

-- ============================================================================
-- TABLE: CompanyStyleEvolution
-- Tracking de l'évolution du style de catch d'une compagnie
-- ============================================================================

CREATE TABLE IF NOT EXISTS CompanyStyleEvolution (
    EvolutionId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,

    -- Ancien et nouveau style
    OldStyleId TEXT NOT NULL,
    NewStyleId TEXT NOT NULL,

    -- Contexte du changement
    ChangeWeek INTEGER NOT NULL,
    Reason TEXT,  -- "Owner change", "Booker preference", "Fan feedback", "Strategic pivot"

    -- Impact
    FanReactionScore INTEGER CHECK(FanReactionScore >= -100 AND FanReactionScore <= 100),

    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (OldStyleId) REFERENCES CatchStyles(CatchStyleId),
    FOREIGN KEY (NewStyleId) REFERENCES CatchStyles(CatchStyleId)
);

CREATE INDEX IF NOT EXISTS idx_company_style_evolution_company ON CompanyStyleEvolution(CompanyId);
CREATE INDEX IF NOT EXISTS idx_company_style_evolution_week ON CompanyStyleEvolution(ChangeWeek DESC);

-- ============================================================================
-- VUES UTILITAIRES
-- ============================================================================

-- Vue combinant Company + CatchStyle
CREATE VIEW IF NOT EXISTS vw_CompanyWithStyle AS
SELECT
    c.CompanyId,
    c.Name AS CompanyName,
    c.FoundedYear,
    c.CompanySize,
    c.CurrentEra,
    c.Prestige,
    c.Treasury,
    c.AverageAudience,
    c.Reach,
    c.IsPlayerControlled,

    -- CatchStyle info
    cs.CatchStyleId,
    cs.Name AS StyleName,
    cs.Description AS StyleDescription,
    cs.WrestlingPurity,
    cs.EntertainmentFocus,
    cs.HardcoreIntensity,
    cs.LuchaInfluence,
    cs.StrongStyleInfluence,
    cs.IconName AS StyleIcon,
    cs.AccentColor AS StyleColor,

    -- Fan Expectations
    cs.FanExpectationMatchQuality,
    cs.FanExpectationStorylines,
    cs.FanExpectationPromos,
    cs.FanExpectationSpectacle

FROM Companies c
LEFT JOIN CatchStyles cs ON c.CatchStyleId = cs.CatchStyleId;

-- ============================================================================
-- FONCTIONS UTILITAIRES (via Application Code)
-- ============================================================================

-- Note: Ces fonctions seront implémentées en C# dans les repositories

-- GetCompatibleStyles(OwnerProductType) -> List<CatchStyle>
--   Retourne les styles compatibles avec les préférences de l'Owner
--   Ex: Owner.PreferredProductType = "Technical" -> Pure Wrestling, Strong Style, Hybrid

-- CalculateStyleMatchBonus(CatchStyleId, MatchAttributes) -> float
--   Calcule le bonus/malus de rating basé sur l'adéquation match/style
--   Ex: Pure Wrestling style + high workrate match = +15% rating

-- CalculateAudienceGrowth(CatchStyleId, CompanySize, Region) -> int
--   Prédit la croissance d'audience selon le style et le marché
--   Ex: Lucha style + Mexico region = +50% audience growth

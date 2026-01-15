-- ============================================================================
-- Migration 012: Trends & Compatibility System
-- Description: Ajoute les tables pour les tendances et matrices de compatibilité
--              - Tendances mondiales/régionales/locales
--              - Calcul de compatibilité avec l'ADN du roster
-- Created: January 2026 (Phase 6 - Structural Analysis & Niche Strategies)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: Trends
-- Description: Tendances affectant l'industrie du catch
-- ============================================================================
CREATE TABLE IF NOT EXISTS Trends (
    TrendId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Type TEXT NOT NULL CHECK(Type IN ('Global', 'Regional', 'Local')),
    Category TEXT NOT NULL CHECK(Category IN ('Style', 'Format', 'Audience')),
    Description TEXT NOT NULL,
    HardcoreAffinity REAL NOT NULL CHECK(HardcoreAffinity BETWEEN 0 AND 100),
    TechnicalAffinity REAL NOT NULL CHECK(TechnicalAffinity BETWEEN 0 AND 100),
    LuchaAffinity REAL NOT NULL CHECK(LuchaAffinity BETWEEN 0 AND 100),
    EntertainmentAffinity REAL NOT NULL CHECK(EntertainmentAffinity BETWEEN 0 AND 100),
    StrongStyleAffinity REAL NOT NULL CHECK(StrongStyleAffinity BETWEEN 0 AND 100),
    StartDate TEXT NOT NULL,
    EndDate TEXT NULL,
    Intensity INTEGER NOT NULL CHECK(Intensity BETWEEN 0 AND 100),
    DurationWeeks INTEGER NOT NULL CHECK(DurationWeeks > 0),
    MarketPenetration REAL NOT NULL CHECK(MarketPenetration BETWEEN 0 AND 100),
    AffectedRegions TEXT NOT NULL, -- JSON array
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_trends_active ON Trends(IsActive) WHERE IsActive = 1;
CREATE INDEX IF NOT EXISTS idx_trends_type ON Trends(Type);
CREATE INDEX IF NOT EXISTS idx_trends_category ON Trends(Category);
CREATE INDEX IF NOT EXISTS idx_trends_dates ON Trends(StartDate, EndDate);

-- ============================================================================
-- TABLE: CompatibilityMatrices
-- Description: Matrice de compatibilité entre tendances et ADN de roster
-- ============================================================================
CREATE TABLE IF NOT EXISTS CompatibilityMatrices (
    MatrixId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    TrendId TEXT NOT NULL,
    CompatibilityCoefficient REAL NOT NULL,
    Level TEXT NOT NULL CHECK(Level IN ('Alignment', 'Hybridation', 'Refusal')),
    QualityBonus REAL NOT NULL DEFAULT 0 CHECK(QualityBonus BETWEEN 0 AND 100),
    GrowthMultiplier REAL NOT NULL DEFAULT 1.0 CHECK(GrowthMultiplier BETWEEN 0.5 AND 2.0),
    NicheLoyaltyBonus REAL NOT NULL DEFAULT 0 CHECK(NicheLoyaltyBonus BETWEEN 0 AND 100),
    MarketingCostReduction REAL NOT NULL DEFAULT 0 CHECK(MarketingCostReduction BETWEEN 0 AND 100),
    CalculatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (TrendId) REFERENCES Trends(TrendId) ON DELETE CASCADE,
    UNIQUE(CompanyId, TrendId)
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_compatibility_company ON CompatibilityMatrices(CompanyId);
CREATE INDEX IF NOT EXISTS idx_compatibility_trend ON CompatibilityMatrices(TrendId);
CREATE INDEX IF NOT EXISTS idx_compatibility_level ON CompatibilityMatrices(Level);
CREATE INDEX IF NOT EXISTS idx_compatibility_coefficient ON CompatibilityMatrices(CompatibilityCoefficient);

-- ============================================================================
-- Migration 011: Roster Structural Analysis System
-- Description: Ajoute les tables pour l'analyse structurelle du roster
--              - Analyse agrégée des capacités d'une fédération
--              - ADN du roster (composition stylistique)
-- Created: January 2026 (Phase 6 - Structural Analysis & Niche Strategies)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: RosterDNAs
-- Description: ADN du roster - Composition stylistique d'une fédération
-- ============================================================================
CREATE TABLE IF NOT EXISTS RosterDNAs (
    DnaId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL UNIQUE,
    HardcorePercentage REAL NOT NULL CHECK(HardcorePercentage BETWEEN 0 AND 100),
    TechnicalPercentage REAL NOT NULL CHECK(TechnicalPercentage BETWEEN 0 AND 100),
    LuchaPercentage REAL NOT NULL CHECK(LuchaPercentage BETWEEN 0 AND 100),
    EntertainmentPercentage REAL NOT NULL CHECK(EntertainmentPercentage BETWEEN 0 AND 100),
    StrongStylePercentage REAL NOT NULL CHECK(StrongStylePercentage BETWEEN 0 AND 100),
    DominantStyle TEXT NOT NULL CHECK(DominantStyle IN ('Hardcore', 'Technical', 'Lucha', 'Entertainment', 'StrongStyle')),
    CoherenceScore REAL NOT NULL CHECK(CoherenceScore BETWEEN 0 AND 100),
    CalculatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_dna_company ON RosterDNAs(CompanyId);
CREATE INDEX IF NOT EXISTS idx_dna_dominant_style ON RosterDNAs(DominantStyle);

-- ============================================================================
-- TABLE: RosterStructuralAnalyses
-- Description: Analyse structurelle du roster - Vue agrégée des capacités
-- ============================================================================
CREATE TABLE IF NOT EXISTS RosterStructuralAnalyses (
    AnalysisId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    StarPowerMoyen REAL NOT NULL CHECK(StarPowerMoyen BETWEEN 0 AND 100),
    WorkrateMoyen REAL NOT NULL CHECK(WorkrateMoyen BETWEEN 0 AND 100),
    SpecialisationDominante TEXT NOT NULL CHECK(SpecialisationDominante IN ('Hardcore', 'Technical', 'Lucha', 'Entertainment', 'StrongStyle', 'Hybrid')),
    Profondeur INTEGER NOT NULL CHECK(Profondeur >= 0),
    IndiceDependance REAL NOT NULL CHECK(IndiceDependance BETWEEN 0 AND 100),
    Polyvalence REAL NOT NULL CHECK(Polyvalence BETWEEN 0 AND 100),
    CalculatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    WeekNumber INTEGER NOT NULL CHECK(WeekNumber BETWEEN 1 AND 52),
    Year INTEGER NOT NULL CHECK(Year BETWEEN 1950 AND 2100),
    
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_analyses_company ON RosterStructuralAnalyses(CompanyId);
CREATE INDEX IF NOT EXISTS idx_analyses_date ON RosterStructuralAnalyses(Year, WeekNumber);
CREATE INDEX IF NOT EXISTS idx_analyses_specialisation ON RosterStructuralAnalyses(SpecialisationDominante);

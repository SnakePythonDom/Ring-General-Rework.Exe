-- ============================================================================
-- Migration 013: Niche Federation System
-- Description: Ajoute les tables pour les fédérations de niche
--              - Profils de niche avec caractéristiques économiques
--              - Modèle de survie pour fédérations spécialisées
-- Created: January 2026 (Phase 6 - Structural Analysis & Niche Strategies)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: NicheFederationProfiles
-- Description: Profil de fédération de niche avec caractéristiques économiques
-- ============================================================================
CREATE TABLE IF NOT EXISTS NicheFederationProfiles (
    ProfileId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL UNIQUE,
    IsNicheFederation INTEGER NOT NULL DEFAULT 0 CHECK(IsNicheFederation IN (0, 1)),
    NicheType TEXT CHECK(NicheType IN ('Hardcore', 'Technical', 'Lucha', 'StrongStyle', 'Entertainment')),
    CaptiveAudiencePercentage REAL NOT NULL DEFAULT 0 CHECK(CaptiveAudiencePercentage BETWEEN 0 AND 100),
    TvDependencyReduction REAL NOT NULL DEFAULT 0 CHECK(TvDependencyReduction BETWEEN 0 AND 100),
    MerchandiseMultiplier REAL NOT NULL DEFAULT 1.0 CHECK(MerchandiseMultiplier BETWEEN 1.0 AND 2.0),
    TicketSalesStability REAL NOT NULL DEFAULT 0 CHECK(TicketSalesStability BETWEEN 0 AND 100),
    TalentSalaryReduction REAL NOT NULL DEFAULT 0 CHECK(TalentSalaryReduction BETWEEN 0 AND 100),
    TalentLoyaltyBonus REAL NOT NULL DEFAULT 0 CHECK(TalentLoyaltyBonus BETWEEN 0 AND 100),
    HasGrowthCeiling INTEGER NOT NULL DEFAULT 0 CHECK(HasGrowthCeiling IN (0, 1)),
    MaxSize TEXT CHECK(MaxSize IN ('Local', 'Regional', 'National', 'International', 'Worldwide')),
    EstablishedAt TEXT NOT NULL DEFAULT (datetime('now')),
    CeasedAt TEXT NULL,
    
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_niche_company ON NicheFederationProfiles(CompanyId);
CREATE INDEX IF NOT EXISTS idx_niche_type ON NicheFederationProfiles(NicheType);
CREATE INDEX IF NOT EXISTS idx_niche_active ON NicheFederationProfiles(IsNicheFederation) WHERE IsNicheFederation = 1;

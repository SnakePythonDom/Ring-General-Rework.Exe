-- ============================================================================
-- Migration 014: Child Companies Extended System
-- Description: Extension du système de filiales pour supporter les objectifs stratégiques
--              - Objectifs assignables (Entertainment, Niche, Independence, Development)
--              - Autonomie et laboratoires
-- Created: January 2026 (Phase 6 - Structural Analysis & Niche Strategies)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- Extension de CompanyHierarchies pour supporter les filiales
-- ============================================================================
-- Note: SQLite ne supporte pas ALTER TABLE ADD COLUMN avec CHECK constraints
-- On utilise une approche avec des valeurs par défaut et validation applicative

-- Ajout de colonnes à CompanyHierarchies (si elles n'existent pas déjà)
-- Note: SQLite ne supporte pas IF NOT EXISTS pour ALTER TABLE ADD COLUMN
-- Ces colonnes seront ajoutées uniquement si elles n'existent pas déjà

-- ============================================================================
-- TABLE: ChildCompaniesExtended
-- Description: Extension du modèle ChildCompany pour objectifs stratégiques
-- ============================================================================
CREATE TABLE IF NOT EXISTS ChildCompaniesExtended (
    ChildCompanyId TEXT PRIMARY KEY,
    ParentCompanyId TEXT NOT NULL,
    Objective TEXT NOT NULL CHECK(Objective IN ('Entertainment', 'Niche', 'Independence', 'Development')),
    HasFullAutonomy INTEGER NOT NULL DEFAULT 0 CHECK(HasFullAutonomy IN (0, 1)),
    AssignedBookerId TEXT NULL,
    IsLaboratory INTEGER NOT NULL DEFAULT 0 CHECK(IsLaboratory IN (0, 1)),
    TestStyle TEXT NULL,
    NicheType TEXT CHECK(NicheType IN ('Hardcore', 'Technical', 'Lucha', 'StrongStyle', 'Entertainment')),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    
    FOREIGN KEY (ParentCompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedBookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL,
    FOREIGN KEY (ChildCompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_child_parent ON ChildCompaniesExtended(ParentCompanyId);
CREATE INDEX IF NOT EXISTS idx_child_objective ON ChildCompaniesExtended(Objective);
CREATE INDEX IF NOT EXISTS idx_child_active ON ChildCompaniesExtended(IsActive) WHERE IsActive = 1;
CREATE INDEX IF NOT EXISTS idx_child_booker ON ChildCompaniesExtended(AssignedBookerId);

-- ============================================================================
-- Migration 015: DNA Transition System
-- Description: Ajoute les tables pour les transitions progressives d'ADN
--              - Gestion de l'inertie du roster
--              - Transitions sur 1-3 ans de temps de jeu
-- Created: January 2026 (Phase 6 - Structural Analysis & Niche Strategies)
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- TABLE: DNATransitions
-- Description: Transition progressive de l'ADN d'un roster
-- ============================================================================
CREATE TABLE IF NOT EXISTS DNATransitions (
    TransitionId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    StartDNAId TEXT NOT NULL,
    TargetDNAId TEXT NOT NULL,
    CurrentWeek INTEGER NOT NULL DEFAULT 0 CHECK(CurrentWeek >= 0),
    TotalWeeks INTEGER NOT NULL CHECK(TotalWeeks > 0),
    ProgressPercentage REAL NOT NULL DEFAULT 0 CHECK(ProgressPercentage BETWEEN 0 AND 100),
    InertiaScore REAL NOT NULL CHECK(InertiaScore BETWEEN 0 AND 100),
    StartedAt TEXT NOT NULL DEFAULT (datetime('now')),
    CompletedAt TEXT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (StartDNAId) REFERENCES RosterDNAs(DnaId) ON DELETE RESTRICT,
    FOREIGN KEY (TargetDNAId) REFERENCES RosterDNAs(DnaId) ON DELETE RESTRICT
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_transitions_company ON DNATransitions(CompanyId);
CREATE INDEX IF NOT EXISTS idx_transitions_active ON DNATransitions(IsActive) WHERE IsActive = 1;
CREATE INDEX IF NOT EXISTS idx_transitions_dna ON DNATransitions(StartDNAId, TargetDNAId);

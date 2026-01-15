-- ============================================================================
-- Migration 006: Nepotism & Relations Enrichment
-- Date: 2026-01-08
-- Description: Enriches WorkerRelations with nepotism tracking and impact logging
-- ============================================================================

-- Enrichir WorkerRelations avec népotisme
-- Note: WorkerRelations table doit déjà exister
ALTER TABLE WorkerRelations ADD COLUMN IF NOT EXISTS IsHidden BOOLEAN DEFAULT 0; -- Relation backstage vs publique (kayfabe)
ALTER TABLE WorkerRelations ADD COLUMN IF NOT EXISTS BiasStrength INTEGER DEFAULT 0 CHECK(BiasStrength BETWEEN 0 AND 100); -- Intensité du biais
ALTER TABLE WorkerRelations ADD COLUMN IF NOT EXISTS OriginEvent TEXT; -- Event qui a créé la relation
ALTER TABLE WorkerRelations ADD COLUMN IF NOT EXISTS LastImpact TEXT; -- Dernier impact observable

-- Table des impacts de népotisme (log des décisions biaisées)
CREATE TABLE IF NOT EXISTS NepotismImpacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RelationId INTEGER NOT NULL,
    ImpactType TEXT NOT NULL, -- 'Push', 'Protection', 'Sanction', 'Opportunity', 'Firing'
    TargetEntityId TEXT NOT NULL, -- Worker/Staff concerné
    DecisionMakerId TEXT NOT NULL, -- Qui a pris la décision biaisée (Owner, Booker, etc.)
    Severity INTEGER DEFAULT 1 CHECK(Severity BETWEEN 1 AND 5), -- 1=subtil, 5=flagrant
    IsVisible BOOLEAN DEFAULT 0, -- Si le joueur peut l'observer (severity >= 3)
    Description TEXT, -- Description courte de l'impact
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (RelationId) REFERENCES WorkerRelations(Id) ON DELETE CASCADE
);

-- Index pour performances
CREATE INDEX IF NOT EXISTS idx_nepotism_relation ON NepotismImpacts(RelationId);
CREATE INDEX IF NOT EXISTS idx_nepotism_target ON NepotismImpacts(TargetEntityId);
CREATE INDEX IF NOT EXISTS idx_nepotism_decision_maker ON NepotismImpacts(DecisionMakerId);
CREATE INDEX IF NOT EXISTS idx_nepotism_visible ON NepotismImpacts(IsVisible, Severity DESC);
CREATE INDEX IF NOT EXISTS idx_nepotism_date ON NepotismImpacts(CreatedAt DESC);

-- Table des décisions biaisées détectées (pour UI)
CREATE TABLE IF NOT EXISTS BiasedDecisions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DecisionType TEXT NOT NULL, -- 'Push', 'Sanction', 'Promotion', 'Firing'
    TargetEntityId TEXT NOT NULL,
    DecisionMakerId TEXT NOT NULL,
    IsBiased BOOLEAN DEFAULT 0,
    BiasReason TEXT, -- 'FamilyTie', 'Mentorship', 'Favoritism', 'Grudge'
    Justification TEXT, -- Metrics qui justifient ou non la décision
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Index
CREATE INDEX IF NOT EXISTS idx_biased_decisions_target ON BiasedDecisions(TargetEntityId);
CREATE INDEX IF NOT EXISTS idx_biased_decisions_biased ON BiasedDecisions(IsBiased);

-- ============================================================================
-- Rollback Script (à exécuter manuellement si besoin)
-- ============================================================================
-- DROP TABLE IF EXISTS BiasedDecisions;
-- DROP TABLE IF EXISTS NepotismImpacts;
-- DROP INDEX IF EXISTS idx_nepotism_relation;
-- DROP INDEX IF EXISTS idx_nepotism_target;
-- DROP INDEX IF EXISTS idx_nepotism_decision_maker;
-- DROP INDEX IF EXISTS idx_nepotism_visible;
-- DROP INDEX IF EXISTS idx_nepotism_date;
-- DROP INDEX IF EXISTS idx_biased_decisions_target;
-- DROP INDEX IF EXISTS idx_biased_decisions_biased;

-- -- Enlever colonnes de WorkerRelations (SQLite ne supporte pas DROP COLUMN facilement)
-- -- Créer nouvelle table sans les colonnes, copier données, drop ancienne, rename
-- -- À faire manuellement si rollback nécessaire

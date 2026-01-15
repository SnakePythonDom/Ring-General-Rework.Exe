-- ============================================================================
-- Migration 009: Crisis & Communication Systems
-- Description: Ajoute les tables pour le système de gestion de crises
--              et communications backstage
-- Created: January 2026 (Phase 5 - Week 1)
-- ============================================================================

-- ============================================================================
-- TABLE: Crises
-- Description: Gestion du pipeline de crises (5 stages)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Crises (
    CrisisId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    CrisisType TEXT NOT NULL CHECK(CrisisType IN (
        'MoraleCollapse', 'RumorEscalation', 'WorkerGrievance',
        'PublicScandal', 'FinancialCrisis', 'TalentExodus'
    )),
    Stage TEXT NOT NULL CHECK(Stage IN (
        'WeakSignals', 'Rumors', 'Declared', 'InResolution', 'Resolved', 'Ignored'
    )),
    Severity INTEGER NOT NULL CHECK(Severity BETWEEN 1 AND 5),
    Description TEXT NOT NULL,
    AffectedWorkers TEXT, -- JSON array of worker IDs
    EscalationScore INTEGER NOT NULL DEFAULT 0 CHECK(EscalationScore BETWEEN 0 AND 100),
    ResolutionAttempts INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    ResolvedAt TEXT,

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_crises_company ON Crises(CompanyId);
CREATE INDEX IF NOT EXISTS idx_crises_stage ON Crises(Stage);
CREATE INDEX IF NOT EXISTS idx_crises_type ON Crises(CrisisType);
CREATE INDEX IF NOT EXISTS idx_crises_active ON Crises(CompanyId, Stage) WHERE Stage NOT IN ('Resolved', 'Ignored');

-- ============================================================================
-- TABLE: Communications
-- Description: Historique des communications pour résoudre crises
-- ============================================================================
CREATE TABLE IF NOT EXISTS Communications (
    CommunicationId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    CrisisId INTEGER,
    CommunicationType TEXT NOT NULL CHECK(CommunicationType IN (
        'One-on-One', 'LockerRoomMeeting', 'PublicStatement', 'Mediation'
    )),
    InitiatorId TEXT NOT NULL, -- Worker ou Owner ID
    TargetId TEXT, -- Worker ID (NULL pour meetings publics)
    Message TEXT NOT NULL,
    Tone TEXT NOT NULL CHECK(Tone IN ('Diplomatic', 'Firm', 'Apologetic', 'Confrontational')),
    SuccessChance INTEGER NOT NULL CHECK(SuccessChance BETWEEN 0 AND 100),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (CrisisId) REFERENCES Crises(CrisisId) ON DELETE SET NULL
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_communications_company ON Communications(CompanyId);
CREATE INDEX IF NOT EXISTS idx_communications_crisis ON Communications(CrisisId);
CREATE INDEX IF NOT EXISTS idx_communications_type ON Communications(CommunicationType);
CREATE INDEX IF NOT EXISTS idx_communications_created ON Communications(CreatedAt DESC);

-- ============================================================================
-- TABLE: CommunicationOutcomes
-- Description: Résultats des communications
-- ============================================================================
CREATE TABLE IF NOT EXISTS CommunicationOutcomes (
    OutcomeId INTEGER PRIMARY KEY AUTOINCREMENT,
    CommunicationId INTEGER NOT NULL,
    WasSuccessful INTEGER NOT NULL CHECK(WasSuccessful IN (0, 1)),
    MoraleImpact INTEGER NOT NULL CHECK(MoraleImpact BETWEEN -50 AND 50),
    RelationshipImpact INTEGER NOT NULL CHECK(RelationshipImpact BETWEEN -30 AND 30),
    CrisisEscalationChange INTEGER NOT NULL CHECK(CrisisEscalationChange BETWEEN -50 AND 50),
    Feedback TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CommunicationId) REFERENCES Communications(CommunicationId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_outcomes_communication ON CommunicationOutcomes(CommunicationId);
CREATE INDEX IF NOT EXISTS idx_outcomes_success ON CommunicationOutcomes(WasSuccessful);

-- ============================================================================
-- DONNÉES DE TEST (optionnel - à commenter en production)
-- ============================================================================

-- Exemple Crisis pour la compagnie du joueur
-- INSERT INTO Crises (CompanyId, CrisisType, Stage, Severity, Description, EscalationScore)
-- VALUES ('COMP-PLAYER', 'MoraleCollapse', 'WeakSignals', 2, 'Rumeurs de mécontentement backstage', 15);

-- Exemple Communication
-- INSERT INTO Communications (CompanyId, CrisisId, CommunicationType, InitiatorId, TargetId, Message, Tone, SuccessChance)
-- VALUES ('COMP-PLAYER', 1, 'One-on-One', 'OWN-00001', 'WORK-00001', 'Discutons de tes préoccupations', 'Diplomatic', 70);

-- ============================================================================
-- ROLLBACK SCRIPT (à utiliser en cas de problème)
-- ============================================================================
-- DROP TABLE IF EXISTS CommunicationOutcomes;
-- DROP TABLE IF EXISTS Communications;
-- DROP TABLE IF EXISTS Crises;

-- ============================================================================
-- VALIDATION QUERIES
-- ============================================================================
-- SELECT * FROM Crises;
-- SELECT * FROM Communications;
-- SELECT * FROM CommunicationOutcomes;

-- Vérifier les contraintes
-- SELECT sql FROM sqlite_master WHERE type='table' AND name IN ('Crises', 'Communications', 'CommunicationOutcomes');

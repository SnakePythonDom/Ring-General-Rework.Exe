-- ============================================================================
-- Migration 010: Eras, Brands & Staff Management Systems
-- Description: Ajoute les tables pour:
--              - Système d'Eras de compagnie avec transitions progressives
--              - Hiérarchie et gestion Multi-Brand
--              - Staff global & spécialisé (Creative, Structural, Trainers)
--              - Compatibilités Staff-Booker
-- Created: January 2026 (Phase 5 - Advanced Company Management)
-- ============================================================================

-- ============================================================================
-- TABLE: Eras
-- Description: Ères de compagnie définissant le style de produit et structure
-- ============================================================================
CREATE TABLE IF NOT EXISTS Eras (
    EraId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Type TEXT NOT NULL CHECK(Type IN ('Technical', 'Entertainment', 'Hardcore', 'SportsEntertainment', 'LuchaLibre', 'StrongStyle', 'Developmental', 'Mainstream')),
    CustomName TEXT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NULL,
    Intensity INTEGER NOT NULL CHECK(Intensity BETWEEN 0 AND 100),
    PreferredMatchDuration INTEGER NOT NULL CHECK(PreferredMatchDuration BETWEEN 5 AND 60) DEFAULT 15,
    PreferredSegmentCount INTEGER NOT NULL CHECK(PreferredSegmentCount BETWEEN 1 AND 20) DEFAULT 8,
    MatchToSegmentRatio INTEGER NOT NULL CHECK(MatchToSegmentRatio BETWEEN 0 AND 100) DEFAULT 60,
    DominantMatchTypes TEXT NOT NULL DEFAULT 'Singles',
    AudienceExpectations TEXT NOT NULL DEFAULT 'Quality',
    IsCurrentEra INTEGER NOT NULL DEFAULT 0 CHECK(IsCurrentEra IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_eras_company ON Eras(CompanyId);
CREATE INDEX IF NOT EXISTS idx_eras_current ON Eras(CompanyId, IsCurrentEra) WHERE IsCurrentEra = 1;
CREATE INDEX IF NOT EXISTS idx_eras_type ON Eras(Type);

-- ============================================================================
-- TABLE: EraTransitions
-- Description: Transitions progressives entre ères avec impacts calculés
-- ============================================================================
CREATE TABLE IF NOT EXISTS EraTransitions (
    TransitionId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    FromEraId TEXT NOT NULL,
    ToEraId TEXT NOT NULL,
    StartDate TEXT NOT NULL,
    PlannedEndDate TEXT NOT NULL,
    ActualEndDate TEXT NULL,
    ProgressPercentage INTEGER NOT NULL CHECK(ProgressPercentage BETWEEN 0 AND 100) DEFAULT 0,
    Speed TEXT NOT NULL CHECK(Speed IN ('VerySlow', 'Slow', 'Moderate', 'Fast', 'Brutal')),
    MoraleImpact INTEGER NOT NULL CHECK(MoraleImpact BETWEEN -50 AND 50) DEFAULT 0,
    AudienceImpact INTEGER NOT NULL CHECK(AudienceImpact BETWEEN -50 AND 50) DEFAULT 0,
    ChangeResistance INTEGER NOT NULL CHECK(ChangeResistance BETWEEN 0 AND 100) DEFAULT 0,
    InitiatedByBookerId TEXT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    Notes TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (FromEraId) REFERENCES Eras(EraId) ON DELETE RESTRICT,
    FOREIGN KEY (ToEraId) REFERENCES Eras(EraId) ON DELETE RESTRICT,
    FOREIGN KEY (InitiatedByBookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_transitions_company ON EraTransitions(CompanyId);
CREATE INDEX IF NOT EXISTS idx_transitions_active ON EraTransitions(IsActive) WHERE IsActive = 1;
CREATE INDEX IF NOT EXISTS idx_transitions_booker ON EraTransitions(InitiatedByBookerId);

-- ============================================================================
-- TABLE: CompanyHierarchies
-- Description: Structure hiérarchique mono-brand ou multi-brand
-- ============================================================================
CREATE TABLE IF NOT EXISTS CompanyHierarchies (
    HierarchyId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL UNIQUE,
    Type TEXT NOT NULL CHECK(Type IN ('MonoBrand', 'MultiBrand')),
    OwnerId TEXT NOT NULL,
    HeadBookerId TEXT NULL,
    ActiveBrandCount INTEGER NOT NULL CHECK(ActiveBrandCount BETWEEN 1 AND 10) DEFAULT 1,
    AllowsBrandAutonomy INTEGER NOT NULL DEFAULT 1 CHECK(AllowsBrandAutonomy IN (0, 1)),
    CentralizationLevel INTEGER NOT NULL CHECK(CentralizationLevel BETWEEN 0 AND 100) DEFAULT 50,
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    LastModifiedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (OwnerId) REFERENCES Owners(OwnerId) ON DELETE CASCADE,
    FOREIGN KEY (HeadBookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL,

    -- Contrainte: MultiBrand DOIT avoir un HeadBooker
    CHECK (
        (Type = 'MonoBrand' AND HeadBookerId IS NULL) OR
        (Type = 'MultiBrand' AND HeadBookerId IS NOT NULL)
    )
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_hierarchies_company ON CompanyHierarchies(CompanyId);
CREATE INDEX IF NOT EXISTS idx_hierarchies_owner ON CompanyHierarchies(OwnerId);
CREATE INDEX IF NOT EXISTS idx_hierarchies_headbooker ON CompanyHierarchies(HeadBookerId);

-- ============================================================================
-- TABLE: Brands
-- Description: Brands/Shows dans une structure multi-brand
-- ============================================================================
CREATE TABLE IF NOT EXISTS Brands (
    BrandId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Objective TEXT NOT NULL CHECK(Objective IN ('Flagship', 'Development', 'Experimental', 'Mainstream', 'Prestige', 'Regional', 'Women', 'Touring')),
    BookerId TEXT NULL,
    CurrentEraId TEXT NULL,
    Prestige INTEGER NOT NULL CHECK(Prestige BETWEEN 0 AND 100) DEFAULT 50,
    BudgetPerShow REAL NOT NULL CHECK(BudgetPerShow >= 0) DEFAULT 0,
    AverageAudience INTEGER NOT NULL CHECK(AverageAudience >= 0) DEFAULT 0,
    Priority INTEGER NOT NULL CHECK(Priority BETWEEN 1 AND 10) DEFAULT 1,
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    AirDay TEXT NULL,
    ShowDuration INTEGER NOT NULL CHECK(ShowDuration BETWEEN 30 AND 240) DEFAULT 120,
    TargetRegion TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    DeactivatedAt TEXT NULL,

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL,
    FOREIGN KEY (CurrentEraId) REFERENCES Eras(EraId) ON DELETE SET NULL
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_brands_company ON Brands(CompanyId);
CREATE INDEX IF NOT EXISTS idx_brands_booker ON Brands(BookerId);
CREATE INDEX IF NOT EXISTS idx_brands_active ON Brands(IsActive) WHERE IsActive = 1;
CREATE INDEX IF NOT EXISTS idx_brands_priority ON Brands(Priority);
CREATE INDEX IF NOT EXISTS idx_brands_objective ON Brands(Objective);

-- ============================================================================
-- TABLE: StaffMembers
-- Description: Base commune pour tous les membres du staff
-- ============================================================================
CREATE TABLE IF NOT EXISTS StaffMembers (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    BrandId TEXT NULL,
    Name TEXT NOT NULL,
    Role TEXT NOT NULL CHECK(Role IN (
        'HeadBooker', 'Booker', 'RoadAgent', 'LeadWriter', 'CreativeWriter', 'Commentator', 'Interviewer',
        'MedicalDirector', 'MedicalStaff', 'PRManager', 'FinancialDirector', 'TalentScout', 'PerformancePsychologist', 'LegalCounsel',
        'HeadTrainer', 'WrestlingTrainer', 'PromoTrainer', 'StrengthCoach'
    )),
    Department TEXT NOT NULL CHECK(Department IN ('Creative', 'Structural', 'Training')),
    ExpertiseLevel TEXT NOT NULL CHECK(ExpertiseLevel IN ('Junior', 'MidLevel', 'Senior', 'Expert', 'Legend')),
    YearsOfExperience INTEGER NOT NULL CHECK(YearsOfExperience BETWEEN 0 AND 50) DEFAULT 0,
    SkillScore INTEGER NOT NULL CHECK(SkillScore BETWEEN 0 AND 100),
    PersonalityScore INTEGER NOT NULL CHECK(PersonalityScore BETWEEN 0 AND 100) DEFAULT 50,
    AnnualSalary REAL NOT NULL CHECK(AnnualSalary >= 0) DEFAULT 0,
    HireDate TEXT NOT NULL,
    ContractEndDate TEXT NULL,
    EmploymentStatus TEXT NOT NULL CHECK(EmploymentStatus IN ('Active', 'OnLeave', 'Suspended', 'Fired')) DEFAULT 'Active',
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    Notes TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (BrandId) REFERENCES Brands(BrandId) ON DELETE SET NULL,

    -- Contrainte: Trainers DOIVENT avoir un BrandId
    CHECK (
        (Department != 'Training') OR
        (Department = 'Training' AND BrandId IS NOT NULL)
    )
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_staff_company ON StaffMembers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_staff_brand ON StaffMembers(BrandId);
CREATE INDEX IF NOT EXISTS idx_staff_department ON StaffMembers(Department);
CREATE INDEX IF NOT EXISTS idx_staff_role ON StaffMembers(Role);
CREATE INDEX IF NOT EXISTS idx_staff_active ON StaffMembers(IsActive, EmploymentStatus);
CREATE INDEX IF NOT EXISTS idx_staff_contract_expiring ON StaffMembers(ContractEndDate) WHERE ContractEndDate IS NOT NULL;

-- ============================================================================
-- TABLE: CreativeStaff
-- Description: Staff créatif avec compatibilité Booker et biais
-- ============================================================================
CREATE TABLE IF NOT EXISTS CreativeStaff (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    BookerId TEXT NULL,
    CreativityScore INTEGER NOT NULL CHECK(CreativityScore BETWEEN 0 AND 100),
    ConsistencyScore INTEGER NOT NULL CHECK(ConsistencyScore BETWEEN 0 AND 100),
    PreferredStyle TEXT NOT NULL CHECK(PreferredStyle IN ('Technical', 'Brawler', 'HighFlyer', 'PowerHouse', 'Storyteller', 'Hardcore', 'Comedy', 'Realistic', 'Theatrical')),
    WorkerBias TEXT NOT NULL CHECK(WorkerBias IN ('None', 'BigMen', 'Cruiserweights', 'Veterans', 'Rookies', 'TechnicalWorkers', 'Entertainers', 'Men', 'Women', 'LocalTalent', 'InternationalStars')),
    LongTermStorylinePreference INTEGER NOT NULL CHECK(LongTermStorylinePreference BETWEEN 0 AND 100) DEFAULT 50,
    CreativeRiskTolerance INTEGER NOT NULL CHECK(CreativeRiskTolerance BETWEEN 0 AND 100) DEFAULT 50,
    BookerCompatibilityScore INTEGER NOT NULL CHECK(BookerCompatibilityScore BETWEEN 0 AND 100) DEFAULT 50,
    GimmickPreferences TEXT NOT NULL DEFAULT 'Balanced',
    CanRuinStorylines INTEGER NOT NULL DEFAULT 0 CHECK(CanRuinStorylines IN (0, 1)),
    ProposedStorylines TEXT NULL,
    ProposalAcceptanceRate INTEGER NOT NULL CHECK(ProposalAcceptanceRate BETWEEN 0 AND 100) DEFAULT 50,
    Specialty TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_creative_company ON CreativeStaff(CompanyId);
CREATE INDEX IF NOT EXISTS idx_creative_booker ON CreativeStaff(BookerId);
CREATE INDEX IF NOT EXISTS idx_creative_compatibility ON CreativeStaff(BookerCompatibilityScore);
CREATE INDEX IF NOT EXISTS idx_creative_dangerous ON CreativeStaff(CanRuinStorylines) WHERE CanRuinStorylines = 1;

-- ============================================================================
-- TABLE: StructuralStaff
-- Description: Staff structurel/transversal (Medical, PR, Finance, etc.)
-- ============================================================================
CREATE TABLE IF NOT EXISTS StructuralStaff (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    EfficiencyScore INTEGER NOT NULL CHECK(EfficiencyScore BETWEEN 0 AND 100),
    ProactivityScore INTEGER NOT NULL CHECK(ProactivityScore BETWEEN 0 AND 100) DEFAULT 50,
    ExpertiseDomain TEXT NOT NULL CHECK(ExpertiseDomain IN ('Medical', 'PR', 'Finance', 'Scouting', 'Psychology', 'Legal')),
    GlobalImpactAreas TEXT NOT NULL DEFAULT '',

    -- Medical bonuses
    InjuryRecoveryBonus INTEGER NOT NULL CHECK(InjuryRecoveryBonus BETWEEN 0 AND 50) DEFAULT 0,
    InjuryPreventionScore INTEGER NOT NULL CHECK(InjuryPreventionScore BETWEEN 0 AND 100) DEFAULT 0,

    -- PR bonuses
    CrisisManagementScore INTEGER NOT NULL CHECK(CrisisManagementScore BETWEEN 0 AND 100) DEFAULT 0,
    ReputationBonus INTEGER NOT NULL CHECK(ReputationBonus BETWEEN 0 AND 30) DEFAULT 0,

    -- Finance bonuses
    DealNegotiationScore INTEGER NOT NULL CHECK(DealNegotiationScore BETWEEN 0 AND 100) DEFAULT 0,
    CostReductionBonus INTEGER NOT NULL CHECK(CostReductionBonus BETWEEN 0 AND 25) DEFAULT 0,

    -- Scouting bonuses
    TalentDiscoveryScore INTEGER NOT NULL CHECK(TalentDiscoveryScore BETWEEN 0 AND 100) DEFAULT 0,
    IndustryNetworkScore INTEGER NOT NULL CHECK(IndustryNetworkScore BETWEEN 0 AND 100) DEFAULT 0,

    -- Psychology bonuses
    MoraleBonus INTEGER NOT NULL CHECK(MoraleBonus BETWEEN 0 AND 30) DEFAULT 0,
    ConflictResolutionScore INTEGER NOT NULL CHECK(ConflictResolutionScore BETWEEN 0 AND 100) DEFAULT 0,

    -- Legal bonuses
    LitigationManagementScore INTEGER NOT NULL CHECK(LitigationManagementScore BETWEEN 0 AND 100) DEFAULT 0,
    ContractNegotiationScore INTEGER NOT NULL CHECK(ContractNegotiationScore BETWEEN 0 AND 100) DEFAULT 0,

    SuccessfulInterventions INTEGER NOT NULL DEFAULT 0,
    TotalInterventions INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_structural_company ON StructuralStaff(CompanyId);
CREATE INDEX IF NOT EXISTS idx_structural_domain ON StructuralStaff(ExpertiseDomain);
CREATE INDEX IF NOT EXISTS idx_structural_efficiency ON StructuralStaff(EfficiencyScore DESC);

-- ============================================================================
-- TABLE: Trainers
-- Description: Entraîneurs liés aux infrastructures (Dojo/Performance Center)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Trainers (
    StaffId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    InfrastructureId TEXT NOT NULL,
    TrainingSpecialization TEXT NOT NULL CHECK(TrainingSpecialization IN ('InRing', 'Promo', 'Strength', 'AllRound')),
    TrainingEfficiency INTEGER NOT NULL CHECK(TrainingEfficiency BETWEEN 0 AND 100),
    ProgressionBonus INTEGER NOT NULL CHECK(ProgressionBonus BETWEEN 0 AND 50) DEFAULT 0,
    YouthDevelopmentScore INTEGER NOT NULL CHECK(YouthDevelopmentScore BETWEEN 0 AND 100),
    WrestlingExperience INTEGER NOT NULL CHECK(WrestlingExperience BETWEEN 0 AND 30) DEFAULT 0,
    WrestlingStyle TEXT NOT NULL DEFAULT 'AllRound',
    Reputation INTEGER NOT NULL CHECK(Reputation BETWEEN 0 AND 100) DEFAULT 0,
    CurrentStudents INTEGER NOT NULL CHECK(CurrentStudents BETWEEN 0 AND 50) DEFAULT 0,
    MaxStudentCapacity INTEGER NOT NULL CHECK(MaxStudentCapacity BETWEEN 1 AND 50) DEFAULT 10,
    GraduatedStudents INTEGER NOT NULL DEFAULT 0,
    FailedStudents INTEGER NOT NULL DEFAULT 0,
    TeachingSpecialty TEXT NULL,
    CanDevelopStars INTEGER NOT NULL DEFAULT 0 CHECK(CanDevelopStars IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (StaffId) REFERENCES StaffMembers(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (InfrastructureId) REFERENCES Brands(BrandId) ON DELETE CASCADE
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_trainers_company ON Trainers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_trainers_infrastructure ON Trainers(InfrastructureId);
CREATE INDEX IF NOT EXISTS idx_trainers_specialization ON Trainers(TrainingSpecialization);
CREATE INDEX IF NOT EXISTS idx_trainers_reputation ON Trainers(Reputation DESC);
CREATE INDEX IF NOT EXISTS idx_trainers_capacity ON Trainers(CurrentStudents, MaxStudentCapacity);

-- ============================================================================
-- TABLE: StaffCompatibilities
-- Description: Cache des compatibilités Staff-Booker calculées
-- ============================================================================
CREATE TABLE IF NOT EXISTS StaffCompatibilities (
    CompatibilityId TEXT PRIMARY KEY,
    StaffId TEXT NOT NULL,
    BookerId TEXT NOT NULL,
    OverallScore INTEGER NOT NULL CHECK(OverallScore BETWEEN 0 AND 100),
    CreativeVisionScore INTEGER NOT NULL CHECK(CreativeVisionScore BETWEEN 0 AND 100),
    BookingStyleScore INTEGER NOT NULL CHECK(BookingStyleScore BETWEEN 0 AND 100),
    BiasAlignmentScore INTEGER NOT NULL CHECK(BiasAlignmentScore BETWEEN 0 AND 100),
    RiskToleranceScore INTEGER NOT NULL CHECK(RiskToleranceScore BETWEEN 0 AND 100),
    PersonalChemistryScore INTEGER NOT NULL CHECK(PersonalChemistryScore BETWEEN 0 AND 100),
    PositiveFactors TEXT NOT NULL DEFAULT '',
    NegativeFactors TEXT NOT NULL DEFAULT '',
    SuccessfulCollaborations INTEGER NOT NULL DEFAULT 0,
    FailedCollaborations INTEGER NOT NULL DEFAULT 0,
    ConflictHistory INTEGER NOT NULL DEFAULT 0,
    LastCalculatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (StaffId) REFERENCES CreativeStaff(StaffId) ON DELETE CASCADE,
    FOREIGN KEY (BookerId) REFERENCES Bookers(BookerId) ON DELETE CASCADE,

    UNIQUE(StaffId, BookerId)
);

-- Index pour recherches rapides
CREATE INDEX IF NOT EXISTS idx_compatibility_staff ON StaffCompatibilities(StaffId);
CREATE INDEX IF NOT EXISTS idx_compatibility_booker ON StaffCompatibilities(BookerId);
CREATE INDEX IF NOT EXISTS idx_compatibility_score ON StaffCompatibilities(OverallScore);
CREATE INDEX IF NOT EXISTS idx_compatibility_dangerous ON StaffCompatibilities(OverallScore) WHERE OverallScore <= 30;
CREATE INDEX IF NOT EXISTS idx_compatibility_excellent ON StaffCompatibilities(OverallScore) WHERE OverallScore >= 80;
CREATE INDEX IF NOT EXISTS idx_compatibility_recalc ON StaffCompatibilities(LastCalculatedAt);

-- ============================================================================
-- VALIDATION CONSTRAINTS
-- ============================================================================

-- Trigger pour s'assurer qu'une compagnie n'a qu'une seule ère actuelle
CREATE TRIGGER IF NOT EXISTS trg_eras_one_current_per_company
BEFORE INSERT ON Eras
WHEN NEW.IsCurrentEra = 1
BEGIN
    UPDATE Eras SET IsCurrentEra = 0 WHERE CompanyId = NEW.CompanyId AND IsCurrentEra = 1;
END;

-- Trigger pour mettre à jour ActiveBrandCount dans CompanyHierarchies
CREATE TRIGGER IF NOT EXISTS trg_brands_update_hierarchy_count
AFTER INSERT ON Brands
WHEN NEW.IsActive = 1
BEGIN
    UPDATE CompanyHierarchies
    SET ActiveBrandCount = (
        SELECT COUNT(*) FROM Brands WHERE CompanyId = NEW.CompanyId AND IsActive = 1
    ),
    LastModifiedAt = datetime('now')
    WHERE CompanyId = NEW.CompanyId;
END;

-- ============================================================================
-- ROLLBACK SCRIPT (à utiliser en cas de problème)
-- ============================================================================
-- DROP TRIGGER IF EXISTS trg_brands_update_hierarchy_count;
-- DROP TRIGGER IF EXISTS trg_eras_one_current_per_company;
-- DROP TABLE IF EXISTS StaffCompatibilities;
-- DROP TABLE IF EXISTS Trainers;
-- DROP TABLE IF EXISTS StructuralStaff;
-- DROP TABLE IF EXISTS CreativeStaff;
-- DROP TABLE IF EXISTS StaffMembers;
-- DROP TABLE IF EXISTS Brands;
-- DROP TABLE IF EXISTS CompanyHierarchies;
-- DROP TABLE IF EXISTS EraTransitions;
-- DROP TABLE IF EXISTS Eras;

-- ============================================================================
-- VALIDATION QUERIES
-- ============================================================================
-- SELECT * FROM Eras;
-- SELECT * FROM EraTransitions;
-- SELECT * FROM CompanyHierarchies;
-- SELECT * FROM Brands;
-- SELECT * FROM StaffMembers;
-- SELECT * FROM CreativeStaff;
-- SELECT * FROM StructuralStaff;
-- SELECT * FROM Trainers;
-- SELECT * FROM StaffCompatibilities;

-- Vérifier les contraintes
-- SELECT sql FROM sqlite_master WHERE type='table' AND name IN (
--     'Eras', 'EraTransitions', 'CompanyHierarchies', 'Brands',
--     'StaffMembers', 'CreativeStaff', 'StructuralStaff', 'Trainers', 'StaffCompatibilities'
-- );

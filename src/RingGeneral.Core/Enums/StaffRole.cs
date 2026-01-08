namespace RingGeneral.Core.Enums;

/// <summary>
/// Rôles de staff dans une compagnie de catch.
/// Divisés en trois catégories: Créatif, Structurel, et Entraînement.
/// </summary>
public enum StaffRole
{
    // === STAFF CRÉATIF (Direct Impact on Booking) ===

    /// <summary>
    /// Head Booker - Responsable du booking global (multi-brand uniquement)
    /// Impacte: Vision créative globale, cohérence entre brands
    /// </summary>
    HeadBooker,

    /// <summary>
    /// Booker - Responsable créatif d'un show/brand
    /// Impacte: Booking quotidien, storylines, matchs
    /// </summary>
    Booker,

    /// <summary>
    /// Road Agent - Agent sur la route, aide aux matchs
    /// Impacte: Qualité des matchs, layout des segments
    /// </summary>
    RoadAgent,

    /// <summary>
    /// Lead Writer - Écrivain principal des storylines
    /// Impacte: Qualité narrative, cohérence des storylines
    /// </summary>
    LeadWriter,

    /// <summary>
    /// Creative Writer - Écrivain créatif
    /// Impacte: Promos, segments, angles
    /// </summary>
    CreativeWriter,

    /// <summary>
    /// Commentator - Commentateur
    /// Impacte: Perception des matchs, over avec le public
    /// </summary>
    Commentator,

    /// <summary>
    /// Interviewer - Intervieweur backstage
    /// Impacte: Segments backstage, kayfabe
    /// </summary>
    Interviewer,

    // === STAFF STRUCTUREL (Transversal, Long Terme) ===

    /// <summary>
    /// Medical Director - Directeur médical
    /// Impacte: Santé des workers, durée des blessures, prévention
    /// </summary>
    MedicalDirector,

    /// <summary>
    /// Medical Staff - Personnel médical
    /// Impacte: Soins quotidiens, récupération
    /// </summary>
    MedicalStaff,

    /// <summary>
    /// PR Manager - Responsable relations publiques
    /// Impacte: Image de la compagnie, gestion de crises
    /// </summary>
    PRManager,

    /// <summary>
    /// Financial Director - Directeur financier
    /// Impacte: Budget, gestion financière, deals TV
    /// </summary>
    FinancialDirector,

    /// <summary>
    /// Talent Scout - Recruteur de talents
    /// Impacte: Qualité des recrues, découverte de talents
    /// </summary>
    TalentScout,

    /// <summary>
    /// Performance Psychologist - Psychologue du sport
    /// Impacte: Mental des workers, gestion du stress, morale
    /// </summary>
    PerformancePsychologist,

    /// <summary>
    /// Legal Counsel - Conseiller juridique
    /// Impacte: Contrats, litiges, conformité
    /// </summary>
    LegalCounsel,

    // === TRAINERS (Infrastructures uniquement) ===

    /// <summary>
    /// Head Trainer - Entraîneur principal d'un dojo/PC
    /// Impacte: Progression des jeunes, qualité de formation
    /// </summary>
    HeadTrainer,

    /// <summary>
    /// Wrestling Trainer - Entraîneur de catch
    /// Impacte: Progression in-ring des élèves
    /// </summary>
    WrestlingTrainer,

    /// <summary>
    /// Promo Trainer - Entraîneur de promos
    /// Impacte: Progression entertainment/charisma
    /// </summary>
    PromoTrainer,

    /// <summary>
    /// Strength Coach - Coach de force et conditionnement
    /// Impacte: Physique, endurance, prévention blessures
    /// </summary>
    StrengthCoach
}

/// <summary>
/// Départements de staff
/// </summary>
public enum StaffDepartment
{
    /// <summary>
    /// Département créatif - Direct impact sur le booking
    /// </summary>
    Creative,

    /// <summary>
    /// Département structurel - Impact transversal long terme
    /// </summary>
    Structural,

    /// <summary>
    /// Département entraînement - Lié aux infrastructures uniquement
    /// </summary>
    Training
}

/// <summary>
/// Niveau d'expertise du staff
/// </summary>
public enum StaffExpertiseLevel
{
    /// <summary>
    /// Junior (0-2 ans d'expérience) - Apprend le métier
    /// </summary>
    Junior,

    /// <summary>
    /// Mid-Level (3-5 ans) - Compétent
    /// </summary>
    MidLevel,

    /// <summary>
    /// Senior (6-10 ans) - Expérimenté
    /// </summary>
    Senior,

    /// <summary>
    /// Expert (10-15 ans) - Référence
    /// </summary>
    Expert,

    /// <summary>
    /// Legend (15+ ans) - Légende de l'industrie
    /// </summary>
    Legend
}

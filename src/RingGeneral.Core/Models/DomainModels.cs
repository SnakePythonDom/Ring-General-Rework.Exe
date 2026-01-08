namespace RingGeneral.Core.Models;

public sealed record ShowDefinition(
    string ShowId,
    string Nom,
    int Semaine,
    string Region,
    int DureeMinutes,
    string CompagnieId,
    string? DealTvId,
    string Lieu,
    string Diffusion);

public sealed record CompanyState(
    string CompagnieId,
    string Nom,
    string Region,
    int Prestige,
    double Tresorerie,
    int AudienceMoyenne,
    int Reach,
    // Company Identity & Governance (Migration 005)
    int FoundedYear = 2024,
    string CompanySize = "Local",
    string CurrentEra = "Foundation Era",
    string? CatchStyleId = null,
    bool IsPlayerControlled = false,
    double MonthlyBurnRate = 0.0,
    // Governance (Migration 004)
    string? OwnerId = null,
    string? BookerId = null);

public sealed record WorkerSnapshot(
    string WorkerId,
    string NomComplet,
    int InRing,
    int Entertainment,
    int Story,
    int Popularite,
    int Fatigue,
    string Blessure,
    int Momentum,
    string RoleTv,
    int Morale);

public sealed record WorkerHealth(
    int Fatigue,
    string Blessure);

public sealed record TitleInfo(
    string TitreId,
    string Nom,
    int Prestige,
    string? DetenteurId);

public sealed record StorylineParticipant(
    string WorkerId,
    string Role);

public sealed record StorylineInfo(
    string StorylineId,
    string Nom,
    StorylinePhase Phase,
    int Heat,
    StorylineStatus Status,
    string? Resume,
    IReadOnlyList<StorylineParticipant> Participants);

public sealed record SegmentDefinition(
    string SegmentId,
    string TypeSegment,
    IReadOnlyList<string> Participants,
    int DureeMinutes,
    bool EstMainEvent,
    string? StorylineId,
    string? TitreId,
    int Intensite,
    string? VainqueurId,
    string? PerdantId,
    IReadOnlyDictionary<string, string>? Settings = null);

public sealed record ShowContext(
    ShowDefinition Show,
    CompanyState Compagnie,
    IReadOnlyList<WorkerSnapshot> Workers,
    IReadOnlyList<TitleInfo> Titres,
    IReadOnlyList<StorylineInfo> Storylines,
    IReadOnlyList<SegmentDefinition> Segments,
    IReadOnlyDictionary<string, int> Chimies,
    TvDeal? DealTv = null);

public sealed record FinanceTransaction(
    string Type,
    double Montant,
    string Libelle);

public sealed record ShowHistoryEntry(
    string ShowId,
    int Week,
    int Note,
    int Audience,
    string Summary,
    string? CreatedAt);

public sealed record InboxItem(
    string Type,
    string Titre,
    string Contenu,
    int Semaine);

public sealed record ScoutTargetProfile(
    string WorkerId,
    string NomComplet,
    string Region,
    int InRing,
    int Entertainment,
    int Story,
    int Popularite,
    int Momentum);

public sealed record GameStateDelta(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, string> Blessures,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteWorkersDelta,
    IReadOnlyDictionary<string, int> PopulariteCompagnieDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<FinanceTransaction> Finances);

// ============================================================================
// CATCH STYLE SYSTEM (Migration 006)
// ============================================================================

public sealed record CatchStyle(
    string CatchStyleId,
    string Name,
    string? Description,
    // Style Characteristics (0-100)
    int WrestlingPurity,
    int EntertainmentFocus,
    int HardcoreIntensity,
    int LuchaInfluence,
    int StrongStyleInfluence,
    // Fan Expectations (0-100)
    int FanExpectationMatchQuality,
    int FanExpectationStorylines,
    int FanExpectationPromos,
    int FanExpectationSpectacle,
    // Rating Multipliers
    double MatchRatingMultiplier,
    double PromoRatingMultiplier,
    // UI
    string? IconName = null,
    string? AccentColor = null,
    bool IsActive = true);

// ============================================================================
// COMPANY GOVERNANCE (Migration 005)
// ============================================================================

public sealed record CompanyEra(
    string EraId,
    string CompanyId,
    string EraName,
    int StartWeek,
    int? EndWeek,
    string? Description = null,
    double AverageRating = 0.0,
    int PeakAudience = 0);

public sealed record CompanyMilestone(
    string MilestoneId,
    string CompanyId,
    string MilestoneType,
    string Title,
    string? Description,
    int Week,
    string EventDate);

// ============================================================================
// OWNER & BOOKER (Migration 004)
// ============================================================================

public sealed record OwnerSnapshot(
    string OwnerId,
    string CompanyId,
    string Name,
    string VisionType,
    int RiskTolerance,
    string PreferredProductType,
    string ShowFrequencyPreference,
    int TalentDevelopmentFocus,
    int FinancialPriority,
    int FanSatisfactionPriority);

public sealed record BookerSnapshot(
    string BookerId,
    string CompanyId,
    string Name,
    int CreativityScore,
    int LogicScore,
    int BiasResistance,
    string PreferredStyle,
    bool LikesUnderdog,
    bool LikesVeteran,
    bool LikesFastRise,
    bool LikesSlowBurn,
    bool IsAutoBookingEnabled,
    string EmploymentStatus,
    string HireDate);

public sealed record BookerMemoryEntry(
    string MemoryId,
    string BookerId,
    string? WorkerId,
    string EventType,
    int ImpactScore,
    int RecallStrength,
    string? Description,
    string EventDate);

// ============================================================================
// COMPANY HUB - COMBINED VIEW
// ============================================================================

public sealed record CompanyGovernanceView(
    CompanyState Company,
    OwnerSnapshot? Owner,
    BookerSnapshot? ActiveBooker,
    CatchStyle? Style,
    CompanyEra? CurrentEra);

public sealed record CompanyMainStar(
    string WorkerId,
    string NomComplet,
    int Popularite,
    int Momentum,
    string? TitleHeld,
    string? GimmickName);

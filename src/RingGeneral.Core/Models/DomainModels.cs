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
    int Reach);

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
    string Phase,
    int Heat,
    StorylinePhase Phase,
    StorylineStatus Status,
    IReadOnlyList<string> Participants);

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

public sealed record ScoutReport(
    string ReportId,
    string WorkerId,
    string WorkerNom,
    string Region,
    int Semaine,
    int Note,
    string Forces,
    string Faiblesses,
    string Recommendation,
    string Resume);

public sealed record ScoutShortlistEntry(
    string WorkerId,
    string WorkerNom,
    int Note,
    string Notes,
    int SemaineAjout,
    string? RapportId);

public sealed record ScoutMission(
    string MissionId,
    string Region,
    int SemaineDebut,
    int DureeSemaines,
    int Progression,
    string Statut,
    string? RapportId);

public sealed record GameStateDelta(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, string> Blessures,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteWorkersDelta,
    IReadOnlyDictionary<string, int> PopulariteCompagnieDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<FinanceTransaction> Finances);

public sealed record StorylineEvent(
    long StorylineEventId,
    string StorylineId,
    string TypeEvenement,
    int? Semaine,
    string? Details);

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
    string RoleTv);

public sealed record WorkerHealth(
    int Fatigue,
    string Blessure);

public sealed record TitleInfo(
    string TitreId,
    string Nom,
    int Prestige,
    string? DetenteurId);

public sealed record StorylineInfo(
    string StorylineId,
    string Nom,
    int Heat,
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
    string? PerdantId);

public sealed record ShowContext(
    ShowDefinition Show,
    CompanyState Compagnie,
    IReadOnlyList<WorkerSnapshot> Workers,
    IReadOnlyList<TitleInfo> Titres,
    IReadOnlyList<StorylineInfo> Storylines,
    IReadOnlyList<SegmentDefinition> Segments,
    IReadOnlyDictionary<string, int> Chimies);

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

public sealed record GameStateDelta(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, string> Blessures,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteWorkersDelta,
    IReadOnlyDictionary<string, int> PopulariteCompagnieDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<FinanceTransaction> Finances);

namespace RingGeneral.Core.Models;

public sealed record SegmentBreakdownItem(
    string Libelle,
    int Impact);

public sealed record SegmentImpact(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<string> Blessures);

public sealed record SegmentReport(
    string SegmentId,
    string TypeSegment,
    int Note,
    int InRing,
    int Entertainment,
    int Story,
    int CrowdHeatAvant,
    int CrowdHeatApres,
    int PacingPenalty,
    int ChimieBonus,
    IReadOnlyList<string> Evenements,
    IReadOnlyList<SegmentBreakdownItem> Facteurs,
    SegmentImpact Impact);

public sealed record ShowReport(
    string ShowId,
    int NoteGlobale,
    int Audience,
    AudienceDetails AudienceDetails,
    double Billetterie,
    double Merch,
    double Tv,
    int PopulariteCompagnieDelta,
    IReadOnlyList<SegmentReport> Segments,
    IReadOnlyList<string> PointsCles);

public sealed record ShowSimulationResult(
    ShowReport RapportShow,
    GameStateDelta Delta);

namespace RingGeneral.Core.Models;

public sealed record SimulationContext(
    int Seed,
    string CompagnieJoueurId,
    IReadOnlyList<string> CompagniesMajeures,
    IReadOnlyList<string> CompagniesSecondaires);

public sealed record SimulationOptions(
    int NiveauDetailCompagnieJoueur,
    int NiveauDetailCompagniesMajeures,
    int NiveauDetailCompagniesSecondaires);

public sealed record SimulationResult(
    bool Reussite,
    string RapportHebdo,
    IReadOnlyList<SegmentResult> Segments);

public sealed record SegmentSimulationContext(
    string SegmentId,
    string TypeSegment,
    IReadOnlyList<string> Participants,
    int DureeMinutes,
    bool EstMainEvent = false,
    string? StorylineId = null,
    string? TitreId = null,
    int Intensite = 50,
    string? VainqueurId = null,
    string? PerdantId = null,
    IReadOnlyList<WorkerSnapshot>? ParticipantsDetails = null);

public sealed record SegmentRating(
    int Note,
    string Explication);

public sealed record SegmentResult(
    string SegmentId,
    int Note,
    string Resume,
    SegmentReport? Rapport = null);

public sealed record BookingPlan(
    string ShowId,
    IReadOnlyList<SegmentSimulationContext> Segments,
    int? DureeShowMinutes = null,
    IReadOnlyDictionary<string, WorkerHealth>? EtatWorkers = null);

public sealed record ValidationResult(
    bool EstValide,
    IReadOnlyList<string> Erreurs,
    IReadOnlyList<string> Avertissements);

public sealed record ImpactContext(
    string ShowId,
    IReadOnlyList<SegmentResult> Segments,
    IReadOnlyList<string> Storylines,
    ShowReport? RapportShow = null,
    GameStateDelta? Delta = null);

public sealed record ImpactReport(
    IReadOnlyList<string> Changements,
    string Resume);

namespace RingGeneral.Core.Models;

/// <summary>
/// État de travail quotidien d'un worker
/// </summary>
public enum DailyActivityState
{
    /// <summary>
    /// Repos : Récupération maximale
    /// </summary>
    Repos,

    /// <summary>
    /// Entraînement : Gain léger de stats, fatigue légère
    /// </summary>
    Entrainement,

    /// <summary>
    /// Voyage : Fatigue moyenne (si le show de demain est loin)
    /// </summary>
    Voyage,

    /// <summary>
    /// Show : Fatigue forte, risque blessure, déclenche paiement frais d'apparition
    /// </summary>
    Show
}

/// <summary>
/// Contexte pour le tick quotidien
/// </summary>
public sealed record DailyTimeContext(
    string CompanyId,
    int CurrentDay,
    DateTime CurrentDate,
    IReadOnlyList<WorkerDailyState> WorkersStates);

/// <summary>
/// État quotidien d'un worker
/// </summary>
public sealed record WorkerDailyState(
    string WorkerId,
    DailyActivityState Activity,
    int FatigueDelta,
    bool IsInjured,
    DateTime? InjuryRecoveryDate);

/// <summary>
/// Résultat du passage d'un jour
/// </summary>
public sealed record DailyTickResult(
    int Day,
    DateTime CurrentDate,
    IReadOnlyList<string> Events);

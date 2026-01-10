namespace RingGeneral.Core.Models;

/// <summary>
/// Résultat de la détection d'un show
/// </summary>
public sealed record ShowDayDetectionResult(
    bool ShowDetecte,
    ShowSchedule? Show,
    string Message);

/// <summary>
/// Résultat de la finalisation d'un show
/// </summary>
public sealed record ShowDayFinalizationResult(
    bool Succes,
    IReadOnlyList<string> Changements,
    IReadOnlyList<TitleChangeInfo> TitresChanges,
    GameStateDelta? Delta);

/// <summary>
/// Information sur un changement de titre
/// </summary>
public sealed record TitleChangeInfo(
    string TitreId,
    string TitreNom,
    string AncienChampion,
    string NouveauChampion,
    int PrestigeDelta);

/// <summary>
/// Résultat du flux complet Show Day
/// </summary>
public sealed record ShowDayFluxCompletResult(
    bool Succes,
    IReadOnlyList<string> Erreurs,
    IReadOnlyList<string> Changements,
    ShowReport? Rapport);
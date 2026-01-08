using System.Collections.Generic;

namespace RingGeneral.Core.Models.Booker;

/// <summary>
/// Contraintes imposées par l'Owner pour l'auto-booking.
/// Permet au joueur de garder le contrôle sur certains aspects du booking.
/// </summary>
public sealed record AutoBookingConstraints
{
    /// <summary>
    /// Budget maximum pour le show (coûts des segments, stipulations, etc.)
    /// </summary>
    public double? MaxBudget { get; init; }

    /// <summary>
    /// Liste des WorkerId interdits d'utilisation (suspendus, en désaccord, etc.)
    /// </summary>
    public List<string> BannedWorkers { get; init; } = new();

    /// <summary>
    /// Liste des WorkerId obligatoires à utiliser
    /// </summary>
    public List<string> RequiredWorkers { get; init; } = new();

    /// <summary>
    /// Interdire l'utilisation de workers blessés
    /// </summary>
    public bool ForbidInjuredWorkers { get; init; } = true;

    /// <summary>
    /// Niveau de fatigue maximum accepté pour utiliser un worker (0-100)
    /// </summary>
    public int MaxFatigueLevel { get; init; } = 80;

    /// <summary>
    /// Nombre minimum de segments à générer
    /// </summary>
    public int MinSegments { get; init; } = 4;

    /// <summary>
    /// Nombre maximum de segments à générer
    /// </summary>
    public int MaxSegments { get; init; } = 8;

    /// <summary>
    /// Interdire l'utilisation d'un worker dans plusieurs segments du même show
    /// </summary>
    public bool ForbidMultipleAppearances { get; init; } = true;

    /// <summary>
    /// Priorité aux storylines en cours (utiliser les workers impliqués)
    /// </summary>
    public bool PrioritizeActiveStorylines { get; init; } = true;

    /// <summary>
    /// Utiliser automatiquement les titres disponibles
    /// </summary>
    public bool UseTitles { get; init; } = true;

    /// <summary>
    /// Forcer la création d'un main event
    /// </summary>
    public bool RequireMainEvent { get; init; } = true;

    /// <summary>
    /// Durée cible du show en minutes (le booker essaiera de respecter cette durée)
    /// </summary>
    public int? TargetDuration { get; init; }
}

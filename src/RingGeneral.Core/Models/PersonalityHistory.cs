using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Historique des changements de personnalité d'une entité.
/// Permet de tracker l'évolution dans le temps.
/// </summary>
public class PersonalityHistory
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string OldLabel { get; set; } = string.Empty;
    public string NewLabel { get; set; } = string.Empty;
    public string? ChangeReason { get; set; } // Success, Failure, Trauma, Growth
    public DateTime ChangedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Description formatée du changement
    /// </summary>
    public string ChangeDescription => $"{OldLabel} → {NewLabel}";

    /// <summary>
    /// Indique si le changement est récent (< 90 jours)
    /// </summary>
    public bool IsRecent => (DateTime.Now - ChangedAt).TotalDays < 90;
}

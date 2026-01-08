using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Personnalité visible (FM-style label).
/// Calculée à partir des MentalAttributes cachés.
/// Incomplet et interprétatif par design.
/// </summary>
public class Personality
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;

    // Label visible (FM-like)
    public string PersonalityLabel { get; set; } = "Balanced";

    // Traits secondaires (max 2)
    public string? SecondaryTrait1 { get; set; }
    public string? SecondaryTrait2 { get; set; }

    // Evolution tracking
    public string? PreviousLabel { get; set; }
    public DateTime? LabelChangedAt { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // Helper properties
    public bool HasSecondaryTraits => !string.IsNullOrEmpty(SecondaryTrait1);

    public bool RecentlyChanged => LabelChangedAt.HasValue &&
        (DateTime.Now - LabelChangedAt.Value).TotalDays < 30;

    /// <summary>
    /// Retourne la description complète (label + traits)
    /// </summary>
    public string FullDescription
    {
        get
        {
            var desc = PersonalityLabel;
            if (HasSecondaryTraits)
            {
                desc += $" ({SecondaryTrait1}";
                if (!string.IsNullOrEmpty(SecondaryTrait2))
                    desc += $", {SecondaryTrait2}";
                desc += ")";
            }
            return desc;
        }
    }
}

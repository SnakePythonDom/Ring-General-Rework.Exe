using System;

namespace RingGeneral.Core.Models.Relations;

/// <summary>
/// Enregistre une décision et si elle est biaisée ou non.
/// Utilisé pour tracking et détection de patterns de népotisme.
/// </summary>
public class BiasedDecision
{
    public int Id { get; set; }

    /// <summary>
    /// Type de décision: Push, Sanction, Promotion, Firing, Opportunity
    /// </summary>
    public string DecisionType { get; set; } = string.Empty;

    /// <summary>
    /// ID de l'entité ciblée par la décision
    /// </summary>
    public string TargetEntityId { get; set; } = string.Empty;

    /// <summary>
    /// ID du décideur
    /// </summary>
    public string DecisionMakerId { get; set; } = string.Empty;

    /// <summary>
    /// La décision est-elle biaisée?
    /// </summary>
    public bool IsBiased { get; set; } = false;

    /// <summary>
    /// Raison du biais (si IsBiased = true):
    /// FamilyTie, Mentorship, Favoritism, Grudge
    /// </summary>
    public string? BiasReason { get; set; }

    /// <summary>
    /// Justification métrique de la décision
    /// Ex: "Popularity: 45, Skill: 60, Performance: 70 → Push justifié"
    /// Ou: "Popularity: 30, Skill: 40 → Push NON justifié (népotisme)"
    /// </summary>
    public string? Justification { get; set; }

    /// <summary>
    /// Date de la décision
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ====================================================================
    // HELPER PROPERTIES
    // ====================================================================

    /// <summary>
    /// Est-ce une décision récente? (< 14 jours)
    /// </summary>
    public bool IsRecent => (DateTime.Now - CreatedAt).TotalDays < 14;
}

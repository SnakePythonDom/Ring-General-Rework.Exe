using System;

namespace RingGeneral.Core.Models.Relations;

/// <summary>
/// Enregistre un impact de népotisme - une décision biaisée par une relation.
/// Utilisé pour tracking interne et génération de signaux/rumeurs.
/// </summary>
public class NepotismImpact
{
    public int Id { get; set; }

    /// <summary>
    /// ID de la relation qui a causé le biais
    /// </summary>
    public int RelationId { get; set; }

    /// <summary>
    /// Type d'impact:
    /// - Push: Worker pushed au-delà de son mérite
    /// - Protection: Worker protégé d'une sanction/firing
    /// - Sanction: Worker sanctionné malgré relation forte (rare)
    /// - Opportunity: Worker reçoit une opportunité injustifiée
    /// - Firing: Worker viré malgré relation (override)
    /// </summary>
    public string ImpactType { get; set; } = string.Empty;

    /// <summary>
    /// ID de l'entité ciblée (Worker/Staff affecté par la décision)
    /// </summary>
    public string TargetEntityId { get; set; } = string.Empty;

    /// <summary>
    /// ID du décideur (Owner, Booker, etc.)
    /// </summary>
    public string DecisionMakerId { get; set; } = string.Empty;

    /// <summary>
    /// Sévérité de l'impact (1-5)
    /// 1 = subtil, difficilement détectable
    /// 2 = discret, visible aux observateurs attentifs
    /// 3 = modéré, commence à être évident
    /// 4 = important, clairement biaisé
    /// 5 = flagrant, scandaleux
    /// </summary>
    public int Severity { get; set; } = 1;

    /// <summary>
    /// Est-ce que le joueur peut observer cet impact?
    /// Généralement true si Severity >= 3
    /// </summary>
    public bool IsVisible { get; set; } = false;

    /// <summary>
    /// Description courte de l'impact
    /// Ex: "Pushed to main event despite low popularity"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date de création de l'impact
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public WorkerRelation? Relation { get; set; }

    // ====================================================================
    // HELPER PROPERTIES
    // ====================================================================

    /// <summary>
    /// Est-ce un impact récent? (< 30 jours)
    /// </summary>
    public bool IsRecent => (DateTime.Now - CreatedAt).TotalDays < 30;

    /// <summary>
    /// Est-ce un impact flagrant? (Severity >= 4)
    /// </summary>
    public bool IsFlagrant => Severity >= 4;

    /// <summary>
    /// Label de sévérité
    /// </summary>
    public string SeverityLabel => Severity switch
    {
        1 => "Subtil",
        2 => "Discret",
        3 => "Modéré",
        4 => "Important",
        5 => "Flagrant",
        _ => "Inconnu"
    };

    /// <summary>
    /// Emoji de sévérité
    /// </summary>
    public string SeverityEmoji => Severity switch
    {
        >= 4 => "🚨", // Flagrant
        >= 3 => "⚠️", // Modéré
        >= 2 => "ℹ️", // Discret
        _ => "•" // Subtil
    };
}

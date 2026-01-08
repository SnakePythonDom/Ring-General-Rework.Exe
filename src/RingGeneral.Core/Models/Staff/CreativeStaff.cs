using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Staff;

/// <summary>
/// Représente un membre du staff créatif (Booker, Writer, Road Agent, Commentator).
/// Impact direct sur le booking et les storylines.
/// Possède des préférences et biais qui influencent les décisions créatives.
/// </summary>
public sealed record CreativeStaff
{
    /// <summary>
    /// Identifiant du membre du staff (référence à StaffMember)
    /// </summary>
    [Required]
    public required string StaffId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Identifiant du booker avec qui il travaille
    /// Important pour calculer la compatibilité
    /// </summary>
    public string? BookerId { get; init; }

    /// <summary>
    /// Score de créativité (0-100)
    /// Influence la qualité et l'originalité des angles/storylines
    /// </summary>
    [Range(0, 100)]
    public required int CreativityScore { get; init; }

    /// <summary>
    /// Score de cohérence narrative (0-100)
    /// Capacité à maintenir la cohérence long terme
    /// </summary>
    [Range(0, 100)]
    public required int ConsistencyScore { get; init; }

    /// <summary>
    /// Style de produit préféré
    /// </summary>
    [Required]
    public required ProductStyle PreferredStyle { get; init; }

    /// <summary>
    /// Biais de type de worker
    /// </summary>
    [Required]
    public required WorkerTypeBias WorkerBias { get; init; }

    /// <summary>
    /// Préférence pour les storylines longues (0-100)
    /// - 0: Préfère one-shots et angles courts
    /// - 50: Équilibré
    /// - 100: Préfère slow burns et storylines de 6+ mois
    /// </summary>
    [Range(0, 100)]
    public int LongTermStorylinePreference { get; init; } = 50;

    /// <summary>
    /// Tolérance au risque créatif (0-100)
    /// - 0: Conservateur, formules éprouvées
    /// - 50: Équilibré
    /// - 100: Aime les risques, innovations audacieuses
    /// </summary>
    [Range(0, 100)]
    public int CreativeRiskTolerance { get; init; } = 50;

    /// <summary>
    /// Score de compatibilité avec le Booker actuel (0-100)
    /// Calculé automatiquement, impacte l'efficacité
    /// </summary>
    [Range(0, 100)]
    public int BookerCompatibilityScore { get; init; }

    /// <summary>
    /// Préférence de gimmicks/personnages
    /// Ex: "Realistic,Serious,Antihero" ou "Comedy,Theatrical,OverTheTop"
    /// </summary>
    public string GimmickPreferences { get; init; } = "Balanced";

    /// <summary>
    /// Indique si le créatif peut "ruiner" une storyline s'il est incompatible
    /// Les créatifs très expérimentés mais incompatibles sont dangereux
    /// </summary>
    public bool CanRuinStorylines { get; init; }

    /// <summary>
    /// Liste des storylines proposées par ce créatif
    /// Format: "StorylineId1,StorylineId2,..."
    /// </summary>
    public string? ProposedStorylines { get; init; }

    /// <summary>
    /// Taux d'acceptation des propositions par le Booker (0-100%)
    /// Historique de succès
    /// </summary>
    [Range(0, 100)]
    public int ProposalAcceptanceRate { get; init; } = 50;

    /// <summary>
    /// Spécialité créative (pour Writers et Road Agents)
    /// Ex: "PromoWriting", "MatchLayout", "SegmentProduction"
    /// </summary>
    public string? Specialty { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le CreativeStaff respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(StaffId))
        {
            errorMessage = "StaffId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (CreativityScore is < 0 or > 100)
        {
            errorMessage = "CreativityScore doit être entre 0 et 100";
            return false;
        }

        if (ConsistencyScore is < 0 or > 100)
        {
            errorMessage = "ConsistencyScore doit être entre 0 et 100";
            return false;
        }

        if (LongTermStorylinePreference is < 0 or > 100)
        {
            errorMessage = "LongTermStorylinePreference doit être entre 0 et 100";
            return false;
        }

        if (CreativeRiskTolerance is < 0 or > 100)
        {
            errorMessage = "CreativeRiskTolerance doit être entre 0 et 100";
            return false;
        }

        if (BookerCompatibilityScore is < 0 or > 100)
        {
            errorMessage = "BookerCompatibilityScore doit être entre 0 et 100";
            return false;
        }

        if (ProposalAcceptanceRate is < 0 or > 100)
        {
            errorMessage = "ProposalAcceptanceRate doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le créatif est compatible avec le Booker
    /// </summary>
    public bool IsCompatibleWithBooker() => BookerCompatibilityScore >= 60;

    /// <summary>
    /// Détermine si le créatif représente un risque (incompatibilité + capacité de ruiner)
    /// </summary>
    public bool IsDangerous() => !IsCompatibleWithBooker() && CanRuinStorylines;

    /// <summary>
    /// Calcule un score de fiabilité créative (0-100)
    /// Basé sur cohérence, taux d'acceptation, et compatibilité
    /// </summary>
    public int CalculateReliabilityScore()
    {
        var consistencyWeight = ConsistencyScore * 0.4;
        var acceptanceWeight = ProposalAcceptanceRate * 0.3;
        var compatibilityWeight = BookerCompatibilityScore * 0.3;

        return (int)(consistencyWeight + acceptanceWeight + compatibilityWeight);
    }

    /// <summary>
    /// Retourne le profil créatif sous forme textuelle
    /// </summary>
    public string GetCreativeProfile()
    {
        if (CreativityScore >= 80 && ConsistencyScore >= 80)
            return "Créatif de génie - Innovant et fiable";

        if (CreativityScore >= 70)
            return "Visionnaire créatif - Très innovant";

        if (ConsistencyScore >= 70)
            return "Planificateur stratégique - Très cohérent";

        if (CreativityScore <= 30 && ConsistencyScore <= 30)
            return "Créatif chaotique - Peu fiable";

        return "Créatif équilibré";
    }

    /// <summary>
    /// Détermine le type de storylines que ce créatif préfère
    /// </summary>
    public string GetStorylinePreferenceType()
    {
        return LongTermStorylinePreference switch
        {
            >= 80 => "Slow burn long terme (6+ mois)",
            >= 60 => "Storylines moyennes (3-6 mois)",
            >= 40 => "Storylines courtes (1-3 mois)",
            _ => "One-shots et angles rapides"
        };
    }

    /// <summary>
    /// Calcule le risque de conflit créatif (0-100)
    /// Basé sur l'incompatibilité et le risque créatif
    /// </summary>
    public int CalculateCreativeConflictRisk()
    {
        var incompatibility = 100 - BookerCompatibilityScore;
        var riskBonus = CreativeRiskTolerance > 70 ? 20 : 0;
        var dangerBonus = CanRuinStorylines ? 30 : 0;

        var totalRisk = (int)(incompatibility * 0.5) + riskBonus + dangerBonus;

        return Math.Clamp(totalRisk, 0, 100);
    }
}

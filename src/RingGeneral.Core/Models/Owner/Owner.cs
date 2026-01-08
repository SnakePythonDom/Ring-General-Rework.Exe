using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Owner;

/// <summary>
/// Représente un propriétaire de compagnie avec vision stratégique.
/// Influence les décisions à long terme: fréquence des shows, type de produit, priorités budgétaires.
/// </summary>
public sealed record Owner
{
    /// <summary>
    /// Identifiant unique du propriétaire
    /// </summary>
    [Required]
    public required string OwnerId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie possédée
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Nom du propriétaire
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>
    /// Type de vision stratégique: "Creative", "Business", "Balanced"
    /// - Creative: Priorité à la qualité artistique et créative
    /// - Business: Priorité aux profits et à la croissance financière
    /// - Balanced: Équilibre entre créativité et business
    /// </summary>
    [Required]
    public required string VisionType { get; init; }

    /// <summary>
    /// Tolérance au risque (0-100)
    /// - 0-30: Conservateur, évite les risques
    /// - 31-70: Modéré, prend des risques calculés
    /// - 71-100: Agressif, prend des grands risques pour grandes récompenses
    /// </summary>
    [Range(0, 100)]
    public required int RiskTolerance { get; init; }

    /// <summary>
    /// Type de produit préféré: "Technical", "Entertainment", "Hardcore", "Family-Friendly"
    /// - Technical: Matches techniques, workrate élevé
    /// - Entertainment: Spectacle, storylines, segments
    /// - Hardcore: Violence, sang, extreme
    /// - Family-Friendly: Contenu pour tous publics
    /// </summary>
    [Required]
    public required string PreferredProductType { get; init; }

    /// <summary>
    /// Fréquence de shows préférée: "Weekly", "BiWeekly", "Monthly"
    /// Influence la planification du calendrier de shows
    /// </summary>
    [Required]
    public required string ShowFrequencyPreference { get; init; }

    /// <summary>
    /// Focus sur le développement de talents (0-100)
    /// - 0-30: Recrute des stars établies
    /// - 31-70: Mix de vétérans et développement
    /// - 71-100: Privilégie développement interne
    /// </summary>
    [Range(0, 100)]
    public required int TalentDevelopmentFocus { get; init; }

    /// <summary>
    /// Priorité financière (0-100)
    /// Influence les décisions budgétaires et de salaires
    /// </summary>
    [Range(0, 100)]
    public required int FinancialPriority { get; init; }

    /// <summary>
    /// Priorité satisfaction des fans (0-100)
    /// Influence les décisions de booking pour plaire aux fans
    /// </summary>
    [Range(0, 100)]
    public required int FanSatisfactionPriority { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le Owner respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(OwnerId))
        {
            errorMessage = "OwnerId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
        {
            errorMessage = "Name doit contenir au moins 2 caractères";
            return false;
        }

        var validVisionTypes = new[] { "Creative", "Business", "Balanced" };
        if (!validVisionTypes.Contains(VisionType))
        {
            errorMessage = $"VisionType doit être: {string.Join(", ", validVisionTypes)}";
            return false;
        }

        var validProductTypes = new[] { "Technical", "Entertainment", "Hardcore", "Family-Friendly" };
        if (!validProductTypes.Contains(PreferredProductType))
        {
            errorMessage = $"PreferredProductType doit être: {string.Join(", ", validProductTypes)}";
            return false;
        }

        var validFrequencies = new[] { "Weekly", "BiWeekly", "Monthly" };
        if (!validFrequencies.Contains(ShowFrequencyPreference))
        {
            errorMessage = $"ShowFrequencyPreference doit être: {string.Join(", ", validFrequencies)}";
            return false;
        }

        if (RiskTolerance is < 0 or > 100)
        {
            errorMessage = "RiskTolerance doit être entre 0 et 100";
            return false;
        }

        if (TalentDevelopmentFocus is < 0 or > 100)
        {
            errorMessage = "TalentDevelopmentFocus doit être entre 0 et 100";
            return false;
        }

        if (FinancialPriority is < 0 or > 100)
        {
            errorMessage = "FinancialPriority doit être entre 0 et 100";
            return false;
        }

        if (FanSatisfactionPriority is < 0 or > 100)
        {
            errorMessage = "FanSatisfactionPriority doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le propriétaire accepterait un risque donné
    /// </summary>
    /// <param name="riskLevel">Niveau de risque (0-100)</param>
    public bool WouldAcceptRisk(int riskLevel)
    {
        return riskLevel <= RiskTolerance;
    }

    /// <summary>
    /// Calcule la priorité stratégique dominante
    /// </summary>
    public string GetDominantPriority()
    {
        var max = Math.Max(TalentDevelopmentFocus, Math.Max(FinancialPriority, FanSatisfactionPriority));

        if (max == TalentDevelopmentFocus) return "Talent Development";
        if (max == FinancialPriority) return "Financial";
        return "Fan Satisfaction";
    }
}

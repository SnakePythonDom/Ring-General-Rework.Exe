using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Représente un brand/show dans une structure multi-brand.
/// Chaque brand a son propre booker, objectif stratégique, et roster.
/// </summary>
public sealed record Brand
{
    /// <summary>
    /// Identifiant unique du brand
    /// </summary>
    [Required]
    public required string BrandId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie parente
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Nom du brand
    /// Ex: "Raw", "SmackDown", "NXT", "Dynamite"
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>
    /// Objectif stratégique du brand
    /// </summary>
    [Required]
    public required BrandObjective Objective { get; init; }

    /// <summary>
    /// Identifiant du booker assigné à ce brand
    /// Null si pas encore de booker assigné
    /// </summary>
    public string? BookerId { get; init; }

    /// <summary>
    /// Identifiant de l'ère actuelle du brand
    /// Peut être différente de l'ère globale de la compagnie
    /// </summary>
    public string? CurrentEraId { get; init; }

    /// <summary>
    /// Prestige du brand (0-100)
    /// Influence la perception et l'attractivité
    /// </summary>
    [Range(0, 100)]
    public int Prestige { get; init; } = 50;

    /// <summary>
    /// Budget alloué au brand (par show)
    /// </summary>
    [Range(0, double.MaxValue)]
    public double BudgetPerShow { get; init; }

    /// <summary>
    /// Audience moyenne du brand
    /// </summary>
    [Range(0, int.MaxValue)]
    public int AverageAudience { get; init; }

    /// <summary>
    /// Priorité du brand dans la hiérarchie (1 = flagship, 2+ = secondary)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; init; } = 1;

    /// <summary>
    /// Indique si le brand est actif
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Jour de diffusion du show principal
    /// Ex: "Monday", "Friday"
    /// </summary>
    public string? AirDay { get; init; }

    /// <summary>
    /// Durée du show principal en minutes
    /// </summary>
    [Range(30, 240)]
    public int ShowDuration { get; init; } = 120;

    /// <summary>
    /// Région cible du brand (peut être différente de la compagnie)
    /// </summary>
    public string? TargetRegion { get; init; }

    /// <summary>
    /// Date de création du brand
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Date de désactivation (null si actif)
    /// </summary>
    public DateTime? DeactivatedAt { get; init; }

    /// <summary>
    /// Valide que le Brand respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(BrandId))
        {
            errorMessage = "BrandId ne peut pas être vide";
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

        if (Prestige is < 0 or > 100)
        {
            errorMessage = "Prestige doit être entre 0 et 100";
            return false;
        }

        if (BudgetPerShow < 0)
        {
            errorMessage = "BudgetPerShow ne peut pas être négatif";
            return false;
        }

        if (AverageAudience < 0)
        {
            errorMessage = "AverageAudience ne peut pas être négatif";
            return false;
        }

        if (Priority is < 1 or > 10)
        {
            errorMessage = "Priority doit être entre 1 et 10";
            return false;
        }

        if (ShowDuration is < 30 or > 240)
        {
            errorMessage = "ShowDuration doit être entre 30 et 240 minutes";
            return false;
        }

        if (DeactivatedAt.HasValue && DeactivatedAt.Value < CreatedAt)
        {
            errorMessage = "DeactivatedAt ne peut pas être avant CreatedAt";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le brand est le flagship (priorité 1)
    /// </summary>
    public bool IsFlagship() => Priority == 1 && Objective == BrandObjective.Flagship;

    /// <summary>
    /// Calcule un score de santé du brand (0-100)
    /// Basé sur prestige, audience, et budget
    /// </summary>
    public int CalculateHealthScore()
    {
        // Prestige compte pour 40%
        var prestigeScore = Prestige * 0.4;

        // Audience relative compte pour 40% (normalisée sur 10000 max)
        var audienceScore = Math.Min(AverageAudience / 100.0, 100) * 0.4;

        // Budget compte pour 20% (normalisé sur 50000 max)
        var budgetScore = Math.Min(BudgetPerShow / 500.0, 100) * 0.2;

        return (int)(prestigeScore + audienceScore + budgetScore);
    }

    /// <summary>
    /// Détermine le niveau de priorité du brand
    /// </summary>
    public string GetPriorityLevel()
    {
        return Priority switch
        {
            1 => "Flagship",
            2 => "Secondary A",
            3 => "Secondary B",
            4 or 5 => "Tertiary",
            _ => "Developmental/Experimental"
        };
    }

    /// <summary>
    /// Retourne une description de l'objectif du brand
    /// </summary>
    public string GetObjectiveDescription()
    {
        return Objective switch
        {
            BrandObjective.Flagship => "Brand principal - Focus prestige et revenus",
            BrandObjective.Development => "Développement de talents - Formation de futurs stars",
            BrandObjective.Experimental => "Expérimental - Innovation et tests créatifs",
            BrandObjective.Mainstream => "Grand public - Accessibilité et merchandising",
            BrandObjective.Prestige => "Prestige workrate - Qualité en ring prioritaire",
            BrandObjective.Regional => "Régional - Marché local et identité régionale",
            BrandObjective.Women => "Division féminine - Focus total sur les femmes",
            BrandObjective.Touring => "Tournée internationale - Exposition globale",
            _ => "Non défini"
        };
    }
}

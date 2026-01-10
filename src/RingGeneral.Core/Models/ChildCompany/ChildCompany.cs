using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.ChildCompany;

/// <summary>
/// Représente une Child Company (compagnie affiliée de développement)
/// </summary>
public sealed record ChildCompany(
    /// <summary>
    /// Identifiant unique de la Child Company
    /// </summary>
    [Required]
    string ChildCompanyId,

    /// <summary>
    /// Identifiant de la compagnie mère
    /// </summary>
    [Required]
    string ParentCompanyId,

    /// <summary>
    /// Nom de la Child Company
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    /// <summary>
    /// Région géographique où se trouve la Child Company
    /// </summary>
    string? RegionId,

    /// <summary>
    /// Niveau de développement de la Child Company
    /// </summary>
    [Required]
    ChildCompanyLevel Level,

    /// <summary>
    /// Budget mensuel alloué à la Child Company
    /// </summary>
    [Range(0, double.MaxValue)]
    decimal MonthlyBudget,

    /// <summary>
    /// Date de création de la Child Company
    /// </summary>
    DateTime CreatedAt);

/// <summary>
/// Niveau de développement d'une Child Company
/// </summary>
public enum ChildCompanyLevel
{
    /// <summary>
    /// Niveau développement - Structure basique, focus recrutement
    /// </summary>
    Development,

    /// <summary>
    /// Niveau officiel - Territoire de développement reconnu
    /// </summary>
    Official,

    /// <summary>
    /// Niveau avancé - Centre de développement d'élite
    /// </summary>
    Advanced
}
using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Extension du modèle ChildCompany pour supporter les objectifs stratégiques
/// (Entertainment, Niche, Independence, Development)
/// </summary>
public sealed record ChildCompanyExtended
{
    /// <summary>
    /// Identifiant unique de la filiale (référence à Company)
    /// </summary>
    [Required]
    public required string ChildCompanyId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie mère
    /// </summary>
    [Required]
    public required string ParentCompanyId { get; init; }

    /// <summary>
    /// Objectif assignable à la filiale
    /// </summary>
    [Required]
    public required ChildCompanyObjective Objective { get; init; }

    /// <summary>
    /// Indique si la filiale a une autonomie complète
    /// Booker IA gère complètement si true
    /// </summary>
    public required bool HasFullAutonomy { get; init; }

    /// <summary>
    /// Identifiant du booker assigné (peut être IA)
    /// </summary>
    public string? AssignedBookerId { get; init; }

    /// <summary>
    /// Indique si la filiale est un laboratoire
    /// Test de gimmicks risquées
    /// </summary>
    public required bool IsLaboratory { get; init; }

    /// <summary>
    /// Style testé si laboratoire
    /// </summary>
    public string? TestStyle { get; init; }

    /// <summary>
    /// Type de niche si objectif = Niche
    /// </summary>
    public NicheType? NicheType { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Indique si la filiale est active
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Nom de la filiale
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Acronyme de la filiale (ex: WWE)
    /// </summary>
    public string? Acronym { get; init; }

    /// <summary>
    /// Région de la filiale
    /// </summary>
    public string? RegionId { get; init; }

    /// <summary>
    /// Limite de taille du roster
    /// </summary>
    public int RosterLimit { get; init; } = 20;

    /// <summary>
    /// Phase 2.3 - Identifiant de la YouthStructure liée si Objective = Development
    /// </summary>
    public string? YouthStructureId { get; init; }

    /// <summary>
    /// Valide que le modèle respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(ChildCompanyId))
        {
            errorMessage = "ChildCompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ParentCompanyId))
        {
            errorMessage = "ParentCompanyId ne peut pas être vide";
            return false;
        }

        if (Objective == ChildCompanyObjective.Niche && !NicheType.HasValue)
        {
            errorMessage = "NicheType est requis si Objective = Niche";
            return false;
        }

        if (IsLaboratory && string.IsNullOrWhiteSpace(TestStyle))
        {
            errorMessage = "TestStyle est requis si IsLaboratory = true";
            return false;
        }

        return true;
    }
}

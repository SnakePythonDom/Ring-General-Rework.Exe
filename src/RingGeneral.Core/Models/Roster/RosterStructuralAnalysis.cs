using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Models.Roster;

namespace RingGeneral.Core.Models.Roster;

/// <summary>
/// Analyse structurelle du roster - Vue agrégée des capacités d'une fédération
/// </summary>
public sealed record RosterStructuralAnalysis
{
    /// <summary>
    /// Identifiant unique de l'analyse
    /// </summary>
    [Required]
    public required string AnalysisId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Star Power Moyen (0-100)
    /// Capacité globale à attirer l'œil (Charisme/Aura)
    /// </summary>
    [Range(0, 100)]
    public required double StarPowerMoyen { get; init; }

    /// <summary>
    /// Workrate Moyen (0-100)
    /// Qualité technique globale des matchs
    /// </summary>
    [Range(0, 100)]
    public required double WorkrateMoyen { get; init; }

    /// <summary>
    /// Spécialisation dominante
    /// Hardcore, Technical, Lucha, Entertainment, StrongStyle
    /// </summary>
    [Required]
    public required string SpecialisationDominante { get; init; }

    /// <summary>
    /// Profondeur
    /// Nombre de talents capables de tenir un Main Event
    /// </summary>
    [Range(0, int.MaxValue)]
    public required int Profondeur { get; init; }

    /// <summary>
    /// Indice de Dépendance (0-100)
    /// Pourcentage de l'audience liée aux 2 stars majeures
    /// </summary>
    [Range(0, 100)]
    public required double IndiceDependance { get; init; }

    /// <summary>
    /// Polyvalence (0-100)
    /// Capacité du roster à changer de style
    /// </summary>
    [Range(0, 100)]
    public required double Polyvalence { get; init; }

    /// <summary>
    /// ADN du Roster (calculé)
    /// </summary>
    public RosterDNA? Dna { get; init; }

    /// <summary>
    /// Date de calcul
    /// </summary>
    public required DateTime CalculatedAt { get; init; }

    /// <summary>
    /// Numéro de semaine du calcul
    /// </summary>
    [Range(1, 52)]
    public required int WeekNumber { get; init; }

    /// <summary>
    /// Année du calcul
    /// </summary>
    [Range(1950, 2100)]
    public required int Year { get; init; }

    /// <summary>
    /// Valide que l'analyse respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(AnalysisId))
        {
            errorMessage = "AnalysisId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SpecialisationDominante))
        {
            errorMessage = "SpecialisationDominante ne peut pas être vide";
            return false;
        }

        var validSpecialisations = new[] { "Hardcore", "Technical", "Lucha", "Entertainment", "StrongStyle", "Hybrid" };
        if (!validSpecialisations.Contains(SpecialisationDominante))
        {
            errorMessage = $"Spécialisation invalide: {SpecialisationDominante}";
            return false;
        }

        return true;
    }
}

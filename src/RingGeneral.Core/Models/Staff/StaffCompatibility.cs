using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Staff;

/// <summary>
/// Représente la compatibilité entre un membre du staff créatif et un booker.
/// Calculé dynamiquement et stocké pour éviter les recalculs constants.
/// </summary>
public sealed record StaffCompatibility
{
    /// <summary>
    /// Identifiant unique de la compatibilité
    /// </summary>
    [Required]
    public required string CompatibilityId { get; init; }

    /// <summary>
    /// Identifiant du staff créatif
    /// </summary>
    [Required]
    public required string StaffId { get; init; }

    /// <summary>
    /// Identifiant du booker
    /// </summary>
    [Required]
    public required string BookerId { get; init; }

    /// <summary>
    /// Score de compatibilité global (0-100)
    /// - 0-30: Incompatible, risque de ruiner les storylines
    /// - 31-60: Compatibilité moyenne, fonctionnel
    /// - 61-80: Bonne compatibilité
    /// - 81-100: Excellente synergie
    /// </summary>
    [Range(0, 100)]
    public required int OverallScore { get; init; }

    /// <summary>
    /// Compatibilité de vision créative (0-100)
    /// Basée sur créativité vs logique, style préféré
    /// </summary>
    [Range(0, 100)]
    public int CreativeVisionScore { get; init; }

    /// <summary>
    /// Compatibilité de style de booking (0-100)
    /// Long-term vs short-term, storytelling preferences
    /// </summary>
    [Range(0, 100)]
    public int BookingStyleScore { get; init; }

    /// <summary>
    /// Compatibilité de biais/préférences (0-100)
    /// Big men vs cruiserweights, veterans vs rookies, etc.
    /// </summary>
    [Range(0, 100)]
    public int BiasAlignmentScore { get; init; }

    /// <summary>
    /// Compatibilité de tolérance au risque (0-100)
    /// Conservative vs risk-taker
    /// </summary>
    [Range(0, 100)]
    public int RiskToleranceScore { get; init; }

    /// <summary>
    /// Score de personnalité/relations (0-100)
    /// Chimie personnelle, historique de conflits
    /// </summary>
    [Range(0, 100)]
    public int PersonalChemistryScore { get; init; }

    /// <summary>
    /// Facteurs positifs de compatibilité
    /// Ex: "Même vision créative, Préférences alignées"
    /// </summary>
    public string PositiveFactors { get; init; } = "";

    /// <summary>
    /// Facteurs négatifs de compatibilité
    /// Ex: "Biais opposés, Conflits de personnalité"
    /// </summary>
    public string NegativeFactors { get; init; } = "";

    /// <summary>
    /// Nombre de storylines réussies ensemble
    /// </summary>
    public int SuccessfulCollaborations { get; init; }

    /// <summary>
    /// Nombre de storylines ratées/ruinées ensemble
    /// </summary>
    public int FailedCollaborations { get; init; }

    /// <summary>
    /// Historique de conflits (nombre de désaccords majeurs)
    /// </summary>
    public int ConflictHistory { get; init; }

    /// <summary>
    /// Date de dernier calcul de compatibilité
    /// </summary>
    public DateTime LastCalculatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Indique si la compatibilité nécessite une recalculation
    /// (si plus de 30 jours depuis dernier calcul)
    /// </summary>
    public bool NeedsRecalculation => (DateTime.Now - LastCalculatedAt).Days > 30;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que la StaffCompatibility respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(CompatibilityId))
        {
            errorMessage = "CompatibilityId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(StaffId))
        {
            errorMessage = "StaffId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(BookerId))
        {
            errorMessage = "BookerId ne peut pas être vide";
            return false;
        }

        if (OverallScore is < 0 or > 100)
        {
            errorMessage = "OverallScore doit être entre 0 et 100";
            return false;
        }

        if (CreativeVisionScore is < 0 or > 100)
        {
            errorMessage = "CreativeVisionScore doit être entre 0 et 100";
            return false;
        }

        if (BookingStyleScore is < 0 or > 100)
        {
            errorMessage = "BookingStyleScore doit être entre 0 et 100";
            return false;
        }

        if (BiasAlignmentScore is < 0 or > 100)
        {
            errorMessage = "BiasAlignmentScore doit être entre 0 et 100";
            return false;
        }

        if (RiskToleranceScore is < 0 or > 100)
        {
            errorMessage = "RiskToleranceScore doit être entre 0 et 100";
            return false;
        }

        if (PersonalChemistryScore is < 0 or > 100)
        {
            errorMessage = "PersonalChemistryScore doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le staff et le booker sont compatibles
    /// </summary>
    public bool AreCompatible() => OverallScore >= 60;

    /// <summary>
    /// Détermine si la compatibilité est excellente
    /// </summary>
    public bool HasExcellentSynergy() => OverallScore >= 80;

    /// <summary>
    /// Détermine si la compatibilité est dangereuse (risque de ruiner storylines)
    /// </summary>
    public bool IsDangerous() => OverallScore <= 30;

    /// <summary>
    /// Calcule le taux de succès des collaborations (0-100%)
    /// </summary>
    public int CalculateCollaborationSuccessRate()
    {
        var totalCollaborations = SuccessfulCollaborations + FailedCollaborations;
        if (totalCollaborations == 0) return 50; // Neutre si pas d'historique

        return (int)((SuccessfulCollaborations / (double)totalCollaborations) * 100);
    }

    /// <summary>
    /// Calcule un score de risque de conflit (0-100)
    /// </summary>
    public int CalculateConflictRisk()
    {
        var incompatibilityRisk = (100 - OverallScore) * 0.5;
        var historyRisk = Math.Min(ConflictHistory * 5, 30);
        var failureRisk = (100 - CalculateCollaborationSuccessRate()) * 0.2;

        var totalRisk = incompatibilityRisk + historyRisk + failureRisk;

        return (int)Math.Clamp(totalRisk, 0, 100);
    }

    /// <summary>
    /// Retourne le niveau de compatibilité sous forme textuelle
    /// </summary>
    public string GetCompatibilityLevel()
    {
        return OverallScore switch
        {
            >= 90 => "Synergie parfaite - Dream team créatif",
            >= 80 => "Excellente compatibilité - Collaboration fluide",
            >= 60 => "Bonne compatibilité - Fonctionnel",
            >= 40 => "Compatibilité moyenne - Tensions possibles",
            >= 20 => "Faible compatibilité - Conflits fréquents",
            _ => "Incompatibilité totale - Risque de désastre"
        };
    }

    /// <summary>
    /// Retourne le domaine de compatibilité le plus fort
    /// </summary>
    public string GetStrongestArea()
    {
        var scores = new[]
        {
            (Score: CreativeVisionScore, Name: "Vision créative"),
            (Score: BookingStyleScore, Name: "Style de booking"),
            (Score: BiasAlignmentScore, Name: "Alignement des biais"),
            (Score: RiskToleranceScore, Name: "Tolérance au risque"),
            (Score: PersonalChemistryScore, Name: "Chimie personnelle")
        };

        return scores.OrderByDescending(s => s.Score).First().Name;
    }

    /// <summary>
    /// Retourne le domaine de compatibilité le plus faible
    /// </summary>
    public string GetWeakestArea()
    {
        var scores = new[]
        {
            (Score: CreativeVisionScore, Name: "Vision créative"),
            (Score: BookingStyleScore, Name: "Style de booking"),
            (Score: BiasAlignmentScore, Name: "Alignement des biais"),
            (Score: RiskToleranceScore, Name: "Tolérance au risque"),
            (Score: PersonalChemistryScore, Name: "Chimie personnelle")
        };

        return scores.OrderBy(s => s.Score).First().Name;
    }

    /// <summary>
    /// Génère un rapport de compatibilité détaillé
    /// </summary>
    public string GenerateCompatibilityReport()
    {
        var report = $@"Compatibilité Staff-Booker: {GetCompatibilityLevel()} ({OverallScore}/100)

Points forts: {GetStrongestArea()} ({GetStrongestArea() switch
        {
            "Vision créative" => CreativeVisionScore,
            "Style de booking" => BookingStyleScore,
            "Alignement des biais" => BiasAlignmentScore,
            "Tolérance au risque" => RiskToleranceScore,
            _ => PersonalChemistryScore
        }}/100)

Points faibles: {GetWeakestArea()} ({GetWeakestArea() switch
        {
            "Vision créative" => CreativeVisionScore,
            "Style de booking" => BookingStyleScore,
            "Alignement des biais" => BiasAlignmentScore,
            "Tolérance au risque" => RiskToleranceScore,
            _ => PersonalChemistryScore
        }}/100)

Historique:
- Collaborations réussies: {SuccessfulCollaborations}
- Collaborations ratées: {FailedCollaborations}
- Taux de succès: {CalculateCollaborationSuccessRate()}%
- Conflits passés: {ConflictHistory}
- Risque de conflit: {CalculateConflictRisk()}/100

Facteurs positifs: {(string.IsNullOrWhiteSpace(PositiveFactors) ? "Aucun" : PositiveFactors)}
Facteurs négatifs: {(string.IsNullOrWhiteSpace(NegativeFactors) ? "Aucun" : NegativeFactors)}";

        return report;
    }
}

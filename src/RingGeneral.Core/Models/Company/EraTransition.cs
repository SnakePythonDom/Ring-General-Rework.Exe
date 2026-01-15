using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Représente une transition entre deux ères.
/// Les transitions sont progressives et peuvent causer des conflits si trop rapides.
/// </summary>
public sealed record EraTransition
{
    /// <summary>
    /// Identifiant unique de la transition
    /// </summary>
    [Required]
    public required string TransitionId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Identifiant de l'ère source (actuelle)
    /// </summary>
    [Required]
    public required string FromEraId { get; init; }

    /// <summary>
    /// Identifiant de l'ère cible (nouvelle)
    /// </summary>
    [Required]
    public required string ToEraId { get; init; }

    /// <summary>
    /// Date de début de la transition
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Date de fin prévue de la transition
    /// </summary>
    public required DateTime PlannedEndDate { get; init; }

    /// <summary>
    /// Date de fin réelle de la transition (null si en cours)
    /// </summary>
    public DateTime? ActualEndDate { get; init; }

    /// <summary>
    /// Progression de la transition (0-100%)
    /// 0 = Début, 100 = Transition complète
    /// </summary>
    [Range(0, 100)]
    public required int ProgressPercentage { get; init; }

    /// <summary>
    /// Vitesse de transition souhaitée
    /// </summary>
    [Required]
    public required EraTransitionSpeed Speed { get; init; }

    /// <summary>
    /// Impact sur le moral de la compagnie durant la transition (-50 à +50)
    /// Négatif si transition trop rapide ou mal acceptée
    /// </summary>
    [Range(-50, 50)]
    public int MoraleImpact { get; init; }

    /// <summary>
    /// Impact sur l'audience durant la transition (-50 à +50)
    /// Négatif si le public rejette la nouvelle ère
    /// </summary>
    [Range(-50, 50)]
    public int AudienceImpact { get; init; }

    /// <summary>
    /// Résistance au changement (0-100)
    /// Calculée en fonction de la compatibilité des ères et de l'attachement du public
    /// </summary>
    [Range(0, 100)]
    public int ChangeResistance { get; init; }

    /// <summary>
    /// Identifiant du booker qui a initié la transition
    /// </summary>
    public string? InitiatedByBookerId { get; init; }

    /// <summary>
    /// Indique si la transition est en cours
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Notes sur la transition (événements marquants, réactions)
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que l'EraTransition respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(TransitionId))
        {
            errorMessage = "TransitionId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FromEraId))
        {
            errorMessage = "FromEraId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ToEraId))
        {
            errorMessage = "ToEraId ne peut pas être vide";
            return false;
        }

        if (FromEraId == ToEraId)
        {
            errorMessage = "FromEraId et ToEraId doivent être différents";
            return false;
        }

        if (PlannedEndDate <= StartDate)
        {
            errorMessage = "PlannedEndDate doit être après StartDate";
            return false;
        }

        if (ActualEndDate.HasValue && ActualEndDate.Value < StartDate)
        {
            errorMessage = "ActualEndDate ne peut pas être avant StartDate";
            return false;
        }

        if (ProgressPercentage is < 0 or > 100)
        {
            errorMessage = "ProgressPercentage doit être entre 0 et 100";
            return false;
        }

        if (MoraleImpact is < -50 or > 50)
        {
            errorMessage = "MoraleImpact doit être entre -50 et 50";
            return false;
        }

        if (AudienceImpact is < -50 or > 50)
        {
            errorMessage = "AudienceImpact doit être entre -50 et 50";
            return false;
        }

        if (ChangeResistance is < 0 or > 100)
        {
            errorMessage = "ChangeResistance doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la transition est terminée
    /// </summary>
    public bool IsCompleted() => ProgressPercentage >= 100 || ActualEndDate.HasValue;

    /// <summary>
    /// Calcule la durée prévue de la transition en jours
    /// </summary>
    public int GetPlannedDurationInDays() => (PlannedEndDate - StartDate).Days;

    /// <summary>
    /// Calcule la durée réelle de la transition en jours
    /// </summary>
    public int GetActualDurationInDays()
    {
        var end = ActualEndDate ?? DateTime.Now;
        return (end - StartDate).Days;
    }

    /// <summary>
    /// Détermine si la transition est en avance sur le planning
    /// </summary>
    public bool IsAheadOfSchedule()
    {
        if (IsCompleted() && ActualEndDate.HasValue)
        {
            return ActualEndDate.Value < PlannedEndDate;
        }

        // Calcul basé sur progression vs temps écoulé
        var plannedDuration = GetPlannedDurationInDays();
        var actualDuration = GetActualDurationInDays();
        var expectedProgress = (int)((actualDuration / (double)plannedDuration) * 100);

        return ProgressPercentage > expectedProgress + 10;
    }

    /// <summary>
    /// Calcule le risque de rejet par le public (0-100)
    /// Basé sur la vitesse, la résistance et les impacts
    /// </summary>
    public int CalculateRejectionRisk()
    {
        var baseRisk = Speed switch
        {
            EraTransitionSpeed.VerySlow => 5,
            EraTransitionSpeed.Slow => 15,
            EraTransitionSpeed.Moderate => 30,
            EraTransitionSpeed.Fast => 55,
            EraTransitionSpeed.Brutal => 80,
            _ => 30
        };

        // Ajouter la résistance au changement
        var resistanceBonus = (int)(ChangeResistance * 0.3);

        // Réduire si impacts positifs
        var impactPenalty = (MoraleImpact + AudienceImpact) / 4;

        var totalRisk = baseRisk + resistanceBonus - impactPenalty;

        return Math.Clamp(totalRisk, 0, 100);
    }

    /// <summary>
    /// Retourne une description textuelle de l'état de la transition
    /// </summary>
    public string GetStatusDescription()
    {
        if (IsCompleted())
            return "Transition terminée";

        if (ProgressPercentage < 25)
            return "Début de transition";

        if (ProgressPercentage < 50)
            return "Transition en cours (early)";

        if (ProgressPercentage < 75)
            return "Transition en cours (mid)";

        return "Transition presque terminée";
    }
}

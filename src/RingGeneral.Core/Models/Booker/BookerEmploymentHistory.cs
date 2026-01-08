using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Booker;

/// <summary>
/// Représente l'historique d'emploi d'un booker dans différentes compagnies.
/// Permet de tracker les performances passées et raisons de départ.
/// </summary>
public sealed record BookerEmploymentHistory
{
    /// <summary>
    /// Identifiant unique de l'entrée d'historique
    /// </summary>
    public int HistoryId { get; init; }

    /// <summary>
    /// Identifiant du booker
    /// </summary>
    [Required]
    public required string BookerId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Date de début d'emploi
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Date de fin d'emploi (NULL si toujours actif)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Raison de la terminaison (NULL si toujours actif)
    /// Ex: "Fired", "Resigned", "Mutual Agreement", "Poached"
    /// </summary>
    [StringLength(200)]
    public string? TerminationReason { get; init; }

    /// <summary>
    /// Score de performance final (0-100, NULL si toujours actif ou non évalué)
    /// Calculé basé sur:
    /// - Qualité moyenne des matches bookés
    /// - Moral de roster sous son booking
    /// - Satisfaction du propriétaire
    /// </summary>
    [Range(0, 100)]
    public int? PerformanceScore { get; init; }

    /// <summary>
    /// Valide que l'historique respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(BookerId))
        {
            errorMessage = "BookerId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (EndDate.HasValue && EndDate < StartDate)
        {
            errorMessage = "EndDate ne peut pas être antérieure à StartDate";
            return false;
        }

        if (PerformanceScore.HasValue && (PerformanceScore < 0 || PerformanceScore > 100))
        {
            errorMessage = "PerformanceScore doit être entre 0 et 100";
            return false;
        }

        // Si l'emploi est terminé, doit avoir une raison
        if (EndDate.HasValue && string.IsNullOrWhiteSpace(TerminationReason))
        {
            errorMessage = "TerminationReason requis si EndDate est défini";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si l'emploi est toujours actif
    /// </summary>
    public bool IsCurrentEmployment() => !EndDate.HasValue;

    /// <summary>
    /// Calcule la durée d'emploi en jours
    /// </summary>
    public int GetEmploymentDurationDays()
    {
        var endDate = EndDate ?? DateTime.Now;
        return (int)(endDate - StartDate).TotalDays;
    }

    /// <summary>
    /// Calcule la durée d'emploi en semaines
    /// </summary>
    public int GetEmploymentDurationWeeks()
    {
        return GetEmploymentDurationDays() / 7;
    }

    /// <summary>
    /// Détermine si le booker a été viré (fired)
    /// </summary>
    public bool WasFired() => TerminationReason?.ToLower().Contains("fired") ?? false;

    /// <summary>
    /// Détermine si le booker a démissionné
    /// </summary>
    public bool Resigned() => TerminationReason?.ToLower().Contains("resign") ?? false;

    /// <summary>
    /// Détermine si le booker a été débauché par une autre compagnie
    /// </summary>
    public bool WasPoached() => TerminationReason?.ToLower().Contains("poach") ?? false;

    /// <summary>
    /// Retourne le label de performance basé sur le score
    /// </summary>
    public string GetPerformanceLabel()
    {
        if (!PerformanceScore.HasValue)
            return "Non évalué";

        return PerformanceScore.Value switch
        {
            >= 90 => "Exceptionnel",
            >= 75 => "Excellent",
            >= 60 => "Bon",
            >= 40 => "Moyen",
            >= 20 => "Médiocre",
            _ => "Mauvais"
        };
    }

    /// <summary>
    /// Crée une nouvelle entrée d'historique pour un nouvel emploi
    /// </summary>
    public static BookerEmploymentHistory CreateNew(string bookerId, string companyId, DateTime hireDate)
    {
        return new BookerEmploymentHistory
        {
            BookerId = bookerId,
            CompanyId = companyId,
            StartDate = hireDate,
            EndDate = null,
            TerminationReason = null,
            PerformanceScore = null
        };
    }

    /// <summary>
    /// Termine l'emploi actuel avec raison et score de performance
    /// </summary>
    public BookerEmploymentHistory Terminate(DateTime endDate, string reason, int? performanceScore = null)
    {
        return this with
        {
            EndDate = endDate,
            TerminationReason = reason,
            PerformanceScore = performanceScore
        };
    }
}

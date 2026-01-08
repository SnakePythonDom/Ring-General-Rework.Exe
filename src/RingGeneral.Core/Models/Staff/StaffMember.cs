using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Staff;

/// <summary>
/// Classe de base pour tous les membres du staff.
/// Contient les propriétés communes à tous les types de staff.
/// </summary>
public sealed record StaffMember
{
    /// <summary>
    /// Identifiant unique du membre du staff
    /// </summary>
    [Required]
    public required string StaffId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Identifiant du brand (null si staff global/transversal)
    /// </summary>
    public string? BrandId { get; init; }

    /// <summary>
    /// Nom du membre du staff
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>
    /// Rôle du staff
    /// </summary>
    [Required]
    public required StaffRole Role { get; init; }

    /// <summary>
    /// Département du staff
    /// </summary>
    [Required]
    public required StaffDepartment Department { get; init; }

    /// <summary>
    /// Niveau d'expertise
    /// </summary>
    [Required]
    public required StaffExpertiseLevel ExpertiseLevel { get; init; }

    /// <summary>
    /// Années d'expérience dans l'industrie
    /// </summary>
    [Range(0, 50)]
    public int YearsOfExperience { get; init; }

    /// <summary>
    /// Score de compétence global (0-100)
    /// Influence l'efficacité dans le rôle
    /// </summary>
    [Range(0, 100)]
    public required int SkillScore { get; init; }

    /// <summary>
    /// Score de personnalité/attitude (0-100)
    /// - 0-30: Difficile, conflictuel
    /// - 31-70: Professionnel, normal
    /// - 71-100: Excellent, facilitateur
    /// </summary>
    [Range(0, 100)]
    public int PersonalityScore { get; init; } = 50;

    /// <summary>
    /// Salaire annuel
    /// </summary>
    [Range(0, double.MaxValue)]
    public double AnnualSalary { get; init; }

    /// <summary>
    /// Date d'embauche
    /// </summary>
    public required DateTime HireDate { get; init; }

    /// <summary>
    /// Date de fin de contrat (null si indéterminé)
    /// </summary>
    public DateTime? ContractEndDate { get; init; }

    /// <summary>
    /// Statut d'emploi: "Active", "OnLeave", "Suspended", "Fired"
    /// </summary>
    [Required]
    public string EmploymentStatus { get; init; } = "Active";

    /// <summary>
    /// Indique si le staff est actif
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Notes sur le membre du staff
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le StaffMember respecte les contraintes métier
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

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
        {
            errorMessage = "Name doit contenir au moins 2 caractères";
            return false;
        }

        if (YearsOfExperience is < 0 or > 50)
        {
            errorMessage = "YearsOfExperience doit être entre 0 et 50";
            return false;
        }

        if (SkillScore is < 0 or > 100)
        {
            errorMessage = "SkillScore doit être entre 0 et 100";
            return false;
        }

        if (PersonalityScore is < 0 or > 100)
        {
            errorMessage = "PersonalityScore doit être entre 0 et 100";
            return false;
        }

        if (AnnualSalary < 0)
        {
            errorMessage = "AnnualSalary ne peut pas être négatif";
            return false;
        }

        var validStatuses = new[] { "Active", "OnLeave", "Suspended", "Fired" };
        if (!validStatuses.Contains(EmploymentStatus))
        {
            errorMessage = $"EmploymentStatus doit être: {string.Join(", ", validStatuses)}";
            return false;
        }

        if (ContractEndDate.HasValue && ContractEndDate.Value < HireDate)
        {
            errorMessage = "ContractEndDate ne peut pas être avant HireDate";
            return false;
        }

        // Validation: Trainers DOIVENT être liés à un infrastructure (Brand ou Dojo)
        if (Department == StaffDepartment.Training && string.IsNullOrWhiteSpace(BrandId))
        {
            errorMessage = "Les Trainers doivent être liés à un Brand/Infrastructure (BrandId requis)";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le membre du staff est actif et employé
    /// </summary>
    public bool IsActiveEmployee() => IsActive && EmploymentStatus == "Active";

    /// <summary>
    /// Calcule le coût mensuel du staff
    /// </summary>
    public double GetMonthlyCost() => AnnualSalary / 12.0;

    /// <summary>
    /// Détermine si le staff est transversal (global à la compagnie)
    /// </summary>
    public bool IsGlobalStaff() => string.IsNullOrWhiteSpace(BrandId);

    /// <summary>
    /// Calcule un score de valeur (skill / salaire ratio)
    /// Plus le score est élevé, meilleur est le rapport qualité/prix
    /// </summary>
    public double CalculateValueScore()
    {
        if (AnnualSalary == 0) return SkillScore;

        // Normaliser: skill de 100 pour 100k$ de salaire = 1.0
        return (SkillScore / 100.0) / (AnnualSalary / 100000.0);
    }

    /// <summary>
    /// Retourne le niveau d'expérience sous forme textuelle
    /// </summary>
    public string GetExperienceDescription()
    {
        return ExpertiseLevel switch
        {
            StaffExpertiseLevel.Junior => $"Junior ({YearsOfExperience} ans)",
            StaffExpertiseLevel.MidLevel => $"Mid-Level ({YearsOfExperience} ans)",
            StaffExpertiseLevel.Senior => $"Senior ({YearsOfExperience} ans)",
            StaffExpertiseLevel.Expert => $"Expert ({YearsOfExperience} ans)",
            StaffExpertiseLevel.Legend => $"Légende ({YearsOfExperience} ans)",
            _ => $"{YearsOfExperience} ans d'expérience"
        };
    }

    /// <summary>
    /// Détermine si le contrat arrive à expiration (moins de 90 jours)
    /// </summary>
    public bool IsContractExpiringSoon()
    {
        if (!ContractEndDate.HasValue) return false;
        return (ContractEndDate.Value - DateTime.Now).Days <= 90;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Staff;

/// <summary>
/// Représente un entraîneur (Trainer) lié à une infrastructure (Dojo/Performance Center).
/// Impact uniquement sur le long terme via la progression des stats des jeunes talents.
/// AUCUN lien direct avec le booking.
/// </summary>
public sealed record Trainer
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
    /// Identifiant du Brand/Infrastructure (OBLIGATOIRE pour trainers)
    /// Les trainers sont TOUJOURS liés à une structure physique
    /// </summary>
    [Required]
    public required string InfrastructureId { get; init; }

    /// <summary>
    /// Type d'entraînement spécialisé
    /// "InRing" = Améliore attributs InRing
    /// "Promo" = Améliore attributs Entertainment
    /// "Strength" = Améliore physique et prévention blessures
    /// "AllRound" = Améliore tous les attributs (moins efficace)
    /// </summary>
    [Required]
    public required string TrainingSpecialization { get; init; }

    /// <summary>
    /// Efficacité de l'entraînement (0-100)
    /// Multiplie la vitesse de progression des élèves
    /// </summary>
    [Range(0, 100)]
    public required int TrainingEfficiency { get; init; }

    /// <summary>
    /// Bonus de progression appliqué aux élèves (0-50%)
    /// Un trainer à 50% double la vitesse de progression
    /// </summary>
    [Range(0, 50)]
    public int ProgressionBonus { get; init; }

    /// <summary>
    /// Capacité à développer les jeunes talents (0-100)
    /// Influence le plafond de progression des rookies
    /// </summary>
    [Range(0, 100)]
    public int YouthDevelopmentScore { get; init; }

    /// <summary>
    /// Expérience en tant que wrestler (0-30 ans)
    /// Les anciens wrestlers sont souvent meilleurs trainers
    /// </summary>
    [Range(0, 30)]
    public int WrestlingExperience { get; init; }

    /// <summary>
    /// Style de wrestling enseigné
    /// Ex: "Technical", "Brawler", "HighFlyer", "PowerHouse"
    /// </summary>
    public string WrestlingStyle { get; init; } = "AllRound";

    /// <summary>
    /// Réputation en tant que trainer (0-100)
    /// Influence l'attractivité de l'infrastructure pour les recrues
    /// </summary>
    [Range(0, 100)]
    public int Reputation { get; init; }

    /// <summary>
    /// Nombre d'élèves actuels
    /// </summary>
    [Range(0, 50)]
    public int CurrentStudents { get; init; }

    /// <summary>
    /// Capacité maximale d'élèves
    /// </summary>
    [Range(1, 50)]
    public int MaxStudentCapacity { get; init; } = 10;

    /// <summary>
    /// Nombre d'élèves qui ont "gradué" (devenu main roster)
    /// Indicateur de succès
    /// </summary>
    public int GraduatedStudents { get; init; }

    /// <summary>
    /// Nombre d'élèves qui ont échoué/abandonné
    /// </summary>
    public int FailedStudents { get; init; }

    /// <summary>
    /// Spécialité d'enseignement (pour trainers avancés)
    /// Ex: "MatchPsychology", "Selling", "Promo", "Cardio"
    /// </summary>
    public string? TeachingSpecialty { get; init; }

    /// <summary>
    /// Indique si le trainer peut former des futurs stars
    /// Basé sur réputation et efficacité
    /// </summary>
    public bool CanDevelopStars { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le Trainer respecte les contraintes métier
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

        if (string.IsNullOrWhiteSpace(InfrastructureId))
        {
            errorMessage = "InfrastructureId ne peut pas être vide (Trainers DOIVENT être liés à une infrastructure)";
            return false;
        }

        if (string.IsNullOrWhiteSpace(TrainingSpecialization))
        {
            errorMessage = "TrainingSpecialization ne peut pas être vide";
            return false;
        }

        var validSpecializations = new[] { "InRing", "Promo", "Strength", "AllRound" };
        if (!validSpecializations.Contains(TrainingSpecialization))
        {
            errorMessage = $"TrainingSpecialization doit être: {string.Join(", ", validSpecializations)}";
            return false;
        }

        if (TrainingEfficiency is < 0 or > 100)
        {
            errorMessage = "TrainingEfficiency doit être entre 0 et 100";
            return false;
        }

        if (ProgressionBonus is < 0 or > 50)
        {
            errorMessage = "ProgressionBonus doit être entre 0 et 50";
            return false;
        }

        if (YouthDevelopmentScore is < 0 or > 100)
        {
            errorMessage = "YouthDevelopmentScore doit être entre 0 et 100";
            return false;
        }

        if (WrestlingExperience is < 0 or > 30)
        {
            errorMessage = "WrestlingExperience doit être entre 0 et 30 ans";
            return false;
        }

        if (Reputation is < 0 or > 100)
        {
            errorMessage = "Reputation doit être entre 0 et 100";
            return false;
        }

        if (CurrentStudents < 0 || CurrentStudents > MaxStudentCapacity)
        {
            errorMessage = "CurrentStudents doit être entre 0 et MaxStudentCapacity";
            return false;
        }

        if (MaxStudentCapacity is < 1 or > 50)
        {
            errorMessage = "MaxStudentCapacity doit être entre 1 et 50";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le trainer a de la place pour de nouveaux élèves
    /// </summary>
    public bool HasAvailableSlots() => CurrentStudents < MaxStudentCapacity;

    /// <summary>
    /// Calcule le nombre de places disponibles
    /// </summary>
    public int GetAvailableSlots() => MaxStudentCapacity - CurrentStudents;

    /// <summary>
    /// Calcule le taux de succès (graduation rate) (0-100%)
    /// </summary>
    public int CalculateGraduationRate()
    {
        var totalStudents = GraduatedStudents + FailedStudents;
        if (totalStudents == 0) return 50; // Neutre si pas d'historique

        return (int)((GraduatedStudents / (double)totalStudents) * 100);
    }

    /// <summary>
    /// Calcule un score de qualité du trainer (0-100)
    /// Basé sur efficacité, réputation, et taux de succès
    /// </summary>
    public int CalculateQualityScore()
    {
        var efficiencyWeight = TrainingEfficiency * 0.4;
        var reputationWeight = Reputation * 0.3;
        var graduationRateWeight = CalculateGraduationRate() * 0.3;

        return (int)(efficiencyWeight + reputationWeight + graduationRateWeight);
    }

    /// <summary>
    /// Retourne la spécialisation sous forme descriptive
    /// </summary>
    public string GetSpecializationDescription()
    {
        return TrainingSpecialization switch
        {
            "InRing" => "Spécialiste technique en ring - Améliore workrate et in-ring skills",
            "Promo" => "Coach de promo et charisma - Améliore entertainment et mic skills",
            "Strength" => "Coach physique - Améliore force, endurance, prévention blessures",
            "AllRound" => "Formateur polyvalent - Améliore tous les aspects (moins efficace)",
            _ => "Non défini"
        };
    }

    /// <summary>
    /// Calcule le taux d'occupation (0-100%)
    /// </summary>
    public int CalculateOccupancyRate()
    {
        return (int)((CurrentStudents / (double)MaxStudentCapacity) * 100);
    }

    /// <summary>
    /// Détermine si le trainer est surchargé (>80% capacité)
    /// Un trainer surchargé est moins efficace
    /// </summary>
    public bool IsOverloaded() => CalculateOccupancyRate() >= 80;

    /// <summary>
    /// Calcule le bonus de progression effectif (ajusté si surchargé)
    /// </summary>
    public int CalculateEffectiveProgressionBonus()
    {
        var baseBonus = ProgressionBonus;

        // Réduire le bonus si surchargé
        if (IsOverloaded())
        {
            var overloadPenalty = (CalculateOccupancyRate() - 80) / 2;
            baseBonus = Math.Max(0, baseBonus - overloadPenalty);
        }

        return baseBonus;
    }

    /// <summary>
    /// Retourne le niveau de réputation sous forme textuelle
    /// </summary>
    public string GetReputationLevel()
    {
        return Reputation switch
        {
            >= 90 => "Légende - Reconnu mondialement",
            >= 75 => "Excellent - Très respecté",
            >= 60 => "Bon - Respecté",
            >= 40 => "Correct - Connu",
            >= 20 => "Novice - Peu connu",
            _ => "Inconnu - Pas de réputation"
        };
    }
}

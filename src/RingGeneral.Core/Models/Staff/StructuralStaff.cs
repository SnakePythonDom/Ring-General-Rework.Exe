using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Staff;

/// <summary>
/// Représente un membre du staff structurel/transversal.
/// Impact à long terme sur la santé globale de la compagnie.
/// Inclut: Medical, PR, Finance, Scouting, Psychology, Legal.
/// </summary>
public sealed record StructuralStaff
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
    /// Score d'efficacité dans le rôle (0-100)
    /// Impacte directement les résultats dans le domaine
    /// </summary>
    [Range(0, 100)]
    public required int EfficiencyScore { get; init; }

    /// <summary>
    /// Score de proactivité (0-100)
    /// - 0-30: Réactif uniquement, attend les problèmes
    /// - 31-70: Mix proactif/réactif
    /// - 71-100: Très proactif, anticipe les problèmes
    /// </summary>
    [Range(0, 100)]
    public int ProactivityScore { get; init; } = 50;

    /// <summary>
    /// Domaine d'expertise spécifique au rôle
    /// </summary>
    [Required]
    public required string ExpertiseDomain { get; init; }

    /// <summary>
    /// Impact global sur la compagnie (calculé)
    /// </summary>
    public string GlobalImpactAreas { get; init; } = "";

    // === PROPRIÉTÉS SPÉCIFIQUES PAR TYPE ===

    /// <summary>
    /// [MEDICAL] Réduction du temps de blessure (0-50%)
    /// </summary>
    [Range(0, 50)]
    public int InjuryRecoveryBonus { get; init; }

    /// <summary>
    /// [MEDICAL] Capacité de prévention des blessures (0-100)
    /// </summary>
    [Range(0, 100)]
    public int InjuryPreventionScore { get; init; }

    /// <summary>
    /// [PR] Capacité de gestion de crise (0-100)
    /// </summary>
    [Range(0, 100)]
    public int CrisisManagementScore { get; init; }

    /// <summary>
    /// [PR] Bonus de réputation/image (0-30%)
    /// </summary>
    [Range(0, 30)]
    public int ReputationBonus { get; init; }

    /// <summary>
    /// [FINANCE] Capacité de négociation de deals TV (0-100)
    /// </summary>
    [Range(0, 100)]
    public int DealNegotiationScore { get; init; }

    /// <summary>
    /// [FINANCE] Réduction des coûts opérationnels (0-25%)
    /// </summary>
    [Range(0, 25)]
    public int CostReductionBonus { get; init; }

    /// <summary>
    /// [SCOUT] Capacité de découverte de talents (0-100)
    /// </summary>
    [Range(0, 100)]
    public int TalentDiscoveryScore { get; init; }

    /// <summary>
    /// [SCOUT] Réseau de contacts dans l'industrie (0-100)
    /// </summary>
    [Range(0, 100)]
    public int IndustryNetworkScore { get; init; }

    /// <summary>
    /// [PSYCHOLOGIST] Bonus de moral des workers (0-30)
    /// </summary>
    [Range(0, 30)]
    public int MoraleBonus { get; init; }

    /// <summary>
    /// [PSYCHOLOGIST] Capacité de résolution de conflits (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ConflictResolutionScore { get; init; }

    /// <summary>
    /// [LEGAL] Capacité de gestion des litiges (0-100)
    /// </summary>
    [Range(0, 100)]
    public int LitigationManagementScore { get; init; }

    /// <summary>
    /// [LEGAL] Capacité de négociation de contrats (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ContractNegotiationScore { get; init; }

    /// <summary>
    /// Nombre d'interventions réussies (historique)
    /// </summary>
    public int SuccessfulInterventions { get; init; }

    /// <summary>
    /// Nombre total d'interventions
    /// </summary>
    public int TotalInterventions { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le StructuralStaff respecte les contraintes métier
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

        if (string.IsNullOrWhiteSpace(ExpertiseDomain))
        {
            errorMessage = "ExpertiseDomain ne peut pas être vide";
            return false;
        }

        if (EfficiencyScore is < 0 or > 100)
        {
            errorMessage = "EfficiencyScore doit être entre 0 et 100";
            return false;
        }

        if (ProactivityScore is < 0 or > 100)
        {
            errorMessage = "ProactivityScore doit être entre 0 et 100";
            return false;
        }

        // Validation des bonus spécifiques
        if (InjuryRecoveryBonus is < 0 or > 50)
        {
            errorMessage = "InjuryRecoveryBonus doit être entre 0 et 50";
            return false;
        }

        if (ReputationBonus is < 0 or > 30)
        {
            errorMessage = "ReputationBonus doit être entre 0 et 30";
            return false;
        }

        if (CostReductionBonus is < 0 or > 25)
        {
            errorMessage = "CostReductionBonus doit être entre 0 et 25";
            return false;
        }

        if (MoraleBonus is < 0 or > 30)
        {
            errorMessage = "MoraleBonus doit être entre 0 et 30";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calcule le taux de succès des interventions (0-100%)
    /// </summary>
    public int CalculateSuccessRate()
    {
        if (TotalInterventions == 0) return 50; // Neutre si pas d'historique
        return (int)((SuccessfulInterventions / (double)TotalInterventions) * 100);
    }

    /// <summary>
    /// Calcule un score de performance global (0-100)
    /// Basé sur efficacité, proactivité, et taux de succès
    /// </summary>
    public int CalculatePerformanceScore()
    {
        var efficiencyWeight = EfficiencyScore * 0.4;
        var proactivityWeight = ProactivityScore * 0.3;
        var successRateWeight = CalculateSuccessRate() * 0.3;

        return (int)(efficiencyWeight + proactivityWeight + successRateWeight);
    }

    /// <summary>
    /// Retourne l'impact principal selon le domaine d'expertise
    /// </summary>
    public string GetPrimaryImpact()
    {
        return ExpertiseDomain switch
        {
            "Medical" => $"Récupération blessures: -{InjuryRecoveryBonus}% temps, Prévention: {InjuryPreventionScore}/100",
            "PR" => $"Gestion crise: {CrisisManagementScore}/100, Réputation: +{ReputationBonus}%",
            "Finance" => $"Négociation deals: {DealNegotiationScore}/100, Réduction coûts: -{CostReductionBonus}%",
            "Scouting" => $"Découverte talents: {TalentDiscoveryScore}/100, Réseau: {IndustryNetworkScore}/100",
            "Psychology" => $"Moral: +{MoraleBonus}, Résolution conflits: {ConflictResolutionScore}/100",
            "Legal" => $"Litiges: {LitigationManagementScore}/100, Contrats: {ContractNegotiationScore}/100",
            _ => "Impact non défini"
        };
    }

    /// <summary>
    /// Détermine si le staff est proactif
    /// </summary>
    public bool IsProactive() => ProactivityScore >= 60;

    /// <summary>
    /// Calcule la valeur ajoutée pour la compagnie (0-100)
    /// Score composite de tous les impacts
    /// </summary>
    public int CalculateValueAddedScore()
    {
        var baseValue = EfficiencyScore;

        // Ajouter bonus spécifiques selon domaine
        var domainBonus = ExpertiseDomain switch
        {
            "Medical" => InjuryRecoveryBonus + (InjuryPreventionScore / 5),
            "PR" => (CrisisManagementScore / 2) + ReputationBonus,
            "Finance" => (DealNegotiationScore / 2) + CostReductionBonus,
            "Scouting" => (TalentDiscoveryScore + IndustryNetworkScore) / 4,
            "Psychology" => MoraleBonus + (ConflictResolutionScore / 5),
            "Legal" => (LitigationManagementScore + ContractNegotiationScore) / 4,
            _ => 0
        };

        var totalValue = baseValue + domainBonus;
        return Math.Clamp(totalValue, 0, 100);
    }

    /// <summary>
    /// Retourne le niveau de proactivité sous forme textuelle
    /// </summary>
    public string GetProactivityLevel()
    {
        return ProactivityScore switch
        {
            >= 80 => "Très proactif - Anticipe tous les problèmes",
            >= 60 => "Proactif - Anticipe la plupart des problèmes",
            >= 40 => "Équilibré - Mix réactif/proactif",
            >= 20 => "Réactif - Intervient quand nécessaire",
            _ => "Passif - Attend que les problèmes s'aggravent"
        };
    }
}

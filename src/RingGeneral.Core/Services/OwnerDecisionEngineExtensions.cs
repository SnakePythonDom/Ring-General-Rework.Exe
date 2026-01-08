using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Core.Models.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Extensions du OwnerDecisionEngine pour intégrer la gestion des ères, brands, et staff.
/// Permet au propriétaire de prendre des décisions stratégiques sur ces systèmes.
/// </summary>
public static class OwnerDecisionEngineExtensions
{
    // ====================================================================
    // ERA TRANSITION APPROVAL
    // ====================================================================

    /// <summary>
    /// Détermine si le propriétaire approuve une transition d'ère proposée par le booker.
    /// </summary>
    public static async Task<(bool IsApproved, string Reason, int ApprovalScore)> ApproveEraTransitionAsync(
        this OwnerDecisionEngine engine,
        string ownerId,
        Era currentEra,
        EraType targetEraType,
        EraTransitionSpeed proposedSpeed,
        decimal transitionCost,
        decimal companyTreasury,
        int currentFanSatisfaction,
        IOwnerRepository ownerRepository,
        IEraRepository eraRepository)
    {
        var owner = await ownerRepository.GetOwnerByIdAsync(ownerId);
        if (owner == null)
            return (false, "Owner not found", 0);

        // Calculer le score d'approbation basé sur plusieurs facteurs
        var approvalScore = 50; // Score de base

        // 1. Facteur financier: coût de la transition
        var budgetImpact = (transitionCost / companyTreasury) * 100;
        if (budgetImpact > 30 && owner.FinancialPriority >= 70)
        {
            approvalScore -= 30; // Owner financier réticent aux transitions coûteuses
        }
        else if (budgetImpact <= 15)
        {
            approvalScore += 10; // Transition peu coûteuse
        }

        // 2. Facteur risque: vitesse de transition
        var riskLevel = proposedSpeed switch
        {
            EraTransitionSpeed.Brutal => 90,
            EraTransitionSpeed.Fast => 70,
            EraTransitionSpeed.Moderate => 50,
            EraTransitionSpeed.Slow => 30,
            EraTransitionSpeed.VerySlow => 10,
            _ => 50
        };

        var riskAcceptance = owner.WouldAcceptRisk(riskLevel) ? 10 : -20;
        approvalScore += riskAcceptance;

        // 3. Facteur satisfaction: situation actuelle des fans
        if (currentFanSatisfaction < 40 && owner.FanSatisfactionPriority >= 60)
        {
            approvalScore += 20; // Owner accepte le changement si fans mécontents
        }
        else if (currentFanSatisfaction >= 70)
        {
            approvalScore -= 15; // "Don't fix what isn't broken"
        }

        // 4. Facteur vision: alignement avec le type d'owner
        var visionAlignment = CalculateVisionAlignment(owner, currentEra.Type, targetEraType);
        approvalScore += visionAlignment;

        // 5. Compatibilité des ères
        var compatibility = currentEra.GetCompatibilityWith(targetEraType);
        if (compatibility >= 70)
        {
            approvalScore += 15; // Transition naturelle
        }
        else if (compatibility <= 30)
        {
            approvalScore -= 25; // Transition radicale
        }

        // Décision finale (seuil: 55 pour approbation)
        var isApproved = approvalScore >= 55;

        var reason = BuildApprovalReason(isApproved, approvalScore, budgetImpact, riskLevel,
            currentFanSatisfaction, compatibility);

        return (isApproved, reason, approvalScore);
    }

    /// <summary>
    /// Détermine la vitesse maximale de transition d'ère que l'owner accepterait.
    /// </summary>
    public static EraTransitionSpeed GetMaxAcceptableTransitionSpeed(this OwnerDecisionEngine engine, Owner owner)
    {
        // Owner avec haute tolérance au risque accepte transitions rapides
        if (owner.RiskTolerance >= 70)
            return EraTransitionSpeed.Fast;

        if (owner.RiskTolerance >= 50)
            return EraTransitionSpeed.Moderate;

        if (owner.RiskTolerance >= 30)
            return EraTransitionSpeed.Slow;

        return EraTransitionSpeed.VerySlow;
    }

    // ====================================================================
    // BRAND MANAGEMENT DECISIONS
    // ====================================================================

    /// <summary>
    /// Détermine si le propriétaire approuve la création d'un nouveau brand.
    /// </summary>
    public static async Task<(bool IsApproved, string Reason)> ApproveNewBrandCreationAsync(
        this OwnerDecisionEngine engine,
        string ownerId,
        string companyId,
        double proposedBudgetPerShow,
        decimal companyTreasury,
        int currentBrandCount,
        BrandObjective objective,
        IOwnerRepository ownerRepository,
        IBrandRepository brandRepository)
    {
        var owner = await ownerRepository.GetOwnerByIdAsync(ownerId);
        if (owner == null)
            return (false, "Owner not found");

        // Vérifier si la hiérarchie supporte multi-brand
        var hierarchy = await brandRepository.GetHierarchyByCompanyIdAsync(companyId);
        if (hierarchy == null)
            return (false, "Company hierarchy not found");

        // Si mono-brand et déjà un brand, refuser
        if (hierarchy.Type == CompanyHierarchyType.MonoBrand && currentBrandCount >= 1)
        {
            return (false, "Company hierarchy is MonoBrand - cannot create additional brands");
        }

        // Vérifier le budget
        var monthlyBudget = (decimal)(proposedBudgetPerShow * 4); // Approximation 4 shows/mois
        var budgetApproved = engine.ApprovesBudget(ownerId, monthlyBudget, companyTreasury);
        if (!budgetApproved)
        {
            return (false, $"Budget rejected - Monthly cost (${monthlyBudget:N0}) exceeds owner's threshold");
        }

        // Vérifier l'alignement avec les priorités de l'owner
        if (objective == BrandObjective.Development && owner.TalentDevelopmentFocus >= 60)
        {
            return (true, "Approved - Aligns with owner's talent development focus");
        }

        if (objective == BrandObjective.Flagship && owner.FinancialPriority >= 60)
        {
            return (true, "Approved - Flagship brand supports financial goals");
        }

        if (objective == BrandObjective.Experimental && owner.VisionType == "Creative")
        {
            return (true, "Approved - Experimental brand aligns with creative vision");
        }

        // Approval par défaut si budget OK
        return (true, "Approved - Budget acceptable and no conflicts");
    }

    /// <summary>
    /// Détermine si le propriétaire approuve la fermeture d'un brand.
    /// </summary>
    public static async Task<(bool IsApproved, string Reason)> ApproveBrandClosureAsync(
        this OwnerDecisionEngine engine,
        string ownerId,
        Brand brand,
        string closureReason,
        IOwnerRepository ownerRepository)
    {
        var owner = await ownerRepository.GetOwnerByIdAsync(ownerId);
        if (owner == null)
            return (false, "Owner not found");

        // Si brand déficitaire et owner financier, approuver immédiatement
        if (brand.Prestige < 30 && owner.FinancialPriority >= 70)
        {
            return (true, $"Approved - Low prestige brand ({brand.Prestige}/100) draining resources");
        }

        // Si flagship brand, très réticent à fermer
        if (brand.Objective == BrandObjective.Flagship)
        {
            return (false, "DENIED - Cannot close flagship brand without replacement");
        }

        // Owner créatif réticent à fermer experimental brands
        if (brand.Objective == BrandObjective.Experimental && owner.VisionType == "Creative")
        {
            return (false, "DENIED - Creative owner wants to preserve experimental brand");
        }

        // Par défaut, approuver si prestige < 40
        if (brand.Prestige < 40)
        {
            return (true, $"Approved - Brand underperforming (prestige: {brand.Prestige}/100)");
        }

        return (false, $"DENIED - Brand still viable (prestige: {brand.Prestige}/100)");
    }

    // ====================================================================
    // STAFF HIRING DECISIONS
    // ====================================================================

    /// <summary>
    /// Détermine si le propriétaire approuve l'embauche d'un staff member.
    /// </summary>
    public static async Task<(bool IsApproved, string Reason)> ApproveStaffHiringAsync(
        this OwnerDecisionEngine engine,
        string ownerId,
        StaffMember proposedStaff,
        decimal monthlySalary,
        decimal companyTreasury,
        int currentStaffCount,
        IOwnerRepository ownerRepository,
        IStaffRepository staffRepository)
    {
        var owner = await ownerRepository.GetOwnerByIdAsync(ownerId);
        if (owner == null)
            return (false, "Owner not found");

        // Vérifier le budget
        var annualCost = monthlySalary * 12;
        var budgetApproved = engine.ApprovesBudget(ownerId, annualCost, companyTreasury);
        if (!budgetApproved)
        {
            return (false, $"Budget rejected - Annual cost (${annualCost:N0}) exceeds threshold");
        }

        // Évaluer le staff basé sur département et priorités de l'owner
        var (isStrategic, strategicReason) = EvaluateStaffStrategicValue(owner, proposedStaff);

        if (isStrategic)
        {
            return (true, $"Approved - {strategicReason}");
        }

        // Si skill score élevé (80+), owner accepte généralement
        if (proposedStaff.SkillScore >= 80)
        {
            return (true, $"Approved - Exceptional talent (skill: {proposedStaff.SkillScore}/100)");
        }

        // Vérifier si pas déjà trop de staff
        if (currentStaffCount >= 50)
        {
            return (false, "DENIED - Staff roster at capacity (50+)");
        }

        // Par défaut, approuver si skill >= 60 et budget OK
        if (proposedStaff.SkillScore >= 60)
        {
            return (true, $"Approved - Competent hire (skill: {proposedStaff.SkillScore}/100)");
        }

        return (false, $"DENIED - Insufficient skill score ({proposedStaff.SkillScore}/100 < 60)");
    }

    /// <summary>
    /// Détermine si le propriétaire approuve le renvoi d'un staff member.
    /// </summary>
    public static async Task<(bool IsApproved, string Reason)> ApproveStaffTerminationAsync(
        this OwnerDecisionEngine engine,
        string ownerId,
        StaffMember staff,
        string terminationReason,
        int performanceScore,
        IOwnerRepository ownerRepository)
    {
        var owner = await ownerRepository.GetOwnerByIdAsync(ownerId);
        if (owner == null)
            return (false, "Owner not found");

        // Si performance catastrophique (< 30), approuver immédiatement
        if (performanceScore < 30)
        {
            return (true, $"Approved - Poor performance ({performanceScore}/100)");
        }

        // Owner financier approuve facilement pour réduire coûts
        if (owner.FinancialPriority >= 70 && performanceScore < 50)
        {
            return (true, $"Approved - Cost reduction (performance: {performanceScore}/100)");
        }

        // Owner avec haute priorité talent development réticent à virer
        if (owner.TalentDevelopmentFocus >= 70 && performanceScore >= 50)
        {
            return (false, "DENIED - Owner values talent retention");
        }

        // Par défaut, approuver si performance < 40
        if (performanceScore < 40)
        {
            return (true, $"Approved - Underperforming ({performanceScore}/100)");
        }

        return (false, $"DENIED - Staff still performing adequately ({performanceScore}/100)");
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    /// <summary>
    /// Calcule l'alignement entre la vision de l'owner et le changement d'ère.
    /// </summary>
    private static int CalculateVisionAlignment(Owner owner, EraType currentEra, EraType targetEra)
    {
        var alignment = 0;

        // Owner Creative préfère ères créatives
        if (owner.VisionType == "Creative")
        {
            if (targetEra is EraType.Entertainment or EraType.LuchaLibre)
                alignment += 15;
            if (targetEra is EraType.Mainstream or EraType.SportsEntertainment)
                alignment -= 10;
        }

        // Owner Business préfère ères mainstream
        if (owner.VisionType == "Business")
        {
            if (targetEra is EraType.Mainstream or EraType.SportsEntertainment)
                alignment += 15;
            if (targetEra is EraType.Hardcore or EraType.Experimental)
                alignment -= 10;
        }

        // Owner Balanced neutre
        return alignment;
    }

    /// <summary>
    /// Construit le message de raison pour l'approbation/rejet de transition d'ère.
    /// </summary>
    private static string BuildApprovalReason(
        bool isApproved,
        int score,
        decimal budgetImpact,
        int riskLevel,
        int fanSatisfaction,
        int compatibility)
    {
        if (isApproved)
        {
            return $"APPROVED (score: {score}/100) - Budget impact: {budgetImpact:F1}%, " +
                   $"Risk: {riskLevel}/100, Fan satisfaction: {fanSatisfaction}/100, " +
                   $"Era compatibility: {compatibility}/100";
        }
        else
        {
            var reasons = new List<string>();
            if (budgetImpact > 30) reasons.Add("high cost");
            if (riskLevel > 70) reasons.Add("excessive risk");
            if (fanSatisfaction >= 70) reasons.Add("fans satisfied with current era");
            if (compatibility <= 30) reasons.Add("incompatible eras");

            var reasonStr = reasons.Any() ? string.Join(", ", reasons) : "low approval score";
            return $"DENIED (score: {score}/100) - Reasons: {reasonStr}";
        }
    }

    /// <summary>
    /// Évalue si un staff member a une valeur stratégique pour l'owner.
    /// </summary>
    private static (bool IsStrategic, string Reason) EvaluateStaffStrategicValue(
        Owner owner,
        StaffMember staff)
    {
        // Owner avec haute priorité financière valorise Financial staff
        if (owner.FinancialPriority >= 70 && staff.Department == StaffDepartment.Structural)
        {
            return (true, "Financial owner values structural staff");
        }

        // Owner avec haute priorité talent development valorise Trainers
        if (owner.TalentDevelopmentFocus >= 70 && staff.Department == StaffDepartment.Training)
        {
            return (true, "Talent-focused owner values trainers");
        }

        // Owner Creative valorise Creative staff
        if (owner.VisionType == "Creative" && staff.Department == StaffDepartment.Creative)
        {
            return (true, "Creative owner values creative team");
        }

        return (false, "No strategic alignment");
    }
}

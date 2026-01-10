using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.ChildCompany;
using RingGeneral.Core.Models.Staff;
using RingGeneral.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de partage de staff - optimise et valide les arrangements de partage.
/// Responsable de proposer des solutions optimales et de calculer les coûts.
/// </summary>
public sealed class StaffSharingEngine
{
    private readonly IChildCompanyStaffService _staffService;
    private readonly IStaffRepository _staffRepository;
    private readonly IChildCompanyRepository _childCompanyRepository;

    public StaffSharingEngine(
        IChildCompanyStaffService staffService,
        IStaffRepository staffRepository,
        IChildCompanyRepository childCompanyRepository)
    {
        _staffService = staffService ?? throw new ArgumentNullException(nameof(staffService));
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _childCompanyRepository = childCompanyRepository ?? throw new ArgumentNullException(nameof(childCompanyRepository));
    }

    // ====================================================================
    // SHARING PROPOSAL GENERATION
    // ====================================================================

    /// <summary>
    /// Propose un arrangement de partage optimal pour un objectif donné
    /// </summary>
    /// <param name="staff">Membre du staff à partager</param>
    /// <param name="childCompany">Child Company cible</param>
    /// <param name="objective">Objectif du partage</param>
    /// <returns>Proposition optimisée</returns>
    public async Task<StaffSharingProposal> ProposeSharingArrangementAsync(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective)
    {
        if (staff is null) throw new ArgumentNullException(nameof(staff));
        if (childCompany is null) throw new ArgumentNullException(nameof(childCompany));

        // Calculer la disponibilité actuelle du staff
        var availability = await _staffService.CalculateStaffAvailabilityAsync(staff.StaffId, DateTime.Now);

        // Déterminer le type d'assignation recommandé
        var recommendedType = DetermineRecommendedAssignmentType(staff, childCompany, objective, availability);

        // Calculer le pourcentage de temps optimal
        var optimalTimePercentage = CalculateOptimalTimePercentage(staff, childCompany, objective, availability);

        // Calculer l'impact estimé
        var estimatedImpact = await EstimateImpactAsync(staff, childCompany, optimalTimePercentage);

        // Calculer le coût estimé
        var estimatedCost = CalculateEstimatedCost(staff, optimalTimePercentage, recommendedType);

        // Calculer le score de recommandation
        var recommendationScore = CalculateRecommendationScore(
            staff, childCompany, objective, availability, optimalTimePercentage, estimatedImpact);

        // Générer les raisons et risques
        var (reasons, risks) = GenerateRecommendationDetails(
            staff, childCompany, objective, availability, optimalTimePercentage);

        return new StaffSharingProposal(
            StaffId: staff.StaffId,
            ChildCompanyId: childCompany.ChildCompanyId,
            RecommendedAssignmentType: recommendedType.ToString(),
            RecommendedTimePercentage: optimalTimePercentage,
            EstimatedImpact: estimatedImpact,
            EstimatedMonthlyCost: estimatedCost,
            RecommendationScore: recommendationScore,
            RecommendationReasons: reasons,
            PotentialRisks: risks);
    }

    /// <summary>
    /// Génère plusieurs propositions alternatives pour un partage
    /// </summary>
    /// <param name="staff">Membre du staff</param>
    /// <param name="childCompany">Child Company</param>
    /// <param name="objective">Objectif</param>
    /// <returns>Liste de propositions alternatives</returns>
    public async Task<IReadOnlyList<StaffSharingProposal>> GenerateAlternativeProposalsAsync(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective)
    {
        var proposals = new List<StaffSharingProposal>();

        // Proposition principale
        var mainProposal = await ProposeSharingArrangementAsync(staff, childCompany, objective);
        proposals.Add(mainProposal);

        // Alternatives avec différents % temps
        var timeAlternatives = new[] { 0.2, 0.4, 0.6, 0.8 };
        foreach (var timePct in timeAlternatives.Where(t => Math.Abs(t - mainProposal.RecommendedTimePercentage) > 0.1))
        {
            var altProposal = await CreateAlternativeProposalAsync(
                staff, childCompany, objective, timePct, mainProposal.RecommendedAssignmentType);
            proposals.Add(altProposal);
        }

        // Trier par score de recommandation
        return proposals.OrderByDescending(p => p.RecommendationScore).ToList();
    }

    // ====================================================================
    // VALIDATION & CONSTRAINT CHECKING
    // ====================================================================

    /// <summary>
    /// Valide un arrangement de partage proposé
    /// </summary>
    /// <param name="arrangement">Arrangement à valider</param>
    /// <returns>Résultat de validation</returns>
    public async Task<ValidationResult> ValidateSharingArrangementAsync(StaffSharingArrangement arrangement)
    {
        if (arrangement is null) throw new ArgumentNullException(nameof(arrangement));

        var errors = new List<string>();
        var warnings = new List<string>();
        var validationScore = 100;

        try
        {
            // Validation de base
            if (string.IsNullOrWhiteSpace(arrangement.StaffId))
            {
                errors.Add("StaffId requis");
                validationScore = 0;
            }

            if (string.IsNullOrWhiteSpace(arrangement.ChildCompanyId))
            {
                errors.Add("ChildCompanyId requis");
                validationScore = 0;
            }

            if (arrangement.TimePercentage < 0.1 || arrangement.TimePercentage > 1.0)
            {
                errors.Add("TimePercentage doit être entre 0.1 et 1.0");
                validationScore -= 20;
            }

            if (validationScore == 0)
            {
                return new ValidationResult(false, errors, warnings, 0);
            }

            // Récupérer les entités
            var staff = await _staffRepository.GetStaffMemberByIdAsync(arrangement.StaffId);
            var childCompany = await _childCompanyRepository.GetChildCompanyByIdAsync(arrangement.ChildCompanyId);

            if (staff is null)
            {
                errors.Add("Membre du staff introuvable");
                return new ValidationResult(false, errors, warnings, 0);
            }

            if (childCompany is null)
            {
                errors.Add("Child Company introuvable");
                return new ValidationResult(false, errors, warnings, 0);
            }

            // Validation métier via le service
            var assignment = new ChildCompanyStaffAssignment(
                AssignmentId: Guid.NewGuid().ToString(),
                StaffId: arrangement.StaffId,
                ChildCompanyId: arrangement.ChildCompanyId,
                AssignmentType: Enum.Parse<StaffAssignmentType>(arrangement.AssignmentType),
                TimePercentage: arrangement.TimePercentage,
                StartDate: arrangement.StartDate,
                EndDate: arrangement.EndDate,
                MissionObjective: arrangement.MissionObjective,
                CreatedAt: DateTime.Now);

            var serviceValidation = await _staffService.ValidateStaffSharingRulesAsync(assignment);

            errors.AddRange(serviceValidation.Errors);
            warnings.AddRange(serviceValidation.Warnings);
            validationScore = Math.Min(validationScore, serviceValidation.ValidationScore);

            // Validation spécifique au moteur
            var engineValidation = await ValidateArrangementConstraintsAsync(arrangement, staff, childCompany);
            errors.AddRange(engineValidation.Errors);
            warnings.AddRange(engineValidation.Warnings);
            validationScore = Math.Min(validationScore, engineValidation.ValidationScore);

        }
        catch (Exception ex)
        {
            errors.Add($"Erreur de validation: {ex.Message}");
            validationScore = 0;
        }

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors,
            Warnings: warnings,
            ValidationScore: Math.Max(0, validationScore));
    }

    // ====================================================================
    // COST CALCULATION
    // ====================================================================

    /// <summary>
    /// Calcule le coût d'un arrangement de partage
    /// </summary>
    /// <param name="arrangement">Arrangement</param>
    /// <param name="duration">Durée (TimeSpan)</param>
    /// <returns>Détail des coûts</returns>
    public async Task<SharingCostBreakdown> CalculateSharingCostsAsync(
        StaffSharingArrangement arrangement,
        TimeSpan duration)
    {
        if (arrangement is null) throw new ArgumentNullException(nameof(arrangement));

        var staff = await _staffRepository.GetStaffMemberByIdAsync(arrangement.StaffId);
        if (staff is null)
        {
            throw new InvalidOperationException($"Staff {arrangement.StaffId} introuvable");
        }

        return CalculateSharingCosts(staff, arrangement, duration);
    }

    /// <summary>
    /// Calcule le coût mensuel d'un arrangement
    /// </summary>
    /// <param name="arrangement">Arrangement</param>
    /// <returns>Coût mensuel</returns>
    public async Task<decimal> CalculateMonthlyCostAsync(StaffSharingArrangement arrangement)
    {
        var breakdown = await CalculateSharingCostsAsync(arrangement, TimeSpan.FromDays(30));
        return breakdown.TotalMonthlyCost;
    }

    // ====================================================================
    // OPTIMIZATION METHODS
    // ====================================================================

    /// <summary>
    /// Optimise automatiquement les assignations pour une Child Company
    /// </summary>
    /// <param name="childCompanyId">ID de la Child Company</param>
    /// <param name="availableStaff">Staff disponible</param>
    /// <param name="objectives">Objectifs à atteindre</param>
    /// <returns>Plan d'assignation optimisé</returns>
    public async Task<OptimizedAssignmentPlan> OptimizeAssignmentsForChildCompanyAsync(
        string childCompanyId,
        IReadOnlyList<StaffMember> availableStaff,
        IReadOnlyList<SharingObjective> objectives)
    {
        var childCompany = await _childCompanyRepository.GetChildCompanyByIdAsync(childCompanyId);
        if (childCompany is null)
        {
            throw new InvalidOperationException($"Child Company {childCompanyId} introuvable");
        }

        var proposals = new List<StaffSharingProposal>();
        var totalBudget = childCompany.MonthlyBudget;

        // Générer des propositions pour chaque staff/objectif
        foreach (var staff in availableStaff.Where(s => s.CanBeShared))
        {
            foreach (var objective in objectives)
            {
                try
                {
                    var proposal = await ProposeSharingArrangementAsync(staff, childCompany, objective);
                    if (proposal.RecommendationScore > 60) // Seulement les bonnes propositions
                    {
                        proposals.Add(proposal);
                    }
                }
                catch
                {
                    // Ignorer les propositions qui échouent
                }
            }
        }

        // Sélectionner les meilleures propositions dans les limites budgétaires
        var selectedProposals = SelectOptimalProposals(proposals, totalBudget);

        return new OptimizedAssignmentPlan(
            ChildCompanyId: childCompanyId,
            SelectedProposals: selectedProposals,
            TotalEstimatedCost: selectedProposals.Sum(p => p.EstimatedMonthlyCost),
            ExpectedEfficiencyGain: CalculateExpectedEfficiencyGain(selectedProposals),
            OptimizationScore: CalculateOptimizationScore(selectedProposals, totalBudget));
    }

    // ====================================================================
    // PRIVATE HELPER METHODS
    // ====================================================================

    private StaffAssignmentType DetermineRecommendedAssignmentType(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective,
        StaffAvailabilityResult availability)
    {
        // Logique de détermination du type d'assignation
        if (objective.DurationDays <= 30)
            return StaffAssignmentType.TemporarySupport;

        if (staff.MobilityRating == StaffMobilityRating.High && availability.AvailabilityPercentage >= 0.6)
            return StaffAssignmentType.DedicatedRotation;

        return StaffAssignmentType.PartTime;
    }

    private double CalculateOptimalTimePercentage(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective,
        StaffAvailabilityResult availability)
    {
        var basePercentage = objective switch
        {
            { Priority: Priority.High } => 0.7,
            { Priority: Priority.Medium } => 0.5,
            { Priority: Priority.Low } => 0.3,
            _ => 0.4
        };

        // Ajuster selon la disponibilité
        var adjustedPercentage = Math.Min(basePercentage, availability.AvailabilityPercentage);

        // Ajuster selon la mobilité
        var mobilityMultiplier = staff.MobilityRating switch
        {
            StaffMobilityRating.High => 1.0,
            StaffMobilityRating.Medium => 0.8,
            StaffMobilityRating.Low => 0.6,
            _ => 0.7
        };

        return Math.Min(0.8, adjustedPercentage * mobilityMultiplier);
    }

    private async Task<StaffProgressionImpact> EstimateImpactAsync(
        StaffMember staff,
        ChildCompany childCompany,
        double timePercentage)
    {
        // Estimation simplifiée - en production, utiliserait le service complet
        return await _staffService.CalculateProgressionImpactAsync(
            staff.StaffId,
            childCompany.ChildCompanyId);
    }

    private decimal CalculateEstimatedCost(
        StaffMember staff,
        double timePercentage,
        StaffAssignmentType assignmentType)
    {
        var baseMonthlyCost = staff.GetMonthlyCost();

        // Ajustements selon le type d'assignation
        var typeMultiplier = assignmentType switch
        {
            StaffAssignmentType.DedicatedRotation => 1.2m, // +20% pour rotation dédiée
            StaffAssignmentType.TemporarySupport => 1.1m,  // +10% pour support temporaire
            StaffAssignmentType.PartTime => 1.0m,           // Coût normal
            _ => 1.0m
        };

        // Ajustement selon le temps
        var timeMultiplier = (decimal)timePercentage;

        return (decimal)baseMonthlyCost * typeMultiplier * timeMultiplier;
    }

    private int CalculateRecommendationScore(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective,
        StaffAvailabilityResult availability,
        double timePercentage,
        StaffProgressionImpact impact)
    {
        var score = 50; // Score de base

        // Facteur compétence (0-20 points)
        score += (int)(staff.SkillScore * 0.2);

        // Facteur disponibilité (0-15 points)
        score += (int)(availability.AvailabilityPercentage * 15);

        // Facteur mobilité (0-10 points)
        score += staff.MobilityRating switch
        {
            StaffMobilityRating.High => 10,
            StaffMobilityRating.Medium => 5,
            StaffMobilityRating.Low => 0,
            _ => 2
        };

        // Facteur priorité (0-5 points)
        score += objective.Priority switch
        {
            Priority.High => 5,
            Priority.Medium => 3,
            Priority.Low => 1,
            _ => 2
        };

        return Math.Min(100, Math.Max(0, score));
    }

    private (IReadOnlyList<string> Reasons, IReadOnlyList<string> Risks) GenerateRecommendationDetails(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective,
        StaffAvailabilityResult availability,
        double timePercentage)
    {
        var reasons = new List<string>();
        var risks = new List<string>();

        // Raisons positives
        if (staff.SkillScore >= 80)
            reasons.Add($"Excellente compétence ({staff.SkillScore}/100)");

        if (availability.AvailabilityPercentage >= 0.7)
            reasons.Add("Bonne disponibilité");

        if (staff.MobilityRating == StaffMobilityRating.High)
            reasons.Add("Haute mobilité - adaptable");

        // Risques potentiels
        if (availability.AvailabilityPercentage < 0.3)
            risks.Add("Disponibilité limitée - peut causer de la fatigue");

        if (staff.MobilityRating == StaffMobilityRating.Low)
            risks.Add("Faible mobilité - peut nécessiter adaptation");

        if (timePercentage > 0.6)
            risks.Add("Charge de travail élevée - risque de burnout");

        return (reasons, risks);
    }

    private async Task<ValidationResult> ValidateArrangementConstraintsAsync(
        StaffSharingArrangement arrangement,
        StaffMember staff,
        ChildCompany childCompany)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var score = 100;

        // Validation temporelle
        if (arrangement.EndDate.HasValue && arrangement.EndDate.Value <= arrangement.StartDate)
        {
            errors.Add("Date de fin doit être postérieure à la date de début");
            score -= 30;
        }

        // Validation durée pour support temporaire
        if (arrangement.AssignmentType == StaffAssignmentType.TemporarySupport.ToString() &&
            arrangement.EndDate.HasValue &&
            (arrangement.EndDate.Value - arrangement.StartDate).TotalDays > 180)
        {
            warnings.Add("Support temporaire > 6 mois - envisager rotation dédiée");
            score -= 10;
        }

        // Validation budget Child Company
        var estimatedCost = CalculateEstimatedCost(staff, arrangement.TimePercentage,
            Enum.Parse<StaffAssignmentType>(arrangement.AssignmentType));

        if (estimatedCost > childCompany.MonthlyBudget * 0.3m)
        {
            warnings.Add($"Coût élevé: {estimatedCost:C0}/mois (>30% du budget)");
            score -= 15;
        }

        return new ValidationResult(errors.Count == 0, errors, warnings, score);
    }

    private SharingCostBreakdown CalculateSharingCosts(
        StaffMember staff,
        StaffSharingArrangement arrangement,
        TimeSpan duration)
    {
        var monthlyBaseCost = staff.GetMonthlyCost();

        // Ajustements selon le type
        var typeMultiplier = arrangement.AssignmentType switch
        {
            var t when t == StaffAssignmentType.DedicatedRotation.ToString() => 1.2m,
            var t when t == StaffAssignmentType.TemporarySupport.ToString() => 1.1m,
            _ => 1.0m
        };

        // Ajustements selon le temps
        var timeMultiplier = (decimal)arrangement.TimePercentage;

        // Calculs détaillés
        var baseCost = (decimal)monthlyBaseCost * timeMultiplier;
        var typeAdjustment = baseCost * (typeMultiplier - 1.0m);
        var totalMonthly = baseCost * typeMultiplier;

        // Prorata selon la durée
        var durationRatio = (decimal)(duration.TotalDays / 30.0);
        var proratedCost = totalMonthly * durationRatio;

        return new SharingCostBreakdown(
            BaseMonthlyCost: (decimal)monthlyBaseCost,
            TimeAdjustedCost: baseCost,
            TypeAdjustedCost: typeAdjustment,
            TotalMonthlyCost: totalMonthly,
            ProratedCost: proratedCost,
            Duration: duration);
    }

    private IReadOnlyList<StaffSharingProposal> SelectOptimalProposals(
        List<StaffSharingProposal> proposals,
        decimal budgetLimit)
    {
        // Tri par score décroissant
        var sorted = proposals.OrderByDescending(p => p.RecommendationScore).ToList();

        var selected = new List<StaffSharingProposal>();
        var totalCost = 0m;

        foreach (var proposal in sorted)
        {
            if (totalCost + proposal.EstimatedMonthlyCost <= budgetLimit)
            {
                selected.Add(proposal);
                totalCost += proposal.EstimatedMonthlyCost;
            }
        }

        return selected;
    }

    private double CalculateExpectedEfficiencyGain(IReadOnlyList<StaffSharingProposal> proposals)
    {
        if (!proposals.Any()) return 0;

        // Calcul simplifié du gain d'efficacité
        var totalImpact = proposals.Sum(p => p.EstimatedImpact.AttributeBonuses.Values.Sum());
        var totalCost = proposals.Sum(p => (double)p.EstimatedMonthlyCost);

        return totalCost > 0 ? (totalImpact / totalCost) * 100 : 0;
    }

    private int CalculateOptimizationScore(
        IReadOnlyList<StaffSharingProposal> proposals,
        decimal budgetLimit)
    {
        if (!proposals.Any()) return 0;

        var totalCost = proposals.Sum(p => p.EstimatedMonthlyCost);
        var budgetUtilization = (double)(totalCost / budgetLimit);

        // Score basé sur l'utilisation du budget et la qualité des propositions
        var utilizationScore = budgetUtilization <= 1.0 ? 100 : 50; // Pénalité si dépassement
        var qualityScore = proposals.Average(p => p.RecommendationScore);

        return (int)((utilizationScore + qualityScore) / 2);
    }

    private async Task<StaffSharingProposal> CreateAlternativeProposalAsync(
        StaffMember staff,
        ChildCompany childCompany,
        SharingObjective objective,
        double timePercentage,
        string assignmentType)
    {
        // Créer une proposition alternative avec un % temps différent
        var availability = await _staffService.CalculateStaffAvailabilityAsync(staff.StaffId, DateTime.Now);
        var impact = await EstimateImpactAsync(staff, childCompany, timePercentage);
        var cost = CalculateEstimatedCost(staff, timePercentage, Enum.Parse<StaffAssignmentType>(assignmentType));

        var recommendationScore = CalculateRecommendationScore(
            staff, childCompany, objective, availability, timePercentage, impact) - 10; // Légère pénalité pour alternative

        var (reasons, risks) = GenerateRecommendationDetails(
            staff, childCompany, objective, availability, timePercentage);

        reasons = reasons.Concat(new[] { $"Alternative: {timePercentage:P0} temps" }).ToList();

        return new StaffSharingProposal(
            StaffId: staff.StaffId,
            ChildCompanyId: childCompany.ChildCompanyId,
            RecommendedAssignmentType: assignmentType,
            RecommendedTimePercentage: timePercentage,
            EstimatedImpact: impact,
            EstimatedMonthlyCost: cost,
            RecommendationScore: recommendationScore,
            RecommendationReasons: reasons,
            PotentialRisks: risks);
    }
}
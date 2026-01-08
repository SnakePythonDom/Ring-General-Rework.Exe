using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Intégration entre le StaffProposalService et le StorylineService.
/// Permet au staff créatif de proposer des modifications aux storylines existantes.
/// </summary>
public sealed class StaffStorylineIntegration
{
    private readonly StorylineService _storylineService;
    private readonly StaffProposalService _staffProposalService;
    private readonly IStaffRepository _staffRepository;
    private readonly IStaffCompatibilityRepository _compatibilityRepository;

    public StaffStorylineIntegration(
        StorylineService storylineService,
        StaffProposalService staffProposalService,
        IStaffRepository staffRepository,
        IStaffCompatibilityRepository compatibilityRepository)
    {
        _storylineService = storylineService;
        _staffProposalService = staffProposalService;
        _staffRepository = staffRepository;
        _compatibilityRepository = compatibilityRepository;
    }

    // ====================================================================
    // STAFF PROPOSAL GENERATION
    // ====================================================================

    /// <summary>
    /// Génère une proposition créative d'un staff pour modifier une storyline existante.
    /// </summary>
    public async Task<CreativeProposal?> GenerateStorylineProposalAsync(
        string staffId,
        string bookerId,
        StorylineInfo currentStoryline)
    {
        var staff = await _staffRepository.GetCreativeStaffByIdAsync(staffId);
        if (staff == null)
            return null;

        // Générer une proposition basée sur le staff
        var proposal = new CreativeProposal
        {
            ProposalId = Guid.NewGuid().ToString(),
            StaffId = staffId,
            BookerId = bookerId,
            ProposalType = "StorylineModification",
            TargetEntityId = currentStoryline.StorylineId,
            Title = GenerateProposalTitle(staff, currentStoryline),
            Description = GenerateProposalDescription(staff, currentStoryline),
            QualityScore = CalculateProposalQuality(staff),
            Originality = staff.CreativityScore,
            RiskLevel = CalculateProposalRisk(staff),
            WorkerTypeBias = staff.WorkerBias,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        return proposal;
    }

    /// <summary>
    /// Évalue et applique une proposition de modification de storyline.
    /// Si acceptée, modifie la storyline. Si rejetée, crée un conflit potentiel.
    /// </summary>
    public async Task<(bool IsAccepted, StorylineInfo? UpdatedStoryline, string Reason)>
        EvaluateAndApplyProposalAsync(
            CreativeProposal proposal,
            StorylineInfo currentStoryline,
            string bookerId)
    {
        // Évaluer la proposition
        var compatibility = await _compatibilityRepository.GetCompatibilityAsync(proposal.StaffId, bookerId);
        if (compatibility == null)
        {
            return (false, null, "Compatibility not calculated - cannot evaluate proposal");
        }

        var evaluation = _staffProposalService.EvaluateProposal(proposal, compatibility);

        if (!evaluation.IsAccepted)
        {
            // Incrémenter le compteur de rejets
            await IncrementRejectionHistoryAsync(proposal.StaffId, bookerId);
            return (false, null, evaluation.Reason);
        }

        // Appliquer la proposition à la storyline
        var updatedStoryline = ApplyProposalToStoryline(proposal, currentStoryline, evaluation.AcceptanceScore);

        // Incrémenter le compteur de collaborations réussies
        if (compatibility != null)
        {
            await _compatibilityRepository.IncrementCollaborationAsync(compatibility.CompatibilityId, successful: true);
        }

        return (true, updatedStoryline, evaluation.Reason);
    }

    /// <summary>
    /// Détecte si un staff créatif a "ruiné" une storyline par ses modifications.
    /// </summary>
    public async Task<(bool IsRuined, int DamageLevel, List<string> Reasons)> DetectStorylineRuinAsync(
        string staffId,
        string bookerId,
        StorylineInfo storylineBefore,
        StorylineInfo storylineAfter)
    {
        var staff = await _staffRepository.GetCreativeStaffByIdAsync(staffId);
        if (staff == null || !staff.CanRuinStorylines)
            return (false, 0, new List<string>());

        var compatibility = await _compatibilityRepository.GetCompatibilityAsync(staffId, bookerId);
        if (compatibility == null || !compatibility.IsDangerous())
            return (false, 0, new List<string>());

        var reasons = new List<string>();
        var damageLevel = 0;

        // Vérifier la dégradation de heat
        var heatDelta = storylineAfter.Heat - storylineBefore.Heat;
        if (heatDelta < -20)
        {
            damageLevel += 30;
            reasons.Add($"Severe heat loss ({heatDelta} points)");
        }

        // Vérifier la phase (régression = mauvais signe)
        if (storylineAfter.Phase < storylineBefore.Phase)
        {
            damageLevel += 25;
            reasons.Add($"Storyline regressed from {storylineBefore.Phase} to {storylineAfter.Phase}");
        }

        // Vérifier le statut
        if (storylineAfter.Status == StorylineStatus.Cancelled && storylineBefore.Status == StorylineStatus.Active)
        {
            damageLevel += 45;
            reasons.Add("Storyline cancelled due to poor execution");
        }

        // Si compatibilité très dangereuse (≤20), amplifier les dégâts
        if (compatibility.OverallScore <= 20)
        {
            damageLevel = (int)(damageLevel * 1.5);
            reasons.Add($"Critical incompatibility (score: {compatibility.OverallScore}/100)");
        }

        var isRuined = damageLevel >= 50;

        if (isRuined)
        {
            // Incrémenter l'historique de conflits
            await _compatibilityRepository.IncrementConflictHistoryAsync(compatibility.CompatibilityId);
        }

        return (isRuined, damageLevel, reasons);
    }

    // ====================================================================
    // STAFF AUTOMATIC PROPOSALS
    // ====================================================================

    /// <summary>
    /// Génère automatiquement des propositions pour tous les staff créatifs actifs.
    /// À appeler périodiquement (ex: chaque semaine de jeu).
    /// </summary>
    public async Task<List<CreativeProposal>> GenerateAutomaticProposalsAsync(
        string companyId,
        string bookerId,
        List<StorylineInfo> activeStorylines)
    {
        var creativeStaff = await _staffRepository.GetCreativeStaffByCompanyIdAsync(companyId);
        var proposals = new List<CreativeProposal>();

        foreach (var staff in creativeStaff)
        {
            // Probabilité de proposition basée sur créativité et consistance
            var proposalProbability = (staff.CreativityScore + staff.ConsistencyScore) / 2;
            var random = new Random();

            if (random.Next(100) < proposalProbability / 2) // 50% du score en probabilité
            {
                // Sélectionner une storyline aléatoire
                if (activeStorylines.Any())
                {
                    var targetStoryline = activeStorylines[random.Next(activeStorylines.Count)];
                    var proposal = await GenerateStorylineProposalAsync(
                        staff.StaffId,
                        bookerId,
                        targetStoryline);

                    if (proposal != null)
                    {
                        proposals.Add(proposal);
                    }
                }
            }
        }

        return proposals;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    /// <summary>
    /// Génère un titre pour la proposition basé sur le style du staff.
    /// </summary>
    private string GenerateProposalTitle(CreativeStaff staff, StorylineInfo storyline)
    {
        var titles = new[]
        {
            $"Twist dramatique pour '{storyline.Nom}'",
            $"Nouvelle direction pour '{storyline.Nom}'",
            $"Intensification de '{storyline.Nom}'",
            $"Résolution alternative pour '{storyline.Nom}'",
            $"Complication supplémentaire pour '{storyline.Nom}'"
        };

        var random = new Random();
        return titles[random.Next(titles.Length)];
    }

    /// <summary>
    /// Génère une description de proposition.
    /// </summary>
    private string GenerateProposalDescription(CreativeStaff staff, StorylineInfo storyline)
    {
        var creativityLevel = staff.CreativityScore >= 70 ? "audacieuse" :
                             staff.CreativityScore >= 50 ? "solide" : "conservatrice";

        return $"Proposition {creativityLevel} du staff créatif pour modifier la storyline '{storyline.Nom}'. " +
               $"Basée sur un biais {staff.WorkerBias} avec un niveau de créativité de {staff.CreativityScore}/100.";
    }

    /// <summary>
    /// Calcule la qualité d'une proposition basée sur le staff.
    /// </summary>
    private int CalculateProposalQuality(CreativeStaff staff)
    {
        // Formule: 60% skill + 25% creativity + 15% consistency
        return (int)(
            staff.SkillScore * 0.60 +
            staff.CreativityScore * 0.25 +
            staff.ConsistencyScore * 0.15
        );
    }

    /// <summary>
    /// Calcule le niveau de risque d'une proposition.
    /// </summary>
    private int CalculateProposalRisk(CreativeStaff staff)
    {
        // Créativité élevée + consistance faible = risque élevé
        var riskFromCreativity = staff.CreativityScore;
        var riskReductionFromConsistency = staff.ConsistencyScore;

        return Math.Clamp(riskFromCreativity - riskReductionFromConsistency / 2, 0, 100);
    }

    /// <summary>
    /// Applique une proposition acceptée à une storyline.
    /// </summary>
    private StorylineInfo ApplyProposalToStoryline(
        CreativeProposal proposal,
        StorylineInfo currentStoryline,
        int acceptanceScore)
    {
        // Calculer l'impact sur le heat basé sur l'acceptance score
        var heatDelta = (acceptanceScore - 50) / 5; // -10 à +10

        var updatedStoryline = currentStoryline with
        {
            Heat = Math.Clamp(currentStoryline.Heat + heatDelta, 0, 100),
            Resume = $"{currentStoryline.Resume}\n[Staff Proposal Applied: {proposal.Title}]"
        };

        // Si proposition de très haute qualité (90+), potentiel avancement de phase
        if (acceptanceScore >= 90 && currentStoryline.Phase != StorylinePhase.Fallout)
        {
            updatedStoryline = _storylineService.Avancer(updatedStoryline);
        }

        return updatedStoryline;
    }

    /// <summary>
    /// Incrémente l'historique de rejets pour ajuster la compatibilité future.
    /// </summary>
    private async Task IncrementRejectionHistoryAsync(string staffId, string bookerId)
    {
        var compatibility = await _compatibilityRepository.GetCompatibilityAsync(staffId, bookerId);
        if (compatibility != null)
        {
            await _compatibilityRepository.IncrementCollaborationAsync(compatibility.CompatibilityId, successful: false);
        }
    }
}

/// <summary>
/// Modèle pour une proposition créative du staff.
/// </summary>
public sealed record CreativeProposal
{
    public required string ProposalId { get; init; }
    public required string StaffId { get; init; }
    public required string BookerId { get; init; }
    public required string ProposalType { get; init; } // "StorylineModification", "AngleIdea", "GimmickChange"
    public required string TargetEntityId { get; init; } // StorylineId, WorkerId, etc.
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required int QualityScore { get; init; } // 0-100
    public required int Originality { get; init; } // 0-100
    public required int RiskLevel { get; init; } // 0-100
    public required WorkerTypeBias WorkerTypeBias { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string Status { get; init; } // "Pending", "Accepted", "Rejected"
}

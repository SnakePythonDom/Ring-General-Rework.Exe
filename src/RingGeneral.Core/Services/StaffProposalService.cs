using System;
using System.Collections.Generic;
using System.Linq;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Staff;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de gestion des propositions créatives du staff.
/// Les créatifs proposent angles, gimmicks, et storylines sous autorité du Booker.
/// </summary>
public class StaffProposalService
{
    private readonly StaffCompatibilityCalculator _compatibilityCalculator;

    public StaffProposalService(StaffCompatibilityCalculator compatibilityCalculator)
    {
        _compatibilityCalculator = compatibilityCalculator;
    }

    /// <summary>
    /// Représente une proposition créative d'un staff
    /// </summary>
    public record CreativeProposal
    {
        public required string ProposalId { get; init; }
        public required string StaffId { get; init; }
        public required string BookerId { get; init; }
        public required string ProposalType { get; init; } // "Storyline", "Gimmick", "Angle", "Match"
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required int QualityScore { get; init; } // 0-100
        public required int Originality { get; init; } // 0-100
        public required int RiskLevel { get; init; } // 0-100
        public required string TargetWorkers { get; init; } // IDs séparés par virgules
        public required DateTime ProposedAt { get; init; }
        public string? BookerResponse { get; init; } // "Accepted", "Rejected", "Modified", "Pending"
        public string? RejectionReason { get; init; }
        public int BookerCompatibilityImpact { get; init; } // Impact de la compatibilité sur acceptation
    }

    /// <summary>
    /// Génère une proposition créative d'un staff
    /// </summary>
    public CreativeProposal GenerateProposal(
        CreativeStaff staff,
        StaffMember staffMember,
        string proposalType,
        string title,
        string description,
        List<string> targetWorkerIds,
        Booker booker)
    {
        // Calculer qualité basée sur skill du staff
        var qualityScore = CalculateProposalQuality(staff, staffMember);

        // Calculer originalité basée sur créativité
        var originality = CalculateOriginality(staff);

        // Calculer risque basé sur tolérance au risque
        var riskLevel = staff.CreativeRiskTolerance;

        // Impact de compatibilité sur l'acceptation
        var compatibilityImpact = CalculateCompatibilityImpact(staff, booker);

        return new CreativeProposal
        {
            ProposalId = $"proposal_{Guid.NewGuid():N}",
            StaffId = staff.StaffId,
            BookerId = booker.BookerId,
            ProposalType = proposalType,
            Title = title,
            Description = description,
            QualityScore = qualityScore,
            Originality = originality,
            RiskLevel = riskLevel,
            TargetWorkers = string.Join(",", targetWorkerIds),
            ProposedAt = DateTime.Now,
            BookerResponse = "Pending",
            RejectionReason = null,
            BookerCompatibilityImpact = compatibilityImpact
        };
    }

    /// <summary>
    /// Évalue si le Booker accepterait une proposition
    /// </summary>
    public (bool IsAccepted, string Reason) EvaluateProposal(
        CreativeProposal proposal,
        Booker booker,
        StaffCompatibility compatibility)
    {
        var acceptanceScore = 0.0;
        var reasons = new List<string>();

        // 1. Qualité de la proposition (40%)
        var qualityWeight = proposal.QualityScore * 0.4;
        acceptanceScore += qualityWeight;

        if (proposal.QualityScore >= 80)
            reasons.Add("Qualité excellente");
        else if (proposal.QualityScore <= 40)
            reasons.Add("Qualité insuffisante");

        // 2. Compatibilité Staff-Booker (30%)
        var compatibilityWeight = compatibility.OverallScore * 0.3;
        acceptanceScore += compatibilityWeight;

        if (compatibility.OverallScore <= 40)
            reasons.Add("Incompatibilité créative");

        // 3. Risque vs Créativité du Booker (20%)
        var riskAlignment = CalculateRiskAlignment(proposal.RiskLevel, booker.CreativityScore);
        var riskWeight = riskAlignment * 0.2;
        acceptanceScore += riskWeight;

        if (proposal.RiskLevel >= 70 && booker.CreativityScore <= 30)
            reasons.Add("Trop risqué pour le style du booker");

        // 4. Originalité (10%)
        var originalityWeight = proposal.Originality * 0.1;
        acceptanceScore += originalityWeight;

        // Bonus/Malus spécifiques
        if (booker.LogicScore >= 70 && proposal.RiskLevel >= 80)
        {
            acceptanceScore -= 15; // Booker logique rejette propositions très risquées
            reasons.Add("Incohérent avec la vision logique du booker");
        }

        if (compatibility.SuccessfulCollaborations >= 5)
        {
            acceptanceScore += 10; // Historique de succès
            reasons.Add("Historique de collaborations réussies");
        }

        if (compatibility.ConflictHistory >= 3)
        {
            acceptanceScore -= 15; // Historique de conflits
            reasons.Add("Historique de conflits");
        }

        // Décision finale
        var isAccepted = acceptanceScore >= 60;

        var finalReason = isAccepted
            ? $"✅ Acceptée ({acceptanceScore:F0}/100): {string.Join(", ", reasons)}"
            : $"❌ Rejetée ({acceptanceScore:F0}/100): {string.Join(", ", reasons)}";

        return (isAccepted, finalReason);
    }

    /// <summary>
    /// Calcule la qualité d'une proposition
    /// </summary>
    private int CalculateProposalQuality(CreativeStaff staff, StaffMember staffMember)
    {
        // Qualité = mix de créativité, cohérence, et skill général
        var creativityWeight = staff.CreativityScore * 0.4;
        var consistencyWeight = staff.ConsistencyScore * 0.3;
        var skillWeight = staffMember.SkillScore * 0.3;

        var quality = creativityWeight + consistencyWeight + skillWeight;

        return (int)Math.Clamp(quality, 0, 100);
    }

    /// <summary>
    /// Calcule l'originalité d'une proposition
    /// </summary>
    private int CalculateOriginality(CreativeStaff staff)
    {
        // Originalité = créativité + tolérance au risque
        var originality = (staff.CreativityScore * 0.7) + (staff.CreativeRiskTolerance * 0.3);

        return (int)Math.Clamp(originality, 0, 100);
    }

    /// <summary>
    /// Calcule l'impact de la compatibilité sur l'acceptation
    /// </summary>
    private int CalculateCompatibilityImpact(CreativeStaff staff, Booker booker)
    {
        // Calculer différences
        var creativityDiff = Math.Abs(staff.CreativityScore - booker.CreativityScore);
        var consistencyDiff = Math.Abs(staff.ConsistencyScore - booker.LogicScore);

        var avgDiff = (creativityDiff + consistencyDiff) / 2.0;

        // Impact inversé (moins de différence = plus d'impact positif)
        var impact = 100 - avgDiff;

        return (int)Math.Clamp(impact, 0, 100);
    }

    /// <summary>
    /// Calcule l'alignement risque entre proposition et booker
    /// </summary>
    private int CalculateRiskAlignment(int proposalRisk, int bookerCreativity)
    {
        // Booker très créatif accepte risques élevés
        if (bookerCreativity >= 70 && proposalRisk >= 60)
            return 90;

        // Booker peu créatif préfère propositions safe
        if (bookerCreativity <= 30 && proposalRisk <= 40)
            return 85;

        // Calculer alignement
        var diff = Math.Abs(proposalRisk - bookerCreativity);
        var alignment = 100 - diff;

        return Math.Clamp(alignment, 0, 100);
    }

    /// <summary>
    /// Génère des gimmicks basés sur les biais du staff créatif
    /// </summary>
    public List<string> GenerateGimmickSuggestions(CreativeStaff staff)
    {
        var suggestions = new List<string>();

        // Basé sur le biais de worker
        suggestions.AddRange(staff.WorkerBias switch
        {
            WorkerTypeBias.BigMen => new[] { "Monster Heel", "Dominant Champion", "Unstoppable Force" },
            WorkerTypeBias.Cruiserweights => new[] { "High-Flyer", "Daredevil", "Underdog Hero" },
            WorkerTypeBias.Veterans => new[] { "Grizzled Veteran", "Mentor Figure", "Old School" },
            WorkerTypeBias.Rookies => new[] { "Young Lion", "Future Star", "Prodigy" },
            WorkerTypeBias.TechnicalWorkers => new[] { "Master Technician", "Submission Specialist", "Ring General" },
            WorkerTypeBias.Entertainers => new[] { "Charismatic Star", "Showman", "People's Champion" },
            _ => new[] { "Balanced Character", "Versatile Performer" }
        });

        // Basé sur le style préféré
        suggestions.AddRange(staff.PreferredStyle switch
        {
            ProductStyle.Technical => new[] { "Wrestling Purist", "Chain Wrestler" },
            ProductStyle.Brawler => new[] { "Street Fighter", "Brawler" },
            ProductStyle.HighFlyer => new[] { "Aerial Artist", "Risk-Taker" },
            ProductStyle.PowerHouse => new[] { "Powerhouse", "Enforcer" },
            ProductStyle.Storyteller => new[] { "Character-Driven Performer", "Story-First" },
            ProductStyle.Hardcore => new[] { "Hardcore Legend", "Extreme Fighter" },
            ProductStyle.Comedy => new[] { "Comedy Act", "Entertainer" },
            ProductStyle.Realistic => new[] { "MMA Fighter", "Shooter" },
            ProductStyle.Theatrical => new[] { "Larger Than Life", "Over-The-Top Character" },
            _ => Array.Empty<string>()
        });

        return suggestions.Distinct().ToList();
    }

    /// <summary>
    /// Génère un rapport de performance pour un staff créatif
    /// </summary>
    public string GenerateStaffPerformanceReport(
        CreativeStaff staff,
        StaffMember staffMember,
        List<CreativeProposal> proposals)
    {
        var acceptedCount = proposals.Count(p => p.BookerResponse == "Accepted");
        var rejectedCount = proposals.Count(p => p.BookerResponse == "Rejected");
        var pendingCount = proposals.Count(p => p.BookerResponse == "Pending");

        var acceptanceRate = proposals.Any()
            ? (acceptedCount / (double)proposals.Count) * 100
            : 0;

        var avgQuality = proposals.Any()
            ? proposals.Average(p => p.QualityScore)
            : 0;

        var avgOriginality = proposals.Any()
            ? proposals.Average(p => p.Originality)
            : 0;

        var report = $@"=== RAPPORT DE PERFORMANCE CRÉATIVE ===
Staff: {staffMember.Name}
Rôle: {staffMember.Role}
Période: {staffMember.CreatedAt:d} - {DateTime.Now:d}

STATISTIQUES DE PROPOSITIONS:
- Total propositions: {proposals.Count}
- Acceptées: {acceptedCount} ({acceptanceRate:F1}%)
- Rejetées: {rejectedCount}
- En attente: {pendingCount}

QUALITÉ MOYENNE:
- Score qualité: {avgQuality:F0}/100
- Originalité: {avgOriginality:F0}/100

PROFIL CRÉATIF:
- Créativité: {staff.CreativityScore}/100
- Cohérence: {staff.ConsistencyScore}/100
- Tolérance au risque: {staff.CreativeRiskTolerance}/100
- Style préféré: {staff.PreferredStyle}
- Biais worker: {staff.WorkerBias}

COMPATIBILITÉ:
- Score compatibilité actuel: {staff.BookerCompatibilityScore}/100
- Taux d'acceptation propositions: {staff.ProposalAcceptanceRate}%

ÉVALUATION:
{GetPerformanceEvaluation(acceptanceRate, avgQuality, staff.BookerCompatibilityScore)}
";

        return report;
    }

    /// <summary>
    /// Retourne une évaluation textuelle de la performance
    /// </summary>
    private string GetPerformanceEvaluation(double acceptanceRate, double avgQuality, int compatibility)
    {
        if (acceptanceRate >= 70 && avgQuality >= 70 && compatibility >= 70)
        {
            return "⭐ EXCELLENT - Staff créatif de très haute qualité, parfaitement aligné";
        }

        if (acceptanceRate >= 50 && avgQuality >= 60)
        {
            return "✅ BON - Staff compétent avec contributions régulières";
        }

        if (acceptanceRate <= 30 || compatibility <= 40)
        {
            return "⚠️ PROBLÉMATIQUE - Faible alignement avec le booker, envisagez un changement";
        }

        if (avgQuality <= 40)
        {
            return "📉 INSUFFISANT - Qualité des propositions trop faible, formation nécessaire";
        }

        return "🔄 MOYEN - Performance acceptable mais peut s'améliorer";
    }
}

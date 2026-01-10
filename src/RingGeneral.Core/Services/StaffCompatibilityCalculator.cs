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
/// Calculateur de compatibilité entre staff et structures de jeunes.
/// Évalue l'adéquation du staff avec les philosophies et besoins des structures.
/// </summary>
public sealed class StaffCompatibilityCalculator
{
    private readonly IChildCompanyRepository _childCompanyRepository;

    public StaffCompatibilityCalculator(
        IChildCompanyRepository childCompanyRepository)
    {
        _childCompanyRepository = childCompanyRepository ?? throw new ArgumentNullException(nameof(childCompanyRepository));
    }

    // ====================================================================
    // COMPATIBILITY CALCULATION
    // ====================================================================

    /// <summary>
    /// Calcule le score de compatibilité entre un staff et une structure de jeunes
    /// </summary>
    /// <param name="staff">Membre du staff</param>
    /// <param name="youthStructureId">ID de la structure de jeunes</param>
    /// <returns>Score de compatibilité (0.7-1.3)</returns>
    public async Task<double> CalculateCompatibilityScoreAsync(StaffMember staff, string youthStructureId)
    {
        if (staff is null) throw new ArgumentNullException(nameof(staff));
        if (string.IsNullOrWhiteSpace(youthStructureId)) throw new ArgumentException("YouthStructureId requis", nameof(youthStructureId));

        try
        {
            // Récupérer les informations de la structure
            var youthStructure = await GetYouthStructureInfoAsync(youthStructureId);
            if (youthStructure is null) return 1.0; // Score neutre si structure introuvable

            // Calculer les différents facteurs de compatibilité
            var philosophyCompatibility = CalculatePhilosophyCompatibility(staff, youthStructure);
            var roleCompatibility = CalculateRoleCompatibility(staff, youthStructure);
            var experienceCompatibility = CalculateExperienceCompatibility(staff, youthStructure);
            var specializationCompatibility = CalculateSpecializationCompatibility(staff, youthStructure);

            // Score pondéré (moyenne pondérée)
            var weights = new Dictionary<string, double>
            {
                ["philosophy"] = 0.4,    // 40% - Philosophie la plus importante
                ["role"] = 0.3,          // 30% - Rôle spécifique
                ["experience"] = 0.2,    // 20% - Expérience
                ["specialization"] = 0.1 // 10% - Spécialisations
            };

            var weightedScore = (philosophyCompatibility * weights["philosophy"]) +
                               (roleCompatibility * weights["role"]) +
                               (experienceCompatibility * weights["experience"]) +
                               (specializationCompatibility * weights["specialization"]);

            // Normaliser dans la plage 0.7-1.3
            return Math.Clamp(weightedScore, 0.7, 1.3);
        }
        catch
        {
            return 1.0; // Score neutre en cas d'erreur
        }
    }

    /// <summary>
    /// Calcule la compatibilité détaillée avec explication
    /// </summary>
    /// <param name="staff">Membre du staff</param>
    /// <param name="youthStructureId">ID de la structure</param>
    /// <returns>Compatibilité détaillée</returns>
    public async Task<DetailedCompatibility> CalculateDetailedCompatibilityAsync(StaffMember staff, string youthStructureId)
    {
        var overallScore = await CalculateCompatibilityScoreAsync(staff, youthStructureId);
        var youthStructure = await GetYouthStructureInfoAsync(youthStructureId);

        if (youthStructure is null)
        {
            return new DetailedCompatibility(
                OverallScore: 1.0,
                PhilosophyCompatibility: 1.0,
                RoleCompatibility: 1.0,
                ExperienceCompatibility: 1.0,
                SpecializationCompatibility: 1.0,
                Strengths: new[] { "Structure non trouvée - score neutre" },
                Weaknesses: Array.Empty<string>(),
                Recommendations: new[] { "Vérifier la configuration de la structure" });
        }

        var philosophyScore = CalculatePhilosophyCompatibility(staff, youthStructure);
        var roleScore = CalculateRoleCompatibility(staff, youthStructure);
        var experienceScore = CalculateExperienceCompatibility(staff, youthStructure);
        var specializationScore = CalculateSpecializationCompatibility(staff, youthStructure);

        var (strengths, weaknesses) = AnalyzeCompatibilityFactors(
            staff, youthStructure, philosophyScore, roleScore, experienceScore, specializationScore);

        var recommendations = GenerateCompatibilityRecommendations(
            staff, youthStructure, philosophyScore, roleScore, experienceScore, specializationScore);

        return new DetailedCompatibility(
            OverallScore: overallScore,
            PhilosophyCompatibility: philosophyScore,
            RoleCompatibility: roleScore,
            ExperienceCompatibility: experienceScore,
            SpecializationCompatibility: specializationScore,
            Strengths: strengths,
            Weaknesses: weaknesses,
            Recommendations: recommendations);
    }

    /// <summary>
    /// Trouve le staff le plus compatible pour une structure donnée
    /// </summary>
    /// <param name="availableStaff">Staff disponible</param>
    /// <param name="youthStructureId">ID de la structure</param>
    /// <param name="topCount">Nombre de résultats à retourner</param>
    /// <returns>Staff trié par compatibilité</returns>
    public async Task<IReadOnlyList<StaffCompatibilityRanking>> FindMostCompatibleStaffAsync(
        IReadOnlyList<StaffMember> availableStaff,
        string youthStructureId,
        int topCount = 5)
    {
        if (availableStaff is null) throw new ArgumentNullException(nameof(availableStaff));
        if (string.IsNullOrWhiteSpace(youthStructureId)) throw new ArgumentException("YouthStructureId requis", nameof(youthStructureId));

        var rankings = new List<StaffCompatibilityRanking>();

        foreach (var staff in availableStaff.Where(s => s.CanBeShared))
        {
            try
            {
                var detailedCompatibility = await CalculateDetailedCompatibilityAsync(staff, youthStructureId);
                rankings.Add(new StaffCompatibilityRanking(
                    Staff: staff,
                    Compatibility: detailedCompatibility,
                    Rank: 0)); // Sera défini après tri
            }
            catch
            {
                // Ignorer les erreurs individuelles
            }
        }

        // Trier par score décroissant et assigner les rangs
        var sortedRankings = rankings
            .OrderByDescending(r => r.Compatibility.OverallScore)
            .Take(topCount)
            .Select((ranking, index) => ranking with { Rank = index + 1 })
            .ToList();

        return sortedRankings;
    }

    // ====================================================================
    // PRIVATE CALCULATION METHODS
    // ====================================================================

    private async Task<YouthStructureInfo?> GetYouthStructureInfoAsync(string youthStructureId)
    {
        try
        {
            // Note: Cette méthode nécessiterait une implémentation complète du YouthRepository
            // Pour l'instant, on retourne un mock basé sur des données typiques
            return new YouthStructureInfo(
                YouthStructureId: youthStructureId,
                Philosophie: "BALANCED", // Philosophie par défaut
                NiveauEquipements: 3,
                BudgetAnnuel: 50000,
                Type: "PERFORMANCE_CENTER",
                Region: "DEFAULT_REGION");
        }
        catch
        {
            return null;
        }
    }

    private double CalculatePhilosophyCompatibility(StaffMember staff, YouthStructureInfo structure)
    {
        // Logique de compatibilité basée sur la philosophie
        // Pour simplifier, on utilise le département du staff comme proxy de philosophie

        var staffPhilosophy = staff.Department switch
        {
            StaffDepartment.Creative => "ENTERTAINMENT_FOCUS",
            StaffDepartment.Training => "TECHNICAL_FOCUS",
            StaffDepartment.Structural => "BALANCED",
            _ => "BALANCED"
        };

        var structurePhilosophy = structure.Philosophie;

        // Matrice de compatibilité simplifiée
        var compatibilityMatrix = new Dictionary<(string, string), double>
        {
            [("ENTERTAINMENT_FOCUS", "ENTERTAINMENT_FOCUS")] = 1.2,
            [("ENTERTAINMENT_FOCUS", "TECHNICAL_FOCUS")] = 0.8,
            [("ENTERTAINMENT_FOCUS", "BALANCED")] = 1.0,
            [("TECHNICAL_FOCUS", "ENTERTAINMENT_FOCUS")] = 0.8,
            [("TECHNICAL_FOCUS", "TECHNICAL_FOCUS")] = 1.2,
            [("TECHNICAL_FOCUS", "BALANCED")] = 1.0,
            [("BALANCED", "ENTERTAINMENT_FOCUS")] = 1.0,
            [("BALANCED", "TECHNICAL_FOCUS")] = 1.0,
            [("BALANCED", "BALANCED")] = 1.1
        };

        var key = (staffPhilosophy, structurePhilosophy);
        return compatibilityMatrix.GetValueOrDefault(key, 1.0);
    }

    private double CalculateRoleCompatibility(StaffMember staff, YouthStructureInfo structure)
    {
        // Compatibilité basée sur le rôle spécifique du staff
        return staff.Role switch
        {
            // Staff créatif - Bon pour entertainment/story
            StaffRole.LeadWriter or StaffRole.CreativeWriter or StaffRole.Booker =>
                structure.Philosophie.Contains("ENTERTAINMENT") ? 1.15 : 0.9,

            // Staff entraînement - Bon pour technical/performance
            StaffRole.HeadTrainer or StaffRole.WrestlingTrainer or StaffRole.PromoTrainer =>
                structure.Philosophie.Contains("TECHNICAL") ? 1.15 : 0.9,

            // Staff médical - Bon partout pour prévention blessures
            StaffRole.MedicalDirector or StaffRole.MedicalStaff => 1.1,

            // Staff psychologue - Bon pour mental/resilience
            StaffRole.PerformancePsychologist => 1.05,

            // Par défaut - compatibilité moyenne
            _ => 1.0
        };
    }

    private double CalculateExperienceCompatibility(StaffMember staff, YouthStructureInfo structure)
    {
        // Compatibilité basée sur l'expérience
        var experienceFactor = staff.YearsOfExperience switch
        {
            < 3 => 0.85,   // Junior - moins expérimenté pour jeunes
            < 8 => 1.0,    // Mid-level - bon équilibre
            < 15 => 1.1,   // Senior - bonne expérience
            _ => 1.05      // Expert - très expérimenté mais peut être moins flexible
        };

        // Ajustement selon le type de structure
        var structureAdjustment = structure.Type switch
        {
            "PERFORMANCE_CENTER" => staff.YearsOfExperience >= 5 ? 1.05 : 0.95,
            "DOJO" => staff.YearsOfExperience >= 3 ? 1.02 : 0.98,
            "CLUB" => 1.0, // Flexible
            _ => 1.0
        };

        return experienceFactor * structureAdjustment;
    }

    private double CalculateSpecializationCompatibility(StaffMember staff, YouthStructureInfo structure)
    {
        // Pour l'instant, compatibilité neutre - pourrait être étendu avec les spécialisations JSON
        // TODO: Parser ChildSpecializations JSON et calculer compatibilité détaillée
        return 1.0;
    }

    private (IReadOnlyList<string> Strengths, IReadOnlyList<string> Weaknesses) AnalyzeCompatibilityFactors(
        StaffMember staff,
        YouthStructureInfo structure,
        double philosophyScore,
        double roleScore,
        double experienceScore,
        double specializationScore)
    {
        var strengths = new List<string>();
        var weaknesses = new List<string>();

        // Analyse philosophie
        if (philosophyScore >= 1.1)
            strengths.Add("Excellente alignment philosophique");
        else if (philosophyScore <= 0.9)
            weaknesses.Add("Mauvaise compatibilité philosophique");

        // Analyse rôle
        if (roleScore >= 1.1)
            strengths.Add($"Rôle {staff.Role} parfaitement adapté");
        else if (roleScore <= 0.9)
            weaknesses.Add($"Rôle {staff.Role} peu adapté à la structure");

        // Analyse expérience
        if (experienceScore >= 1.05)
            strengths.Add($"{staff.YearsOfExperience} ans d'expérience idéals");
        else if (experienceScore <= 0.9)
            weaknesses.Add($"Expérience ({staff.YearsOfExperience} ans) insuffisante");

        // Analyse compétence
        if (staff.SkillScore >= 80)
            strengths.Add($"Très compétent (score {staff.SkillScore})");
        else if (staff.SkillScore <= 60)
            weaknesses.Add($"Compétence limitée (score {staff.SkillScore})");

        return (strengths, weaknesses);
    }

    private IReadOnlyList<string> GenerateCompatibilityRecommendations(
        StaffMember staff,
        YouthStructureInfo structure,
        double philosophyScore,
        double roleScore,
        double experienceScore,
        double specializationScore)
    {
        var recommendations = new List<string>();

        if (philosophyScore <= 0.9)
            recommendations.Add("Envisager un staff avec une philosophie plus alignée");

        if (roleScore <= 0.9)
        {
            var suggestedRole = structure.Philosophie.Contains("ENTERTAINMENT")
                ? "Creative Writer ou Booker"
                : "Wrestling Trainer ou Head Trainer";
            recommendations.Add($"Préférer un {suggestedRole} pour cette structure");
        }

        if (experienceScore <= 0.9)
            recommendations.Add("Rechercher un staff avec plus d'expérience");

        if (staff.SkillScore <= 70)
            recommendations.Add("Prioriser des membres du staff plus qualifiés");

        if (staff.MobilityRating == StaffMobilityRating.Low)
            recommendations.Add("Considérer la mobilité du staff pour les déplacements");

        if (!recommendations.Any())
            recommendations.Add("Excellent choix de staff pour cette structure");

        return recommendations;
    }
}
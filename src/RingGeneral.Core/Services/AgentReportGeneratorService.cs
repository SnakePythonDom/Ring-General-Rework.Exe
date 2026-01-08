using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service responsible for generating agent reports from mental attributes.
/// Creates narrative analysis using template-based text generation.
/// Inspired by Football Manager scout reports.
/// </summary>
public sealed class AgentReportGeneratorService
{
    // =====================================================================
    // TEMPLATE DICTIONARIES FOR EACH PILLAR
    // =====================================================================

    private static readonly Dictionary<string, Dictionary<string, string>> _templates = new()
    {
        ["Professionnalisme"] = new()
        {
            ["VeryLow"] = "Worker extrêmement problématique avec un professionnalisme défaillant. Manque total de respect pour le métier et les collègues. Risque élevé de conflits et d'absences.",
            ["Low"] = "Professionnalisme en dessous de la moyenne. Peut causer des problèmes dans le vestiaire. Éthique de travail limitée.",
            ["Average"] = "Professionnalisme correct, sans plus. Fait le minimum requis. Fiable dans les situations standards mais n'ira pas au-delà.",
            ["Good"] = "Worker professionnel et respectueux. Bonne éthique de travail. Respecte les règles et les collègues. Fiable au quotidien.",
            ["VeryGood"] = "Très professionnel, fiable et respecté dans le vestiaire. Fait honneur au métier. Exemple pour les plus jeunes.",
            ["Exceptional"] = "Professionnalisme exemplaire absolu. Modèle parfait pour toute l'organisation. Éthique de travail irréprochable. Pilier du vestiaire."
        },

        ["Pression"] = new()
        {
            ["VeryLow"] = "Craque systématiquement sous pression. Performances catastrophiques dans les grands moments. À éviter absolument en PPV et main events. Panique visible.",
            ["Low"] = "Performances instables et décevantes dans les moments importants. Difficile de lui faire confiance en situations critiques.",
            ["Average"] = "Gestion moyenne de la pression. Fiable dans les mid-card matches mais incertain en PPV. Peut délivrer mais pas garanti.",
            ["Good"] = "Solide sous pression. Peut être utilisé en PPV sans trop de risque. Maintient son niveau dans les grands matchs.",
            ["VeryGood"] = "Très bon dans les grands moments. Élève son niveau en PPV. Thrives sous les projecteurs. Fiable pour les title matches.",
            ["Exceptional"] = "Clutch performer absolu. Brille dans les main events, PPV majeurs et moments critiques. Plus il y a de pression, meilleur il devient."
        },

        ["Égoïsme"] = new()
        {
            ["VeryLow"] = "Altruiste remarquable, met toujours l'équipe et la storyline avant son ego personnel. Accepte tous les rôles sans résistance.",
            ["Low"] = "Peu égoïste, accepte facilement de mettre over les autres. Bon esprit d'équipe. Flexible sur les finishes.",
            ["Average"] = "Niveau d'ego normal pour un wrestler professionnel. Équilibre entre ambition personnelle et esprit d'équipe.",
            ["Good"] = "Tendance égocentrique notable. Peut résister à certaines finishes. Nécessite parfois des discussions pour accepter de perdre.",
            ["VeryGood"] = "Très égoïste. Négociations difficiles pour le faire jobber. Privilégie toujours sa propre carrière. Résiste aux creative decisions.",
            ["Exceptional"] = "Diva absolue avec ego démesuré. Refuse catégoriquement de jobber sauf circonstances exceptionnelles. Politique backstage intensive. Creative control exigé."
        },

        ["Influence"] = new()
        {
            ["VeryLow"] = "Aucune influence backstage. Suivra toutes les directives sans discussion. Aucun pouvoir politique ou créatif.",
            ["Low"] = "Faible influence dans le vestiaire. Peu de pouvoir politique. Opinions rarement consultées sur les décisions créatives.",
            ["Average"] = "Influence modérée dans le vestiaire. Respecté par certains. Peut donner son avis mais décisions finales lui échappent.",
            ["Good"] = "Respecté et écouté backstage. Influence certaines décisions créatives concernant ses storylines. Veteran respect.",
            ["VeryGood"] = "Leader de vestiaire avec forte influence politique. Consulté régulièrement sur les angles. Peut vétoer certaines idées.",
            ["Exceptional"] = "Booker de l'ombre avec creative control de facto. Décisions majeures passent par lui. Peut bloquer toute storyline. Véritable pouvoir backstage."
        }
    };

    // =====================================================================
    // REPORT GENERATION
    // =====================================================================

    /// <summary>
    /// Generate complete agent report from mental attributes
    /// </summary>
    /// <param name="mental">Worker's mental attributes</param>
    /// <param name="generatedBy">Optional: Scout/Agent name</param>
    /// <returns>Complete agent report</returns>
    public AgentReport GenerateReport(WorkerMentalAttributes mental, string? generatedBy = null)
    {
        var report = new AgentReport
        {
            WorkerId = mental.WorkerId,
            ProfessionnalismeScore = mental.ProfessionnalismeScore,
            PressionScore = mental.PressionScore,
            ÉgoïsmeScore = mental.ÉgoïsmeScore,
            InfluenceScore = mental.InfluenceScore,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = generatedBy
        };

        // Generate full text report (4 paragraphs, one per pillar)
        var paragraphs = new List<string>
        {
            $"**PROFESSIONNALISME ({report.ProfessionnalismeScore:F1}/20):** {GetPillarText("Professionnalisme", report.ProfessionnalismeScore)}",
            $"**GESTION DE LA PRESSION ({report.PressionScore:F1}/20):** {GetPillarText("Pression", report.PressionScore)}",
            $"**NIVEAU D'ÉGO ({report.ÉgoïsmeScore:F1}/20):** {GetPillarText("Égoïsme", report.ÉgoïsmeScore)}",
            $"**INFLUENCE BACKSTAGE ({report.InfluenceScore:F1}/20):** {GetPillarText("Influence", report.InfluenceScore)}"
        };

        report.ReportText = string.Join("\n\n", paragraphs);
        report.Summary = GenerateSummary(mental);

        return report;
    }

    /// <summary>
    /// Get template text for a specific pillar and score
    /// </summary>
    private static string GetPillarText(string pillar, double score)
    {
        var level = GetScoreLevel(score);
        return _templates[pillar][level];
    }

    /// <summary>
    /// Convert score (0-20) to level category
    /// </summary>
    private static string GetScoreLevel(double score)
    {
        return score switch
        {
            <= 5 => "VeryLow",
            <= 9 => "Low",
            <= 13 => "Average",
            <= 16 => "Good",
            <= 19 => "VeryGood",
            _ => "Exceptional"
        };
    }

    /// <summary>
    /// Generate one-sentence summary based on dominant traits
    /// </summary>
    private static string GenerateSummary(WorkerMentalAttributes mental)
    {
        var traits = new List<string>();

        // Professionnalisme
        if (mental.Professionnalisme >= 15)
            traits.Add("très professionnel");
        else if (mental.Professionnalisme <= 6)
            traits.Add("peu professionnel");

        // Pression
        if (mental.Pression >= 15)
            traits.Add("fiable sous pression");
        else if (mental.Pression <= 6)
            traits.Add("craque sous pression");

        // Égoïsme (inverted - low ego is positive)
        if (mental.Égoïsme <= 6)
            traits.Add("peu égoïste");
        else if (mental.Égoïsme >= 15)
            traits.Add("très égocentrique");

        // Influence
        if (mental.Influence >= 15)
            traits.Add("influent backstage");
        else if (mental.Influence <= 6)
            traits.Add("sans influence");

        // Loyauté
        if (mental.Loyauté >= 15)
            traits.Add("très loyal");
        else if (mental.Loyauté <= 6)
            traits.Add("mercenaire");

        // Tempérament
        if (mental.Tempérament <= 6)
            traits.Add("explosif");
        else if (mental.Tempérament >= 15)
            traits.Add("très calme");

        // Détermination
        if (mental.Détermination >= 15)
            traits.Add("déterminé");
        else if (mental.Détermination <= 6)
            traits.Add("abandonne facilement");

        // Ambition
        if (mental.Ambition >= 15)
            traits.Add("ambitieux");
        else if (mental.Ambition <= 6)
            traits.Add("sans ambition");

        return traits.Count > 0
            ? $"Worker {string.Join(", ", traits)}."
            : "Profil équilibré sans traits dominants.";
    }

    /// <summary>
    /// Generate quick assessment (for list views)
    /// </summary>
    public static string GenerateQuickAssessment(WorkerMentalAttributes mental)
    {
        // Calculate overall "quality" score (low ego is good)
        var quality = (mental.ProfessionnalismeScore + mental.PressionScore +
                      (20 - mental.ÉgoïsmeScore) + mental.InfluenceScore) / 4.0;

        return quality switch
        {
            >= 16 => "⭐⭐⭐ EXCELLENT",
            >= 13 => "⭐⭐ TRÈS BON",
            >= 10 => "⭐ BON",
            >= 7 => "⚠️ MOYEN",
            _ => "❌ RISQUE"
        };
    }

    /// <summary>
    /// Generate detailed recommendations for booking based on mental attributes
    /// </summary>
    public static List<string> GenerateBookingRecommendations(WorkerMentalAttributes mental)
    {
        var recommendations = new List<string>();

        // Pressure handling
        if (mental.Pression >= 15)
            recommendations.Add("✅ Recommandé pour PPV et main events");
        else if (mental.Pression <= 6)
            recommendations.Add("⚠️ À éviter dans les grands matchs");

        // Ego management
        if (mental.Égoïsme >= 15)
            recommendations.Add("⚠️ Difficile à convaincre de perdre - négociations nécessaires");
        else if (mental.Égoïsme <= 6)
            recommendations.Add("✅ Accepte facilement de mettre over les autres");

        // Professionalism
        if (mental.Professionnalisme >= 15)
            recommendations.Add("✅ Fiable, aucun problème attendu");
        else if (mental.Professionnalisme <= 6)
            recommendations.Add("❌ Risque de conflits et problèmes backstage");

        // Temperament
        if (mental.Tempérament <= 6)
            recommendations.Add("⚠️ Risque élevé d'incidents backstage");
        else if (mental.Tempérament >= 15)
            recommendations.Add("✅ Tempérament calme, bon pour le vestiaire");

        // Loyalty
        if (mental.Loyauté <= 6)
            recommendations.Add("⚠️ Risque de départ pour concurrent - surveiller offres");
        else if (mental.Loyauté >= 15)
            recommendations.Add("✅ Loyal à l'entreprise, peu de risque de départ");

        // Determination
        if (mental.Détermination >= 15)
            recommendations.Add("✅ Rebondira bien après une losing streak");
        else if (mental.Détermination <= 6)
            recommendations.Add("⚠️ Peut perdre confiance rapidement si enterré");

        // Adaptability
        if (mental.Adaptabilité >= 15)
            recommendations.Add("✅ Polyvalent - peut jouer différents rôles");
        else if (mental.Adaptabilité <= 6)
            recommendations.Add("⚠️ Difficile à faire changer de gimmick ou style");

        // Sportsmanship
        if (mental.Sportivité <= 6)
            recommendations.Add("⚠️ Peut refuser de faire briller les jeunes");
        else if (mental.Sportivité >= 15)
            recommendations.Add("✅ Excellent pour élever les talents montants");

        return recommendations;
    }

    /// <summary>
    /// Generate red flags list (warning signs)
    /// </summary>
    public static List<string> GenerateRedFlags(WorkerMentalAttributes mental)
    {
        var flags = new List<string>();

        if (mental.Professionnalisme <= 6)
            flags.Add("🚩 Professionnalisme très faible");

        if (mental.Tempérament <= 6)
            flags.Add("🚩 Tempérament explosif");

        if (mental.Égoïsme >= 17 && mental.Sportivité <= 6)
            flags.Add("🚩 Égoïste et refusera de jobber");

        if (mental.Pression <= 6)
            flags.Add("🚩 Choke sous pression");

        if (mental.Loyauté <= 6 && mental.Ambition >= 13)
            flags.Add("🚩 Mercenaire - partira pour mieux");

        if (mental.Sportivité <= 5 && mental.Égoïsme >= 15 && mental.Influence >= 10)
            flags.Add("🚨 DANGER: Saboteur potentiel");

        if (mental.Professionnalisme <= 5 && mental.Détermination <= 5 && mental.Ambition <= 5)
            flags.Add("🚨 POIDS MORT - Considérer release");

        return flags;
    }
}

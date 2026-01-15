namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Agent Report - Dynamically generated personality analysis.
    /// Inspired by Football Manager's scout reports.
    /// Provides narrative analysis of a worker's mental attributes.
    /// </summary>
    public class AgentReport
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Worker ID (Foreign Key)
        /// </summary>
        public int WorkerId { get; set; }

        // =====================================================================
        // 4 PILIERS - CORE PERSONALITY SCORES (0-20 scale)
        // =====================================================================

        /// <summary>
        /// Pilier 1: Professionnalisme
        /// Average of: Professionnalisme, Sportivité, Loyauté
        /// Measures work ethic, reliability, and respect for the business.
        /// </summary>
        public double ProfessionnalismeScore { get; set; }

        /// <summary>
        /// Pilier 2: Gestion de la Pression
        /// Average of: Pression, Détermination
        /// Measures ability to perform in big matches and handle adversity.
        /// </summary>
        public double PressionScore { get; set; }

        /// <summary>
        /// Pilier 3: Niveau d'Égo
        /// Direct value: Égoïsme
        /// Measures selfishness and willingness to prioritize team over self.
        /// </summary>
        public double ÉgoïsmeScore { get; set; }

        /// <summary>
        /// Pilier 4: Influence Backstage
        /// Average of: Influence, Tempérament
        /// Measures backstage power and political clout.
        /// </summary>
        public double InfluenceScore { get; set; }

        // =====================================================================
        // GENERATED TEXT
        // =====================================================================

        /// <summary>
        /// Full report text (2-4 paragraphs)
        /// Example: "Worker modèle avec un professionnalisme exemplaire.
        /// Fiable sous pression et très respecté dans le vestiaire..."
        /// </summary>
        public string ReportText { get; set; } = string.Empty;

        /// <summary>
        /// Short summary (1 sentence)
        /// Example: "Professionnel fiable sous pression, peu égoïste, influence modérée"
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// When was this report generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Agent/Scout who generated this report (optional)
        /// </summary>
        public string? GeneratedBy { get; set; }

        // =====================================================================
        // NAVIGATION PROPERTIES
        // =====================================================================

        /// <summary>
        /// Navigation property to Worker
        /// </summary>
        public Worker? Worker { get; set; }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        /// <summary>
        /// Get color code for a pillar score (for UI visualization)
        /// </summary>
        /// <param name="score">Score 0-20</param>
        /// <returns>Hex color code</returns>
        public static string GetScoreColor(double score)
        {
            return score switch
            {
                <= 5 => "#ef4444",    // Red - Very Low
                <= 9 => "#f97316",    // Orange - Low
                <= 13 => "#eab308",   // Yellow - Average
                <= 16 => "#84cc16",   // Lime - Good
                <= 19 => "#22c55e",   // Green - Very Good
                _ => "#10b981"        // Emerald - Exceptional
            };
        }

        /// <summary>
        /// Get text description for a pillar score
        /// </summary>
        public static string GetScoreDescription(double score)
        {
            return score switch
            {
                <= 5 => "Très Faible",
                <= 9 => "Faible",
                <= 13 => "Moyen",
                <= 16 => "Bon",
                <= 19 => "Très Bon",
                _ => "Exceptionnel"
            };
        }

        /// <summary>
        /// Get progress bar percentage for UI (0-100%)
        /// </summary>
        public static double GetProgressPercentage(double score)
        {
            return Math.Round((score / 20.0) * 100.0, 1);
        }

        /// <summary>
        /// Get all 4 pillars as dictionary
        /// </summary>
        public Dictionary<string, double> GetAllPillars()
        {
            return new Dictionary<string, double>
            {
                ["Professionnalisme"] = ProfessionnalismeScore,
                ["Pression"] = PressionScore,
                ["Égoïsme"] = ÉgoïsmeScore,
                ["Influence"] = InfluenceScore
            };
        }

        /// <summary>
        /// Validate report data
        /// </summary>
        public bool Validate()
        {
            return ProfessionnalismeScore >= 0 && ProfessionnalismeScore <= 20 &&
                   PressionScore >= 0 && PressionScore <= 20 &&
                   ÉgoïsmeScore >= 0 && ÉgoïsmeScore <= 20 &&
                   InfluenceScore >= 0 && InfluenceScore <= 20 &&
                   !string.IsNullOrEmpty(ReportText);
        }

        /// <summary>
        /// Get overall assessment based on 4 pillars
        /// </summary>
        public string GetOverallAssessment()
        {
            var avgScore = (ProfessionnalismeScore + PressionScore + (20 - ÉgoïsmeScore) + InfluenceScore) / 4.0;

            return avgScore switch
            {
                >= 16 => "EXCELLENT - Worker d'élite, peu ou pas de problèmes attendus",
                >= 13 => "TRÈS BON - Worker fiable avec quelques points d'attention",
                >= 10 => "BON - Worker solide, potentiellement quelques défis",
                >= 7 => "MOYEN - Worker utilisable mais nécessite management attentif",
                _ => "RISQUE - Problèmes potentiels significatifs à anticiper"
            };
        }
    }
}

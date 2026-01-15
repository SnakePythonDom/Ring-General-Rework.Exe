namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Match result types
    /// </summary>
    public enum MatchResult
    {
        /// <summary>
        /// 🏆 Win
        /// </summary>
        Win,

        /// <summary>
        /// ❌ Loss
        /// </summary>
        Loss,

        /// <summary>
        /// 🤝 Draw
        /// </summary>
        Draw,

        /// <summary>
        /// ⚠️ No Contest
        /// </summary>
        NoContest
    }

    /// <summary>
    /// Represents a single match in a wrestler's history.
    /// Tracks opponents, results, ratings, and duration.
    /// </summary>
    public class MatchHistoryItem
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Worker ID (Foreign Key)
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// Show ID (Foreign Key, nullable)
        /// </summary>
        public int? ShowId { get; set; }

        /// <summary>
        /// Date of the match
        /// </summary>
        public DateTime MatchDate { get; set; }

        /// <summary>
        /// Type of match (Singles, Tag Team, Triple Threat, etc.)
        /// </summary>
        public string? MatchType { get; set; }

        /// <summary>
        /// Opponent worker ID (Foreign Key, nullable for multi-person matches)
        /// </summary>
        public int? OpponentId { get; set; }

        /// <summary>
        /// Result of the match (Win, Loss, Draw, NoContest)
        /// </summary>
        public MatchResult? Result { get; set; }

        /// <summary>
        /// Match rating (0-100)
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Duration of the match in minutes
        /// </summary>
        public int? Duration { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The worker this match belongs to
        /// </summary>
        public Worker? Worker { get; set; }

        /// <summary>
        /// The opponent worker (if applicable)
        /// </summary>
        public Worker? Opponent { get; set; }

        // Note: Show navigation property would require Show model

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Get result icon emoji
        /// </summary>
        public string ResultIcon => Result switch
        {
            MatchResult.Win => "🏆",
            MatchResult.Loss => "❌",
            MatchResult.Draw => "🤝",
            MatchResult.NoContest => "⚠️",
            _ => "?"
        };

        /// <summary>
        /// Get result display name (French)
        /// </summary>
        public string ResultDisplayName => Result switch
        {
            MatchResult.Win => "Victoire",
            MatchResult.Loss => "Défaite",
            MatchResult.Draw => "Match Nul",
            MatchResult.NoContest => "Sans Résultat",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get result color (hex) for UI
        /// </summary>
        public string ResultColor => Result switch
        {
            MatchResult.Win => "#10b981",      // Green
            MatchResult.Loss => "#ef4444",     // Red
            MatchResult.Draw => "#f59e0b",     // Orange
            MatchResult.NoContest => "#6b7280", // Gray
            _ => "#6b7280"
        };

        /// <summary>
        /// Get formatted match date
        /// </summary>
        public string MatchDateFormatted => MatchDate.ToString("dd/MM/yyyy");

        /// <summary>
        /// Get formatted duration (e.g., "15:30")
        /// </summary>
        public string DurationFormatted
        {
            get
            {
                if (!Duration.HasValue) return "N/A";
                int minutes = Duration.Value;
                int seconds = 0; // Could be extended to track seconds
                return $"{minutes}:{seconds:D2}";
            }
        }

        /// <summary>
        /// Get rating display with stars (e.g., "★★★★☆ 82/100")
        /// </summary>
        public string RatingDisplay
        {
            get
            {
                if (!Rating.HasValue) return "Non évalué";

                // Convert 0-100 to 0-5 stars
                int stars = (Rating.Value * 5) / 100;
                string starDisplay = new string('★', stars) + new string('☆', 5 - stars);
                return $"{starDisplay} {Rating.Value}/100";
            }
        }

        /// <summary>
        /// Is this a high quality match? (Rating >= 80)
        /// </summary>
        public bool IsHighQuality => Rating.HasValue && Rating.Value >= 80;

        /// <summary>
        /// Did this worker win?
        /// </summary>
        public bool IsWin => Result == MatchResult.Win;

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Validate the match history item
        /// </summary>
        public bool Validate()
        {
            if (WorkerId <= 0) return false;
            if (Rating.HasValue && (Rating.Value < 0 || Rating.Value > 100)) return false;
            if (Duration.HasValue && Duration.Value < 0) return false;

            return true;
        }

        /// <summary>
        /// Get match description (for display)
        /// </summary>
        public string GetMatchDescription()
        {
            var type = string.IsNullOrWhiteSpace(MatchType) ? "Match" : MatchType;
            var vs = OpponentId.HasValue && Opponent != null
                ? $" vs {Opponent.Name}"
                : "";
            var result = Result.HasValue ? $" - {ResultDisplayName}" : "";

            return $"{type}{vs}{result}";
        }
    }
}

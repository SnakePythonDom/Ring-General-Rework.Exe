namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Represents a championship title reign for a wrestler.
    /// Tracks when they won and lost the title, and reign statistics.
    /// </summary>
    public class TitleReign
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
        /// Title ID (Foreign Key)
        /// </summary>
        public int TitleId { get; set; }

        /// <summary>
        /// Date when the title was won
        /// </summary>
        public DateTime WonDate { get; set; }

        /// <summary>
        /// Show ID where title was won (Foreign Key, nullable)
        /// </summary>
        public int? WonShowId { get; set; }

        /// <summary>
        /// Date when the title was lost (null if currently held)
        /// </summary>
        public DateTime? LostDate { get; set; }

        /// <summary>
        /// Show ID where title was lost (Foreign Key, nullable)
        /// </summary>
        public int? LostShowId { get; set; }

        /// <summary>
        /// Number of days the title was held
        /// </summary>
        public int? DaysHeld { get; set; }

        /// <summary>
        /// Reign number for this worker (1st reign, 2nd reign, etc.)
        /// </summary>
        public int ReignNumber { get; set; } = 1;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The worker who holds/held this title
        /// </summary>
        public Worker? Worker { get; set; }

        // Note: Title, WonShow, LostShow navigation properties would require those models

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Is this worker currently the champion?
        /// </summary>
        public bool IsCurrentChampion => !LostDate.HasValue;

        /// <summary>
        /// Calculate days held (for current reigns)
        /// </summary>
        public int DaysHeldCalculated
        {
            get
            {
                if (DaysHeld.HasValue)
                    return DaysHeld.Value;

                // Calculate for current reign
                var endDate = LostDate ?? DateTime.Now;
                var duration = endDate - WonDate;
                return duration.Days;
            }
        }

        /// <summary>
        /// Get formatted won date
        /// </summary>
        public string WonDateFormatted => WonDate.ToString("dd/MM/yyyy");

        /// <summary>
        /// Get formatted lost date (or "Actuel" if current champion)
        /// </summary>
        public string LostDateFormatted => LostDate.HasValue
            ? LostDate.Value.ToString("dd/MM/yyyy")
            : "Actuel";

        /// <summary>
        /// Get reign duration text (e.g., "142 jours" or "2 ans, 3 mois")
        /// </summary>
        public string ReignDurationText
        {
            get
            {
                int days = DaysHeldCalculated;

                if (days < 30)
                    return $"{days} jour{(days > 1 ? "s" : "")}";

                if (days < 365)
                {
                    int months = days / 30;
                    return $"{months} mois";
                }

                int years = days / 365;
                int remainingMonths = (days % 365) / 30;

                if (remainingMonths == 0)
                    return $"{years} an{(years > 1 ? "s" : "")}";

                return $"{years} an{(years > 1 ? "s" : "")}, {remainingMonths} mois";
            }
        }

        /// <summary>
        /// Get reign ordinal text (French: "1er", "2e", "3e", etc.)
        /// </summary>
        public string ReignNumberOrdinal
        {
            get
            {
                if (ReignNumber == 1) return "1er";
                return $"{ReignNumber}e";
            }
        }

        /// <summary>
        /// Is this a long reign? (> 180 days)
        /// </summary>
        public bool IsLongReign => DaysHeldCalculated > 180;

        /// <summary>
        /// Is this a short reign? (< 30 days)
        /// </summary>
        public bool IsShortReign => DaysHeldCalculated < 30;

        /// <summary>
        /// Get status color based on reign type
        /// </summary>
        public string StatusColor
        {
            get
            {
                if (IsCurrentChampion) return "#10b981";  // Green - Current champion
                if (IsLongReign) return "#3b82f6";        // Blue - Historical long reign
                return "#6b7280";                          // Gray - Normal historical reign
            }
        }

        /// <summary>
        /// Get status icon
        /// </summary>
        public string StatusIcon => IsCurrentChampion ? "🏆" : "📜";

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// End the title reign (mark as lost)
        /// </summary>
        public void EndReign(DateTime lostDate, int? lostShowId = null)
        {
            LostDate = lostDate;
            LostShowId = lostShowId;

            // Calculate days held
            DaysHeld = (lostDate - WonDate).Days;
        }

        /// <summary>
        /// Recalculate days held (for current reigns or corrections)
        /// </summary>
        public void RecalculateDaysHeld()
        {
            var endDate = LostDate ?? DateTime.Now;
            DaysHeld = (endDate - WonDate).Days;
        }

        /// <summary>
        /// Validate the title reign
        /// </summary>
        public bool Validate()
        {
            if (WorkerId <= 0 || TitleId <= 0) return false;
            if (ReignNumber < 1) return false;

            // Lost date must be after won date
            if (LostDate.HasValue && LostDate.Value <= WonDate) return false;

            // Days held must be non-negative
            if (DaysHeld.HasValue && DaysHeld.Value < 0) return false;

            return true;
        }

        /// <summary>
        /// Get reign summary text
        /// </summary>
        public string GetReignSummary()
        {
            var duration = ReignDurationText;
            var status = IsCurrentChampion ? "Champion actuel" : "Ancien champion";

            return $"{ReignNumberOrdinal} règne - {duration} ({status})";
        }
    }
}

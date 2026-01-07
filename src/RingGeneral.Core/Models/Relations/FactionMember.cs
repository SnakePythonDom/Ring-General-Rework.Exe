namespace RingGeneral.Core.Models.Relations
{
    /// <summary>
    /// Represents a member's participation in a faction.
    /// Tracks when they joined and when they left (if applicable).
    /// </summary>
    public class FactionMember
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Faction ID (Foreign Key)
        /// </summary>
        public int FactionId { get; set; }

        /// <summary>
        /// Worker ID (Foreign Key)
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// Week when the member joined
        /// </summary>
        public int JoinedWeek { get; set; }

        /// <summary>
        /// Year when the member joined
        /// </summary>
        public int JoinedYear { get; set; }

        /// <summary>
        /// Week when the member left (null if still active)
        /// </summary>
        public int? LeftWeek { get; set; }

        /// <summary>
        /// Year when the member left (null if still active)
        /// </summary>
        public int? LeftYear { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The faction this membership belongs to
        /// </summary>
        public Faction? Faction { get; set; }

        /// <summary>
        /// The worker who is/was a member
        /// </summary>
        public Worker? Worker { get; set; }

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Is this member currently active in the faction?
        /// </summary>
        public bool IsActiveMember => !LeftWeek.HasValue && !LeftYear.HasValue;

        /// <summary>
        /// Get joined date text
        /// </summary>
        public string JoinedDateText => $"Semaine {JoinedWeek}/{JoinedYear}";

        /// <summary>
        /// Get left date text (or "Present" if still active)
        /// </summary>
        public string LeftDateText =>
            LeftWeek.HasValue && LeftYear.HasValue
                ? $"Semaine {LeftWeek}/{LeftYear}"
                : "Présent";

        /// <summary>
        /// Get tenure text (e.g., "2012-2014" or "2012-Present")
        /// </summary>
        public string TenureText =>
            $"{JoinedYear}{(LeftYear.HasValue ? $"-{LeftYear}" : "-Présent")}";

        /// <summary>
        /// Calculate duration in weeks (approximate)
        /// </summary>
        public int? DurationInWeeks
        {
            get
            {
                if (!LeftWeek.HasValue || !LeftYear.HasValue)
                    return null; // Still active

                int years = LeftYear.Value - JoinedYear;
                int weeks = (years * 52) + (LeftWeek.Value - JoinedWeek);
                return weeks > 0 ? weeks : 0;
            }
        }

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Leave the faction
        /// </summary>
        public void Leave(int week, int year)
        {
            LeftWeek = week;
            LeftYear = year;
        }

        /// <summary>
        /// Rejoin the faction (clear left dates)
        /// </summary>
        public void Rejoin()
        {
            LeftWeek = null;
            LeftYear = null;
        }

        /// <summary>
        /// Check if this member was active during a specific week/year
        /// </summary>
        public bool WasActiveDuring(int week, int year)
        {
            // Joined before or during the target date
            bool joinedBefore = (JoinedYear < year) ||
                               (JoinedYear == year && JoinedWeek <= week);

            // Either still active, or left after the target date
            bool notLeftYet = !LeftYear.HasValue ||
                             LeftYear.Value > year ||
                             (LeftYear.Value == year && LeftWeek!.Value > week);

            return joinedBefore && notLeftYet;
        }

        /// <summary>
        /// Validate the membership
        /// </summary>
        public bool Validate()
        {
            if (FactionId <= 0 || WorkerId <= 0) return false;
            if (JoinedWeek < 1 || JoinedWeek > 52) return false;
            if (JoinedYear < 1900) return false;

            // If left, validate left date is after joined date
            if (LeftWeek.HasValue && LeftYear.HasValue)
            {
                if (LeftYear.Value < JoinedYear) return false;
                if (LeftYear.Value == JoinedYear && LeftWeek.Value <= JoinedWeek) return false;
            }

            return true;
        }
    }
}

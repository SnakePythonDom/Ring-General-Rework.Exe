namespace RingGeneral.Core.Models.Relations
{
    /// <summary>
    /// Type of faction
    /// </summary>
    public enum FactionType
    {
        /// <summary>
        /// 🤜🤛 Tag Team - 2 members
        /// </summary>
        TagTeam,

        /// <summary>
        /// 🎯 Trio - 3 members
        /// </summary>
        Trio,

        /// <summary>
        /// 👊 Faction - 3+ members (typically 4-6)
        /// </summary>
        Faction
    }

    /// <summary>
    /// Status of the faction
    /// </summary>
    public enum FactionStatus
    {
        /// <summary>
        /// Active - Currently performing together
        /// </summary>
        Active,

        /// <summary>
        /// Inactive - On hiatus but not disbanded
        /// </summary>
        Inactive,

        /// <summary>
        /// Disbanded - No longer together
        /// </summary>
        Disbanded
    }

    /// <summary>
    /// Represents a group of wrestlers working together.
    /// Can be a Tag Team (2), Trio (3), or larger Faction (4+).
    /// </summary>
    public class Faction
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Faction name (e.g., "The Shield", "Evolution", "DX")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of faction (Tag Team, Trio, Faction)
        /// </summary>
        public FactionType FactionType { get; set; }

        /// <summary>
        /// Optional leader of the faction (can be null for equal partnerships)
        /// </summary>
        public int? LeaderId { get; set; }

        /// <summary>
        /// Current status of the faction
        /// </summary>
        public FactionStatus Status { get; set; } = FactionStatus.Active;

        /// <summary>
        /// Week when the faction was created
        /// </summary>
        public int CreatedWeek { get; set; }

        /// <summary>
        /// Year when the faction was created
        /// </summary>
        public int CreatedYear { get; set; }

        /// <summary>
        /// Week when the faction was disbanded (null if active)
        /// </summary>
        public int? DisbandedWeek { get; set; }

        /// <summary>
        /// Year when the faction was disbanded (null if active)
        /// </summary>
        public int? DisbandedYear { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Leader of the faction (if any)
        /// </summary>
        public Worker? Leader { get; set; }

        /// <summary>
        /// Members of the faction
        /// </summary>
        public List<FactionMember> Members { get; set; } = new();

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Get icon emoji for this faction type
        /// </summary>
        public string FactionTypeIcon => FactionType switch
        {
            FactionType.TagTeam => "🤜🤛",
            FactionType.Trio => "🎯",
            FactionType.Faction => "👊",
            _ => "?"
        };

        /// <summary>
        /// Get status color (hex)
        /// </summary>
        public string StatusColor => Status switch
        {
            FactionStatus.Active => "#10b981",    // Green
            FactionStatus.Inactive => "#f59e0b",  // Orange
            FactionStatus.Disbanded => "#666666", // Gray
            _ => "#666666"
        };

        /// <summary>
        /// Get created date text
        /// </summary>
        public string CreatedDateText => $"Semaine {CreatedWeek}/{CreatedYear}";

        /// <summary>
        /// Get disbanded date text
        /// </summary>
        public string? DisbandedDateText =>
            DisbandedWeek.HasValue && DisbandedYear.HasValue
                ? $"Semaine {DisbandedWeek}/{DisbandedYear}"
                : null;

        /// <summary>
        /// Does this faction have a leader?
        /// </summary>
        public bool HasLeader => LeaderId.HasValue;

        /// <summary>
        /// Number of active members
        /// </summary>
        public int ActiveMemberCount =>
            Members?.Count(m => !m.LeftWeek.HasValue) ?? 0;

        /// <summary>
        /// Is this faction currently active?
        /// </summary>
        public bool IsActive => Status == FactionStatus.Active;

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Disband the faction
        /// </summary>
        public void Disband(int week, int year)
        {
            Status = FactionStatus.Disbanded;
            DisbandedWeek = week;
            DisbandedYear = year;

            // Remove all active members
            if (Members != null)
            {
                foreach (var member in Members.Where(m => !m.LeftWeek.HasValue))
                {
                    member.LeftWeek = week;
                    member.LeftYear = year;
                }
            }
        }

        /// <summary>
        /// Set faction to inactive
        /// </summary>
        public void SetInactive()
        {
            Status = FactionStatus.Inactive;
        }

        /// <summary>
        /// Reactivate the faction
        /// </summary>
        public void Reactivate()
        {
            Status = FactionStatus.Active;
        }

        /// <summary>
        /// Check if a worker is currently a member
        /// </summary>
        public bool HasActiveMember(int workerId)
        {
            return Members?.Any(m => m.WorkerId == workerId && !m.LeftWeek.HasValue) ?? false;
        }

        /// <summary>
        /// Validate the faction
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (CreatedWeek < 1 || CreatedWeek > 52) return false;
            if (CreatedYear < 1900) return false;

            // Validate member count matches faction type
            var activeMemberCount = ActiveMemberCount;
            return FactionType switch
            {
                FactionType.TagTeam => activeMemberCount == 2,
                FactionType.Trio => activeMemberCount == 3,
                FactionType.Faction => activeMemberCount >= 3,
                _ => false
            };
        }
    }
}

namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Wrestling specialization/style
    /// </summary>
    public enum SpecializationType
    {
        /// <summary>
        /// 🥊 Brawler - Brutal physical combat style
        /// </summary>
        Brawler,

        /// <summary>
        /// 🎯 Technical - Mat wrestling and technical submissions
        /// </summary>
        Technical,

        /// <summary>
        /// 🤸 HighFlyer - Aerial and acrobatic style
        /// </summary>
        HighFlyer,

        /// <summary>
        /// 💪 Power - Raw strength and power moves
        /// </summary>
        Power,

        /// <summary>
        /// 🔪 Hardcore - Weapons and extreme matches
        /// </summary>
        Hardcore,

        /// <summary>
        /// 🤼 Submission - Submission specialist
        /// </summary>
        Submission,

        /// <summary>
        /// 🎭 Showman - Entertainer and spectacular performer
        /// </summary>
        Showman,

        /// <summary>
        /// ⭐ AllRounder - Jack of all trades, master of all
        /// </summary>
        AllRounder
    }

    /// <summary>
    /// Represents a worker's wrestling specialization/style.
    /// Workers can have multiple specializations with different priority levels.
    /// </summary>
    public class WorkerSpecialization
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
        /// Type of specialization
        /// </summary>
        public SpecializationType Specialization { get; set; }

        /// <summary>
        /// Priority level:
        /// 1 = Primary specialization (main style)
        /// 2 = Secondary specialization
        /// 3 = Tertiary specialization
        /// </summary>
        public int Level { get; set; } = 1;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The worker this specialization belongs to
        /// </summary>
        public Worker? Worker { get; set; }

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Is this the primary specialization?
        /// </summary>
        public bool IsPrimary => Level == 1;

        /// <summary>
        /// Is this a secondary specialization?
        /// </summary>
        public bool IsSecondary => Level == 2;

        /// <summary>
        /// Is this a tertiary specialization?
        /// </summary>
        public bool IsTertiary => Level == 3;

        /// <summary>
        /// Get level text description
        /// </summary>
        public string LevelText => Level switch
        {
            1 => "Primaire",
            2 => "Secondaire",
            3 => "Tertiaire",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get icon emoji for this specialization
        /// </summary>
        public string SpecializationIcon => Specialization switch
        {
            SpecializationType.Brawler => "🥊",
            SpecializationType.Technical => "🎯",
            SpecializationType.HighFlyer => "🤸",
            SpecializationType.Power => "💪",
            SpecializationType.Hardcore => "🔪",
            SpecializationType.Submission => "🤼",
            SpecializationType.Showman => "🎭",
            SpecializationType.AllRounder => "⭐",
            _ => "?"
        };

        /// <summary>
        /// Get specialization display name (French)
        /// </summary>
        public string SpecializationDisplayName => Specialization switch
        {
            SpecializationType.Brawler => "Bagarreur",
            SpecializationType.Technical => "Technique",
            SpecializationType.HighFlyer => "Aérien",
            SpecializationType.Power => "Puissance",
            SpecializationType.Hardcore => "Hardcore",
            SpecializationType.Submission => "Soumission",
            SpecializationType.Showman => "Spectacle",
            SpecializationType.AllRounder => "Polyvalent",
            _ => "Inconnu"
        };

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Validate the specialization
        /// </summary>
        public bool Validate()
        {
            return WorkerId > 0 &&
                   Level >= 1 && Level <= 3 &&
                   Enum.IsDefined(typeof(SpecializationType), Specialization);
        }
    }
}

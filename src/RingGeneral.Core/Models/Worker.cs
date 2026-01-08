using RingGeneral.Core.Models.Attributes;
using RingGeneral.Core.Models.Relations;

namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Handedness (dominant hand)
    /// </summary>
    public enum Handedness
    {
        /// <summary>
        /// Right-handed
        /// </summary>
        Right,

        /// <summary>
        /// Left-handed
        /// </summary>
        Left,

        /// <summary>
        /// Ambidextrous
        /// </summary>
        Ambidextrous
    }

    /// <summary>
    /// Fighting stance
    /// </summary>
    public enum FightingStance
    {
        /// <summary>
        /// Orthodox (right-handed stance)
        /// </summary>
        Orthodox,

        /// <summary>
        /// Southpaw (left-handed stance)
        /// </summary>
        Southpaw,

        /// <summary>
        /// Switch (can switch between stances)
        /// </summary>
        Switch
    }

    /// <summary>
    /// Character alignment (Face/Heel/Tweener)
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// 😇 Face (Good guy/Hero)
        /// </summary>
        Face,

        /// <summary>
        /// 😈 Heel (Bad guy/Villain)
        /// </summary>
        Heel,

        /// <summary>
        /// 😐 Tweener (Morally ambiguous/Anti-hero)
        /// </summary>
        Tweener
    }

    /// <summary>
    /// Push level (booking position on card)
    /// </summary>
    public enum PushLevel
    {
        /// <summary>
        /// 🌟 Main Event - Top of the card
        /// </summary>
        MainEvent,

        /// <summary>
        /// ⭐ Upper Mid-Card - Strong position
        /// </summary>
        UpperMid,

        /// <summary>
        /// ✨ Mid-Card - Middle of the card
        /// </summary>
        MidCard,

        /// <summary>
        /// 💫 Lower Mid-Card - Enhancement talent
        /// </summary>
        LowerMid,

        /// <summary>
        /// 📉 Jobber - Loses to make others look strong
        /// </summary>
        Jobber
    }

    /// <summary>
    /// Gender
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Male
        /// </summary>
        Male,

        /// <summary>
        /// Female
        /// </summary>
        Female,

        /// <summary>
        /// Other/Non-binary
        /// </summary>
        Other
    }

    /// <summary>
    /// Represents a professional wrestler/worker.
    /// Central model tying together all attributes, relations, contracts, and history.
    /// </summary>
    public class Worker
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        // ====================================================================
        // BASIC INFORMATION
        // ====================================================================

        /// <summary>
        /// Worker's ring name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Worker's real/birth name
        /// </summary>
        public string? RealName { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        public Gender Gender { get; set; } = Gender.Male;

        /// <summary>
        /// Age in years
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Height in centimeters
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Weight in kilograms
        /// </summary>
        public int Weight { get; set; }

        // ====================================================================
        // GEOGRAPHY (5 new properties)
        // ====================================================================

        /// <summary>
        /// Birth city
        /// </summary>
        public string? BirthCity { get; set; }

        /// <summary>
        /// Birth country
        /// </summary>
        public string? BirthCountry { get; set; }

        /// <summary>
        /// Current residence city
        /// </summary>
        public string? ResidenceCity { get; set; }

        /// <summary>
        /// Current residence state/province
        /// </summary>
        public string? ResidenceState { get; set; }

        /// <summary>
        /// Current residence country
        /// </summary>
        public string? ResidenceCountry { get; set; }

        // ====================================================================
        // PHYSICAL ATTRIBUTES (3 new properties)
        // ====================================================================

        /// <summary>
        /// Path to worker's photo/headshot
        /// </summary>
        public string? PhotoPath { get; set; }

        /// <summary>
        /// Dominant hand
        /// </summary>
        public Handedness Handedness { get; set; } = Handedness.Right;

        /// <summary>
        /// Fighting stance
        /// </summary>
        public FightingStance FightingStance { get; set; } = FightingStance.Orthodox;

        // ====================================================================
        // GIMMICK & PUSH (5 new properties)
        // ====================================================================

        /// <summary>
        /// Current character/gimmick name or description
        /// </summary>
        public string? CurrentGimmick { get; set; }

        /// <summary>
        /// Character alignment (Face/Heel/Tweener)
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.Face;

        /// <summary>
        /// Current push level on the card
        /// </summary>
        public PushLevel PushLevel { get; set; } = PushLevel.MidCard;

        /// <summary>
        /// TV/Screen time role (0-100)
        /// Higher = more TV time
        /// </summary>
        public int TvRole { get; set; } = 50;

        /// <summary>
        /// Booker's booking intent notes for this worker
        /// </summary>
        public string? BookingIntent { get; set; }

        // ====================================================================
        // CAREER INFORMATION
        // ====================================================================

        /// <summary>
        /// Years of experience in the industry
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Is this worker currently employed by the company?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is this worker currently injured?
        /// </summary>
        public bool IsInjured { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES (11 models)
        // ====================================================================

        /// <summary>
        /// In-Ring performance attributes (10 attributes)
        /// </summary>
        public WorkerInRingAttributes? InRingAttributes { get; set; }

        /// <summary>
        /// Entertainment performance attributes (10 attributes)
        /// </summary>
        public WorkerEntertainmentAttributes? EntertainmentAttributes { get; set; }

        /// <summary>
        /// Story/Character attributes (10 attributes)
        /// </summary>
        public WorkerStoryAttributes? StoryAttributes { get; set; }

        /// <summary>
        /// Worker's specializations (Brawler, Technical, etc.)
        /// </summary>
        public List<WorkerSpecialization> Specializations { get; set; } = new();

        /// <summary>
        /// Relationships with other workers (as Worker1)
        /// </summary>
        public List<WorkerRelation> RelationsAsWorker1 { get; set; } = new();

        /// <summary>
        /// Relationships with other workers (as Worker2)
        /// </summary>
        public List<WorkerRelation> RelationsAsWorker2 { get; set; } = new();

        /// <summary>
        /// Faction memberships
        /// </summary>
        public List<FactionMember> FactionMemberships { get; set; } = new();

        /// <summary>
        /// Factions where this worker is the leader
        /// </summary>
        public List<Faction> LeadingFactions { get; set; } = new();

        /// <summary>
        /// Booker notes about this worker
        /// </summary>
        public List<WorkerNote> Notes { get; set; } = new();

        /// <summary>
        /// Contract history
        /// </summary>
        public List<ContractHistory> ContractHistory { get; set; } = new();

        /// <summary>
        /// Match history
        /// </summary>
        public List<MatchHistoryItem> MatchHistory { get; set; } = new();

        /// <summary>
        /// Title reigns
        /// </summary>
        public List<TitleReign> TitleReigns { get; set; } = new();

        // ====================================================================
        // CALCULATED PROPERTIES
        // ====================================================================

        /// <summary>
        /// Get all relations (combining Worker1 and Worker2 sides)
        /// </summary>
        public List<WorkerRelation> AllRelations
        {
            get
            {
                var relations = new List<WorkerRelation>();
                relations.AddRange(RelationsAsWorker1);
                relations.AddRange(RelationsAsWorker2);
                return relations;
            }
        }

        /// <summary>
        /// Get current active contract
        /// </summary>
        public ContractHistory? CurrentContract => ContractHistory?
            .Where(c => c.Status == ContractStatus.Active)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefault();

        /// <summary>
        /// Is this worker currently under contract?
        /// </summary>
        public bool HasActiveContract => CurrentContract != null;

        /// <summary>
        /// Get current faction memberships (active only)
        /// </summary>
        public List<FactionMember> CurrentFactionMemberships => FactionMemberships?
            .Where(fm => fm.IsActiveMember)
            .ToList() ?? new();

        /// <summary>
        /// Is this worker in a faction?
        /// </summary>
        public bool IsInFaction => CurrentFactionMemberships.Count > 0;

        /// <summary>
        /// Get current title reigns (championships currently held)
        /// </summary>
        public List<TitleReign> CurrentTitleReigns => TitleReigns?
            .Where(tr => tr.IsCurrentChampion)
            .ToList() ?? new();

        /// <summary>
        /// Is this worker currently a champion?
        /// </summary>
        public bool IsChampion => CurrentTitleReigns.Count > 0;

        /// <summary>
        /// Total career matches
        /// </summary>
        public int TotalMatches => MatchHistory?.Count ?? 0;

        /// <summary>
        /// Total career wins
        /// </summary>
        public int TotalWins => MatchHistory?.Count(m => m.Result == MatchResult.Win) ?? 0;

        /// <summary>
        /// Total career losses
        /// </summary>
        public int TotalLosses => MatchHistory?.Count(m => m.Result == MatchResult.Loss) ?? 0;

        /// <summary>
        /// Win percentage (0-100)
        /// </summary>
        public double WinPercentage
        {
            get
            {
                if (TotalMatches == 0) return 0;
                return Math.Round((double)TotalWins / TotalMatches * 100, 1);
            }
        }

        /// <summary>
        /// Overall average rating (average of 30 attributes)
        /// </summary>
        public int OverallRating
        {
            get
            {
                if (InRingAttributes == null || EntertainmentAttributes == null || StoryAttributes == null)
                    return 50; // Default

                return (InRingAttributes.InRingAvg +
                        EntertainmentAttributes.EntertainmentAvg +
                        StoryAttributes.StoryAvg) / 3;
            }
        }

        /// <summary>
        /// Get primary specialization (Level 1)
        /// </summary>
        public WorkerSpecialization? PrimarySpecialization => Specializations?
            .FirstOrDefault(s => s.Level == 1);

        /// <summary>
        /// Get full location text (City, State, Country)
        /// </summary>
        public string ResidenceFullText
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(ResidenceCity))
                    parts.Add(ResidenceCity);
                if (!string.IsNullOrWhiteSpace(ResidenceState))
                    parts.Add(ResidenceState);
                if (!string.IsNullOrWhiteSpace(ResidenceCountry))
                    parts.Add(ResidenceCountry);

                return parts.Count > 0 ? string.Join(", ", parts) : "Inconnu";
            }
        }

        /// <summary>
        /// Get birth location text (City, Country)
        /// </summary>
        public string BirthLocationText
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(BirthCity))
                    parts.Add(BirthCity);
                if (!string.IsNullOrWhiteSpace(BirthCountry))
                    parts.Add(BirthCountry);

                return parts.Count > 0 ? string.Join(", ", parts) : "Inconnu";
            }
        }

        /// <summary>
        /// Get alignment icon
        /// </summary>
        public string AlignmentIcon => Alignment switch
        {
            Alignment.Face => "😇",
            Alignment.Heel => "😈",
            Alignment.Tweener => "😐",
            _ => "?"
        };

        /// <summary>
        /// Get alignment display name (French)
        /// </summary>
        public string AlignmentDisplayName => Alignment switch
        {
            Alignment.Face => "Face",
            Alignment.Heel => "Heel",
            Alignment.Tweener => "Tweener",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get push level display name (French)
        /// </summary>
        public string PushLevelDisplayName => PushLevel switch
        {
            PushLevel.MainEvent => "Main Event",
            PushLevel.UpperMid => "Haut de Carte",
            PushLevel.MidCard => "Milieu de Carte",
            PushLevel.LowerMid => "Bas de Carte",
            PushLevel.Jobber => "Faire-Valoir",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get push level icon
        /// </summary>
        public string PushLevelIcon => PushLevel switch
        {
            PushLevel.MainEvent => "🌟",
            PushLevel.UpperMid => "⭐",
            PushLevel.MidCard => "✨",
            PushLevel.LowerMid => "💫",
            PushLevel.Jobber => "📉",
            _ => "?"
        };

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Validate worker data
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (Age < 18 || Age > 100) return false;
            if (Height < 100 || Height > 250) return false;
            if (Weight < 40 || Weight > 300) return false;
            if (TvRole < 0 || TvRole > 100) return false;
            if (Experience < 0) return false;

            return true;
        }

        /// <summary>
        /// Get worker display name (with real name if available)
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(RealName))
                return $"{Name} ({RealName})";
            return Name;
        }

        /// <summary>
        /// Get worker bio summary
        /// </summary>
        public string GetBioSummary()
        {
            var age = Age > 0 ? $"{Age} ans" : "?";
            var location = !string.IsNullOrWhiteSpace(BirthCountry) ? BirthCountry : "?";
            var exp = Experience > 0 ? $"{Experience} ans d'expérience" : "Débutant";

            return $"{age} • {location} • {exp}";
        }
    }
}

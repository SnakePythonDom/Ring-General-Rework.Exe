namespace RingGeneral.Core.Models.Relations
{
    /// <summary>
    /// Type of relationship between two wrestlers
    /// </summary>
    public enum RelationType
    {
        /// <summary>
        /// 🤝 Friendship - Professional respect and camaraderie
        /// </summary>
        Amitie,

        /// <summary>
        /// ❤ Romantic Relationship - Love interest storyline
        /// </summary>
        Couple,

        /// <summary>
        /// 👊 Brotherhood - Deep bond, tag team partnership
        /// </summary>
        Fraternite,

        /// <summary>
        /// ⚔ Rivalry - Ongoing feud or competition
        /// </summary>
        Rivalite
    }

    /// <summary>
    /// Represents a relationship between two wrestlers.
    /// Can be kayfabe (storyline) or backstage (real-life chemistry).
    /// </summary>
    public class WorkerRelation
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// First worker in the relationship (lower ID)
        /// </summary>
        public int WorkerId1 { get; set; }

        /// <summary>
        /// Second worker in the relationship (higher ID)
        /// </summary>
        public int WorkerId2 { get; set; }

        /// <summary>
        /// Type of relationship (Friendship, Romance, Brotherhood, Rivalry)
        /// </summary>
        public RelationType RelationType { get; set; }

        /// <summary>
        /// Strength of the relationship (0-100)
        /// - 0-39: Weak
        /// - 40-69: Medium
        /// - 70-89: Strong
        /// - 90-100: Very Strong
        /// </summary>
        public int RelationStrength { get; set; } = 50;

        /// <summary>
        /// Optional notes about the relationship
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Is this relationship public (kayfabe visible)?
        /// True = Visible in storylines
        /// False = Backstage chemistry only
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// When the relationship was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// First worker
        /// </summary>
        public Worker? Worker1 { get; set; }

        /// <summary>
        /// Second worker
        /// </summary>
        public Worker? Worker2 { get; set; }

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Get icon emoji for this relation type
        /// </summary>
        public string RelationTypeIcon => RelationType switch
        {
            RelationType.Amitie => "🤝",
            RelationType.Couple => "❤",
            RelationType.Fraternite => "👊",
            RelationType.Rivalite => "⚔",
            _ => "?"
        };

        /// <summary>
        /// Get strength text description
        /// </summary>
        public string RelationStrengthText => RelationStrength switch
        {
            >= 90 => "Très Fort",
            >= 70 => "Fort",
            >= 40 => "Moyen",
            _ => "Faible"
        };

        /// <summary>
        /// Is this a strong relationship? (>= 70)
        /// </summary>
        public bool IsStrongRelation => RelationStrength >= 70;

        /// <summary>
        /// Is this a medium relationship? (40-69)
        /// </summary>
        public bool IsMediumRelation => RelationStrength >= 40 && RelationStrength < 70;

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Check if this relation involves a specific worker
        /// </summary>
        public bool InvolvesWorker(int workerId)
        {
            return WorkerId1 == workerId || WorkerId2 == workerId;
        }

        /// <summary>
        /// Get the other worker ID in this relation
        /// </summary>
        public int GetOtherWorkerId(int workerId)
        {
            if (workerId == WorkerId1) return WorkerId2;
            if (workerId == WorkerId2) return WorkerId1;
            throw new ArgumentException("Worker ID not part of this relation");
        }

        /// <summary>
        /// Validate the relation
        /// </summary>
        public bool Validate()
        {
            return WorkerId1 > 0 &&
                   WorkerId2 > 0 &&
                   WorkerId1 != WorkerId2 &&
                   WorkerId1 < WorkerId2 && // Ensure proper ordering
                   RelationStrength >= 0 && RelationStrength <= 100;
        }
    }
}

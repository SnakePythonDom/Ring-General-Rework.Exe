namespace RingGeneral.Core.Models.Attributes
{
    /// <summary>
    /// Mental and psychological attributes for wrestlers.
    /// These attributes are HIDDEN by default and revealed through scouting.
    /// Scale: 0-20 (different from performance attributes which use 0-100)
    /// </summary>
    public class WorkerMentalAttributes
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Worker ID (Foreign Key to Workers table)
        /// </summary>
        public int WorkerId { get; set; }

        // ====================================================================
        // AMBITION & DRIVE (0-20)
        // ====================================================================

        /// <summary>
        /// Ambition: Desire for success, titles, and main event status.
        /// 0-5: Content with current position, no desire to climb
        /// 6-12: Moderate ambition, wants to improve but not obsessed
        /// 13-16: Highly ambitious, driven to reach the top
        /// 17-20: Ultra-competitive, will do anything for main event spot
        /// Affects: Contract negotiations, push satisfaction, backstage politics
        /// </summary>
        public int Ambition { get; set; } = 10;

        /// <summary>
        /// Détermination: Resilience and perseverance when facing adversity.
        /// 0-5: Gives up easily when things get tough
        /// 6-12: Average determination, can push through some obstacles
        /// 13-16: Very determined, doesn't back down
        /// 17-20: Unstoppable force, never discouraged
        /// Affects: Recovery from losses, reaction to creative burial, momentum retention
        /// </summary>
        public int Détermination { get; set; } = 10;

        // ====================================================================
        // LOYALTY & PROFESSIONALISM (0-20)
        // ====================================================================

        /// <summary>
        /// Loyauté: Faithfulness to company and colleagues.
        /// 0-5: Mercenary, will leave for any better offer
        /// 6-12: Conditional loyalty, can be swayed
        /// 13-16: Loyal to company and friends
        /// 17-20: Absolute loyalty, company lifer
        /// Affects: Contract renewal, competing offers, faction stability
        /// </summary>
        public int Loyauté { get; set; } = 10;

        /// <summary>
        /// Professionnalisme: Work ethic, respect for the business, reliability.
        /// 0-5: Lazy, problematic, misses shows
        /// 6-12: Basic professionalism, does the minimum
        /// 13-16: Very professional, reliable worker
        /// 17-20: Model professional, sets the standard
        /// Affects: Locker room morale, booker trust, training consistency
        /// </summary>
        public int Professionnalisme { get; set; } = 10;

        /// <summary>
        /// Sportivité: Fair play, respect for opponents, willingness to put others over.
        /// 0-5: Cheater, saboteur, refuses to make others look good
        /// 6-12: Basic sportsmanship
        /// 13-16: Fair player, respects the craft
        /// 17-20: Ultimate teammate, elevates everyone
        /// Affects: Willingness to job, match quality with lower card workers, respect
        /// </summary>
        public int Sportivité { get; set; } = 10;

        // ====================================================================
        // PRESSURE & TEMPERAMENT (0-20)
        // ====================================================================

        /// <summary>
        /// Pression: Ability to perform under pressure and in big moments.
        /// 0-5: Chokes in important matches, crumbles under spotlight
        /// 6-12: Inconsistent under pressure
        /// 13-16: Reliable in big matches
        /// 17-20: Clutch performer, thrives in main events and PPVs
        /// Affects: Match quality in PPVs, title match performance, live TV consistency
        /// </summary>
        public int Pression { get; set; } = 10;

        /// <summary>
        /// Tempérament: Emotional control and calmness backstage.
        /// 0-5: Explosive, frequent backstage fights and incidents
        /// 6-12: Can lose temper occasionally
        /// 13-16: Calm and composed
        /// 17-20: Zen master, never loses cool
        /// Affects: Backstage incidents, conflict probability, locker room harmony
        /// </summary>
        public int Tempérament { get; set; } = 10;

        // ====================================================================
        // EGO & ADAPTABILITY (0-20)
        // ====================================================================

        /// <summary>
        /// Égoïsme: Self-centeredness, prioritizing personal success over team.
        /// 0-5: Completely selfless, always puts others first
        /// 6-12: Balanced ego, team player
        /// 13-16: Self-centered, prioritizes own push
        /// 17-20: Massive ego, refuses to lose, backstage politician
        /// Affects: Booking flexibility, willingness to lose, creative control demands
        /// </summary>
        public int Égoïsme { get; set; } = 10;

        /// <summary>
        /// Adaptabilité: Flexibility to change roles, styles, and gimmicks.
        /// 0-5: One-trick pony, can't adapt
        /// 6-12: Limited adaptability
        /// 13-16: Versatile, can play multiple roles
        /// 17-20: Chameleon, masters any character
        /// Affects: Gimmick change success, heel/face turns, style variety
        /// </summary>
        public int Adaptabilité { get; set; } = 10;

        // ====================================================================
        // INFLUENCE (0-20)
        // ====================================================================

        /// <summary>
        /// Influence: Backstage power and ability to affect creative decisions.
        /// 0-5: No backstage pull, follows all directives
        /// 6-12: Some respect, minimal influence
        /// 13-16: Veteran respect, consulted on storylines
        /// 17-20: Booker in the shadows, has creative control
        /// Affects: Creative control, ability to refuse angles, locker room leadership
        /// </summary>
        public int Influence { get; set; } = 10;

        // ====================================================================
        // METADATA
        // ====================================================================

        /// <summary>
        /// Have these mental attributes been revealed through scouting?
        /// False = Hidden from player, True = Visible in UI
        /// </summary>
        public bool IsRevealed { get; set; } = false;

        /// <summary>
        /// Scouting completion level:
        /// 0 = Not scouted
        /// 1 = Basic scout (4 pillars visible: Professionnalisme, Pression, Égoïsme, Influence)
        /// 2 = Full scout (all 10 attributes visible)
        /// </summary>
        public int ScoutingLevel { get; set; } = 0;

        /// <summary>
        /// Last time these attributes were updated (mental attributes can evolve)
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Navigation property to Worker
        /// </summary>
        public Worker? Worker { get; set; }

        // ====================================================================
        // CALCULATED PROPERTIES - 4 PILLARS FOR AGENT REPORT
        // ====================================================================

        /// <summary>
        /// Pillar 1: Professionnalisme Score (0-20)
        /// Average of: Professionnalisme, Sportivité, Loyauté
        /// </summary>
        public double ProfessionnalismeScore =>
            Math.Round((Professionnalisme + Sportivité + Loyauté) / 3.0, 1);

        /// <summary>
        /// Pillar 2: Gestion de la Pression Score (0-20)
        /// Average of: Pression, Détermination
        /// </summary>
        public double PressionScore =>
            Math.Round((Pression + Détermination) / 2.0, 1);

        /// <summary>
        /// Pillar 3: Niveau d'Égo Score (0-20)
        /// Direct value: Égoïsme
        /// </summary>
        public double ÉgoïsmeScore =>
            Math.Round((double)Égoïsme, 1);

        /// <summary>
        /// Pillar 4: Influence Backstage Score (0-20)
        /// Average of: Influence, Tempérament (high temperament = calm = more influence)
        /// </summary>
        public double InfluenceScore =>
            Math.Round((Influence + Tempérament) / 2.0, 1);

        /// <summary>
        /// Overall Mental Average (0-20)
        /// Average of all 10 mental attributes
        /// </summary>
        public double MentalAverage =>
            Math.Round((Ambition + Loyauté + Professionnalisme + Pression + Tempérament +
                        Égoïsme + Détermination + Adaptabilité + Influence + Sportivité) / 10.0, 1);

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Get attribute value by name (for dynamic access)
        /// </summary>
        public int GetAttributeValue(string attributeName)
        {
            return attributeName switch
            {
                nameof(Ambition) => Ambition,
                nameof(Loyauté) => Loyauté,
                nameof(Professionnalisme) => Professionnalisme,
                nameof(Pression) => Pression,
                nameof(Tempérament) => Tempérament,
                nameof(Égoïsme) => Égoïsme,
                nameof(Détermination) => Détermination,
                nameof(Adaptabilité) => Adaptabilité,
                nameof(Influence) => Influence,
                nameof(Sportivité) => Sportivité,
                _ => 0
            };
        }

        /// <summary>
        /// Set attribute value by name (for dynamic access)
        /// </summary>
        public void SetAttributeValue(string attributeName, int value)
        {
            // Clamp value between 0 and 20 (mental attributes use 0-20 scale)
            value = Math.Clamp(value, 0, 20);

            switch (attributeName)
            {
                case nameof(Ambition): Ambition = value; break;
                case nameof(Loyauté): Loyauté = value; break;
                case nameof(Professionnalisme): Professionnalisme = value; break;
                case nameof(Pression): Pression = value; break;
                case nameof(Tempérament): Tempérament = value; break;
                case nameof(Égoïsme): Égoïsme = value; break;
                case nameof(Détermination): Détermination = value; break;
                case nameof(Adaptabilité): Adaptabilité = value; break;
                case nameof(Influence): Influence = value; break;
                case nameof(Sportivité): Sportivité = value; break;
            }

            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Validate that all attributes are within valid range (0-20)
        /// </summary>
        public bool Validate()
        {
            return Ambition >= 0 && Ambition <= 20 &&
                   Loyauté >= 0 && Loyauté <= 20 &&
                   Professionnalisme >= 0 && Professionnalisme <= 20 &&
                   Pression >= 0 && Pression <= 20 &&
                   Tempérament >= 0 && Tempérament <= 20 &&
                   Égoïsme >= 0 && Égoïsme <= 20 &&
                   Détermination >= 0 && Détermination <= 20 &&
                   Adaptabilité >= 0 && Adaptabilité <= 20 &&
                   Influence >= 0 && Influence <= 20 &&
                   Sportivité >= 0 && Sportivité <= 20;
        }

        /// <summary>
        /// Reveal attributes through scouting
        /// </summary>
        /// <param name="level">1 = Basic (4 pillars), 2 = Full (all 10)</param>
        public void RevealThroughScouting(int level)
        {
            ScoutingLevel = Math.Clamp(level, 0, 2);
            if (level > 0)
            {
                IsRevealed = true;
            }
        }

        /// <summary>
        /// Get all attributes as dictionary (for serialization/display)
        /// </summary>
        public Dictionary<string, int> GetAllAttributes()
        {
            return new Dictionary<string, int>
            {
                [nameof(Ambition)] = Ambition,
                [nameof(Loyauté)] = Loyauté,
                [nameof(Professionnalisme)] = Professionnalisme,
                [nameof(Pression)] = Pression,
                [nameof(Tempérament)] = Tempérament,
                [nameof(Égoïsme)] = Égoïsme,
                [nameof(Détermination)] = Détermination,
                [nameof(Adaptabilité)] = Adaptabilité,
                [nameof(Influence)] = Influence,
                [nameof(Sportivité)] = Sportivité
            };
        }
    }
}

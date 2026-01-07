namespace RingGeneral.Core.Models.Attributes
{
    /// <summary>
    /// In-Ring performance attributes for wrestlers.
    /// Represents technical skills, physical abilities, and ring psychology.
    /// </summary>
    public class WorkerInRingAttributes
    {
        /// <summary>
        /// Worker ID (Foreign Key to Workers table)
        /// </summary>
        public int WorkerId { get; set; }

        // ====================================================================
        // STRIKING & COMBAT STYLES (0-100)
        // ====================================================================

        /// <summary>
        /// Striking: Precision and impact of strikes (punches, kicks, elbows, knees).
        /// Influences the credibility of striking sequences in matches.
        /// </summary>
        public int Striking { get; set; } = 50;

        /// <summary>
        /// Grappling: Mastery of ground wrestling, mat wrestling, and submissions.
        /// Determines quality of technical sequences.
        /// </summary>
        public int Grappling { get; set; } = 50;

        /// <summary>
        /// High-Flying: Agility, acrobatics, and aerial moves.
        /// Essential for high-risk, high-reward performances.
        /// </summary>
        public int HighFlying { get; set; } = 50;

        /// <summary>
        /// Powerhouse: Ability to lift heavy opponents and execute power moves.
        /// Raw strength and impact.
        /// </summary>
        public int Powerhouse { get; set; } = 50;

        // ====================================================================
        // TECHNICAL EXECUTION (0-100)
        // ====================================================================

        /// <summary>
        /// Timing: Surgical precision in move execution and sequencing.
        /// Critical for match flow and storytelling.
        /// </summary>
        public int Timing { get; set; } = 50;

        /// <summary>
        /// Selling: Ability to make opponent's offense look credible and impactful.
        /// Essential for believable matches.
        /// </summary>
        public int Selling { get; set; } = 50;

        /// <summary>
        /// Psychology: Understanding of match structure and in-ring storytelling.
        /// Ability to construct a logical narrative through wrestling.
        /// </summary>
        public int Psychology { get; set; } = 50;

        // ====================================================================
        // PHYSICAL ATTRIBUTES (0-100)
        // ====================================================================

        /// <summary>
        /// Stamina: Endurance to maintain high-quality performance for 30+ minutes.
        /// Critical for main event matches.
        /// </summary>
        public int Stamina { get; set; } = 50;

        /// <summary>
        /// Safety: Ability to protect opponents and minimize injury risk.
        /// Higher values = more trust from peers.
        /// </summary>
        public int Safety { get; set; } = 50;

        /// <summary>
        /// Hardcore/Brawl: Proficiency with weapons and brawling style.
        /// Important for hardcore/street fight matches.
        /// </summary>
        public int HardcoreBrawl { get; set; } = 50;

        // ====================================================================
        // CALCULATED PROPERTIES
        // ====================================================================

        /// <summary>
        /// In-Ring Average: Calculated average of all 10 in-ring attributes.
        /// Represents overall in-ring ability (0-100).
        /// </summary>
        public int InRingAvg => (Striking + Grappling + HighFlying + Powerhouse +
                                  Timing + Selling + Psychology + Stamina +
                                  Safety + HardcoreBrawl) / 10;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Navigation property to Worker
        /// </summary>
        public Worker? Worker { get; set; }

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
                "Striking" => Striking,
                "Grappling" => Grappling,
                "HighFlying" => HighFlying,
                "Powerhouse" => Powerhouse,
                "Timing" => Timing,
                "Selling" => Selling,
                "Psychology" => Psychology,
                "Stamina" => Stamina,
                "Safety" => Safety,
                "HardcoreBrawl" => HardcoreBrawl,
                _ => 0
            };
        }

        /// <summary>
        /// Set attribute value by name (for dynamic access)
        /// </summary>
        public void SetAttributeValue(string attributeName, int value)
        {
            // Clamp value between 0 and 100
            value = Math.Clamp(value, 0, 100);

            switch (attributeName)
            {
                case "Striking": Striking = value; break;
                case "Grappling": Grappling = value; break;
                case "HighFlying": HighFlying = value; break;
                case "Powerhouse": Powerhouse = value; break;
                case "Timing": Timing = value; break;
                case "Selling": Selling = value; break;
                case "Psychology": Psychology = value; break;
                case "Stamina": Stamina = value; break;
                case "Safety": Safety = value; break;
                case "HardcoreBrawl": HardcoreBrawl = value; break;
            }
        }

        /// <summary>
        /// Validate that all attributes are within valid range (0-100)
        /// </summary>
        public bool Validate()
        {
            return Striking >= 0 && Striking <= 100 &&
                   Grappling >= 0 && Grappling <= 100 &&
                   HighFlying >= 0 && HighFlying <= 100 &&
                   Powerhouse >= 0 && Powerhouse <= 100 &&
                   Timing >= 0 && Timing <= 100 &&
                   Selling >= 0 && Selling <= 100 &&
                   Psychology >= 0 && Psychology <= 100 &&
                   Stamina >= 0 && Stamina <= 100 &&
                   Safety >= 0 && Safety <= 100 &&
                   HardcoreBrawl >= 0 && HardcoreBrawl <= 100;
        }
    }
}

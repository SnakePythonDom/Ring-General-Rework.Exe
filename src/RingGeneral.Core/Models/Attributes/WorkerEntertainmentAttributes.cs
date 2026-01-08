namespace RingGeneral.Core.Models.Attributes
{
    /// <summary>
    /// Entertainment performance attributes for wrestlers.
    /// Represents charisma, mic skills, presence, and audience connection.
    /// </summary>
    public class WorkerEntertainmentAttributes
    {
        /// <summary>
        /// Worker ID (Foreign Key to Workers table)
        /// </summary>
        public int WorkerId { get; set; }

        // ====================================================================
        // PRESENCE & CHARISMA (0-100)
        // ====================================================================

        /// <summary>
        /// Charisma: Natural magnetism and presence, even without speaking.
        /// The "It Factor" that draws audience attention.
        /// </summary>
        public int Charisma { get; set; } = 50;

        /// <summary>
        /// Mic Work (Promo): Verbal skills and ability to deliver scripted promos.
        /// Critical for storyline advancement and character development.
        /// </summary>
        public int MicWork { get; set; } = 50;

        /// <summary>
        /// Acting: Credibility in facial expressions and backstage segments.
        /// Ability to convey emotion and tell stories outside the ring.
        /// </summary>
        public int Acting { get; set; } = 50;

        /// <summary>
        /// Crowd Connection: Ability to elicit reactions (heat or cheers).
        /// Measures how effectively the wrestler controls crowd emotions.
        /// </summary>
        public int CrowdConnection { get; set; } = 50;

        // ====================================================================
        // STAR POWER & PRESENCE (0-100)
        // ====================================================================

        /// <summary>
        /// Star Power: The aura of being a "Main Eventer".
        /// Look, presence, and overall package.
        /// </summary>
        public int StarPower { get; set; } = 50;

        /// <summary>
        /// Improvisation: Ability to react to unexpected situations or crowd chants.
        /// Essential for live performances and handling mistakes.
        /// </summary>
        public int Improvisation { get; set; } = 50;

        /// <summary>
        /// Entrance: Visual impact and theatricality of ring entrance.
        /// First impression and atmosphere creation.
        /// </summary>
        public int Entrance { get; set; } = 50;

        // ====================================================================
        // MARKETABILITY (0-100)
        // ====================================================================

        /// <summary>
        /// Sex Appeal / Cool Factor: Aesthetic appeal or "trendy" factor.
        /// Marketability through appearance and style.
        /// </summary>
        public int SexAppeal { get; set; } = 50;

        /// <summary>
        /// Merchandise Appeal: Potential to sell products (shirts, toys, etc.).
        /// Logo design, catchphrases, and brand potential.
        /// </summary>
        public int MerchandiseAppeal { get; set; } = 50;

        /// <summary>
        /// Crossover Potential: Ability to attract non-wrestling audiences.
        /// Potential for movies, TV, mainstream media.
        /// </summary>
        public int CrossoverPotential { get; set; } = 50;

        // ====================================================================
        // CALCULATED PROPERTIES
        // ====================================================================

        /// <summary>
        /// Entertainment Average: Calculated average of all 10 entertainment attributes.
        /// Represents overall entertainment value (0-100).
        /// </summary>
        public int EntertainmentAvg => (Charisma + MicWork + Acting + CrowdConnection +
                                         StarPower + Improvisation + Entrance + SexAppeal +
                                         MerchandiseAppeal + CrossoverPotential) / 10;

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
                "Charisma" => Charisma,
                "MicWork" => MicWork,
                "Acting" => Acting,
                "CrowdConnection" => CrowdConnection,
                "StarPower" => StarPower,
                "Improvisation" => Improvisation,
                "Entrance" => Entrance,
                "SexAppeal" => SexAppeal,
                "MerchandiseAppeal" => MerchandiseAppeal,
                "CrossoverPotential" => CrossoverPotential,
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
                case "Charisma": Charisma = value; break;
                case "MicWork": MicWork = value; break;
                case "Acting": Acting = value; break;
                case "CrowdConnection": CrowdConnection = value; break;
                case "StarPower": StarPower = value; break;
                case "Improvisation": Improvisation = value; break;
                case "Entrance": Entrance = value; break;
                case "SexAppeal": SexAppeal = value; break;
                case "MerchandiseAppeal": MerchandiseAppeal = value; break;
                case "CrossoverPotential": CrossoverPotential = value; break;
            }
        }

        /// <summary>
        /// Validate that all attributes are within valid range (0-100)
        /// </summary>
        public bool Validate()
        {
            return Charisma >= 0 && Charisma <= 100 &&
                   MicWork >= 0 && MicWork <= 100 &&
                   Acting >= 0 && Acting <= 100 &&
                   CrowdConnection >= 0 && CrowdConnection <= 100 &&
                   StarPower >= 0 && StarPower <= 100 &&
                   Improvisation >= 0 && Improvisation <= 100 &&
                   Entrance >= 0 && Entrance <= 100 &&
                   SexAppeal >= 0 && SexAppeal <= 100 &&
                   MerchandiseAppeal >= 0 && MerchandiseAppeal <= 100 &&
                   CrossoverPotential >= 0 && CrossoverPotential <= 100;
        }
    }
}

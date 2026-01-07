namespace RingGeneral.Core.Models.Attributes
{
    /// <summary>
    /// Story/Character performance attributes for wrestlers.
    /// Represents character depth, storytelling ability, and narrative versatility.
    /// </summary>
    public class WorkerStoryAttributes
    {
        /// <summary>
        /// Worker ID (Foreign Key to Workers table)
        /// </summary>
        public int WorkerId { get; set; }

        // ====================================================================
        // CHARACTER & GIMMICK (0-100)
        // ====================================================================

        /// <summary>
        /// Character Depth: Complexity and nuances of the character.
        /// Not just "good guy" or "bad guy" - layers and motivations.
        /// </summary>
        public int CharacterDepth { get; set; } = 50;

        /// <summary>
        /// Consistency: Faithfulness to character over long-term.
        /// Ability to maintain character integrity across storylines.
        /// </summary>
        public int Consistency { get; set; } = 50;

        /// <summary>
        /// Heel Performance: Effectiveness as an antagonist/villain.
        /// Ability to generate heat and make audiences hate the character.
        /// </summary>
        public int HeelPerformance { get; set; } = 50;

        /// <summary>
        /// Babyface Performance: Effectiveness as a hero/protagonist.
        /// Ability to generate cheers and sympathy from audiences.
        /// </summary>
        public int BabyfacePerformance { get; set; } = 50;

        // ====================================================================
        // STORYTELLING ABILITY (0-100)
        // ====================================================================

        /// <summary>
        /// Storytelling (Long-term): Ability to carry a rivalry over several months.
        /// Maintaining audience interest through extended narratives.
        /// </summary>
        public int StorytellingLongTerm { get; set; } = 50;

        /// <summary>
        /// Emotional Range: Ability to generate various emotions.
        /// Sadness, fear, joy, anger - full emotional spectrum.
        /// </summary>
        public int EmotionalRange { get; set; } = 50;

        /// <summary>
        /// Adaptability: Ease of changing gimmick or evolving character.
        /// Versatility in character portrayal.
        /// </summary>
        public int Adaptability { get; set; } = 50;

        /// <summary>
        /// Rivalry Chemistry: Natural ability to create spark with any opponent.
        /// Some wrestlers just "click" with certain opponents.
        /// </summary>
        public int RivalryChemistry { get; set; } = 50;

        // ====================================================================
        // CREATIVE INPUT (0-100)
        // ====================================================================

        /// <summary>
        /// Creative Input: Wrestler's involvement in their own storyline ideas.
        /// Ability to pitch and develop compelling narratives.
        /// </summary>
        public int CreativeInput { get; set; } = 50;

        /// <summary>
        /// Moral Alignment: Ability to play "Tweener" (morally ambiguous).
        /// Comfort in the gray area between hero and villain.
        /// Higher values = better at playing complex moral characters.
        /// </summary>
        public int MoralAlignment { get; set; } = 50;

        // ====================================================================
        // CALCULATED PROPERTIES
        // ====================================================================

        /// <summary>
        /// Story Average: Calculated average of all 10 story attributes.
        /// Represents overall storytelling and character ability (0-100).
        /// </summary>
        public int StoryAvg => (CharacterDepth + Consistency + HeelPerformance + BabyfacePerformance +
                                 StorytellingLongTerm + EmotionalRange + Adaptability + RivalryChemistry +
                                 CreativeInput + MoralAlignment) / 10;

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
                "CharacterDepth" => CharacterDepth,
                "Consistency" => Consistency,
                "HeelPerformance" => HeelPerformance,
                "BabyfacePerformance" => BabyfacePerformance,
                "StorytellingLongTerm" => StorytellingLongTerm,
                "EmotionalRange" => EmotionalRange,
                "Adaptability" => Adaptability,
                "RivalryChemistry" => RivalryChemistry,
                "CreativeInput" => CreativeInput,
                "MoralAlignment" => MoralAlignment,
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
                case "CharacterDepth": CharacterDepth = value; break;
                case "Consistency": Consistency = value; break;
                case "HeelPerformance": HeelPerformance = value; break;
                case "BabyfacePerformance": BabyfacePerformance = value; break;
                case "StorytellingLongTerm": StorytellingLongTerm = value; break;
                case "EmotionalRange": EmotionalRange = value; break;
                case "Adaptability": Adaptability = value; break;
                case "RivalryChemistry": RivalryChemistry = value; break;
                case "CreativeInput": CreativeInput = value; break;
                case "MoralAlignment": MoralAlignment = value; break;
            }
        }

        /// <summary>
        /// Validate that all attributes are within valid range (0-100)
        /// </summary>
        public bool Validate()
        {
            return CharacterDepth >= 0 && CharacterDepth <= 100 &&
                   Consistency >= 0 && Consistency <= 100 &&
                   HeelPerformance >= 0 && HeelPerformance <= 100 &&
                   BabyfacePerformance >= 0 && BabyfacePerformance <= 100 &&
                   StorytellingLongTerm >= 0 && StorytellingLongTerm <= 100 &&
                   EmotionalRange >= 0 && EmotionalRange <= 100 &&
                   Adaptability >= 0 && Adaptability <= 100 &&
                   RivalryChemistry >= 0 && RivalryChemistry <= 100 &&
                   CreativeInput >= 0 && CreativeInput <= 100 &&
                   MoralAlignment >= 0 && MoralAlignment <= 100;
        }
    }
}

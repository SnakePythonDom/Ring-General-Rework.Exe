using System;

namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Represents a match for a championship title.
    /// Tracks defenses and title changes.
    /// </summary>
    public class TitleMatch
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int TitleMatchId { get; set; }

        /// <summary>
        /// Title ID (Foreign Key)
        /// </summary>
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// Show ID (Foreign Key)
        /// </summary>
        public string? ShowId { get; set; }

        /// <summary>
        /// Game week number
        /// </summary>
        public int Week { get; set; }

        /// <summary>
        /// Defending champion ID (Foreign Key)
        /// </summary>
        public string? ChampionId { get; set; }

        /// <summary>
        /// Challenger ID (Foreign Key)
        /// </summary>
        public string ChallengerId { get; set; } = string.Empty;

        /// <summary>
        /// Winner ID (Foreign Key)
        /// </summary>
        public string WinnerId { get; set; } = string.Empty;

        /// <summary>
        /// Did the title change hands?
        /// </summary>
        public bool IsTitleChange { get; set; }

        /// <summary>
        /// Change in title prestige calculated after the match
        /// </summary>
        public int PrestigeDelta { get; set; }

        /// <summary>
        /// Date created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Was this a successful defense?
        /// </summary>
        public bool IsSuccessfulDefense => !IsTitleChange && WinnerId == ChampionId;

        /// <summary>
        /// Get match result description
        /// </summary>
        public string GetResultDescription()
        {
            if (IsTitleChange)
                return "Nouveau champion !";
            return "Titre conservé";
        }
    }
}

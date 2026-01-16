using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces
{
    /// <summary>
    /// Service for managing the natural evolution of relationships between workers over time.
    /// Handles growth, decay, and random social events (marriages, split-ups, new friends).
    /// </summary>
    public interface IRelationshipEvolutionService
    {
        /// <summary>
        /// Processes the weekly evolution of all relationships in the universe.
        /// </summary>
        /// <param name="week">The current game week.</param>
        /// <param name="currentDate">The current game date.</param>
        Task ProcessWeeklyEvolutionAsync(int week, DateTime currentDate);

        /// <summary>
        /// Triggers a specific life event between two workers.
        /// </summary>
        Task TriggerLifeEventAsync(string workerId1, string workerId2, string eventType);
    }
}

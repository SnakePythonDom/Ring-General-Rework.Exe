using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services
{
    public class RelationshipEvolutionService : IRelationshipEvolutionService
    {
        private readonly IRelationsRepository _relationsRepository;
        private readonly IWorkerRepository _workerRepository;
        private readonly IFactionRepository _factionRepository;
        private readonly IRandomProvider _random;
        private readonly System.Random _sysRandom = new System.Random();

        public RelationshipEvolutionService(
            IRelationsRepository relationsRepository,
            IWorkerRepository workerRepository,
            IFactionRepository factionRepository,
            IRandomProvider random)
        {
            _relationsRepository = relationsRepository;
            _workerRepository = workerRepository;
            _factionRepository = factionRepository;
            _random = random;
        }

        public async Task ProcessWeeklyEvolutionAsync(int week, DateTime currentDate)
        {
            var relations = _relationsRepository.GetAllRelations();

            foreach (var relation in relations)
            {
                await EvolveRelationAsync(relation);
            }

            // Also check for NEW random relations (small chance per week)
            // This is computationally expensive if done for EVERY pair, 
            // so we might just pick a few random workers.
            await SeedNewRandomRelationshipsAsync();
        }

        private async Task EvolveRelationAsync(WorkerRelation relation)
        {
            await Task.CompletedTask;
            var worker1 = _workerRepository.GetWorker(relation.WorkerId1);
            var worker2 = _workerRepository.GetWorker(relation.WorkerId2);

            if (worker1 == null || worker2 == null) return;

            // 1. Natural Growth/Decay
            // If they are on the same roster, relationship usually grows or stays stable
            if (worker1.CompanyId == worker2.CompanyId && worker1.CompanyId != null)
            {
                // Same roster = Growth chance
                var chance = 0.1; // Default 10%

                // If they share a faction/team, they grow faster (25% chance)
                var sharedFactions = _factionRepository.GetFactionsForWorker(worker1.WorkerId);
                var commonFaction = sharedFactions.Any(f => f.Members.Any(m => m.WorkerId == worker2.WorkerId && m.IsActiveMember));

                if (commonFaction)
                {
                    chance = 0.25;
                }

                if (_sysRandom.NextDouble() < chance)
                {
                    relation.RelationStrength = Math.Min(100, relation.RelationStrength + 1);
                }
            }
            else
            {
                // Different rosters = Decay chance (unless family)
                if (relation.RelationType != RelationType.Fraternite && _sysRandom.NextDouble() < 0.05)
                {
                    relation.RelationStrength = Math.Max(1, relation.RelationStrength - 1);
                }
            }

            // 2. Trigger "Life Events" (Very rare)
            if (_sysRandom.NextDouble() < 0.001) // 0.1% chance for a major change per week
            {
                await TriggerRandomLifeEventAsync(relation);
            }

            _relationsRepository.UpdateRelation(relation);
            await Task.CompletedTask;
        }

        private async Task TriggerRandomLifeEventAsync(WorkerRelation relation)
        {
            await Task.CompletedTask;
            // Friendship -> Brotherhood or Romance?
            // Romance -> Breakup or Marriage (Notes only for now)?

            if (relation.RelationType == RelationType.Amitie && relation.RelationStrength > 80)
            {
                // Deep friendship turns into Brotherhood (Kayfabe usually, but here real bond)
                relation.RelationType = RelationType.Fraternite;
                relation.Notes = "Devenus inséparables.";
            }
            else if (relation.RelationType == RelationType.Couple && _sysRandom.NextDouble() < 0.1)
            {
                // Breakup
                relation.RelationType = RelationType.Rivalite;
                relation.RelationStrength = 20;
                relation.Notes = "Séparation difficile.";
            }
        }

        public async Task TriggerLifeEventAsync(string workerId1, string workerId2, string eventType)
        {
            // Force a specific event (CM Tool or special interaction)
            var relation = _relationsRepository.GetRelationsForWorker(workerId1)
                .FirstOrDefault(r => r.InvolvesWorker(workerId2));

            if (relation == null) return; // Need existing relation to trigger event on

            switch (eventType.ToLower())
            {
                case "marriage":
                    relation.Notes = "Mariés";
                    relation.RelationStrength = 100;
                    break;
                case "breakup":
                    relation.RelationType = RelationType.Rivalite;
                    relation.RelationStrength = 10;
                    break;
            }

            _relationsRepository.UpdateRelation(relation);
            await Task.CompletedTask;
        }

        private async Task SeedNewRandomRelationshipsAsync()
        {
            // Pick a few workers and see if they become friends
            // This would normally be handled by segments, but "World Evolution" needs some background glue.
            await Task.CompletedTask;
        }
    }
}

using RingGeneral.Core.Interfaces;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Relations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerRelationsViewModel : ViewModelBase
    {
        private readonly Worker _worker;
        private readonly IReadOnlyDictionary<string, string> _workerNames;
        private readonly IGameRepository _repository;

        public ObservableCollection<RelationDisplayItem> Relations { get; } = new();

        public ReactiveCommand<Unit, Unit> TalkCommand { get; }
        public ReactiveCommand<Unit, Unit> AllianceCommand { get; }
        public ReactiveCommand<Unit, Unit> RivalCommand { get; }

        public string FactionName => "The Power Trip"; // Placeholder
        public string FactionRole => "Leader"; // Placeholder
        public string FactionDescription => "A dominant faction controlling the main event scene."; // Placeholder

        // New properties for Mockup
        public string CloseAlliesDisplay => string.Join(", ", Relations.Where(r => r.RelationTypeName == "Amitie" || r.RelationTypeName == "Fraternite").Select(r => r.OtherWorkerName).DefaultIfEmpty("None"));
        public string RivalsDisplay => string.Join(", ", Relations.Where(r => r.RelationTypeName == "Rivalite").Select(r => r.OtherWorkerName).DefaultIfEmpty("None"));
        public string MentorsDisplay => string.Join(", ", Relations.Where(r => r.RelationTypeName == "Protege").Select(r => r.OtherWorkerName).DefaultIfEmpty("None"));

        public string InfluenceVestiaire => "50 (Neutral)"; // Placeholder
        public string RespectStaff => "75%"; // Placeholder
        public string TagTeamName => "None"; // Placeholder
        public string TagTeamChemistry => "Average"; // Placeholder

        public WorkerRelationsViewModel(
            Worker worker,
            IReadOnlyDictionary<string, string> workerNames,
            IGameRepository repository)
        {
            _worker = worker;
            _workerNames = workerNames;
            _repository = repository;

            // Initialize Lists
            LoadRelations();

            // Initialize Commands
            TalkCommand = ReactiveCommand.Create(() =>
            {
                var result = _repository.PersonalityEngine.ProcessInteraction(_worker, InteractionType.Talk);
                Logger.Info(result.Message);
                System.Diagnostics.Debug.WriteLine($"Talk Result: {result.Success} - {result.Message}");
            });

            AllianceCommand = ReactiveCommand.Create(() =>
            {
                var result = _repository.PersonalityEngine.ProcessInteraction(_worker, InteractionType.Alliance);
                Logger.Info(result.Message);

                if (result.Success)
                {
                    // Create a "Amitie" if none exists with a dummy/player char
                    string playerWorkerId = "W_PLAYER"; // Placeholder for player character ID

                    var relation = new WorkerRelation
                    {
                        WorkerId1 = !string.IsNullOrEmpty(_worker.WorkerId) ? _worker.WorkerId : _worker.Id.ToString(),
                        WorkerId2 = playerWorkerId,
                        RelationType = RelationType.Amitie,
                        RelationStrength = 50,
                        Notes = "Alliance acceptée via interaction profil.",
                        IsPublic = true
                    };

                    _repository.AddOrUpdateRelation(relation);
                    LoadRelations();
                }
            });

            RivalCommand = ReactiveCommand.Create(() =>
            {
                var result = _repository.PersonalityEngine.ProcessInteraction(_worker, InteractionType.Rivalry);
                Logger.Info(result.Message);

                string playerWorkerId = "W_PLAYER";

                var relation = new WorkerRelation
                {
                    WorkerId1 = !string.IsNullOrEmpty(_worker.WorkerId) ? _worker.WorkerId : _worker.Id.ToString(),
                    WorkerId2 = playerWorkerId,
                    RelationType = RelationType.Rivalite,
                    RelationStrength = 40,
                    Notes = "Rivalité déclarée via le profil.",
                    IsPublic = true
                };

                _repository.AddOrUpdateRelation(relation);

                // Potential morale impact from result
                if (result.MoraleChange != 0)
                {
                    // For now just log it, morale system integration comes later in Phase 3
                    Logger.Warning($"Impact sur le moral : {result.MoraleChange}");
                }

                LoadRelations();
            });
        }

        private void LoadRelations()
        {
            Relations.Clear();
            if (_worker.AllRelations != null)
            {
                foreach (var rel in _worker.AllRelations)
                {
                    // Identify the 'other' worker ID
                    var workerId = !string.IsNullOrEmpty(_worker.WorkerId) ? _worker.WorkerId : _worker.Id.ToString();
                    string otherId;
                    try
                    {
                        otherId = rel.GetOtherWorkerId(workerId);
                    }
                    catch
                    {
                        // Fallback if ID mismatch (e.g. data issue)
                        continue;
                    }

                    // Resolve name
                    var otherName = _workerNames.TryGetValue(otherId, out var name) ? name : $"Unknown ({otherId})";

                    Relations.Add(new RelationDisplayItem
                    {
                        RelationTypeIcon = rel.RelationTypeIcon, // "🤝"
                        RelationTypeName = rel.RelationType.ToString(),
                        OtherWorkerName = otherName,
                        StrengthDisplay = rel.RelationStrengthText, // "Fort"
                        StrengthValue = rel.RelationStrength,
                        Notes = rel.Notes ?? ""
                    });
                }
            }
        }
    }

    public class RelationDisplayItem
    {
        public string RelationTypeIcon { get; set; } = "";
        public string RelationTypeName { get; set; } = "";
        public string OtherWorkerName { get; set; } = "";
        public string StrengthDisplay { get; set; } = "";
        public int StrengthValue { get; set; }
        public string Notes { get; set; } = "";
    }
}

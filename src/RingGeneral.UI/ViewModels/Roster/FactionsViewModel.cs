using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Relations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace RingGeneral.UI.ViewModels.Roster
{
    public class FactionsViewModel : ViewModelBase
    {
        private readonly IFactionRepository _factionRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IWorkerRepository _workerRepository;

        private ObservableCollection<FactionListItemViewModel> _factions = new();
        public ObservableCollection<FactionListItemViewModel> Factions
        {
            get => _factions;
            set => this.RaiseAndSetIfChanged(ref _factions, value);
        }

        private FactionListItemViewModel? _selectedFaction;
        public FactionListItemViewModel? SelectedFaction
        {
            get => _selectedFaction;
            set => this.RaiseAndSetIfChanged(ref _selectedFaction, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> AddFactionCommand { get; }

        public FactionsViewModel(
            IFactionRepository factionRepository,
            IGameRepository gameRepository,
            IWorkerRepository workerRepository)
        {
            _factionRepository = factionRepository;
            _gameRepository = gameRepository;
            _workerRepository = workerRepository;

            RefreshCommand = ReactiveCommand.CreateFromTask(LoadFactionsAsync);
            AddFactionCommand = ReactiveCommand.Create(() => { /* TODO: Dialog */ });

            Task.Run(LoadFactionsAsync);
        }

        private async Task LoadFactionsAsync()
        {
            var factions = _factionRepository.GetAllFactions();
            var viewModels = factions.Select(f => new FactionListItemViewModel(f, _workerRepository)).ToList();
            
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Factions.Clear();
                foreach (var vm in viewModels)
                {
                    Factions.Add(vm);
                }
            });
        }
    }

    public class FactionListItemViewModel : ViewModelBase
    {
        private readonly Faction _faction;
        private readonly IWorkerRepository _workerRepository;

        public int Id => _faction.Id;
        public string Name => _faction.Name;
        public string FactionType => _faction.FactionType;
        public string Status => _faction.Status;
        public int MemberCount => _faction.Members.Count(m => m.IsActiveMember);
        
        public string LeaderName { get; private set; } = "Inconnu";

        public FactionListItemViewModel(Faction faction, IWorkerRepository workerRepository)
        {
            _faction = faction;
            _workerRepository = workerRepository;
            
            if (!string.IsNullOrEmpty(faction.LeaderId))
            {
                var leader = _workerRepository.GetWorker(faction.LeaderId);
                LeaderName = leader?.NomComplet ?? "Inconnu";
            }
        }
    }
}

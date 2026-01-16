using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Core.Models.GameSystem;
using System.Collections.Generic;

namespace RingGeneral.UI.ViewModels.Roster
{
    public class BackstageViewModel : ViewModelBase
    {
        private readonly IGameRepository _repository;
        private readonly IWorkerRepository _workerRepository;
        private readonly IFactionRepository _factionRepository;

        // --- Stats ---
        private double _overallMorale;
        public double OverallMorale
        {
            get => _overallMorale;
            set => this.RaiseAndSetIfChanged(ref _overallMorale, value);
        }

        private string _lockerRoomAtmosphere = "Neutre";
        public string LockerRoomAtmosphere
        {
            get => _lockerRoomAtmosphere;
            set => this.RaiseAndSetIfChanged(ref _lockerRoomAtmosphere, value);
        }

        // --- Collections ---
        public ObservableCollection<WorkerRelationViewModel> Relationships { get; set; } = new();
        public ObservableCollection<MoraleEventViewModel> IncidentLog { get; set; } = new();

        public BackstageViewModel(
            IGameRepository repository,
            IWorkerRepository workerRepository,
            IFactionRepository factionRepository)
        {
            _repository = repository;
            _workerRepository = workerRepository;
            _factionRepository = factionRepository;

            // Chargement des données
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            // 1. Calcul du Moral Global
            var workers = _workerRepository.GetAllWorkers();
            if (workers.Any())
            {
                // Assuming Morale is a property of Worker, need to check Worker model.
                // For now, mocking logic or using existing fields if known.
                // Using a random placeholder logic if Morale field isn't explicitly known yet, 
                // but usually it's Morale or Happiness.
                OverallMorale = 78.5; 
            }

            // 2. Atmosphere (Mock logic based on morale)
            LockerRoomAtmosphere = OverallMorale switch
            {
                >= 90 => "Excellent",
                >= 75 => "Très Bon",
                >= 60 => "Bon",
                >= 40 => "Tendu",
                _ => "Toxique"
            };

            // 3. Charger les Relations (Mock data for now as Repository might not have GetAllRelations exposed)
            // In a real scenario, we'd fetch from _repository.GetAllRelations()
            // We will simulate some data for the UI
            Relationships.Clear();
            Relationships.Add(new WorkerRelationViewModel("John Cena", "The Rock", RelationType.Rivalite, 85, "Icon vs Icon"));
            Relationships.Add(new WorkerRelationViewModel("CM Punk", "Triple H", RelationType.Haine, 95, "Real life heat"));
            Relationships.Add(new WorkerRelationViewModel("Sami Zayn", "Kevin Owens", RelationType.Fraternite, 90, "Best Friends"));
            
            // 4. Incident Log (Mock)
            IncidentLog.Clear();
            IncidentLog.Add(new MoraleEventViewModel("Bagare en coulisse", "CM Punk s'est battu avec Jack Perry.", DateTime.Now.AddDays(-2), "Négatif"));
            IncidentLog.Add(new MoraleEventViewModel("Fête d'anniversaire", "Tout le monde a célébré l'anniversaire de R-Truth.", DateTime.Now.AddDays(-5), "Positif"));
        }
    }

    public class WorkerRelationViewModel
    {
        public string Worker1Name { get; }
        public string Worker2Name { get; }
        public string Type { get; }
        public string Icon { get; }
        public int Strength { get; }
        public string Notes { get; }
        public string StrengthColor { get; }

        public WorkerRelationViewModel(string w1, string w2, RelationType type, int strength, string notes)
        {
            Worker1Name = w1;
            Worker2Name = w2;
            Type = type.ToString();
            Strength = strength;
            Notes = notes;

            Icon = type switch
            {
                RelationType.Amitie => "🤝",
                RelationType.Couple => "❤",
                RelationType.Fraternite => "👊",
                RelationType.Rivalite => "⚔",
                RelationType.Protege => "🎓",
                RelationType.Haine => "😠",
                _ => "?"
            };

            StrengthColor = type switch
            {
                RelationType.Amitie or RelationType.Fraternite or RelationType.Protege or RelationType.Couple => "#10B981", // Green
                RelationType.Rivalite or RelationType.Haine => "#EF4444", // Red
                _ => "#94A3B8" // Grey
            };
        }
    }

    public class MoraleEventViewModel
    {
        public string Title { get; }
        public string Description { get; }
        public string Date { get; }
        public string Impact { get; } // Positif/Négatif
        public string ImpactColor { get; }

        public MoraleEventViewModel(string title, string desc, DateTime date, string impact)
        {
            Title = title;
            Description = desc;
            Date = date.ToShortDateString();
            Impact = impact;
            ImpactColor = impact == "Positif" ? "#10B981" : "#EF4444";
        }
    }
}

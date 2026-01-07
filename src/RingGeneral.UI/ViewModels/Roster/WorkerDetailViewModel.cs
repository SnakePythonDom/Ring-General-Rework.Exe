using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour les détails complets d'un worker
/// </summary>
public sealed class WorkerDetailViewModel : ViewModelBase, INavigableViewModel
{
    private readonly GameRepository? _repository;
    private WorkerSnapshot? _worker;
    private string _workerId = string.Empty;
    private bool _isLoading;

    public WorkerDetailViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        Attributes = new ObservableCollection<AttributeDisplayItem>();
        Storylines = new ObservableCollection<string>();
        Titles = new ObservableCollection<string>();
        RecentMatches = new ObservableCollection<string>();
    }

    /// <summary>
    /// Appelé quand on navigue vers ce ViewModel
    /// </summary>
    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is string workerId && !string.IsNullOrEmpty(workerId))
        {
            WorkerId = workerId;
        }
    }

    /// <summary>
    /// Worker actuellement affiché
    /// </summary>
    public WorkerSnapshot? Worker
    {
        get => _worker;
        private set
        {
            this.RaiseAndSetIfChanged(ref _worker, value);
            // Notifier les propriétés calculées
            this.RaisePropertyChanged(nameof(WorkerName));
            this.RaisePropertyChanged(nameof(OverallRating));
            this.RaisePropertyChanged(nameof(PopularityDisplay));
            this.RaisePropertyChanged(nameof(MomentumDisplay));
            this.RaisePropertyChanged(nameof(FatigueDisplay));
            this.RaisePropertyChanged(nameof(InjuryDisplay));
            this.RaisePropertyChanged(nameof(MoraleDisplay));
            this.RaisePropertyChanged(nameof(HasInjury));
        }
    }

    /// <summary>
    /// ID du worker
    /// </summary>
    public string WorkerId
    {
        get => _workerId;
        set
        {
            this.RaiseAndSetIfChanged(ref _workerId, value);
            LoadWorkerDetails(value);
        }
    }

    /// <summary>
    /// Indique si le chargement est en cours
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    // Propriétés calculées
    public string WorkerName => Worker?.NomComplet ?? "N/A";
    public int OverallRating => Worker != null
        ? (Worker.InRing + Worker.Entertainment + Worker.Story) / 3
        : 0;
    public string PopularityDisplay => Worker?.Popularite.ToString() ?? "0";
    public string MomentumDisplay => Worker?.Momentum.ToString() ?? "0";
    public string FatigueDisplay => Worker?.Fatigue.ToString() ?? "0";
    public string InjuryDisplay => Worker?.Blessure ?? "Aucune";
    public bool HasInjury => !string.IsNullOrEmpty(Worker?.Blessure) && Worker?.Blessure != "Aucune";
    public string MoraleDisplay => Worker?.Morale.ToString() ?? "0";
    public string RoleTvDisplay => Worker?.RoleTv ?? "N/A";

    // Collections
    public ObservableCollection<AttributeDisplayItem> Attributes { get; }
    public ObservableCollection<string> Storylines { get; }
    public ObservableCollection<string> Titles { get; }
    public ObservableCollection<string> RecentMatches { get; }

    /// <summary>
    /// Charge les détails complets du worker
    /// </summary>
    private void LoadWorkerDetails(string workerId)
    {
        if (string.IsNullOrEmpty(workerId) || _repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        IsLoading = true;

        try
        {
            // TODO: Implémenter la récupération depuis le repository
            // Pour l'instant, données placeholder
            LoadPlaceholderData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Charge des données de démonstration
    /// </summary>
    private void LoadPlaceholderData()
    {
        Worker = new WorkerSnapshot(
            WorkerId: "W001",
            NomComplet: "John Cena",
            InRing: 85,
            Entertainment: 92,
            Story: 88,
            Popularite: 95,
            Fatigue: 25,
            Blessure: "Aucune",
            Momentum: 78,
            RoleTv: "Main Eventer",
            Morale: 85
        );

        Attributes.Clear();
        Attributes.Add(new AttributeDisplayItem("In-Ring", Worker.InRing, "#3b82f6"));
        Attributes.Add(new AttributeDisplayItem("Entertainment", Worker.Entertainment, "#8b5cf6"));
        Attributes.Add(new AttributeDisplayItem("Story", Worker.Story, "#f59e0b"));
        Attributes.Add(new AttributeDisplayItem("Overall", OverallRating, "#10b981"));

        Storylines.Clear();
        Storylines.Add("🔥 Rivalité avec Randy Orton (Heat: 85)");
        Storylines.Add("🏆 Contender #1 pour le WWE Championship");

        Titles.Clear();
        Titles.Add("🏆 WWE Championship (278 jours)");

        RecentMatches.Clear();
        RecentMatches.Add("S24 - vs Randy Orton - Note: 88 ⭐⭐⭐⭐");
        RecentMatches.Add("S23 - vs CM Punk - Note: 85 ⭐⭐⭐⭐");
        RecentMatches.Add("S22 - vs The Rock - Note: 92 ⭐⭐⭐⭐⭐");
    }
}

/// <summary>
/// Item d'affichage d'attribut avec barre visuelle
/// </summary>
public sealed class AttributeDisplayItem : ViewModelBase
{
    private string _name = string.Empty;
    private int _value;
    private string _color = "#3b82f6";

    public AttributeDisplayItem(string name, int value, string color)
    {
        _name = name;
        _value = value;
        _color = color;
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            this.RaisePropertyChanged(nameof(PercentageWidth));
        }
    }

    public string Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    /// <summary>
    /// Largeur de la barre en pourcentage (sur 100)
    /// </summary>
    public int PercentageWidth => Value;
}

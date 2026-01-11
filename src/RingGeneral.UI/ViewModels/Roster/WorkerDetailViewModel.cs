using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Shared;

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
    private Workers.Profile.ProfileViewModel _profile;

    public WorkerDetailViewModel(
        GameRepository? repository,
        Workers.Profile.ProfileViewModel profileViewModel)
    {
        _repository = repository;
        _profile = profileViewModel;

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
            // Start UpdateProfileFromWorker replaced by Profile.LoadWorker which is called in LoadWorkerDetails
            if (value != null)
            {
                // We might need to handle this differently if Worker is set directly
            }
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

    public Workers.Profile.ProfileViewModel Profile
    {
        get => _profile;
        private set => this.RaiseAndSetIfChanged(ref _profile, value);
    }

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
            var workerSnapshot = _repository.ChargerWorker(workerId);

            // Try to parse ID to int for the new profile system
            bool profileLoaded = false;
            if (int.TryParse(workerId, out int idInt))
            {
                var fullWorker = _repository.GetWorker(idInt);
                if (fullWorker != null)
                {
                    Profile.LoadWorker(fullWorker);
                    profileLoaded = true;
                }
            }

            if (workerSnapshot == null)
            {
                LoadPlaceholderData();
                return;
            }

            Worker = workerSnapshot;

            // FALLBACK: If we have a snapshot but couldn't load the full profile (e.g. placeholder ID "W001"),
            // create a temporary Worker object from the snapshot so the ProfileView isn't empty.
            if (!profileLoaded)
            {
                // Create attribute objects with all components set to the average value
                // so the calculated Average property matches the snapshot value.
                var inRingAttrs = new RingGeneral.Core.Models.Attributes.WorkerInRingAttributes();
                inRingAttrs.SetAttributeValue("Striking", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Grappling", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("HighFlying", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Powerhouse", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Timing", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Selling", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Psychology", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Stamina", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("Safety", workerSnapshot.InRing);
                inRingAttrs.SetAttributeValue("HardcoreBrawl", workerSnapshot.InRing);

                var entAttrs = new RingGeneral.Core.Models.Attributes.WorkerEntertainmentAttributes();
                entAttrs.SetAttributeValue("Charisma", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("MicWork", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("Acting", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("CrowdConnection", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("StarPower", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("Improvisation", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("Entrance", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("SexAppeal", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("MerchandiseAppeal", workerSnapshot.Entertainment);
                entAttrs.SetAttributeValue("CrossoverPotential", workerSnapshot.Entertainment);

                var storyAttrs = new RingGeneral.Core.Models.Attributes.WorkerStoryAttributes();
                storyAttrs.SetAttributeValue("CharacterDepth", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("Consistency", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("HeelPerformance", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("BabyfacePerformance", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("StorytellingLongTerm", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("EmotionalRange", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("Adaptability", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("RivalryChemistry", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("CreativeInput", workerSnapshot.Story);
                storyAttrs.SetAttributeValue("MoralAlignment", workerSnapshot.Story);

                var fallbackWorker = new Worker
                {
                    Id = int.TryParse(workerId, out int id) ? id : 0,
                    Name = workerSnapshot.NomComplet,
                    PushLevel = PushLevel.MidCard, // Default
                    EntertainmentAttributes = entAttrs,
                    InRingAttributes = inRingAttrs,
                    StoryAttributes = storyAttrs,
                    PersonalityProfile = RingGeneral.Core.Models.PersonalityProfile.NonDéterminé
                };
                Profile.LoadWorker(fallbackWorker);
            }

            // Charger les attributs
            Attributes.Clear();
            Attributes.Add(new AttributeDisplayItem("In-Ring", Worker.InRing, "#3b82f6"));
            Attributes.Add(new AttributeDisplayItem("Entertainment", Worker.Entertainment, "#8b5cf6"));
            Attributes.Add(new AttributeDisplayItem("Story", Worker.Story, "#f59e0b"));
            Attributes.Add(new AttributeDisplayItem("Overall", OverallRating, "#10b981"));

            // TODO: Charger Storylines, Titres, Matches réels depuis le repo
            Storylines.Clear();
            Titles.Clear();
            RecentMatches.Clear();

            // If we couldn't load the full profile but have the snapshot, fallback to snapshot data?
            // The ProfileViewModel expects Worker, so we can't easily pass Snapshot.
            // If profileLoaded is false, the Profile tabs might be empty or show defaults.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading worker details: {ex}");
            // Fallback placeholder en cas d'erreur
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
        // Si on a déjà chargé un worker, on ne l'écrase pas avec le placeholder
        if (Worker != null) return;

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

        // Create a dummy FULL Worker for the profile view
        var dummyWorker = new Worker
        {
            Id = 1,
            Name = "John Cena",
            RealName = "John Cena",
            DateOfBirth = new DateTime(1977, 4, 23),
            Age = 47,
            BirthCountry = "USA",
            // Nationality = "USA", // Removed as property doesn't exist
            Gender = Gender.Male,
            IsActive = true,
            PushLevel = PushLevel.MainEvent
        };
        Profile.LoadWorker(dummyWorker);
    }

    // Legacy mapping removed
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

using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Storylines;

/// <summary>
/// ViewModel pour la gestion des storylines
/// </summary>
public sealed class StorylinesViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private StorylineListItemViewModel? _selectedStoryline;
    private string _searchText = string.Empty;

    public StorylinesViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        ActiveStorylines = new ObservableCollection<StorylineListItemViewModel>();
        SuspendedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        CompletedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        Participants = new ObservableCollection<string>();

        LoadStorylines();
    }

    /// <summary>
    /// Storylines actives
    /// </summary>
    public ObservableCollection<StorylineListItemViewModel> ActiveStorylines { get; }

    /// <summary>
    /// Storylines suspendues
    /// </summary>
    public ObservableCollection<StorylineListItemViewModel> SuspendedStorylines { get; }

    /// <summary>
    /// Storylines terminées
    /// </summary>
    public ObservableCollection<StorylineListItemViewModel> CompletedStorylines { get; }

    /// <summary>
    /// Participants de la storyline sélectionnée
    /// </summary>
    public ObservableCollection<string> Participants { get; }

    /// <summary>
    /// Storyline sélectionnée
    /// </summary>
    public StorylineListItemViewModel? SelectedStoryline
    {
        get => _selectedStoryline;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedStoryline, value);
            LoadParticipants(value?.StorylineId);
        }
    }

    /// <summary>
    /// Texte de recherche
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    /// <summary>
    /// Nombre total de storylines actives
    /// </summary>
    public int TotalActive => ActiveStorylines.Count;

    /// <summary>
    /// Nombre de storylines suspendues
    /// </summary>
    public int TotalSuspended => SuspendedStorylines.Count;

    /// <summary>
    /// Charge les storylines depuis le repository
    /// </summary>
    private void LoadStorylines()
    {
        ActiveStorylines.Clear();
        SuspendedStorylines.Clear();
        CompletedStorylines.Clear();

        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT StorylineId, Name, Heat, Status, Phase
                FROM Storylines
                ORDER BY Heat DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var storyline = new StorylineListItemViewModel
                {
                    StorylineId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Heat = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    Phase = reader.GetString(4)
                };

                // Répartir selon le statut
                if (storyline.Status == "Active")
                    ActiveStorylines.Add(storyline);
                else if (storyline.Status == "Suspended")
                    SuspendedStorylines.Add(storyline);
                else
                    CompletedStorylines.Add(storyline);
            }

            System.Console.WriteLine($"[StorylinesViewModel] {ActiveStorylines.Count} storylines actives chargées");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[StorylinesViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    /// <summary>
    /// Charge les participants d'une storyline
    /// </summary>
    private void LoadParticipants(string? storylineId)
    {
        Participants.Clear();

        if (string.IsNullOrEmpty(storylineId))
            return;

        // TODO: Charger depuis StorylineParticipants
        // Pour l'instant, données placeholder
        Participants.Add("John Cena (Face)");
        Participants.Add("Randy Orton (Heel)");
    }

    /// <summary>
    /// Charge des données placeholder
    /// </summary>
    private void LoadPlaceholderData()
    {
        ActiveStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S001",
            Name = "Cena vs Orton - Championship Feud",
            Heat = 85,
            Status = "Active",
            Phase = "Climax"
        });
        ActiveStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S002",
            Name = "CM Punk - Pipe Bomb",
            Heat = 92,
            Status = "Active",
            Phase = "Rising"
        });

        SuspendedStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S003",
            Name = "Kane vs Undertaker",
            Heat = 65,
            Status = "Suspended",
            Phase = "Midpoint"
        });
    }
}

/// <summary>
/// ViewModel pour un item de storyline
/// </summary>
public sealed class StorylineListItemViewModel : ViewModelBase
{
    private string _storylineId = string.Empty;
    private string _name = string.Empty;
    private int _heat;
    private string _status = string.Empty;
    private string _phase = string.Empty;

    public string StorylineId
    {
        get => _storylineId;
        set => this.RaiseAndSetIfChanged(ref _storylineId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Heat
    {
        get => _heat;
        set => this.RaiseAndSetIfChanged(ref _heat, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Phase
    {
        get => _phase;
        set => this.RaiseAndSetIfChanged(ref _phase, value);
    }

    public string HeatDisplay => $"Heat: {Heat}";
}

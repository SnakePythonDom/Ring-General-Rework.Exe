using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Storylines;

/// <summary>
/// ViewModel pour la gestion des storylines.
/// Enrichi dans Phase 6.3 avec intégration booking.
/// </summary>
public sealed class StorylinesViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private ShowContext? _context;
    private StorylineListItemViewModel? _selectedStoryline;
    private string _searchText = string.Empty;
    private StorylinePhaseOptionViewModel? _selectedPhase;
    private StorylineStatusOptionViewModel? _selectedStatus;

    public StorylinesViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        ActiveStorylines = new ObservableCollection<StorylineListItemViewModel>();
        SuspendedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        CompletedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        Participants = new ObservableCollection<string>();

        // Phase 6.3 - Collection pour booking
        AvailableForBooking = new ObservableCollection<StorylineOptionViewModel>();

        // Phase 6.3 - Options de filtrage
        Phases = new List<StorylinePhaseOptionViewModel>
        {
            new StorylinePhaseOptionViewModel { Phase = "Setup", Label = "Setup", Description = "Phase de lancement" },
            new StorylinePhaseOptionViewModel { Phase = "Rising", Label = "Rising", Description = "Phase montante" },
            new StorylinePhaseOptionViewModel { Phase = "Midpoint", Label = "Midpoint", Description = "Point médian" },
            new StorylinePhaseOptionViewModel { Phase = "Climax", Label = "Climax", Description = "Point culminant" },
            new StorylinePhaseOptionViewModel { Phase = "Resolution", Label = "Resolution", Description = "Résolution" }
        };

        Statuts = new List<StorylineStatusOptionViewModel>
        {
            new StorylineStatusOptionViewModel { Status = "Active", Label = "Active", Color = "#10b981" },
            new StorylineStatusOptionViewModel { Status = "Suspended", Label = "Suspendue", Color = "#f59e0b" },
            new StorylineStatusOptionViewModel { Status = "Completed", Label = "Terminée", Color = "#6b7280" }
        };

        // Phase 6.3 - Commandes
        FilterByPhaseCommand = ReactiveCommand.Create<StorylinePhaseOptionViewModel>(FilterByPhase);
        FilterByStatusCommand = ReactiveCommand.Create<StorylineStatusOptionViewModel>(FilterByStatus);
        AssignToSegmentCommand = ReactiveCommand.Create<string>(AssignToSegment);

        LoadStorylines();
    }

    #region Collections

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
    /// Phase 6.3 - Storylines disponibles pour assignment booking
    /// </summary>
    public ObservableCollection<StorylineOptionViewModel> AvailableForBooking { get; }

    #endregion

    #region Properties

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
    /// Phase 6.3 - Phase sélectionnée pour filtrage
    /// </summary>
    public StorylinePhaseOptionViewModel? SelectedPhase
    {
        get => _selectedPhase;
        set => this.RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    /// <summary>
    /// Phase 6.3 - Statut sélectionné pour filtrage
    /// </summary>
    public StorylineStatusOptionViewModel? SelectedStatus
    {
        get => _selectedStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
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
    /// Phase 6.3 - Options de phases disponibles
    /// </summary>
    public IReadOnlyList<StorylinePhaseOptionViewModel> Phases { get; }

    /// <summary>
    /// Phase 6.3 - Options de statuts disponibles
    /// </summary>
    public IReadOnlyList<StorylineStatusOptionViewModel> Statuts { get; }

    #endregion

    #region Commands

    /// <summary>
    /// Phase 6.3 - Commande pour filtrer par phase
    /// </summary>
    public ReactiveCommand<StorylinePhaseOptionViewModel, Unit> FilterByPhaseCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour filtrer par statut
    /// </summary>
    public ReactiveCommand<StorylineStatusOptionViewModel, Unit> FilterByStatusCommand { get; }

    /// <summary>
    /// Phase 6.3 - Commande pour assigner à un segment
    /// </summary>
    public ReactiveCommand<string, Unit> AssignToSegmentCommand { get; }

    #endregion

    #region Public Methods - Phase 6.3

    /// <summary>
    /// Phase 6.3 - Charge les storylines disponibles pour le booking
    /// </summary>
    public void LoadAvailableStorylines(ShowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        AvailableForBooking.Clear();

        try
        {
            if (_repository == null)
            {
                LoadPlaceholderBookingStorylines();
                return;
            }

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT StorylineId, Name, Heat, Status, Phase
                FROM Storylines
                WHERE Status = 'Active'
                ORDER BY Heat DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                AvailableForBooking.Add(new StorylineOptionViewModel
                {
                    StorylineId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Heat = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    Phase = reader.GetString(4)
                });
            }

            Logger.Info($"{AvailableForBooking.Count} storylines disponibles pour booking");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur chargement storylines booking : {ex.Message}");
            LoadPlaceholderBookingStorylines();
        }
    }

    /// <summary>
    /// Phase 6.3 - Filtre les storylines par phase
    /// </summary>
    public void FilterByPhase(StorylinePhaseOptionViewModel? phase)
    {
        SelectedPhase = phase;

        if (phase == null)
        {
            LoadStorylines();
            return;
        }

        try
        {
            ActiveStorylines.Clear();
            SuspendedStorylines.Clear();
            CompletedStorylines.Clear();

            if (_repository == null) return;

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT StorylineId, Name, Heat, Status, Phase
                FROM Storylines
                WHERE Phase = @phase
                ORDER BY Heat DESC";

            cmd.Parameters.AddWithValue("@phase", phase.Phase);

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

                if (storyline.Status == "Active")
                    ActiveStorylines.Add(storyline);
                else if (storyline.Status == "Suspended")
                    SuspendedStorylines.Add(storyline);
                else
                    CompletedStorylines.Add(storyline);
            }

            Logger.Debug($"Filtré par phase : {phase.Phase} ({ActiveStorylines.Count} actives)");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur filtrage phase : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Filtre les storylines par statut
    /// </summary>
    public void FilterByStatus(StorylineStatusOptionViewModel? status)
    {
        SelectedStatus = status;

        if (status == null)
        {
            LoadStorylines();
            return;
        }

        try
        {
            ActiveStorylines.Clear();
            SuspendedStorylines.Clear();
            CompletedStorylines.Clear();

            if (_repository == null) return;

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT StorylineId, Name, Heat, Status, Phase
                FROM Storylines
                WHERE Status = @status
                ORDER BY Heat DESC";

            cmd.Parameters.AddWithValue("@status", status.Status);

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

                if (storyline.Status == "Active")
                    ActiveStorylines.Add(storyline);
                else if (storyline.Status == "Suspended")
                    SuspendedStorylines.Add(storyline);
                else
                    CompletedStorylines.Add(storyline);
            }

            Logger.Debug($"Filtré par statut : {status.Status} ({ActiveStorylines.Count + SuspendedStorylines.Count + CompletedStorylines.Count} total)");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur filtrage statut : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Assigne une storyline à un segment
    /// </summary>
    public void AssignToSegment(string segmentId)
    {
        if (_repository == null || string.IsNullOrWhiteSpace(segmentId) || SelectedStoryline == null)
        {
            Logger.Warning("Impossible d'assigner : paramètres invalides");
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Segments
                SET StorylineId = @storylineId
                WHERE SegmentId = @segmentId";

            cmd.Parameters.AddWithValue("@storylineId", SelectedStoryline.StorylineId);
            cmd.Parameters.AddWithValue("@segmentId", segmentId);

            cmd.ExecuteNonQuery();

            Logger.Info($"Storyline '{SelectedStoryline.Name}' assignée au segment {segmentId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur assignment storyline : {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 6.3 - Retourne les storylines actives uniquement
    /// </summary>
    public List<StorylineOptionViewModel> GetActiveStorylines()
    {
        return AvailableForBooking
            .Where(s => s.Status == "Active")
            .ToList();
    }

    #endregion

    #region Private Methods

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

            Logger.Info($"{ActiveStorylines.Count} storylines actives chargées");
        }
        catch (Exception ex)
        {
            Logger.Error($"[StorylinesViewModel] Erreur: {ex.Message}");
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

    /// <summary>
    /// Phase 6.3 - Charge des storylines placeholder pour booking
    /// </summary>
    private void LoadPlaceholderBookingStorylines()
    {
        AvailableForBooking.Add(new StorylineOptionViewModel
        {
            StorylineId = "S001",
            Name = "Cena vs Orton - Championship Feud",
            Heat = 85,
            Status = "Active",
            Phase = "Climax"
        });
        AvailableForBooking.Add(new StorylineOptionViewModel
        {
            StorylineId = "S002",
            Name = "CM Punk - Pipe Bomb",
            Heat = 92,
            Status = "Active",
            Phase = "Rising"
        });
    }

    #endregion
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

/// <summary>
/// Phase 6.3 - ViewModel pour une storyline disponible pour booking
/// </summary>
public sealed class StorylineOptionViewModel : ViewModelBase
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

    public string Display => $"{Name} (Heat: {Heat}, {Phase})";
}

/// <summary>
/// Phase 6.3 - Option de phase de storyline
/// </summary>
public sealed class StorylinePhaseOptionViewModel : ViewModelBase
{
    private string _phase = string.Empty;
    private string _label = string.Empty;
    private string _description = string.Empty;

    public string Phase
    {
        get => _phase;
        set => this.RaiseAndSetIfChanged(ref _phase, value);
    }

    public string Label
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}

/// <summary>
/// Phase 6.3 - Option de statut de storyline
/// </summary>
public sealed class StorylineStatusOptionViewModel : ViewModelBase
{
    private string _status = string.Empty;
    private string _label = string.Empty;
    private string _color = string.Empty;

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Label
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
    }

    public string Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }
}

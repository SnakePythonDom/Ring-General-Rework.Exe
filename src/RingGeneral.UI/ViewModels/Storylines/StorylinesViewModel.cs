using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Services;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.ViewModels.Storylines;

/// <summary>
/// ViewModel pour la gestion des storylines.
/// Enrichi dans Phase 6.3 avec intégration booking.
/// </summary>
public sealed class StorylinesViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly StorylineService _storylineService;
    private readonly StaffStorylineIntegration _staffIntegration;
    private readonly IStaffRepository _staffRepository;

    private ShowContext? _context;
    private string _activeTab = "Active";
    private CreativeStaffMemberViewModel? _selectedStaff;
    private StorylineListItemViewModel? _selectedStoryline;
    private string _searchText = string.Empty;
    private StorylinePhaseOptionViewModel? _selectedPhase;
    private StorylineStatusOptionViewModel? _selectedStatus;

    public StorylinesViewModel(
        GameRepository repository,
        StorylineService storylineService,
        StaffStorylineIntegration staffIntegration,
        IStaffRepository staffRepository)
    {
        _repository = repository;
        _storylineService = storylineService;
        _staffIntegration = staffIntegration;
        _staffRepository = staffRepository;

        // Initialize collections
        ActiveStorylines = new ObservableCollection<StorylineListItemViewModel>();
        SuspendedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        CompletedStorylines = new ObservableCollection<StorylineListItemViewModel>();
        Participants = new ObservableCollection<string>();
        AvailableForBooking = new ObservableCollection<StorylineOptionViewModel>();
        CreativeStaff = new ObservableCollection<CreativeStaffMemberViewModel>();
        RecruitableStaff = new ObservableCollection<CreativeStaffMemberViewModel>();
        StorylineSuggestions = new ObservableCollection<StorylineSuggestionViewModel>();

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

        CreateStorylineCommand = ReactiveCommand.Create(() => Logger.Info("Overlay Création"));
        SuspendStorylineCommand = ReactiveCommand.Create(() => Logger.Info("Storyline Suspendue"));
        ResumeStorylineCommand = ReactiveCommand.Create(() => Logger.Info("Storyline Reprise"));
        CompleteStorylineCommand = ReactiveCommand.Create(() => Logger.Info("Bilan de Storyline"));
        SuggestStorylineCommand = ReactiveCommand.CreateFromTask<CreativeStaffMemberViewModel>(OnSuggestStoryline);

        LoadStorylines();
        LoadCreativeStaff();
    }

    #region Collections

    public ObservableCollection<StorylineListItemViewModel> ActiveStorylines { get; }
    public ObservableCollection<StorylineListItemViewModel> SuspendedStorylines { get; }
    public ObservableCollection<StorylineListItemViewModel> CompletedStorylines { get; }
    public ObservableCollection<string> Participants { get; }
    public ObservableCollection<StorylineOptionViewModel> AvailableForBooking { get; }
    public ObservableCollection<CreativeStaffMemberViewModel> CreativeStaff { get; }
    public ObservableCollection<CreativeStaffMemberViewModel> RecruitableStaff { get; }
    public ObservableCollection<StorylineSuggestionViewModel> StorylineSuggestions { get; }

    #endregion

    #region Properties

    public StorylineListItemViewModel? SelectedStoryline
    {
        get => _selectedStoryline;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedStoryline, value);
            LoadParticipants(value?.StorylineId);
        }
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public StorylinePhaseOptionViewModel? SelectedPhase
    {
        get => _selectedPhase;
        set => this.RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    public StorylineStatusOptionViewModel? SelectedStatus
    {
        get => _selectedStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
    }

    public string ActiveTab
    {
        get => _activeTab;
        set => this.RaiseAndSetIfChanged(ref _activeTab, value);
    }

    public CreativeStaffMemberViewModel? SelectedStaff
    {
        get => _selectedStaff;
        set => this.RaiseAndSetIfChanged(ref _selectedStaff, value);
    }

    public IReadOnlyList<StorylinePhaseOptionViewModel> Phases { get; }
    public IReadOnlyList<StorylineStatusOptionViewModel> Statuts { get; }

    public int TotalActive => ActiveStorylines.Count;
    public int TotalSuspended => SuspendedStorylines.Count;

    #endregion

    #region Commands

    public ReactiveCommand<StorylinePhaseOptionViewModel, Unit> FilterByPhaseCommand { get; }
    public ReactiveCommand<StorylineStatusOptionViewModel, Unit> FilterByStatusCommand { get; }
    public ReactiveCommand<string, Unit> AssignToSegmentCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateStorylineCommand { get; }
    public ReactiveCommand<Unit, Unit> SuspendStorylineCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeStorylineCommand { get; }
    public ReactiveCommand<Unit, Unit> CompleteStorylineCommand { get; }
    public ReactiveCommand<CreativeStaffMemberViewModel, Unit> SuggestStorylineCommand { get; }

    #endregion

    #region Command Implementations

    public void FilterByPhase(StorylinePhaseOptionViewModel? phase)
    {
        SelectedPhase = phase;
        Logger.Info($"Filtrage par phase: {phase?.Label ?? "Tous"}");
    }

    public void FilterByStatus(StorylineStatusOptionViewModel? status)
    {
        SelectedStatus = status;
        Logger.Info($"Filtrage par statut: {status?.Label ?? "Tous"}");
    }

    private void AssignToSegment(string storylineId)
    {
        Logger.Info($"Storyline {storylineId} assignée au segment.");
    }

    #endregion

    #region Private Methods

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
            var companyId = GetCurrentCompanyId();
            using var connection = (SqliteConnection)_repository.CreateConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT s.StorylineId, s.Name, s.Heat, s.Status, s.Phase, 
                       s.LeadCreativeId, s.CreativeIdea, s.BookerIdea, 
                       s.StartWeek, s.PauseWeek, s.ReasonForPause, sm.Name as CreativeName
                FROM Storylines s
                LEFT JOIN StaffMembers sm ON s.LeadCreativeId = sm.StaffId
                WHERE s.CompanyId = $companyId
                ORDER BY s.Heat DESC";
            cmd.Parameters.AddWithValue("$companyId", companyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var storyline = new StorylineListItemViewModel
                {
                    StorylineId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Heat = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    Phase = reader.GetString(4),
                    LeadCreativeId = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreativeIdea = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    BookerIdea = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    PauseWeeks = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                    ReasonForPause = reader.IsDBNull(10) ? null : reader.GetString(10),
                    LeadCreativeName = reader.IsDBNull(11) ? "Aucun" : reader.GetString(11)
                };

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
            Logger.Error($"[StorylinesViewModel] Erreur chargement storylines: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadParticipants(string? storylineId)
    {
        Participants.Clear();

        if (string.IsNullOrEmpty(storylineId))
            return;

        try
        {
            using var connection = (SqliteConnection)_repository.CreateConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT w.Name, sp.Role
                FROM StorylineParticipants sp
                JOIN Workers w ON sp.WorkerId = w.WorkerId
                WHERE sp.StorylineId = $storylineId";
            cmd.Parameters.AddWithValue("$storylineId", storylineId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var role = reader.IsDBNull(1) ? "Participant" : reader.GetString(1);
                Participants.Add($"{name} ({role})");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[StorylinesViewModel] Erreur chargement participants: {ex.Message}");
        }
    }

    private void LoadCreativeStaff()
    {
        CreativeStaff.Clear();
        RecruitableStaff.Clear();

        if (_repository == null) return;

        try
        {
            var companyId = GetCurrentCompanyId();
            using var connection = (SqliteConnection)_repository.CreateConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT cs.StaffId, sm.Name, cs.CreativityScore, cs.Specialty, sm.YearsOfExperience
                FROM CreativeStaff cs
                JOIN StaffMembers sm ON cs.StaffId = sm.StaffId
                WHERE cs.CompanyId = $companyId AND sm.IsActive = 1";
            cmd.Parameters.AddWithValue("$companyId", companyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CreativeStaff.Add(new CreativeStaffMemberViewModel
                {
                    StaffId = reader.GetString(0),
                    Name = reader.GetString(1),
                    Creativity = reader.GetInt32(2),
                    Specialty = reader.IsDBNull(3) ? "General" : reader.GetString(3),
                    Experience = reader.GetInt32(4)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[StorylinesViewModel] Erreur chargement staff: {ex.Message}");
        }
    }

    private string GetCurrentCompanyId()
    {
        if (_context != null) return _context.Compagnie.CompagnieId;

        try
        {
            using var connection = (SqliteConnection)_repository.CreateConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT PlayerCompanyId FROM SaveGames WHERE IsActive = 1 LIMIT 1";
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "C001";
        }
        catch
        {
            return "C001";
        }
    }

    private void LoadPlaceholderData()
    {
        ActiveStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S001",
            Name = "Cena vs Punk - Clash of Styles",
            Heat = 85,
            Status = "Active",
            Phase = "Climax",
            LeadCreativeName = "Paul Heyman",
            CreativeIdea = "Punk devrait trahir son allié au prochain PPV pour maximiser le Heat de la phase.",
            BookerIdea = "Assurez-vous que Cena garde son Momentum malgré la défaite prévue."
        });

        SuspendedStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S003",
            Name = "Cody vs Reigns",
            Heat = 92,
            Status = "Suspended",
            Phase = "Setup",
            PauseWeeks = 3,
            LeadCreativeName = "Paul Heyman",
            ReasonForPause = "L'élan est préservé. Une intervention au prochain show relancerait le Heat à 100%."
        });

        CompletedStorylines.Add(new StorylineListItemViewModel
        {
            StorylineId = "S004",
            Name = "The Usos Split",
            Heat = 82,
            Status = "Completed",
            Phase = "Resolution"
        });
    }

    private async Task OnSuggestStoryline(CreativeStaffMemberViewModel staff)
    {
        if (staff == null) return;

        try
        {
            var targetStoryline = SelectedStoryline ?? ActiveStorylines.FirstOrDefault();
            if (targetStoryline == null) return;

            var info = new StorylineInfo(
                targetStoryline.StorylineId,
                targetStoryline.Name,
                Enum.Parse<StorylinePhase>(targetStoryline.Phase),
                targetStoryline.Heat,
                StorylineStatus.Active,
                targetStoryline.CreativeIdea,
                new List<StorylineParticipant>()
            );

            var proposal = await _staffIntegration.GenerateStorylineProposalAsync(staff.StaffId, "PLAYER_BOOKER", info);

            if (proposal != null)
            {
                StorylineSuggestions.Add(new StorylineSuggestionViewModel
                {
                    Title = proposal.Title,
                    Target = targetStoryline.Name,
                    PredictedHeat = proposal.QualityScore,
                    Note = proposal.Description
                });

                Logger.Info($"Suggestion générée par {staff.Name} pour {targetStoryline.Name}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur génération suggestion : {ex.Message}");
        }
    }

    #endregion
}

internal static class Logger
{
    public static void Info(string msg) => Console.WriteLine($"[INFO] {msg}");
    public static void Warning(string msg) => Console.WriteLine($"[WARN] {msg}");
    public static void Error(string msg) => Console.WriteLine($"[ERR] {msg}");
}

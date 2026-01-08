using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;
using RingGeneral.Core.Services;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Personality tab.
/// Manages mental attributes, personality profile detection, and agent reports.
/// Mental attributes are hidden by default and revealed through scouting.
/// </summary>
public sealed class PersonalityTabViewModel : ViewModelBase
{
    private readonly IWorkerAttributesRepository _repository;
    private readonly PersonalityDetectorService _detectorService;
    private readonly AgentReportGeneratorService _reportService;

    private int _workerId;
    private WorkerMentalAttributes? _mentalAttributes;
    private PersonalityProfile _profile;
    private string _profileDisplayName = string.Empty;
    private string _profileDescription = string.Empty;
    private AgentReport? _agentReport;
    private bool _isScoutingCompleted;
    private string _quickAssessment = string.Empty;

    public PersonalityTabViewModel(
        IWorkerAttributesRepository repository,
        PersonalityDetectorService detectorService,
        AgentReportGeneratorService reportService)
    {
        _repository = repository;
        _detectorService = detectorService;
        _reportService = reportService;

        // Commands
        RecalculateProfileCommand = ReactiveCommand.Create(RecalculateProfile);
        LaunchScoutingCommand = ReactiveCommand.Create(LaunchScouting);
        RevealBasicScoutingCommand = ReactiveCommand.Create(RevealBasicScouting);
        RevealFullScoutingCommand = ReactiveCommand.Create(RevealFullScouting);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    /// <summary>
    /// Worker ID
    /// </summary>
    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    /// <summary>
    /// Mental attributes (10 attributes, 0-20 scale)
    /// Null if not yet scouted or revealed
    /// </summary>
    public WorkerMentalAttributes? MentalAttributes
    {
        get => _mentalAttributes;
        private set
        {
            this.RaiseAndSetIfChanged(ref _mentalAttributes, value);
            IsScoutingCompleted = value?.IsRevealed ?? false;
        }
    }

    /// <summary>
    /// Detected personality profile
    /// </summary>
    public PersonalityProfile Profile
    {
        get => _profile;
        private set => this.RaiseAndSetIfChanged(ref _profile, value);
    }

    /// <summary>
    /// Display name with emoji for personality profile
    /// </summary>
    public string ProfileDisplayName
    {
        get => _profileDisplayName;
        private set => this.RaiseAndSetIfChanged(ref _profileDisplayName, value);
    }

    /// <summary>
    /// Short description of the personality profile
    /// </summary>
    public string ProfileDescription
    {
        get => _profileDescription;
        private set => this.RaiseAndSetIfChanged(ref _profileDescription, value);
    }

    /// <summary>
    /// Generated agent report with 4 pillars and narrative text
    /// </summary>
    public AgentReport? AgentReport
    {
        get => _agentReport;
        private set => this.RaiseAndSetIfChanged(ref _agentReport, value);
    }

    /// <summary>
    /// Is scouting completed? (attributes revealed)
    /// </summary>
    public bool IsScoutingCompleted
    {
        get => _isScoutingCompleted;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isScoutingCompleted, value);
            this.RaisePropertyChanged(nameof(IsScoutingNotCompleted));
            this.RaisePropertyChanged(nameof(ScoutingLevelText));
        }
    }

    /// <summary>
    /// Inverse of IsScoutingCompleted (for UI visibility)
    /// </summary>
    public bool IsScoutingNotCompleted => !IsScoutingCompleted;

    /// <summary>
    /// Quick assessment text (⭐⭐⭐ EXCELLENT, etc.)
    /// </summary>
    public string QuickAssessment
    {
        get => _quickAssessment;
        private set => this.RaiseAndSetIfChanged(ref _quickAssessment, value);
    }

    /// <summary>
    /// Scouting level description text
    /// </summary>
    public string ScoutingLevelText
    {
        get
        {
            if (MentalAttributes == null || !MentalAttributes.IsRevealed)
                return "Aucun scouting effectué";

            return MentalAttributes.ScoutingLevel switch
            {
                1 => "Scouting basique (4 piliers visibles)",
                2 => "Scouting complet (10 attributs visibles)",
                _ => "Non scouter"
            };
        }
    }

    /// <summary>
    /// Show detailed attributes? (only if full scouting)
    /// </summary>
    public bool ShowDetailedAttributes => MentalAttributes?.ScoutingLevel >= 2;

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> RecalculateProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchScoutingCommand { get; }
    public ReactiveCommand<Unit, Unit> RevealBasicScoutingCommand { get; }
    public ReactiveCommand<Unit, Unit> RevealFullScoutingCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    /// <summary>
    /// Load personality data for a worker
    /// </summary>
    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load mental attributes from repository
        MentalAttributes = _repository.GetMentalAttributes(workerId);

        // If mental attributes exist, detect profile and generate report
        if (MentalAttributes != null)
        {
            Profile = _detectorService.DetectProfile(MentalAttributes);
            ProfileDisplayName = PersonalityDetectorService.GetProfileDisplayName(Profile);
            ProfileDescription = PersonalityDetectorService.GetProfileDescription(Profile);
            AgentReport = _reportService.GenerateReport(MentalAttributes, "Système d'analyse automatique");
            QuickAssessment = AgentReportGeneratorService.GenerateQuickAssessment(MentalAttributes);
        }
        else
        {
            // No mental attributes yet - initialize defaults
            Profile = PersonalityProfile.NonDéterminé;
            ProfileDisplayName = "❓ Non Déterminé";
            ProfileDescription = "Aucune donnée de personnalité disponible.";
            AgentReport = null;
            QuickAssessment = "Données insuffisantes";
        }

        this.RaisePropertyChanged(nameof(ShowDetailedAttributes));
        this.RaisePropertyChanged(nameof(ScoutingLevelText));
    }

    /// <summary>
    /// Update a specific mental attribute value
    /// </summary>
    public void UpdateMentalAttribute(string attributeName, int value)
    {
        if (WorkerId == 0 || MentalAttributes == null) return;

        _repository.UpdateMentalAttribute(WorkerId, attributeName, value);

        // Reload to reflect changes
        LoadWorker(WorkerId);
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void RecalculateProfile()
    {
        if (WorkerId == 0 || MentalAttributes == null) return;

        // Re-detect profile and regenerate report
        Profile = _detectorService.DetectProfile(MentalAttributes);
        ProfileDisplayName = PersonalityDetectorService.GetProfileDisplayName(Profile);
        ProfileDescription = PersonalityDetectorService.GetProfileDescription(Profile);
        AgentReport = _reportService.GenerateReport(MentalAttributes, "Système d'analyse automatique");
        QuickAssessment = AgentReportGeneratorService.GenerateQuickAssessment(MentalAttributes);
    }

    private void LaunchScouting()
    {
        // Launch full scouting (level 2)
        RevealFullScouting();
    }

    private void RevealBasicScouting()
    {
        if (WorkerId == 0) return;

        // Reveal basic scouting (level 1 - 4 pillars only)
        _repository.RevealMentalAttributes(WorkerId, 1);
        LoadWorker(WorkerId);
    }

    private void RevealFullScouting()
    {
        if (WorkerId == 0) return;

        // Reveal full scouting (level 2 - all 10 attributes)
        _repository.RevealMentalAttributes(WorkerId, 2);
        LoadWorker(WorkerId);
    }

    /// <summary>
    /// Get recommendations for booking this worker
    /// </summary>
    public List<string> GetBookingRecommendations()
    {
        if (MentalAttributes == null)
            return new List<string> { "Effectuez un scouting pour obtenir des recommandations" };

        return AgentReportGeneratorService.GenerateBookingRecommendations(MentalAttributes);
    }

    /// <summary>
    /// Get red flags for this worker
    /// </summary>
    public List<string> GetRedFlags()
    {
        if (MentalAttributes == null)
            return new List<string>();

        return AgentReportGeneratorService.GenerateRedFlags(MentalAttributes);
    }
}

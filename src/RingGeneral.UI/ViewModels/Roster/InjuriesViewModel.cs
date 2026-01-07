using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour la gestion des blessures du roster
/// Affiche les workers blessés, les durées de récupération, et permet de gérer les blessures
/// </summary>
public sealed class InjuriesViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private InjuryRecordViewModel? _selectedInjury;
    private string _filterStatus = "Tous";
    private bool _showOnlyActive = true;

    public InjuriesViewModel(GameRepository repository)
    {
        _repository = repository;

        // Collections
        Injuries = new ObservableCollection<InjuryRecordViewModel>();
        FilterOptions = new ObservableCollection<string>
        {
            "Tous",
            "Actif",
            "En récupération",
            "Guéri",
            "Suspendu"
        };

        // Commands
        ViewWorkerDetailsCommand = ReactiveCommand.Create<InjuryRecordViewModel>(ViewWorkerDetails);
        MarkAsHealedCommand = ReactiveCommand.Create<InjuryRecordViewModel>(MarkAsHealed);
        AddInjuryCommand = ReactiveCommand.Create(AddInjury);
        EditInjuryCommand = ReactiveCommand.Create<InjuryRecordViewModel>(EditInjury);
        DeleteInjuryCommand = ReactiveCommand.Create<InjuryRecordViewModel>(DeleteInjury);
        RefreshCommand = ReactiveCommand.Create(LoadInjuries);

        // Initialisation
        LoadInjuries();
    }

    // ========== COLLECTIONS ==========

    public ObservableCollection<InjuryRecordViewModel> Injuries { get; }
    public ObservableCollection<string> FilterOptions { get; }

    // ========== PROPRIÉTÉS ==========

    public InjuryRecordViewModel? SelectedInjury
    {
        get => _selectedInjury;
        set => this.RaiseAndSetIfChanged(ref _selectedInjury, value);
    }

    public string FilterStatus
    {
        get => _filterStatus;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterStatus, value);
            ApplyFilter();
        }
    }

    public bool ShowOnlyActive
    {
        get => _showOnlyActive;
        set
        {
            this.RaiseAndSetIfChanged(ref _showOnlyActive, value);
            ApplyFilter();
        }
    }

    public int TotalInjuries => Injuries.Count;
    public int ActiveInjuries => Injuries.Count(i => i.IsActive);
    public int RecoveringWorkers => Injuries.Count(i => i.IsRecovering);

    // ========== COMMANDS ==========

    public ReactiveCommand<InjuryRecordViewModel, Unit> ViewWorkerDetailsCommand { get; }
    public ReactiveCommand<InjuryRecordViewModel, Unit> MarkAsHealedCommand { get; }
    public ReactiveCommand<Unit, Unit> AddInjuryCommand { get; }
    public ReactiveCommand<InjuryRecordViewModel, Unit> EditInjuryCommand { get; }
    public ReactiveCommand<InjuryRecordViewModel, Unit> DeleteInjuryCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    // ========== MÉTHODES PUBLIQUES ==========

    public void LoadInjuries()
    {
        // TODO: Charger depuis le repository
        Injuries.Clear();

        // Données de test
        LoadTestData();

        UpdateStatistics();
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void ViewWorkerDetails(InjuryRecordViewModel injury)
    {
        // TODO: Naviguer vers WorkerDetailViewModel
        System.Diagnostics.Debug.WriteLine($"Viewing worker details: {injury.WorkerName}");
    }

    private void MarkAsHealed(InjuryRecordViewModel injury)
    {
        injury.MarkAsHealed();
        UpdateStatistics();
    }

    private void AddInjury()
    {
        var newInjury = new InjuryRecordViewModel(
            $"INJ-{Guid.NewGuid():N}".ToUpperInvariant(),
            "Worker inconnu",
            "WORK000",
            "Blessure légère",
            DateTime.Now,
            7,
            "Actif"
        );

        Injuries.Add(newInjury);
        SelectedInjury = newInjury;
        UpdateStatistics();
    }

    private void EditInjury(InjuryRecordViewModel injury)
    {
        // TODO: Ouvrir dialogue d'édition
        System.Diagnostics.Debug.WriteLine($"Editing injury: {injury.InjuryType} for {injury.WorkerName}");
    }

    private void DeleteInjury(InjuryRecordViewModel injury)
    {
        Injuries.Remove(injury);
        if (SelectedInjury == injury)
        {
            SelectedInjury = Injuries.FirstOrDefault();
        }
        UpdateStatistics();
    }

    private void ApplyFilter()
    {
        // TODO: Implémenter filtre réel
        // Pour l'instant, recharger toutes les données
        LoadInjuries();
    }

    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(TotalInjuries));
        this.RaisePropertyChanged(nameof(ActiveInjuries));
        this.RaisePropertyChanged(nameof(RecoveringWorkers));
    }

    private void LoadTestData()
    {
        // Blessures de test
        Injuries.Add(new InjuryRecordViewModel(
            "INJ001",
            "John Cena",
            "W001",
            "Blessure à l'épaule",
            DateTime.Now.AddDays(-10),
            30,
            "En récupération"
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ002",
            "Randy Orton",
            "W002",
            "Commotion cérébrale",
            DateTime.Now.AddDays(-5),
            21,
            "Actif"
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ003",
            "Seth Rollins",
            "W003",
            "Entorse au genou",
            DateTime.Now.AddDays(-15),
            14,
            "En récupération"
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ004",
            "Drew McIntyre",
            "W004",
            "Côtes fêlées",
            DateTime.Now.AddDays(-20),
            28,
            "En récupération"
        ));
    }
}

// ========== VIEWMODEL HELPER ==========

/// <summary>
/// ViewModel représentant une entrée de blessure
/// </summary>
public sealed class InjuryRecordViewModel : ViewModelBase
{
    private string _status;

    public InjuryRecordViewModel(
        string injuryId,
        string workerName,
        string workerId,
        string injuryType,
        DateTime injuryDate,
        int recoveryDays,
        string status)
    {
        InjuryId = injuryId;
        WorkerName = workerName;
        WorkerId = workerId;
        InjuryType = injuryType;
        InjuryDate = injuryDate;
        RecoveryDays = recoveryDays;
        _status = status;
    }

    public string InjuryId { get; }
    public string WorkerName { get; }
    public string WorkerId { get; }
    public string InjuryType { get; }
    public DateTime InjuryDate { get; }
    public int RecoveryDays { get; }

    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public DateTime ExpectedReturnDate => InjuryDate.AddDays(RecoveryDays);
    public int DaysRemaining => Math.Max(0, (ExpectedReturnDate - DateTime.Now).Days);
    public bool IsActive => Status == "Actif";
    public bool IsRecovering => Status == "En récupération";
    public string RecoverySummary => $"{DaysRemaining} jours restants ({ExpectedReturnDate:dd/MM/yyyy})";

    public void MarkAsHealed()
    {
        Status = "Guéri";
    }
}

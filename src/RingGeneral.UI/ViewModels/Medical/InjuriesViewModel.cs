using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Medical;

/// <summary>
/// ViewModel pour la gestion des blessures du roster
/// Affiche les workers blessés, les durées de récupération, et permet de gérer les blessures
/// </summary>
public sealed class InjuriesViewModel : ViewModelBase
{
    private readonly IMedicalRepository _medicalRepository;
    private readonly IGameRepository _gameRepository;
    private readonly INavigationService _navigationService;
    private InjuryRecordViewModel? _selectedInjury;
    private string _filterStatus = "Tous";
    private string _filterText = string.Empty;
    private bool _showOnlyActive = true;
    private readonly List<InjuryRecordViewModel> _allInjuries = new List<InjuryRecordViewModel>();

    public InjuriesViewModel(
        IGameRepository repository, 
        IMedicalRepository medicalRepository,
        INavigationService navigationService)
    {
        _gameRepository = repository;
        _medicalRepository = medicalRepository;
        _navigationService = navigationService;

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
    public int CriticalInjuries => Injuries.Count(i => i.IsCritical);

    public string FilterText
    {
        get => _filterText;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterText, value);
            ApplyFilter();
        }
    }

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
        Injuries.Clear();
        _allInjuries.Clear();

        try
        {
            Logger.Info("Début du chargement des blessures...");
            
            // Charger toutes les blessures depuis la base de données
            var injuries = _medicalRepository.ChargerToutesBlessures();
            Logger.Info($"{injuries.Count} blessures trouvées dans la base de données");

            // Date de référence pour convertir les semaines en dates
            var baseDate = new DateTime(2024, 1, 1);

            // Charger les noms des workers depuis la base de données
            var workerNames = LoadWorkerNames(injuries.Select(i => i.WorkerId).Distinct().ToList());

            foreach (var injury in injuries)
            {
                // Convertir la semaine de début en DateTime
                var startDate = baseDate.AddDays((injury.StartWeek - 1) * 7);

                // Calculer les jours de récupération
                int recoveryDays = 0;
                string status = "Actif";

                if (injury.EndWeek.HasValue)
                {
                    var endDate = baseDate.AddDays((injury.EndWeek.Value - 1) * 7);
                    recoveryDays = (int)(endDate - startDate).TotalDays;
                    status = injury.IsActive ? "En récupération" : "Guéri";
                }
                else if (injury.IsActive)
                {
                    // Si pas de date de fin mais actif, estimer selon la sévérité
                    recoveryDays = injury.Severity switch
                    {
                        InjurySeverity.Legere => 7,
                        InjurySeverity.Moyenne => 21,
                        InjurySeverity.Grave => 42,
                        _ => 14
                    };
                    status = "En récupération";
                }
                else
                {
                    status = "Guéri";
                }

                // Obtenir le nom du worker
                var workerName = workerNames.GetValueOrDefault(injury.WorkerId, "Worker inconnu");

                var injuryViewModel = new InjuryRecordViewModel(
                    $"INJ{injury.InjuryId}",
                    workerName,
                    injury.WorkerId,
                    injury.Type,
                    startDate,
                    recoveryDays,
                    status,
                    injury.Severity
                );

                _allInjuries.Add(injuryViewModel);
                Injuries.Add(injuryViewModel);
            }

            Logger.Info($"{Injuries.Count} blessures chargées depuis la base de données");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement des blessures: {ex.Message}", ex);
            // En cas d'erreur, charger les données de test pour ne pas laisser l'écran vide
            LoadTestData();
        }

        UpdateStatistics();
    }

    /// <summary>
    /// Charge les noms des workers depuis la base de données
    /// </summary>
    private Dictionary<string, string> LoadWorkerNames(List<string> workerIds)
    {
        var workerNames = new Dictionary<string, string>();

        if (workerIds.Count == 0)
        {
            return workerNames;
        }

        try
        {
            Logger.Info($"Chargement des noms pour {workerIds.Count} workers...");
            
            using var connection = (SqliteConnection)_gameRepository.CreateConnection();
            
            // Vérifier si la table existe
            using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Workers'";
                var tableExists = checkCmd.ExecuteScalar() != null;
                Logger.Info($"Table Workers existe: {tableExists}");
            }
            
            using var command = connection.CreateCommand();

            // Créer les placeholders pour la requête IN
            var placeholders = workerIds.Select((id, index) => $"$id{index}").ToList();
            command.CommandText = $"""
                SELECT WorkerId, 
                       COALESCE(Name, FirstName || ' ' || LastName, RingName, 'Unknown') as FullName
                FROM Workers
                WHERE WorkerId IN ({string.Join(", ", placeholders)});
                """;

            for (var i = 0; i < workerIds.Count; i++)
            {
                command.Parameters.AddWithValue(placeholders[i], workerIds[i]);
            }

            using var reader = command.ExecuteReader();
            int loadedCount = 0;
            while (reader.Read())
            {
                var workerId = reader.GetString(0);
                var fullName = reader.GetString(1);
                workerNames[workerId] = fullName;
                loadedCount++;
            }
            Logger.Info($"{loadedCount} noms de workers chargés");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement des noms de workers: {ex.Message}", ex);
            Logger.Error($"Stack trace: {ex.StackTrace}");
        }

        return workerNames;
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void ViewWorkerDetails(InjuryRecordViewModel injury)
    {
        if (injury != null && !string.IsNullOrEmpty(injury.WorkerId))
        {
            _navigationService.NavigateTo<RingGeneral.UI.ViewModels.Roster.WorkerDetailViewModel>(injury.WorkerId);
        }
    }

    private void MarkAsHealed(InjuryRecordViewModel injury)
    {
        if (injury == null) return;

        try
        {
            // Parse ID (format "INJ123" -> 123)
            // Note: Le ViewModel utilise "INJ" + Id
            var idString = injury.InjuryId.StartsWith("INJ") ? injury.InjuryId.Substring(3) : injury.InjuryId;
            
            if (int.TryParse(idString, out int id))
            {
                var record = _medicalRepository.ChargerBlessure(id);
                if (record != null)
                {
                    // Utiliser le système quotidien pour obtenir la date/semaine
                    int currentWeek = 1;
                    try 
                    { 
                         // Note: SHOW-001 est utilisé comme référence de progression temporelle
                         // Une amélioration future serait d'injecter ITimeOrchestrator
                         var week = _gameRepository.ChargerSemaineShow("SHOW-001");
                         if (week > 0) currentWeek = week;
                    } 
                    catch {}

                    var updated = record with { IsActive = false, EndWeek = currentWeek };
                    _medicalRepository.MettreAJourBlessure(updated);
                    LoadInjuries();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors de la guérison: {ex.Message}");
        }
    }

    private void AddInjury()
    {
        try
        {
            // Simulation d'ajout (TODO: utiliser Dialog pour UX complète)
            var workers = _gameRepository.ChargerNomsWorkers();
            if (!workers.Any()) return;
            
            var randomWorkerId = workers.Keys.Skip(new Random().Next(workers.Count)).First();
            
            int currentWeek = 1; 
             try 
            { 
                 var week = _gameRepository.ChargerSemaineShow("SHOW-001");
                 if (week > 0) currentWeek = week;
            } 
            catch {}

            var newInjury = new InjuryRecord(
                0, // Auto-inc
                randomWorkerId,
                "Blessure Légère (Sim)",
                InjurySeverity.Legere,
                currentWeek,
                null,
                true,
                "Ajouté manuellement"
            );

            _medicalRepository.AjouterBlessure(newInjury);
            LoadInjuries();
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur ajout blessure: {ex.Message}");
        }
    }

    private void EditInjury(InjuryRecordViewModel injury)
    {
        // TODO: Ouvrir dialogue d'édition
        // Pour l'instant non implémenté (low priority)
    }

    private void DeleteInjury(InjuryRecordViewModel injury)
    {
        if (injury == null) return;

        try
        {
            var idString = injury.InjuryId.StartsWith("INJ") ? injury.InjuryId.Substring(3) : injury.InjuryId;
            if (int.TryParse(idString, out int id))
            {
                _medicalRepository.SupprimerBlessure(id);
                LoadInjuries();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur suppression blessure: {ex.Message}");
        }
    }

    private void ApplyFilter()
    {
        // Filtrer les blessures selon FilterText et FilterStatus
        var allInjuries = _allInjuries.ToList();
        var filtered = allInjuries.AsEnumerable();

        // Filtrer par texte de recherche
        if (!string.IsNullOrWhiteSpace(_filterText))
        {
            var searchLower = _filterText.ToLower();
            filtered = filtered.Where(i =>
                i.WorkerName.ToLower().Contains(searchLower) ||
                i.InjuryType.ToLower().Contains(searchLower) ||
                i.Status.ToLower().Contains(searchLower)
            );
        }

        // Filtrer par statut
        if (_filterStatus != "Tous")
        {
            filtered = filtered.Where(i => i.Status == _filterStatus);
        }

        // Filtrer par actif seulement
        if (_showOnlyActive)
        {
            filtered = filtered.Where(i => i.IsActive || i.IsRecovering);
        }

        // Mettre à jour la collection
        Injuries.Clear();
        foreach (var injury in filtered)
        {
            Injuries.Add(injury);
        }

        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(TotalInjuries));
        this.RaisePropertyChanged(nameof(ActiveInjuries));
        this.RaisePropertyChanged(nameof(RecoveringWorkers));
        this.RaisePropertyChanged(nameof(CriticalInjuries));
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
            "En récupération",
            InjurySeverity.Moyenne
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ002",
            "Randy Orton",
            "W002",
            "Commotion cérébrale",
            DateTime.Now.AddDays(-5),
            21,
            "Actif",
            InjurySeverity.Grave
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ003",
            "Seth Rollins",
            "W003",
            "Entorse au genou",
            DateTime.Now.AddDays(-15),
            14,
            "En récupération",
            InjurySeverity.Legere
        ));

        Injuries.Add(new InjuryRecordViewModel(
            "INJ004",
            "Drew McIntyre",
            "W004",
            "Côtes fêlées",
            DateTime.Now.AddDays(-20),
            28,
            "En récupération",
            InjurySeverity.Moyenne
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
    private readonly InjurySeverity _severity;

    public InjuryRecordViewModel(
        string injuryId,
        string workerName,
        string workerId,
        string injuryType,
        DateTime injuryDate,
        int recoveryDays,
        string status,
        InjurySeverity severity)
    {
        InjuryId = injuryId;
        WorkerName = workerName;
        WorkerId = workerId;
        InjuryType = injuryType;
        InjuryDate = injuryDate;
        RecoveryDays = recoveryDays;
        _status = status;
        _severity = severity;
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

    public InjurySeverity Severity => _severity;

    public string SeverityDisplay => _severity switch
    {
        InjurySeverity.Legere => "Légère",
        InjurySeverity.Moyenne => "Moyenne",
        InjurySeverity.Grave => "Grave",
        _ => "Inconnue"
    };

    public DateTime ExpectedReturnDate => InjuryDate.AddDays(RecoveryDays);
    public int DaysRemaining => Math.Max(0, (ExpectedReturnDate - DateTime.Now).Days);
    public int WeeksRemaining => (int)Math.Ceiling(DaysRemaining / 7.0);
    public bool IsActive => Status == "Actif";
    public bool IsRecovering => Status == "En récupération";
    public bool IsCritical => _severity == InjurySeverity.Grave && IsActive;
    public double RecoveryProgress => RecoveryDays > 0 
        ? Math.Max(0, Math.Min(100, ((RecoveryDays - DaysRemaining) / (double)RecoveryDays) * 100))
        : 0;
    public string RecoverySummary => $"{DaysRemaining} jours restants ({ExpectedReturnDate:dd/MM/yyyy})";

    public void MarkAsHealed()
    {
        Status = "Guéri";
    }
}

using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using CrisisModel = RingGeneral.Core.Models.Crisis.Crisis;
using ICrisisRepository = RingGeneral.Data.Repositories.ICrisisRepository;

namespace RingGeneral.UI.ViewModels.Crisis;

/// <summary>
/// ViewModel pour la gestion des crises.
/// Affiche les crises actives et permet de lancer des communications pour les résoudre.
/// </summary>
public sealed class CrisisViewModel : ViewModelBase
{
    private readonly ICrisisRepository? _crisisRepository;
    private readonly ICrisisEngine? _crisisEngine;
    private readonly ICommunicationEngine? _communicationEngine;
    private string _companyId = string.Empty;

    // Properties
    private int _activeCrisesCount = 0;
    private int _criticalCrisesCount = 0;
    private int _totalCrisesResolved = 0;
    private double _communicationSuccessRate = 0.0;
    private CrisisModel? _selectedCrisis = null;
    private bool _isCommuncationDialogOpen = false;

    // Communication dialog properties
    private string _selectedCommunicationType = "One-on-One";
    private string _selectedTone = "Diplomatic";
    private string _communicationMessage = string.Empty;
    private int _predictedSuccessChance = 50;

    public CrisisViewModel(
        ICrisisRepository? crisisRepository = null,
        ICrisisEngine? crisisEngine = null,
        ICommunicationEngine? communicationEngine = null)
    {
        _crisisRepository = crisisRepository;
        _crisisEngine = crisisEngine;
        _communicationEngine = communicationEngine;

        // Collections observables
        ActiveCrises = new ObservableCollection<CrisisItemViewModel>();
        CriticalCrises = new ObservableCollection<CrisisItemViewModel>();
        CommunicationTypes = new ObservableCollection<string>
        {
            "One-on-One",
            "LockerRoomMeeting",
            "PublicStatement",
            "Mediation"
        };
        CommunicationTones = new ObservableCollection<string>
        {
            "Diplomatic",
            "Firm",
            "Apologetic",
            "Confrontational"
        };

        // Commandes
        RefreshDataCommand = ReactiveCommand.Create(OnRefreshData);
        OpenCommunicationDialogCommand = ReactiveCommand.Create<CrisisItemViewModel>(OnOpenCommunicationDialog);
        SendCommunicationCommand = ReactiveCommand.Create(OnSendCommunication);
        CancelCommunicationCommand = ReactiveCommand.Create(OnCancelCommunication);
        EscalateCrisisCommand = ReactiveCommand.Create<CrisisItemViewModel>(OnEscalateCrisis);

        // Charger les données au démarrage
        LoadCrisisData();
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public ObservableCollection<CrisisItemViewModel> ActiveCrises { get; }
    public ObservableCollection<CrisisItemViewModel> CriticalCrises { get; }
    public ObservableCollection<string> CommunicationTypes { get; }
    public ObservableCollection<string> CommunicationTones { get; }

    public int ActiveCrisesCount
    {
        get => _activeCrisesCount;
        set => this.RaiseAndSetIfChanged(ref _activeCrisesCount, value);
    }

    public int CriticalCrisesCount
    {
        get => _criticalCrisesCount;
        set => this.RaiseAndSetIfChanged(ref _criticalCrisesCount, value);
    }

    public int TotalCrisesResolved
    {
        get => _totalCrisesResolved;
        set => this.RaiseAndSetIfChanged(ref _totalCrisesResolved, value);
    }

    public double CommunicationSuccessRate
    {
        get => _communicationSuccessRate;
        set => this.RaiseAndSetIfChanged(ref _communicationSuccessRate, value);
    }

    public CrisisModel? SelectedCrisis
    {
        get => _selectedCrisis;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCrisis, value);
            UpdateRecommendations();
        }
    }

    public bool IsCommunicationDialogOpen
    {
        get => _isCommuncationDialogOpen;
        set => this.RaiseAndSetIfChanged(ref _isCommuncationDialogOpen, value);
    }

    public string SelectedCommunicationType
    {
        get => _selectedCommunicationType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCommunicationType, value);
            RecalculateSuccessChance();
        }
    }

    public string SelectedTone
    {
        get => _selectedTone;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTone, value);
            RecalculateSuccessChance();
        }
    }

    public string CommunicationMessage
    {
        get => _communicationMessage;
        set => this.RaiseAndSetIfChanged(ref _communicationMessage, value);
    }

    public int PredictedSuccessChance
    {
        get => _predictedSuccessChance;
        set => this.RaiseAndSetIfChanged(ref _predictedSuccessChance, value);
    }

    // Recommandations
    private string _recommendedType = "One-on-One";
    private string _recommendedTone = "Diplomatic";

    public string RecommendedType
    {
        get => _recommendedType;
        set => this.RaiseAndSetIfChanged(ref _recommendedType, value);
    }

    public string RecommendedTone
    {
        get => _recommendedTone;
        set => this.RaiseAndSetIfChanged(ref _recommendedTone, value);
    }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }
    public ReactiveCommand<CrisisItemViewModel, Unit> OpenCommunicationDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> SendCommunicationCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommunicationCommand { get; }
    public ReactiveCommand<CrisisItemViewModel, Unit> EscalateCrisisCommand { get; }

    // ====================================================================
    // METHODS
    // ====================================================================

    /// <summary>
    /// Charge les données des crises depuis le repository
    /// </summary>
    public void LoadCrisisData()
    {
        if (_crisisRepository == null || _crisisEngine == null || _communicationEngine == null)
        {
            Console.WriteLine("[CrisisViewModel] Services non initialisés");
            return;
        }

        try
        {
            // Récupérer l'ID de la compagnie du joueur
            _companyId = GetPlayerCompanyId();
            if (string.IsNullOrEmpty(_companyId))
            {
                Console.WriteLine("[CrisisViewModel] Aucune compagnie joueur trouvée");
                return;
            }

            // Charger les crises actives
            var activeCrises = _crisisEngine.GetActiveCrises(_companyId);
            var criticalCrises = _crisisEngine.GetCriticalCrises(_companyId);

            // Mettre à jour les collections
            ActiveCrises.Clear();
            foreach (var crisis in activeCrises)
            {
                ActiveCrises.Add(new CrisisItemViewModel(crisis));
            }

            CriticalCrises.Clear();
            foreach (var crisis in criticalCrises)
            {
                CriticalCrises.Add(new CrisisItemViewModel(crisis));
            }

            // Mettre à jour les statistiques
            ActiveCrisesCount = activeCrises.Count;
            CriticalCrisesCount = criticalCrises.Count;
            CommunicationSuccessRate = _communicationEngine.GetCommunicationSuccessRate(_companyId);

            // Compter les crises résolues
            TotalCrisesResolved = _crisisRepository.GetResolvedCrisesCountAsync(_companyId).Result;

            Console.WriteLine($"[CrisisViewModel] {ActiveCrisesCount} crises actives, {CriticalCrisesCount} critiques");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CrisisViewModel] Erreur lors du chargement: {ex.Message}");
        }
    }

    /// <summary>
    /// Récupère l'ID de la compagnie contrôlée par le joueur
    /// </summary>
    private string GetPlayerCompanyId()
    {
        // TODO: Récupérer depuis un service global ou GameRepository
        return "COMPANY_001"; // Placeholder
    }

    /// <summary>
    /// Met à jour les recommandations basées sur la crise sélectionnée
    /// </summary>
    private void UpdateRecommendations()
    {
        if (_communicationEngine == null || SelectedCrisis == null)
            return;

        RecommendedType = _communicationEngine.RecommendCommunicationType(SelectedCrisis);
        RecommendedTone = _communicationEngine.RecommendTone(SelectedCrisis, RecommendedType);
    }

    /// <summary>
    /// Recalcule la chance de succès basée sur les sélections
    /// </summary>
    private void RecalculateSuccessChance()
    {
        if (_communicationEngine == null || SelectedCrisis == null)
            return;

        // Utiliser une influence de 70 par défaut (player)
        PredictedSuccessChance = _communicationEngine.CalculateSuccessChance(
            SelectedCommunicationType,
            SelectedTone,
            SelectedCrisis,
            70
        );
    }

    /// <summary>
    /// Action pour rafraîchir les données
    /// </summary>
    private void OnRefreshData()
    {
        LoadCrisisData();
        Console.WriteLine("[CrisisViewModel] Données rafraîchies");
    }

    /// <summary>
    /// Ouvre le dialogue de communication pour une crise
    /// </summary>
    private void OnOpenCommunicationDialog(CrisisItemViewModel crisisItem)
    {
        if (crisisItem.Crisis == null)
            return;

        SelectedCrisis = crisisItem.Crisis;
        IsCommunicationDialogOpen = true;

        // Réinitialiser les champs
        CommunicationMessage = string.Empty;
        SelectedCommunicationType = RecommendedType;
        SelectedTone = RecommendedTone;

        Console.WriteLine($"[CrisisViewModel] Dialogue ouvert pour crise #{SelectedCrisis.CrisisId}");
    }

    /// <summary>
    /// Envoie une communication et traite le résultat
    /// </summary>
    private void OnSendCommunication()
    {
        if (_communicationEngine == null || SelectedCrisis == null)
            return;

        try
        {
            // Créer la communication
            var communication = _communicationEngine.CreateCommunication(
                _companyId,
                SelectedCrisis.CrisisId,
                SelectedCommunicationType,
                "PLAYER_ID", // TODO: Get from player service
                null, // Target ID (null for general)
                CommunicationMessage,
                SelectedTone
            );

            // Exécuter la communication
            var outcome = _communicationEngine.ExecuteCommunication(communication.CommunicationId);

            // Afficher le résultat
            Console.WriteLine($"[CrisisViewModel] Communication {(outcome.WasSuccessful ? "réussie" : "échouée")}: {outcome.Feedback}");

            // Fermer le dialogue
            IsCommunicationDialogOpen = false;

            // Recharger les données
            LoadCrisisData();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CrisisViewModel] Erreur lors de l'envoi: {ex.Message}");
        }
    }

    /// <summary>
    /// Annule le dialogue de communication
    /// </summary>
    private void OnCancelCommunication()
    {
        IsCommunicationDialogOpen = false;
        SelectedCrisis = null;
        Console.WriteLine("[CrisisViewModel] Dialogue annulé");
    }

    /// <summary>
    /// Force l'escalade manuelle d'une crise
    /// </summary>
    private void OnEscalateCrisis(CrisisItemViewModel crisisItem)
    {
        if (_crisisEngine == null || crisisItem.Crisis == null)
            return;

        var escalated = _crisisEngine.EscalateCrisis(crisisItem.Crisis.CrisisId);
        if (escalated != null)
        {
            Console.WriteLine($"[CrisisViewModel] Crise escaladée au stage: {escalated.Stage}");
            LoadCrisisData();
        }
    }
}

/// <summary>
/// ViewModel pour un item de crise
/// </summary>
public sealed class CrisisItemViewModel : ViewModelBase
{
    public CrisisModel? Crisis { get; }

    public CrisisItemViewModel(CrisisModel crisis)
    {
        Crisis = crisis;
    }

    public int CrisisId => Crisis?.CrisisId ?? 0;
    public string CrisisType => Crisis?.CrisisType ?? "Unknown";
    public string Stage => Crisis?.Stage ?? "Unknown";
    public int Severity => Crisis?.Severity ?? 1;
    public string Description => Crisis?.Description ?? "";
    public int EscalationScore => Crisis?.EscalationScore ?? 0;
    public int ResolutionAttempts => Crisis?.ResolutionAttempts ?? 0;
    public DateTime CreatedAt => Crisis?.CreatedAt ?? DateTime.Now;

    /// <summary>
    /// Icône basée sur le type de crise
    /// </summary>
    public string CrisisTypeIcon => CrisisType switch
    {
        "MoraleCollapse" => "😞",
        "RumorEscalation" => "💬",
        "WorkerGrievance" => "⚠️",
        "PublicScandal" => "🔥",
        "FinancialCrisis" => "💸",
        "TalentExodus" => "🏃",
        _ => "❗"
    };

    /// <summary>
    /// Label formatté du stage
    /// </summary>
    public string StageLabel => Stage switch
    {
        "WeakSignals" => "Signaux Faibles",
        "Rumors" => "Rumeurs",
        "Declared" => "Déclarée",
        "InResolution" => "En Résolution",
        "Resolved" => "Résolue",
        "Ignored" => "Ignorée",
        _ => Stage
    };

    /// <summary>
    /// Couleur du stage (pour binding)
    /// </summary>
    public string StageColor => Stage switch
    {
        "WeakSignals" => "#fbbf24",
        "Rumors" => "#f59e0b",
        "Declared" => "#ef4444",
        "InResolution" => "#3b82f6",
        _ => "#94a3b8"
    };

    /// <summary>
    /// Label de sévérité
    /// </summary>
    public string SeverityLabel => string.Concat(Enumerable.Repeat("🔴", Severity));

    /// <summary>
    /// Date formatée
    /// </summary>
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Barre d'escalade (visuel)
    /// </summary>
    public string EscalationBar
    {
        get
        {
            var barLength = EscalationScore / 10; // 0-10 caractères
            return new string('█', barLength) + new string('░', 10 - barLength);
        }
    }

    /// <summary>
    /// Indique si la crise est critique
    /// </summary>
    public bool IsCritical => Crisis?.IsCritical() ?? false;

    /// <summary>
    /// Couleur de l'escalade
    /// </summary>
    public string EscalationColor => EscalationScore switch
    {
        >= 80 => "#ef4444",
        >= 60 => "#f59e0b",
        >= 40 => "#fbbf24",
        _ => "#10b981"
    };
}

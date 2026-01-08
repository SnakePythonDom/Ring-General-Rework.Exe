using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Core.Models.Booker;

namespace RingGeneral.UI.ViewModels.OwnerBooker;

/// <summary>
/// ViewModel pour la vue Owner/Booker.
/// Affiche les profils de l'owner et du booker, ainsi que l'historique des décisions.
/// </summary>
public sealed class OwnerBookerViewModel : ViewModelBase
{
    private readonly OwnerRepository? _ownerRepository;
    private readonly BookerRepository? _bookerRepository;
    private string _companyId = string.Empty;

    // Owner properties
    private string _ownerName = "Propriétaire";
    private string _visionType = "Balanced";
    private int _riskTolerance = 50;
    private string _preferredProductType = "Entertainment";
    private int _talentDevelopmentFocus = 33;
    private int _financialPriority = 33;
    private int _fanSatisfactionPriority = 34;
    private string _dominantPriority = "Fan Satisfaction";

    // Booker properties
    private string _bookerName = "Booker";
    private int _creativityScore = 70;
    private int _logicScore = 70;
    private int _biasResistance = 50;
    private string _preferredStyle = "Balanced";
    private bool _likesUnderdog = false;
    private bool _likesVeteran = false;
    private bool _likesFastRise = false;
    private bool _likesSlowBurn = false;
    private bool _isAutoBookingEnabled = false;
    private string _employmentStatus = "Active";
    private DateTime? _hireDate = null;

    // Memory history
    private int _totalMemories = 0;
    private int _strongMemories = 0;

    public OwnerBookerViewModel(
        OwnerRepository? ownerRepository = null,
        BookerRepository? bookerRepository = null)
    {
        _ownerRepository = ownerRepository;
        _bookerRepository = bookerRepository;

        // Commandes
        ToggleAutoBookingCommand = ReactiveCommand.Create(OnToggleAutoBooking);
        RefreshDataCommand = ReactiveCommand.Create(OnRefreshData);

        // Collections observables
        BookerMemories = new ObservableCollection<BookerMemoryItemViewModel>();

        // Charger les données au démarrage
        LoadOwnerBookerData();
    }

    /// <summary>
    /// Commande pour activer/désactiver l'auto-booking
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleAutoBookingCommand { get; }

    /// <summary>
    /// Commande pour rafraîchir les données
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    // ====================================================================
    // OWNER PROPERTIES
    // ====================================================================

    public string OwnerName
    {
        get => _ownerName;
        set => this.RaiseAndSetIfChanged(ref _ownerName, value);
    }

    public string VisionType
    {
        get => _visionType;
        set => this.RaiseAndSetIfChanged(ref _visionType, value);
    }

    public int RiskTolerance
    {
        get => _riskTolerance;
        set => this.RaiseAndSetIfChanged(ref _riskTolerance, value);
    }

    public string PreferredProductType
    {
        get => _preferredProductType;
        set => this.RaiseAndSetIfChanged(ref _preferredProductType, value);
    }

    public int TalentDevelopmentFocus
    {
        get => _talentDevelopmentFocus;
        set => this.RaiseAndSetIfChanged(ref _talentDevelopmentFocus, value);
    }

    public int FinancialPriority
    {
        get => _financialPriority;
        set => this.RaiseAndSetIfChanged(ref _financialPriority, value);
    }

    public int FanSatisfactionPriority
    {
        get => _fanSatisfactionPriority;
        set => this.RaiseAndSetIfChanged(ref _fanSatisfactionPriority, value);
    }

    public string DominantPriority
    {
        get => _dominantPriority;
        set => this.RaiseAndSetIfChanged(ref _dominantPriority, value);
    }

    // ====================================================================
    // BOOKER PROPERTIES
    // ====================================================================

    public string BookerName
    {
        get => _bookerName;
        set => this.RaiseAndSetIfChanged(ref _bookerName, value);
    }

    public int CreativityScore
    {
        get => _creativityScore;
        set => this.RaiseAndSetIfChanged(ref _creativityScore, value);
    }

    public int LogicScore
    {
        get => _logicScore;
        set => this.RaiseAndSetIfChanged(ref _logicScore, value);
    }

    public int BiasResistance
    {
        get => _biasResistance;
        set => this.RaiseAndSetIfChanged(ref _biasResistance, value);
    }

    public string PreferredStyle
    {
        get => _preferredStyle;
        set => this.RaiseAndSetIfChanged(ref _preferredStyle, value);
    }

    public bool LikesUnderdog
    {
        get => _likesUnderdog;
        set => this.RaiseAndSetIfChanged(ref _likesUnderdog, value);
    }

    public bool LikesVeteran
    {
        get => _likesVeteran;
        set => this.RaiseAndSetIfChanged(ref _likesVeteran, value);
    }

    public bool LikesFastRise
    {
        get => _likesFastRise;
        set => this.RaiseAndSetIfChanged(ref _likesFastRise, value);
    }

    public bool LikesSlowBurn
    {
        get => _likesSlowBurn;
        set => this.RaiseAndSetIfChanged(ref _likesSlowBurn, value);
    }

    public bool IsAutoBookingEnabled
    {
        get => _isAutoBookingEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isAutoBookingEnabled, value);
            this.RaisePropertyChanged(nameof(AutoBookingStatusLabel));
            this.RaisePropertyChanged(nameof(AutoBookingStatusIcon));
        }
    }

    public string EmploymentStatus
    {
        get => _employmentStatus;
        set => this.RaiseAndSetIfChanged(ref _employmentStatus, value);
    }

    public DateTime? HireDate
    {
        get => _hireDate;
        set => this.RaiseAndSetIfChanged(ref _hireDate, value);
    }

    /// <summary>
    /// Label pour le statut de l'auto-booking
    /// </summary>
    public string AutoBookingStatusLabel => IsAutoBookingEnabled ? "Auto-Booking Activé" : "Auto-Booking Désactivé";

    /// <summary>
    /// Icône pour le statut de l'auto-booking
    /// </summary>
    public string AutoBookingStatusIcon => IsAutoBookingEnabled ? "✅" : "❌";

    /// <summary>
    /// Date d'embauche formatée
    /// </summary>
    public string HireDateFormatted => HireDate?.ToString("dd/MM/yyyy") ?? "N/A";

    // ====================================================================
    // MEMORY PROPERTIES
    // ====================================================================

    public ObservableCollection<BookerMemoryItemViewModel> BookerMemories { get; }

    public int TotalMemories
    {
        get => _totalMemories;
        set => this.RaiseAndSetIfChanged(ref _totalMemories, value);
    }

    public int StrongMemories
    {
        get => _strongMemories;
        set => this.RaiseAndSetIfChanged(ref _strongMemories, value);
    }

    // ====================================================================
    // METHODS
    // ====================================================================

    /// <summary>
    /// Charge les données de l'owner et du booker depuis le repository
    /// </summary>
    public void LoadOwnerBookerData()
    {
        if (_ownerRepository == null || _bookerRepository == null)
        {
            Console.WriteLine("[OwnerBookerViewModel] Repositories non initialisés");
            return;
        }

        try
        {
            // Récupérer l'ID de la compagnie du joueur
            _companyId = GetPlayerCompanyId();
            if (string.IsNullOrEmpty(_companyId))
            {
                Console.WriteLine("[OwnerBookerViewModel] Aucune compagnie joueur trouvée");
                return;
            }

            // Charger les données de l'owner
            LoadOwnerData(_companyId);

            // Charger les données du booker
            LoadBookerData(_companyId);

            // Charger l'historique des mémoires
            LoadBookerMemories();

            Console.WriteLine($"[OwnerBookerViewModel] Données chargées pour {_companyId}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OwnerBookerViewModel] Erreur lors du chargement: {ex.Message}");
        }
    }

    /// <summary>
    /// Récupère l'ID de la compagnie contrôlée par le joueur
    /// </summary>
    private string GetPlayerCompanyId()
    {
        // TODO: Récupérer depuis un service global ou GameRepository
        // Pour l'instant, on suppose que c'est la première compagnie active
        return "COMPANY_001"; // Placeholder
    }

    /// <summary>
    /// Charge les données de l'owner
    /// </summary>
    private void LoadOwnerData(string companyId)
    {
        if (_ownerRepository == null)
            return;

        var owner = _ownerRepository.GetOwnerByCompanyIdAsync(companyId).Result;
        if (owner != null)
        {
            OwnerName = owner.Name;
            VisionType = owner.VisionType;
            RiskTolerance = owner.RiskTolerance;
            PreferredProductType = owner.PreferredProductType;
            TalentDevelopmentFocus = owner.TalentDevelopmentFocus;
            FinancialPriority = owner.FinancialPriority;
            FanSatisfactionPriority = owner.FanSatisfactionPriority;
            DominantPriority = owner.GetDominantPriority();

            Console.WriteLine($"[OwnerBookerViewModel] Owner chargé: {OwnerName} ({VisionType})");
        }
    }

    /// <summary>
    /// Charge les données du booker actif
    /// </summary>
    private void LoadBookerData(string companyId)
    {
        if (_bookerRepository == null)
            return;

        var booker = _bookerRepository.GetActiveBookerAsync(companyId).Result;
        if (booker != null)
        {
            BookerName = booker.Name;
            CreativityScore = booker.CreativityScore;
            LogicScore = booker.LogicScore;
            BiasResistance = booker.BiasResistance;
            PreferredStyle = booker.PreferredStyle;
            LikesUnderdog = booker.LikesUnderdog;
            LikesVeteran = booker.LikesVeteran;
            LikesFastRise = booker.LikesFastRise;
            LikesSlowBurn = booker.LikesSlowBurn;
            IsAutoBookingEnabled = booker.IsAutoBookingEnabled;
            EmploymentStatus = booker.EmploymentStatus;
            HireDate = booker.HireDate;

            Console.WriteLine($"[OwnerBookerViewModel] Booker chargé: {BookerName} (Auto: {IsAutoBookingEnabled})");
        }
    }

    /// <summary>
    /// Charge l'historique des mémoires du booker
    /// </summary>
    private void LoadBookerMemories()
    {
        if (_bookerRepository == null)
            return;

        try
        {
            var booker = _bookerRepository.GetActiveBookerAsync(_companyId).Result;
            if (booker == null)
                return;

            var memories = _bookerRepository.GetRecentMemoriesAsync(booker.BookerId, 10).Result;

            BookerMemories.Clear();
            foreach (var memory in memories)
            {
                BookerMemories.Add(new BookerMemoryItemViewModel(memory));
            }

            TotalMemories = _bookerRepository.CountMemoriesAsync(booker.BookerId).Result;
            StrongMemories = _bookerRepository.GetStrongMemoriesAsync(booker.BookerId).Result.Count;

            Console.WriteLine($"[OwnerBookerViewModel] {BookerMemories.Count} mémoires chargées ({StrongMemories} fortes)");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OwnerBookerViewModel] Erreur chargement mémoires: {ex.Message}");
        }
    }

    /// <summary>
    /// Action pour activer/désactiver l'auto-booking
    /// </summary>
    private void OnToggleAutoBooking()
    {
        if (_bookerRepository == null)
            return;

        try
        {
            var booker = _bookerRepository.GetActiveBookerAsync(_companyId).Result;
            if (booker == null)
            {
                Console.WriteLine("[OwnerBookerViewModel] Aucun booker actif trouvé");
                return;
            }

            // Toggle
            var updatedBooker = booker with { IsAutoBookingEnabled = !booker.IsAutoBookingEnabled };
            _bookerRepository.UpdateBookerAsync(updatedBooker).Wait();

            // Mettre à jour la propriété
            IsAutoBookingEnabled = updatedBooker.IsAutoBookingEnabled;

            var status = IsAutoBookingEnabled ? "activé" : "désactivé";
            Console.WriteLine($"[OwnerBookerViewModel] Auto-booking {status}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OwnerBookerViewModel] Erreur toggle auto-booking: {ex.Message}");
        }
    }

    /// <summary>
    /// Action pour rafraîchir les données
    /// </summary>
    private void OnRefreshData()
    {
        LoadOwnerBookerData();
        Console.WriteLine("[OwnerBookerViewModel] Données rafraîchies");
    }
}

/// <summary>
/// ViewModel pour un item de mémoire de booker
/// </summary>
public sealed class BookerMemoryItemViewModel : ViewModelBase
{
    private readonly BookerMemory _memory;

    public BookerMemoryItemViewModel(BookerMemory memory)
    {
        _memory = memory;
    }

    public string EventType => _memory.EventType;
    public string EventDescription => _memory.EventDescription;
    public int ImpactScore => _memory.ImpactScore;
    public int RecallStrength => _memory.RecallStrength;
    public DateTime CreatedAt => _memory.CreatedAt;

    /// <summary>
    /// Label formaté pour l'affichage
    /// </summary>
    public string DisplayLabel => $"{EventTypeIcon} {EventDescription}";

    /// <summary>
    /// Impact formaté avec couleur
    /// </summary>
    public string ImpactFormatted => ImpactScore >= 0 ? $"+{ImpactScore}" : $"{ImpactScore}";

    /// <summary>
    /// Couleur de l'impact (pour binding)
    /// </summary>
    public string ImpactColor => ImpactScore >= 0 ? "Green" : "Red";

    /// <summary>
    /// Date formatée
    /// </summary>
    public string DateFormatted => CreatedAt.ToString("dd/MM/yyyy");

    /// <summary>
    /// Icône basée sur le type d'événement
    /// </summary>
    public string EventTypeIcon => EventType switch
    {
        "GoodMatch" => "⭐",
        "BadMatch" => "❌",
        "WorkerComplaint" => "😠",
        "FanReaction" => "👏",
        "OwnerFeedback" => "💼",
        "ChampionshipDecision" => "🏆",
        "PushSuccess" => "📈",
        "PushFailure" => "📉",
        _ => "📝"
    };

    /// <summary>
    /// Barre de force de rappel (visuel)
    /// </summary>
    public string RecallStrengthBar
    {
        get
        {
            var barLength = RecallStrength / 10; // 0-10 caractères
            return new string('█', barLength) + new string('░', 10 - barLength);
        }
    }
}

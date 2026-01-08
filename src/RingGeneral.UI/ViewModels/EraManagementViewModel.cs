using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels;

/// <summary>
/// ViewModel pour l'affichage d'une ère dans la liste.
/// </summary>
public sealed class EraListItemViewModel : ViewModelBase
{
    public EraListItemViewModel(
        string eraId,
        string type,
        int intensity,
        bool isCurrentEra,
        int preferredMatchDuration,
        DateTime createdAt)
    {
        EraId = eraId;
        Type = type;
        Intensity = intensity;
        IsCurrentEra = isCurrentEra;
        PreferredMatchDuration = preferredMatchDuration;
        CreatedAt = createdAt;

        StatusLabel = isCurrentEra ? "✓ ACTUELLE" : "Historique";
        IntensityLabel = intensity >= 70 ? "Haute" :
                        intensity >= 40 ? "Modérée" : "Faible";
    }

    public string EraId { get; }
    public string Type { get; }
    public int Intensity { get; }
    public bool IsCurrentEra { get; }
    public int PreferredMatchDuration { get; }
    public DateTime CreatedAt { get; }
    public string StatusLabel { get; }
    public string IntensityLabel { get; }

    public string Summary => $"{Type} • {IntensityLabel} ({Intensity}/100) • Match: {PreferredMatchDuration}min";
}

/// <summary>
/// ViewModel pour une transition d'ère en cours.
/// </summary>
public sealed class EraTransitionViewModel : ViewModelBase
{
    private int _progressPercentage;
    private int _moraleImpact;
    private int _audienceImpact;

    public EraTransitionViewModel(
        string transitionId,
        string fromEraType,
        string toEraType,
        int progressPercentage,
        string speed,
        int moraleImpact,
        int audienceImpact,
        int changeResistance,
        DateTime startedAt)
    {
        TransitionId = transitionId;
        FromEraType = fromEraType;
        ToEraType = toEraType;
        _progressPercentage = progressPercentage;
        Speed = speed;
        _moraleImpact = moraleImpact;
        _audienceImpact = audienceImpact;
        ChangeResistance = changeResistance;
        StartedAt = startedAt;

        ProgressLabel = $"{progressPercentage}%";
        ImpactSummary = $"Morale: {moraleImpact:+#;-#;0} • Audience: {audienceImpact:+#;-#;0}";
    }

    public string TransitionId { get; }
    public string FromEraType { get; }
    public string ToEraType { get; }

    public int ProgressPercentage
    {
        get => _progressPercentage;
        set => this.RaiseAndSetIfChanged(ref _progressPercentage, value);
    }

    public string Speed { get; }
    public int ChangeResistance { get; }
    public DateTime StartedAt { get; }

    public int MoraleImpact
    {
        get => _moraleImpact;
        set => this.RaiseAndSetIfChanged(ref _moraleImpact, value);
    }

    public int AudienceImpact
    {
        get => _audienceImpact;
        set => this.RaiseAndSetIfChanged(ref _audienceImpact, value);
    }

    public string ProgressLabel { get; private set; }
    public string ImpactSummary { get; private set; }

    public string TransitionSummary => $"{FromEraType} → {ToEraType} ({Speed}) • {ProgressLabel}";

    public void UpdateProgress(int newProgress, int newMoraleImpact, int newAudienceImpact)
    {
        ProgressPercentage = newProgress;
        MoraleImpact = newMoraleImpact;
        AudienceImpact = newAudienceImpact;
        ProgressLabel = $"{newProgress}%";
        ImpactSummary = $"Morale: {newMoraleImpact:+#;-#;0} • Audience: {newAudienceImpact:+#;-#;0}";
    }
}

/// <summary>
/// Option de type d'ère pour les sélecteurs.
/// </summary>
public sealed class EraTypeOptionViewModel
{
    public EraTypeOptionViewModel(EraType type, string description, int compatibility)
    {
        Type = type;
        TypeName = type.ToString();
        Description = description;
        Compatibility = compatibility;
        CompatibilityLabel = compatibility >= 70 ? "Forte" :
                            compatibility >= 40 ? "Modérée" : "Faible";
    }

    public EraType Type { get; }
    public string TypeName { get; }
    public string Description { get; }
    public int Compatibility { get; }
    public string CompatibilityLabel { get; }

    public string DisplayText => $"{TypeName} • Compatibilité: {CompatibilityLabel} ({Compatibility}/100)";
}

/// <summary>
/// ViewModel principal pour la gestion des ères.
/// </summary>
public sealed class EraManagementViewModel : ViewModelBase
{
    private readonly IEraRepository _eraRepository;
    private readonly EraTransitionService _transitionService;
    private readonly string _companyId;

    private EraListItemViewModel? _currentEra;
    private EraTransitionViewModel? _activeTransition;
    private bool _isTransitioning;

    public EraManagementViewModel(
        string companyId,
        IEraRepository eraRepository,
        EraTransitionService transitionService)
    {
        _companyId = companyId;
        _eraRepository = eraRepository;
        _transitionService = transitionService;

        EraHistory = new ObservableCollection<EraListItemViewModel>();
        AvailableEraTypes = new ObservableCollection<EraTypeOptionViewModel>();

        InitiateTransitionCommand = ReactiveCommand.CreateFromTask<EraType>(InitiateTransitionAsync);
        AccelerateTransitionCommand = ReactiveCommand.CreateFromTask(AccelerateTransitionAsync);
        CancelTransitionCommand = ReactiveCommand.CreateFromTask(CancelTransitionAsync);
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public ObservableCollection<EraListItemViewModel> EraHistory { get; }
    public ObservableCollection<EraTypeOptionViewModel> AvailableEraTypes { get; }

    public EraListItemViewModel? CurrentEra
    {
        get => _currentEra;
        private set => this.RaiseAndSetIfChanged(ref _currentEra, value);
    }

    public EraTransitionViewModel? ActiveTransition
    {
        get => _activeTransition;
        private set => this.RaiseAndSetIfChanged(ref _activeTransition, value);
    }

    public bool IsTransitioning
    {
        get => _isTransitioning;
        private set => this.RaiseAndSetIfChanged(ref _isTransitioning, value);
    }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<EraType, Unit> InitiateTransitionCommand { get; }
    public ReactiveCommand<Unit, Unit> AccelerateTransitionCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelTransitionCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public async Task LoadDataAsync()
    {
        try
        {
            Logger.Log("Loading era management data...");

            // Charger l'ère actuelle
            var currentEraModel = await _eraRepository.GetCurrentEraAsync(_companyId);
            if (currentEraModel != null)
            {
                CurrentEra = MapEraToViewModel(currentEraModel);

                // Charger les options d'ères disponibles avec compatibilité
                await LoadAvailableEraTypesAsync(currentEraModel);
            }

            // Charger l'historique des ères
            var eraHistoryModels = await _eraRepository.GetErasByCompanyIdAsync(_companyId);
            EraHistory.Clear();
            foreach (var era in eraHistoryModels.OrderByDescending(e => e.CreatedAt))
            {
                EraHistory.Add(MapEraToViewModel(era));
            }

            // Charger la transition active si existe
            var activeTransitionModel = await _eraRepository.GetActiveTransitionAsync(_companyId);
            if (activeTransitionModel != null)
            {
                ActiveTransition = MapTransitionToViewModel(activeTransitionModel);
                IsTransitioning = true;
            }
            else
            {
                ActiveTransition = null;
                IsTransitioning = false;
            }

            Logger.Log($"Loaded {EraHistory.Count} eras, current: {CurrentEra?.Type ?? "None"}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error loading era data: {ex.Message}");
        }
    }

    // ====================================================================
    // PRIVATE COMMAND HANDLERS
    // ====================================================================

    private async Task InitiateTransitionAsync(EraType targetEraType)
    {
        if (CurrentEra == null || IsTransitioning)
        {
            Logger.Log("Cannot initiate transition: no current era or transition already in progress");
            return;
        }

        try
        {
            Logger.Log($"Initiating era transition to {targetEraType}...");

            var currentEraModel = await _eraRepository.GetEraByIdAsync(CurrentEra.EraId);
            if (currentEraModel == null)
            {
                Logger.Log("Error: Current era not found");
                return;
            }

            // Créer la transition avec vitesse modérée par défaut
            var transition = _transitionService.InitiateTransition(
                currentEraModel,
                targetEraType,
                EraTransitionSpeed.Moderate,
                bookerId: null);

            // Sauvegarder la transition
            await _eraRepository.SaveTransitionAsync(transition);

            // Recharger les données
            await LoadDataAsync();

            Logger.Log($"Era transition initiated: {CurrentEra.Type} → {targetEraType}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error initiating transition: {ex.Message}");
        }
    }

    private async Task AccelerateTransitionAsync()
    {
        if (ActiveTransition == null)
        {
            Logger.Log("No active transition to accelerate");
            return;
        }

        try
        {
            Logger.Log("Accelerating era transition...");

            // Augmenter la progression de 10%
            var newProgress = Math.Min(ActiveTransition.ProgressPercentage + 10, 100);

            // Recalculer les impacts (accélération augmente les impacts négatifs)
            var newMoraleImpact = ActiveTransition.MoraleImpact - 5;
            var newAudienceImpact = ActiveTransition.AudienceImpact - 3;

            // Mettre à jour le ViewModel
            ActiveTransition.UpdateProgress(newProgress, newMoraleImpact, newAudienceImpact);

            // Sauvegarder en DB
            var transitionModel = await _eraRepository.GetTransitionByIdAsync(ActiveTransition.TransitionId);
            if (transitionModel != null)
            {
                var updatedTransition = transitionModel with
                {
                    ProgressPercentage = newProgress,
                    MoraleImpact = newMoraleImpact,
                    AudienceImpact = newAudienceImpact
                };
                await _eraRepository.UpdateTransitionAsync(updatedTransition);
            }

            Logger.Log($"Transition accelerated to {newProgress}%");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error accelerating transition: {ex.Message}");
        }
    }

    private async Task CancelTransitionAsync()
    {
        if (ActiveTransition == null)
        {
            Logger.Log("No active transition to cancel");
            return;
        }

        try
        {
            Logger.Log("Cancelling era transition...");

            await _eraRepository.CancelTransitionAsync(ActiveTransition.TransitionId);

            ActiveTransition = null;
            IsTransitioning = false;

            Logger.Log("Era transition cancelled");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error cancelling transition: {ex.Message}");
        }
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private async Task LoadAvailableEraTypesAsync(Era currentEra)
    {
        AvailableEraTypes.Clear();

        var descriptions = new Dictionary<EraType, string>
        {
            [EraType.Technical] = "Matches techniques et purs",
            [EraType.Entertainment] = "Entertainment et spectacle",
            [EraType.Hardcore] = "Violence et hardcore wrestling",
            [EraType.SportsEntertainment] = "Mix sport et entertainment",
            [EraType.LuchaLibre] = "Style lucha libre mexicain",
            [EraType.StrongStyle] = "Strong style japonais",
            [EraType.Developmental] = "Développement de jeunes talents",
            [EraType.Mainstream] = "Produit grand public"
        };

        foreach (EraType type in Enum.GetValues(typeof(EraType)))
        {
            if (type != currentEra.Type) // Ne pas afficher l'ère actuelle
            {
                var compatibility = currentEra.GetCompatibilityWith(type);
                var description = descriptions.ContainsKey(type) ? descriptions[type] : type.ToString();

                AvailableEraTypes.Add(new EraTypeOptionViewModel(type, description, compatibility));
            }
        }
    }

    private static EraListItemViewModel MapEraToViewModel(Era era)
    {
        return new EraListItemViewModel(
            era.EraId,
            era.Type.ToString(),
            era.Intensity,
            era.IsCurrentEra,
            era.PreferredMatchDuration,
            era.CreatedAt);
    }

    private static EraTransitionViewModel MapTransitionToViewModel(EraTransition transition)
    {
        return new EraTransitionViewModel(
            transition.TransitionId,
            transition.FromEraType.ToString(),
            transition.ToEraType.ToString(),
            transition.ProgressPercentage,
            transition.Speed.ToString(),
            transition.MoraleImpact,
            transition.AudienceImpact,
            transition.ChangeResistance,
            transition.StartedAt);
    }
}

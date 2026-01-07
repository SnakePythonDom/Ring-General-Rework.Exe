using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la configuration du système de booking
/// Gère les paramètres globaux de booking, validation rules, et préférences
/// </summary>
public sealed class BookingSettingsViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private int _defaultSegmentDuration = 15;
    private int _defaultShowDuration = 120;
    private bool _autoValidateOnChange = true;
    private bool _requireMainEvent = true;
    private bool _allowOverrun = false;
    private int _maxOverrunMinutes = 10;
    private bool _enforceWorkerCooldown = true;
    private int _workerCooldownDays = 7;
    private string _selectedTheme = "FM26 Dark";

    public BookingSettingsViewModel(GameRepository repository)
    {
        _repository = repository;

        // Collections
        Themes = new ObservableCollection<string>
        {
            "FM26 Dark",
            "FM26 Light",
            "Classic",
            "Modern"
        };

        ValidationRules = new ObservableCollection<ValidationRuleViewModel>();

        // Commands
        SaveSettingsCommand = ReactiveCommand.Create(SaveSettings);
        ResetToDefaultsCommand = ReactiveCommand.Create(ResetToDefaults);
        AddValidationRuleCommand = ReactiveCommand.Create(AddValidationRule);
        RemoveValidationRuleCommand = ReactiveCommand.Create<ValidationRuleViewModel>(RemoveValidationRule);
        TestValidationCommand = ReactiveCommand.Create(TestValidation);

        // Initialisation
        LoadSettings();
        LoadValidationRules();
    }

    // ========== COLLECTIONS ==========

    public ObservableCollection<string> Themes { get; }
    public ObservableCollection<ValidationRuleViewModel> ValidationRules { get; }

    // ========== PARAMÈTRES GÉNÉRAUX ==========

    public int DefaultSegmentDuration
    {
        get => _defaultSegmentDuration;
        set => this.RaiseAndSetIfChanged(ref _defaultSegmentDuration, value);
    }

    public int DefaultShowDuration
    {
        get => _defaultShowDuration;
        set => this.RaiseAndSetIfChanged(ref _defaultShowDuration, value);
    }

    public bool AutoValidateOnChange
    {
        get => _autoValidateOnChange;
        set => this.RaiseAndSetIfChanged(ref _autoValidateOnChange, value);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
    }

    // ========== RÈGLES DE VALIDATION ==========

    public bool RequireMainEvent
    {
        get => _requireMainEvent;
        set => this.RaiseAndSetIfChanged(ref _requireMainEvent, value);
    }

    public bool AllowOverrun
    {
        get => _allowOverrun;
        set => this.RaiseAndSetIfChanged(ref _allowOverrun, value);
    }

    public int MaxOverrunMinutes
    {
        get => _maxOverrunMinutes;
        set => this.RaiseAndSetIfChanged(ref _maxOverrunMinutes, value);
    }

    public bool EnforceWorkerCooldown
    {
        get => _enforceWorkerCooldown;
        set => this.RaiseAndSetIfChanged(ref _enforceWorkerCooldown, value);
    }

    public int WorkerCooldownDays
    {
        get => _workerCooldownDays;
        set => this.RaiseAndSetIfChanged(ref _workerCooldownDays, value);
    }

    // ========== STATISTIQUES ==========

    public int ActiveRulesCount => ValidationRules.Count(r => r.IsEnabled);
    public string ValidationModeText => AutoValidateOnChange ? "Automatique" : "Manuel";

    // ========== COMMANDS ==========

    public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetToDefaultsCommand { get; }
    public ReactiveCommand<Unit, Unit> AddValidationRuleCommand { get; }
    public ReactiveCommand<ValidationRuleViewModel, Unit> RemoveValidationRuleCommand { get; }
    public ReactiveCommand<Unit, Unit> TestValidationCommand { get; }

    // ========== MÉTHODES PUBLIQUES ==========

    public void LoadSettings()
    {
        // TODO: Charger depuis le repository ou fichier de configuration
        System.Diagnostics.Debug.WriteLine("Loading booking settings...");

        // Les valeurs par défaut sont déjà définies dans les champs
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void SaveSettings()
    {
        // TODO: Sauvegarder dans le repository ou fichier de configuration
        System.Diagnostics.Debug.WriteLine("Saving booking settings...");
        System.Diagnostics.Debug.WriteLine($"  Default Segment Duration: {DefaultSegmentDuration} min");
        System.Diagnostics.Debug.WriteLine($"  Default Show Duration: {DefaultShowDuration} min");
        System.Diagnostics.Debug.WriteLine($"  Auto Validate: {AutoValidateOnChange}");
        System.Diagnostics.Debug.WriteLine($"  Theme: {SelectedTheme}");
    }

    private void ResetToDefaults()
    {
        DefaultSegmentDuration = 15;
        DefaultShowDuration = 120;
        AutoValidateOnChange = true;
        RequireMainEvent = true;
        AllowOverrun = false;
        MaxOverrunMinutes = 10;
        EnforceWorkerCooldown = true;
        WorkerCooldownDays = 7;
        SelectedTheme = "FM26 Dark";

        System.Diagnostics.Debug.WriteLine("Settings reset to defaults");
    }

    private void AddValidationRule()
    {
        var newRule = new ValidationRuleViewModel(
            $"RULE-{Guid.NewGuid():N}".ToUpperInvariant(),
            "Nouvelle règle",
            "Aucune catégorie",
            "error",
            true,
            "Décrivez la règle de validation"
        );

        ValidationRules.Add(newRule);
        UpdateStatistics();
    }

    private void RemoveValidationRule(ValidationRuleViewModel rule)
    {
        ValidationRules.Remove(rule);
        UpdateStatistics();
    }

    private void TestValidation()
    {
        // TODO: Tester les règles de validation
        System.Diagnostics.Debug.WriteLine($"Testing validation with {ActiveRulesCount} active rules");
    }

    private void LoadValidationRules()
    {
        // Règles de test
        ValidationRules.Add(new ValidationRuleViewModel(
            "RULE001",
            "Main Event requis",
            "Booking",
            "error",
            RequireMainEvent,
            "Chaque show doit avoir un main event"
        ));

        ValidationRules.Add(new ValidationRuleViewModel(
            "RULE002",
            "Durée minimale des segments",
            "Segments",
            "warning",
            true,
            "Les segments doivent durer au moins 5 minutes"
        ));

        ValidationRules.Add(new ValidationRuleViewModel(
            "RULE003",
            "Workers en cooldown",
            "Roster",
            "warning",
            EnforceWorkerCooldown,
            "Éviter de booker le même worker trop souvent"
        ));

        ValidationRules.Add(new ValidationRuleViewModel(
            "RULE004",
            "Balance Face/Heel",
            "Booking",
            "info",
            false,
            "Maintenir un équilibre entre faces et heels"
        ));

        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(ActiveRulesCount));
    }
}

// ========== VIEWMODEL HELPER ==========

/// <summary>
/// ViewModel représentant une règle de validation
/// </summary>
public sealed class ValidationRuleViewModel : ViewModelBase
{
    private bool _isEnabled;

    public ValidationRuleViewModel(
        string ruleId,
        string name,
        string category,
        string severity,
        bool isEnabled,
        string description)
    {
        RuleId = ruleId;
        Name = name;
        Category = category;
        Severity = severity;
        _isEnabled = isEnabled;
        Description = description;
    }

    public string RuleId { get; }
    public string Name { get; }
    public string Category { get; }
    public string Severity { get; } // "error", "warning", "info"
    public string Description { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public string SeverityIcon => Severity switch
    {
        "error" => "❌",
        "warning" => "⚠️",
        "info" => "ℹ️",
        _ => "•"
    };

    public string StatusText => IsEnabled ? "Activé" : "Désactivé";
}

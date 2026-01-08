using System;
using System.Reactive;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels.Settings;

/// <summary>
/// ViewModel pour les paramètres de l'application
/// Permet de configurer les préférences et options du jeu
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private bool _autoSave = true;
    private int _autoSaveInterval = 5;
    private bool _showNotifications = true;
    private bool _enableSound = true;
    private int _soundVolume = 70;
    private string _language = "Français";
    private string _theme = "Sombre";
    private bool _confirmActions = true;

    public SettingsViewModel()
    {
        // Commands
        SaveSettingsCommand = ReactiveCommand.Create(SaveSettings);
        ResetDefaultsCommand = ReactiveCommand.Create(ResetDefaults);

        Logger.Info("SettingsViewModel initialisé");
    }

    // ========== SAUVEGARDE ==========

    public bool AutoSave
    {
        get => _autoSave;
        set => this.RaiseAndSetIfChanged(ref _autoSave, value);
    }

    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref _autoSaveInterval, value);
    }

    // ========== NOTIFICATIONS ==========

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => this.RaiseAndSetIfChanged(ref _showNotifications, value);
    }

    public bool ConfirmActions
    {
        get => _confirmActions;
        set => this.RaiseAndSetIfChanged(ref _confirmActions, value);
    }

    // ========== AUDIO ==========

    public bool EnableSound
    {
        get => _enableSound;
        set => this.RaiseAndSetIfChanged(ref _enableSound, value);
    }

    public int SoundVolume
    {
        get => _soundVolume;
        set => this.RaiseAndSetIfChanged(ref _soundVolume, value);
    }

    // ========== INTERFACE ==========

    public string Language
    {
        get => _language;
        set => this.RaiseAndSetIfChanged(ref _language, value);
    }

    public string Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    // ========== COMMANDS ==========

    public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetDefaultsCommand { get; }

    // ========== MÉTHODES ==========

    private void SaveSettings()
    {
        // TODO: Sauvegarder dans le repository
        Logger.Info("Paramètres sauvegardés");
        System.Diagnostics.Debug.WriteLine("Settings saved!");
    }

    private void ResetDefaults()
    {
        AutoSave = true;
        AutoSaveInterval = 5;
        ShowNotifications = true;
        EnableSound = true;
        SoundVolume = 70;
        Language = "Français";
        Theme = "Sombre";
        ConfirmActions = true;

        Logger.Info("Paramètres réinitialisés aux valeurs par défaut");
    }
}

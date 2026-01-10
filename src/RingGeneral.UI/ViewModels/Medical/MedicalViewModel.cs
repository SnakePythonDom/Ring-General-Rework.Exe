using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.UI.ViewModels.Roster;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Medical;

public sealed class MedicalViewModel : ViewModelBase
{
    private MedicalTab _selectedTab = MedicalTab.Active;
    private readonly InjuriesViewModel? _injuriesViewModel;

    public MedicalViewModel(
        GameRepository? gameRepository = null,
        MedicalRepository? medicalRepository = null)
    {
        Workers = new ObservableCollection<MedicalWorkerRow>
        {
            new()
            {
                Name = "John \"The Titan\" Doe",
                ConditionPercent = 95,
                FatiguePercent = 5,
                StatusLabel = "Disponible",
                StatusBackground = "#065f46",
                StatusForeground = "#6ee7b7"
            }
        };

        // Créer InjuriesViewModel si les repositories sont disponibles
        if (gameRepository != null && medicalRepository != null)
        {
            _injuriesViewModel = new InjuriesViewModel(gameRepository, medicalRepository);
        }
    }

    public ObservableCollection<MedicalWorkerRow> Workers { get; }

    /// <summary>
    /// ViewModel pour l'onglet Infirmerie (blessures)
    /// </summary>
    public InjuriesViewModel? InjuriesViewModel => _injuriesViewModel;

    public MedicalTab SelectedTab
    {
        get => _selectedTab;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTab, value);
            this.RaisePropertyChanged(nameof(IsActiveTabSelected));
            this.RaisePropertyChanged(nameof(IsInjuredTabSelected));
            this.RaisePropertyChanged(nameof(IsHistoryTabSelected));
        }
    }

    public bool IsActiveTabSelected
    {
        get => SelectedTab == MedicalTab.Active;
        set
        {
            if (value)
            {
                SelectedTab = MedicalTab.Active;
            }
        }
    }

    public bool IsInjuredTabSelected
    {
        get => SelectedTab == MedicalTab.Injured;
        set
        {
            if (value)
            {
                SelectedTab = MedicalTab.Injured;
            }
        }
    }

    public bool IsHistoryTabSelected
    {
        get => SelectedTab == MedicalTab.History;
        set
        {
            if (value)
            {
                SelectedTab = MedicalTab.History;
            }
        }
    }
}

public enum MedicalTab
{
    Active,
    Injured,
    History
}

public sealed class MedicalWorkerRow : ViewModelBase
{
    private string _name = string.Empty;
    private int _conditionPercent;
    private int _fatiguePercent;
    private string _statusLabel = string.Empty;
    private string _statusBackground = "#065f46";
    private string _statusForeground = "#6ee7b7";

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int ConditionPercent
    {
        get => _conditionPercent;
        set => this.RaiseAndSetIfChanged(ref _conditionPercent, value);
    }

    public int FatiguePercent
    {
        get => _fatiguePercent;
        set => this.RaiseAndSetIfChanged(ref _fatiguePercent, value);
    }

    public string StatusLabel
    {
        get => _statusLabel;
        set => this.RaiseAndSetIfChanged(ref _statusLabel, value);
    }

    public string StatusBackground
    {
        get => _statusBackground;
        set => this.RaiseAndSetIfChanged(ref _statusBackground, value);
    }

    public string StatusForeground
    {
        get => _statusForeground;
        set => this.RaiseAndSetIfChanged(ref _statusForeground, value);
    }
}

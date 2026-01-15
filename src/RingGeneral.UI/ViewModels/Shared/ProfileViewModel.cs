using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.ViewModels.Shared;

public sealed class ProfileViewModel : ViewModelBase
{
    private string _displayName = "";
    private string _role = "";
    private string _status = "";
    private string _gimmick = "";
    private string _nationality = "";
    private string _height = "";
    private string _weight = "";
    private string _age = "";
    private string _salary = "";
    private string _contractStart = "";
    private string _contractEnd = "";
    private string _contractClauses = "";
    private bool _canRenegotiate = true;
    private bool _canRelease = true;

    public ProfileViewModel()
    {
        AttributeGroups = new ObservableCollection<ProfileAttributeGroup>();
        RecentMatches = new ObservableCollection<string>();
        RecentStorylines = new ObservableCollection<string>();
        TitleHistory = new ObservableCollection<string>();
    }

    public string DisplayName
    {
        get => _displayName;
        set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }

    public string Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Gimmick
    {
        get => _gimmick;
        set => this.RaiseAndSetIfChanged(ref _gimmick, value);
    }

    public string Nationality
    {
        get => _nationality;
        set => this.RaiseAndSetIfChanged(ref _nationality, value);
    }

    public string Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    public string Weight
    {
        get => _weight;
        set => this.RaiseAndSetIfChanged(ref _weight, value);
    }

    public string Age
    {
        get => _age;
        set => this.RaiseAndSetIfChanged(ref _age, value);
    }

    public ObservableCollection<ProfileAttributeGroup> AttributeGroups { get; }

    public ObservableCollection<string> RecentMatches { get; }

    public ObservableCollection<string> RecentStorylines { get; }

    public ObservableCollection<string> TitleHistory { get; }

    public string Salary
    {
        get => _salary;
        set => this.RaiseAndSetIfChanged(ref _salary, value);
    }

    public string ContractStart
    {
        get => _contractStart;
        set => this.RaiseAndSetIfChanged(ref _contractStart, value);
    }

    public string ContractEnd
    {
        get => _contractEnd;
        set => this.RaiseAndSetIfChanged(ref _contractEnd, value);
    }

    public string ContractClauses
    {
        get => _contractClauses;
        set => this.RaiseAndSetIfChanged(ref _contractClauses, value);
    }

    public bool CanRenegotiate
    {
        get => _canRenegotiate;
        set => this.RaiseAndSetIfChanged(ref _canRenegotiate, value);
    }

    public bool CanRelease
    {
        get => _canRelease;
        set => this.RaiseAndSetIfChanged(ref _canRelease, value);
    }
}

public sealed class ProfileAttributeGroup : ViewModelBase
{
    private string _title = string.Empty;

    public ProfileAttributeGroup(string title)
    {
        _title = title;
        Attributes = new ObservableCollection<ProfileAttributeItem>();
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public ObservableCollection<ProfileAttributeItem> Attributes { get; }
}

public sealed class ProfileAttributeItem : ViewModelBase
{
    private string _name = string.Empty;
    private int _value;
    private string _color = "#3b82f6";

    public ProfileAttributeItem(string name, int value, string color)
    {
        _name = name;
        _value = value;
        _color = color;
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            this.RaisePropertyChanged(nameof(PercentageWidth));
        }
    }

    public string Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public int PercentageWidth => Value;
}

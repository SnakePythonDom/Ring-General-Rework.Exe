using System.Collections.ObjectModel;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class StorylineOptionViewModel : ViewModelBase
{
    private string? _storylineId;
    private string _name = string.Empty;
    private int _heat;
    private string _status = string.Empty;
    private string _phase = string.Empty;

    public StorylineOptionViewModel() { }

    public StorylineOptionViewModel(string? id, string name)
    {
        StorylineId = id;
        Name = name;
    }

    public string? StorylineId
    {
        get => _storylineId;
        set
        {
            this.RaiseAndSetIfChanged(ref _storylineId, value);
            this.RaisePropertyChanged(nameof(Id));
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(Nom));
        }
    }

    public int Heat
    {
        get => _heat;
        set => this.RaiseAndSetIfChanged(ref _heat, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Phase
    {
        get => _phase;
        set => this.RaiseAndSetIfChanged(ref _phase, value);
    }

    // Compatibility Aliases
    public string? Id => StorylineId;
    public string Nom => Name;

    public string Display => $"{Name} (Heat: {Heat}, {Phase})";
}

public sealed class StorylineParticipantViewModel : ViewModelBase
{
    public StorylineParticipantViewModel() { }

    public StorylineParticipantViewModel(string workerId, string name, string role, int momentum)
    {
        WorkerId = workerId;
        Name = name;
        Role = role;
        Momentum = momentum;
    }

    public string WorkerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Momentum { get; set; }

    public string Resume => $"{Name} ({Role}) • Momentum {Momentum:+#;-#;0}";

    // Compatibility Alis
    public string Nom => Name;
}

public sealed class StorylineListItemViewModel : ViewModelBase
{
    private string _storylineId = string.Empty;
    private string _name = string.Empty;
    private string _phase = string.Empty;
    private int _heat;
    private string _status = string.Empty;
    private string _resume = string.Empty;
    private string? _leadCreativeId;
    private string _leadCreativeName = "Aucun";
    private string _creativeIdea = string.Empty;
    private string _bookerIdea = string.Empty;
    private int _pauseWeeks;
    private string? _reasonForPause;

    public StorylineListItemViewModel()
    {
        Participants = new ObservableCollection<StorylineParticipantViewModel>();
    }

    public StorylineListItemViewModel(
        string storylineId,
        string name,
        string phase,
        int heat,
        string status,
        string resume,
        IEnumerable<StorylineParticipantViewModel> participants)
    {
        StorylineId = storylineId;
        Name = name;
        Phase = phase;
        Heat = heat;
        Status = status;
        Resume = resume;
        Participants = new ObservableCollection<StorylineParticipantViewModel>(participants);
    }

    public string StorylineId
    {
        get => _storylineId;
        set => this.RaiseAndSetIfChanged(ref _storylineId, value);
    }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(Nom));
        }
    }

    public string Phase
    {
        get => _phase;
        set => this.RaiseAndSetIfChanged(ref _phase, value);
    }

    public int Heat
    {
        get => _heat;
        set
        {
            this.RaiseAndSetIfChanged(ref _heat, value);
            this.RaisePropertyChanged(nameof(HeatDisplay));
            this.RaisePropertyChanged(nameof(HeatPercent));
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            this.RaisePropertyChanged(nameof(Statut));
        }
    }

    public string Resume
    {
        get => _resume;
        set => this.RaiseAndSetIfChanged(ref _resume, value);
    }

    public string? LeadCreativeId
    {
        get => _leadCreativeId;
        set => this.RaiseAndSetIfChanged(ref _leadCreativeId, value);
    }

    public string LeadCreativeName
    {
        get => _leadCreativeName;
        set => this.RaiseAndSetIfChanged(ref _leadCreativeName, value);
    }

    public string CreativeIdea
    {
        get => _creativeIdea;
        set => this.RaiseAndSetIfChanged(ref _creativeIdea, value);
    }

    public string BookerIdea
    {
        get => _bookerIdea;
        set => this.RaiseAndSetIfChanged(ref _bookerIdea, value);
    }

    public int PauseWeeks
    {
        get => _pauseWeeks;
        set => this.RaiseAndSetIfChanged(ref _pauseWeeks, value);
    }

    public string? ReasonForPause
    {
        get => _reasonForPause;
        set => this.RaiseAndSetIfChanged(ref _reasonForPause, value);
    }

    public ObservableCollection<StorylineParticipantViewModel> Participants { get; }

    public string ParticipantsResume => string.Join(", ", Participants.Select(p => $"{p.Name} {p.Momentum:+#;-#;0}"));

    public string HeatDisplay => $"{Heat} ⚡";
    public double HeatPercent => Heat;

    public bool IsActive => Status == "Active";
    public bool IsSuspended => Status == "Suspended";
    public bool IsCompleted => Status == "Completed";

    // Compatibility Aliases
    public string Nom => Name;
    public string Statut => Status;
}

public sealed class StorylinePhaseOptionViewModel : ViewModelBase
{
    private string _phase = string.Empty;
    private string _label = string.Empty;
    private string _description = string.Empty;

    public StorylinePhaseOptionViewModel() { }

    public StorylinePhaseOptionViewModel(string phase, string label)
    {
        Phase = phase;
        Label = label;
    }

    public string Phase
    {
        get => _phase;
        set
        {
            this.RaiseAndSetIfChanged(ref _phase, value);
            this.RaisePropertyChanged(nameof(Id));
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            this.RaiseAndSetIfChanged(ref _label, value);
            this.RaisePropertyChanged(nameof(Libelle));
        }
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    // Compatibility Aliases
    public string Id => Phase;
    public string Libelle => Label;
}

public sealed class StorylineStatusOptionViewModel : ViewModelBase
{
    private string _status = string.Empty;
    private string _label = string.Empty;
    private string _color = string.Empty;

    public StorylineStatusOptionViewModel() { }

    public StorylineStatusOptionViewModel(string status, string label)
    {
        Status = status;
        Label = label;
    }

    public string Status
    {
        get => _status;
        set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            this.RaisePropertyChanged(nameof(Id));
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            this.RaiseAndSetIfChanged(ref _label, value);
            this.RaisePropertyChanged(nameof(Libelle));
        }
    }

    public string Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    // Compatibility Aliases
    public string Id => Status;
    public string Libelle => Label;
}

public sealed class CreativeStaffMemberViewModel : ViewModelBase
{
    private string _staffId = string.Empty;
    private string _name = string.Empty;
    private int _creativity;
    private string _specialty = string.Empty;
    private int _experience;

    public string StaffId
    {
        get => _staffId;
        set => this.RaiseAndSetIfChanged(ref _staffId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Creativity
    {
        get => _creativity;
        set => this.RaiseAndSetIfChanged(ref _creativity, value);
    }

    public string Specialty
    {
        get => _specialty;
        set => this.RaiseAndSetIfChanged(ref _specialty, value);
    }

    public int Experience
    {
        get => _experience;
        set => this.RaiseAndSetIfChanged(ref _experience, value);
    }
}

public sealed class StorylineSuggestionViewModel : ViewModelBase
{
    private string _title = string.Empty;
    private string _target = string.Empty;
    private int _predictedHeat;
    private string _note = string.Empty;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Target
    {
        get => _target;
        set => this.RaiseAndSetIfChanged(ref _target, value);
    }

    public int PredictedHeat
    {
        get => _predictedHeat;
        set => this.RaiseAndSetIfChanged(ref _predictedHeat, value);
    }

    public string Note
    {
        get => _note;
        set => this.RaiseAndSetIfChanged(ref _note, value);
    }
}

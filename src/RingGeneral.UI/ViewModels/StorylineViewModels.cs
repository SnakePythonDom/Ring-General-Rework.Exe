using System.Collections.ObjectModel;

namespace RingGeneral.UI.ViewModels;

public sealed class StorylineOptionViewModel
{
    public StorylineOptionViewModel(string? id, string nom)
    {
        Id = id;
        Nom = nom;
    }

    public string? Id { get; }
    public string Nom { get; }
}

public sealed class StorylineParticipantViewModel
{
    public StorylineParticipantViewModel(string workerId, string nom, string role, int momentum)
    {
        WorkerId = workerId;
        Nom = nom;
        Role = role;
        Momentum = momentum;
    }

    public string WorkerId { get; }
    public string Nom { get; }
    public string Role { get; }
    public int Momentum { get; }

    public string Resume => $"{Nom} ({Role}) • Momentum {Momentum:+#;-#;0}";
}

public sealed class StorylineListItemViewModel : ViewModelBase
{
    public StorylineListItemViewModel(
        string storylineId,
        string nom,
        string phase,
        int heat,
        string statut,
        string resume,
        IEnumerable<StorylineParticipantViewModel> participants)
    {
        StorylineId = storylineId;
        Nom = nom;
        Phase = phase;
        Heat = heat;
        Statut = statut;
        Resume = resume;
        Participants = new ObservableCollection<StorylineParticipantViewModel>(participants);
        ParticipantsResume = string.Join(", ", Participants.Select(p => $"{p.Nom} {p.Momentum:+#;-#;0}"));
    }

    public string StorylineId { get; }
    public string Nom { get; }
    public string Phase { get; }
    public int Heat { get; }
    public string Statut { get; }
    public string Resume { get; }
    public ObservableCollection<StorylineParticipantViewModel> Participants { get; }
    public string ParticipantsResume { get; }
}

public sealed class StorylinePhaseOptionViewModel
{
    public StorylinePhaseOptionViewModel(string id, string libelle)
    {
        Id = id;
        Libelle = libelle;
    }

    public string Id { get; }
    public string Libelle { get; }
}

public sealed class StorylineStatusOptionViewModel
{
    public StorylineStatusOptionViewModel(string id, string libelle)
    {
        Id = id;
        Libelle = libelle;
    }

    public string Id { get; }
    public string Libelle { get; }
}

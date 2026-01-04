namespace RingGeneral.UI.ViewModels;

public sealed class ParticipantViewModel
{
    public ParticipantViewModel(string workerId, string nom)
    {
        WorkerId = workerId;
        Nom = nom;
    }

    public string WorkerId { get; }
    public string Nom { get; }
}

namespace RingGeneral.UI.ViewModels;

public sealed class StorylineOptionViewModel
{
    public StorylineOptionViewModel(string? storylineId, string libelle, int heat, string phase, string statut)
    {
        StorylineId = storylineId;
        Libelle = libelle;
        Heat = heat;
        Phase = phase;
        Statut = statut;
    }

    public string? StorylineId { get; }
    public string Libelle { get; }
    public int Heat { get; }
    public string Phase { get; }
    public string Statut { get; }
}

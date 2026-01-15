namespace RingGeneral.UI.ViewModels;

public sealed class ShowCalendarItemViewModel
{
    public ShowCalendarItemViewModel(string showId, string nom, int semaine, int dureeMinutes, string lieu, string diffusion)
    {
        ShowId = showId;
        Nom = nom;
        Semaine = semaine;
        DureeMinutes = dureeMinutes;
        Lieu = lieu;
        Diffusion = diffusion;
    }

    public string ShowId { get; }
    public string Nom { get; }
    public int Semaine { get; }
    public int DureeMinutes { get; }
    public string Lieu { get; }
    public string Diffusion { get; }
}

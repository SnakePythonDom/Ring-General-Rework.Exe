using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class SaveSlotViewModel : ViewModelBase
{
    public SaveSlotViewModel(string nom, string chemin, DateTime derniereModification)
    {
        Nom = nom;
        Chemin = chemin;
        DerniereModification = derniereModification;
    }

    public string Nom { get; }
    public string Chemin { get; }
    public DateTime DerniereModification { get; }

    public string DerniereModificationLabel => $"{DerniereModification:g}";

    public bool EstCourante
    {
        get => _estCourante;
        set => this.RaiseAndSetIfChanged(ref _estCourante, value);
    }
    private bool _estCourante;
}

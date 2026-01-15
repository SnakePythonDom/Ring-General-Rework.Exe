using System.Collections.ObjectModel;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class TableViewItemViewModel : ViewModelBase
{
    public TableViewItemViewModel(
        string id,
        string nom,
        string type,
        string compagnie,
        string role,
        string statut,
        int popularite,
        int momentum,
        int note,
        string resume,
        IEnumerable<string> etiquettes)
    {
        Id = id;
        Nom = nom;
        Type = type;
        Compagnie = compagnie;
        Role = role;
        Statut = statut;
        Popularite = popularite;
        Momentum = momentum;
        Note = note;
        Resume = resume;
        Etiquettes = new ObservableCollection<string>(etiquettes);
    }

    public string Id { get; }
    public string Nom { get; }
    public string Type { get; }
    public string Compagnie { get; }
    public string Role { get; }
    public string Statut { get; }
    public int Popularite { get; }
    public int Momentum { get; }
    public int Note { get; }
    public string Resume { get; }
    public ObservableCollection<string> Etiquettes { get; }

    public bool EstFavori
    {
        get => _estFavori;
        set => this.RaiseAndSetIfChanged(ref _estFavori, value);
    }
    private bool _estFavori;
}

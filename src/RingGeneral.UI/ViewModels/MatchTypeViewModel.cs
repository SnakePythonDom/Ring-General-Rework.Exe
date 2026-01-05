using ReactiveUI;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class MatchTypeViewModel : ReactiveObject
{
    public MatchTypeViewModel(string matchTypeId, string nom, string? description, bool estActif, int ordre)
    {
        MatchTypeId = matchTypeId;
        _nom = nom;
        _description = description;
        _estActif = estActif;
        Ordre = ordre;
    }

    public string MatchTypeId { get; }

    public string Nom
    {
        get => _nom;
        set => this.RaiseAndSetIfChanged(ref _nom, value);
    }
    private string _nom;

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    private string? _description;

    public bool EstActif
    {
        get => _estActif;
        set => this.RaiseAndSetIfChanged(ref _estActif, value);
    }
    private bool _estActif;

    public int Ordre { get; }

    public MatchType VersModele()
        => new(MatchTypeId, Nom, Description, EstActif, Ordre);
}

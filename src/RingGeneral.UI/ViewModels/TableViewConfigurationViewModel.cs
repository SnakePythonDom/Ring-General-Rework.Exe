using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class TableViewConfigurationViewModel : ViewModelBase
{
    public bool AfficherType
    {
        get => _afficherType;
        set => this.RaiseAndSetIfChanged(ref _afficherType, value);
    }
    private bool _afficherType = true;

    public bool AfficherCompagnie
    {
        get => _afficherCompagnie;
        set => this.RaiseAndSetIfChanged(ref _afficherCompagnie, value);
    }
    private bool _afficherCompagnie = true;

    public bool AfficherRole
    {
        get => _afficherRole;
        set => this.RaiseAndSetIfChanged(ref _afficherRole, value);
    }
    private bool _afficherRole = true;

    public bool AfficherStatut
    {
        get => _afficherStatut;
        set => this.RaiseAndSetIfChanged(ref _afficherStatut, value);
    }
    private bool _afficherStatut = true;

    public bool AfficherPopularite
    {
        get => _afficherPopularite;
        set => this.RaiseAndSetIfChanged(ref _afficherPopularite, value);
    }
    private bool _afficherPopularite = true;

    public bool AfficherMomentum
    {
        get => _afficherMomentum;
        set => this.RaiseAndSetIfChanged(ref _afficherMomentum, value);
    }
    private bool _afficherMomentum = true;

    public bool AfficherNote
    {
        get => _afficherNote;
        set => this.RaiseAndSetIfChanged(ref _afficherNote, value);
    }
    private bool _afficherNote = true;
}

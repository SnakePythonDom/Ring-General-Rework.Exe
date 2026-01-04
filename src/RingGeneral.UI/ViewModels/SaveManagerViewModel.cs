using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Data.Database;

namespace RingGeneral.UI.ViewModels;

public sealed class SaveManagerViewModel : ViewModelBase
{
    private readonly SaveGameManager _manager;

    public SaveManagerViewModel()
    {
        _manager = new SaveGameManager(new DbInitializer(), new DbValidator());
        Sauvegardes = new ObservableCollection<SaveGameEntryViewModel>();
        CheminImport = string.Empty;
        NomNouvellePartie = string.Empty;
        Rafraichir();
    }

    public ObservableCollection<SaveGameEntryViewModel> Sauvegardes { get; }

    public SaveGameEntryViewModel? SauvegardeSelectionnee
    {
        get => _sauvegardeSelectionnee;
        set => this.RaiseAndSetIfChanged(ref _sauvegardeSelectionnee, value);
    }
    private SaveGameEntryViewModel? _sauvegardeSelectionnee;

    public string CheminImport
    {
        get => _cheminImport;
        set => this.RaiseAndSetIfChanged(ref _cheminImport, value);
    }
    private string _cheminImport;

    public string NomNouvellePartie
    {
        get => _nomNouvellePartie;
        set => this.RaiseAndSetIfChanged(ref _nomNouvellePartie, value);
    }
    private string _nomNouvellePartie;

    public string? MessageStatut
    {
        get => _messageStatut;
        private set => this.RaiseAndSetIfChanged(ref _messageStatut, value);
    }
    private string? _messageStatut;

    public string? SauvegardeActive
    {
        get => _sauvegardeActive;
        private set => this.RaiseAndSetIfChanged(ref _sauvegardeActive, value);
    }
    private string? _sauvegardeActive;

    public void Rafraichir()
    {
        Sauvegardes.Clear();
        foreach (var save in _manager.ListerSaves())
        {
            Sauvegardes.Add(new SaveGameEntryViewModel(save.Nom, save.Chemin, save.DerniereModification));
        }

        MessageStatut = $"{Sauvegardes.Count} sauvegarde(s) trouvée(s) dans {_manager.SavesDirectory}.";
    }

    public string? CreerNouvellePartie()
    {
        try
        {
            var info = _manager.CreerNouvellePartie(NomNouvellePartie);
            Rafraichir();
            SauvegardeSelectionnee = Sauvegardes.FirstOrDefault(item => item.Chemin == info.Chemin);
            SauvegardeActive = info.Chemin;
            MessageStatut = $"Nouvelle base créée : {info.Nom}.";
            return info.Chemin;
        }
        catch (Exception ex)
        {
            MessageStatut = ex.Message;
            return null;
        }
    }

    public string? ImporterBase()
    {
        try
        {
            var info = _manager.ImporterBase(CheminImport);
            Rafraichir();
            SauvegardeSelectionnee = Sauvegardes.FirstOrDefault(item => item.Chemin == info.Chemin);
            SauvegardeActive = info.Chemin;
            MessageStatut = $"Base importée : {info.Nom}.";
            return info.Chemin;
        }
        catch (Exception ex)
        {
            MessageStatut = ex.Message;
            return null;
        }
    }

    public string? ChargerSelectionnee()
    {
        if (SauvegardeSelectionnee is null)
        {
            MessageStatut = "Sélectionnez une sauvegarde pour la charger.";
            return null;
        }

        SauvegardeActive = SauvegardeSelectionnee.Chemin;
        MessageStatut = $"Sauvegarde active : {SauvegardeSelectionnee.Nom}.";
        return SauvegardeSelectionnee.Chemin;
    }

    public void DupliquerSelectionnee()
    {
        if (SauvegardeSelectionnee is null)
        {
            MessageStatut = "Sélectionnez une sauvegarde à dupliquer.";
            return;
        }

        try
        {
            _manager.DupliquerSauvegarde(SauvegardeSelectionnee.Chemin);
            Rafraichir();
            MessageStatut = "Sauvegarde dupliquée.";
        }
        catch (Exception ex)
        {
            MessageStatut = ex.Message;
        }
    }

    public void SupprimerSelectionnee()
    {
        if (SauvegardeSelectionnee is null)
        {
            MessageStatut = "Sélectionnez une sauvegarde à supprimer.";
            return;
        }

        try
        {
            _manager.SupprimerSauvegarde(SauvegardeSelectionnee.Chemin);
            SauvegardeSelectionnee = null;
            Rafraichir();
            MessageStatut = "Sauvegarde supprimée.";
        }
        catch (Exception ex)
        {
            MessageStatut = ex.Message;
        }
    }
}

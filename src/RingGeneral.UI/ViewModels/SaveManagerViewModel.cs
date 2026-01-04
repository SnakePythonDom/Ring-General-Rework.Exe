using System.Collections.ObjectModel;
using Avalonia.Media;
using ReactiveUI;
using RingGeneral.UI.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class SaveManagerViewModel : ViewModelBase
{
    private readonly SaveStorageService _storage;

    public SaveManagerViewModel(SaveStorageService storage)
    {
        _storage = storage;
        Sauvegardes = new ObservableCollection<SaveSlotViewModel>();
        StatutCouleur = Brushes.Transparent;
    }

    public ObservableCollection<SaveSlotViewModel> Sauvegardes { get; }

    public SaveSlotViewModel? SauvegardeSelectionnee
    {
        get => _sauvegardeSelectionnee;
        set => this.RaiseAndSetIfChanged(ref _sauvegardeSelectionnee, value);
    }
    private SaveSlotViewModel? _sauvegardeSelectionnee;

    public SaveSlotViewModel? SauvegardeCourante
    {
        get => _sauvegardeCourante;
        private set => this.RaiseAndSetIfChanged(ref _sauvegardeCourante, value);
    }
    private SaveSlotViewModel? _sauvegardeCourante;

    public string? NouveauNom
    {
        get => _nouveauNom;
        set => this.RaiseAndSetIfChanged(ref _nouveauNom, value);
    }
    private string? _nouveauNom;

    public string? StatutMessage
    {
        get => _statutMessage;
        private set => this.RaiseAndSetIfChanged(ref _statutMessage, value);
    }
    private string? _statutMessage;

    public IBrush StatutCouleur
    {
        get => _statutCouleur;
        private set => this.RaiseAndSetIfChanged(ref _statutCouleur, value);
    }
    private IBrush _statutCouleur;

    public void Initialiser()
    {
        ActualiserSauvegardes();
        if (Sauvegardes.Count == 0)
        {
            var info = _storage.CreerSauvegarde("Sauvegarde 1");
            ActualiserSauvegardes();
            var slot = Sauvegardes.FirstOrDefault(s => s.Chemin == info.Chemin);
            if (slot is not null)
            {
                DefinirSauvegardeCourante(slot);
            }
        }
        else
        {
            DefinirSauvegardeCourante(Sauvegardes[0]);
        }
    }

    public SaveSlotViewModel? CreerNouvelleSauvegarde()
    {
        try
        {
            var info = _storage.CreerSauvegarde(NouveauNom);
            NouveauNom = string.Empty;
            var slot = ActualiserEtTrouver(info.Chemin);
            if (slot is not null)
            {
                DefinirSauvegardeCourante(slot);
                StatutOk($"Nouvelle sauvegarde créée : {slot.Nom}.");
            }

            return slot;
        }
        catch (Exception ex)
        {
            StatutErreur($"Impossible de créer la sauvegarde : {ex.Message}");
            return null;
        }
    }

    public SaveSlotViewModel? ImporterBase(string cheminSource)
    {
        try
        {
            var info = _storage.ImporterBase(cheminSource);
            var slot = ActualiserEtTrouver(info.Chemin);
            if (slot is not null)
            {
                DefinirSauvegardeCourante(slot);
                StatutOk($"Base importée : {slot.Nom}.");
            }

            return slot;
        }
        catch (Exception ex)
        {
            StatutErreur(ex.Message);
            return null;
        }
    }

    public SaveSlotViewModel? ImporterPack(string cheminSource)
    {
        try
        {
            var info = _storage.ImporterPack(cheminSource);
            var slot = ActualiserEtTrouver(info.Chemin);
            if (slot is not null)
            {
                DefinirSauvegardeCourante(slot);
                StatutOk($"Pack importé : {slot.Nom}.");
            }

            return slot;
        }
        catch (Exception ex)
        {
            StatutErreur(ex.Message);
            return null;
        }
    }

    public void ExporterPack(string cheminDestination, SaveSlotViewModel? slot)
    {
        if (slot is null)
        {
            StatutErreur("Sélectionnez une sauvegarde à exporter.");
            return;
        }

        try
        {
            _storage.ExporterPack(new SaveInfo(slot.Nom, slot.Chemin, slot.DerniereModification), cheminDestination);
            StatutOk($"Pack exporté vers {cheminDestination}.");
        }
        catch (Exception ex)
        {
            StatutErreur($"Impossible d'exporter le pack : {ex.Message}");
        }
    }

    public bool DefinirSauvegardeCourante(SaveSlotViewModel slot)
    {
        var validation = _storage.ValiderBase(slot.Chemin);
        if (!validation.EstValide)
        {
            StatutErreur(validation.Message);
            return false;
        }

        foreach (var save in Sauvegardes)
        {
            save.EstCourante = false;
        }

        slot.EstCourante = true;
        SauvegardeCourante = slot;
        SauvegardeSelectionnee = slot;
        StatutOk($"Sauvegarde chargée : {slot.Nom}.");
        return true;
    }

    private void ActualiserSauvegardes()
    {
        Sauvegardes.Clear();
        foreach (var info in _storage.ListerSauvegardes())
        {
            Sauvegardes.Add(new SaveSlotViewModel(info.Nom, info.Chemin, info.DerniereModification));
        }
    }

    private SaveSlotViewModel? ActualiserEtTrouver(string chemin)
    {
        ActualiserSauvegardes();
        return Sauvegardes.FirstOrDefault(s => s.Chemin == chemin);
    }

    public void SignalerErreur(string message)
    {
        StatutErreur(message);
    }

    private void StatutOk(string message)
    {
        StatutMessage = message;
        StatutCouleur = Brushes.LightGreen;
    }

    private void StatutErreur(string message)
    {
        StatutMessage = message;
        StatutCouleur = Brushes.IndianRed;
    }
}

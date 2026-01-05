using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface ITitleRepository
{
    TitleDetail? ChargerTitre(string titleId);
    TitleReignDetail? ChargerRegneCourant(string titleId);
    int CompterDefenses(string titleId, int depuisSemaine);
    void MettreAJourChampion(string titleId, string? workerId);
    int CreerRegne(string titleId, string workerId, int semaineDebut);
    void CloreRegne(int titleReignId, int semaineFin);
    void AjouterMatch(TitleMatchRecord match);
    void MettreAJourPrestige(string titleId, int delta);
}

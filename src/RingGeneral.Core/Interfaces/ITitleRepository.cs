using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface ITitleRepository
{
    TitleRecord? ChargerTitre(string titleId);
    IReadOnlyList<TitleRecord> ChargerTitresCompagnie(string companyId);
    void CreerTitre(TitleRecord title);
    void MettreAJourTitre(TitleRecord title);
    void SupprimerTitre(string titleId);
    TitleReignRecord? ChargerRegneActuel(string titleId);
    IReadOnlyList<TitleReignRecord> ChargerRegnes(string titleId);
    int AjouterRegne(TitleReignRecord reign);
    void CloreRegne(int reignId, int endDate);
    void MettreAJourDetenteur(string titleId, string? workerId);
    void AjouterMatchTitre(TitleMatchRecord match);
    IReadOnlyList<TitleMatchRecord> ChargerMatchsTitrePourWorker(string workerId);
    void AjusterPrestige(string titleId, int delta);
}

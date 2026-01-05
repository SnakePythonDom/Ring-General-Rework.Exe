using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class TitleService
{
    private readonly ITitleRepository _repository;

    public TitleService(ITitleRepository repository)
    {
        _repository = repository;
    }

    public void CreerTitre(TitleRecord title, int week)
    {
        _repository.CreerTitre(title);

        if (!string.IsNullOrWhiteSpace(title.HolderWorkerId))
        {
            var reign = new TitleReignRecord(0, title.TitleId, title.HolderWorkerId, week, null, true);
            _repository.AjouterRegne(reign);
        }
    }

    public void MettreAJourTitre(TitleRecord title)
    {
        _repository.MettreAJourTitre(title);
    }

    public void SupprimerTitre(string titleId)
    {
        _repository.SupprimerTitre(titleId);
    }

    public TitleChangeResult EnregistrerDefense(TitleDefenseRequest request)
    {
        if (!string.Equals(request.WinnerId, request.ChampionId, StringComparison.OrdinalIgnoreCase))
        {
            return EnregistrerChangementChampion(request);
        }

        var match = new TitleMatchRecord(
            0,
            request.TitleId,
            request.ChampionId,
            request.ChallengerId,
            request.WinnerId,
            request.LoserId,
            request.Week,
            request.ShowId,
            request.SegmentId,
            false);
        _repository.AjouterMatchTitre(match);

        var delta = CalculerDeltaPrestige(defenseReussie: true, changementChampion: false);
        _repository.AjusterPrestige(request.TitleId, delta);

        return new TitleChangeResult(false, delta, null, null);
    }

    private TitleChangeResult EnregistrerChangementChampion(TitleDefenseRequest request)
    {
        int? reignClotureId = null;
        var regneActuel = _repository.ChargerRegneActuel(request.TitleId);
        if (regneActuel is not null)
        {
            _repository.CloreRegne(regneActuel.TitleReignId, request.Week);
            reignClotureId = regneActuel.TitleReignId;
        }

        _repository.MettreAJourDetenteur(request.TitleId, request.WinnerId);

        var nouveauRegne = new TitleReignRecord(0, request.TitleId, request.WinnerId, request.Week, null, true);
        var nouveauRegneId = _repository.AjouterRegne(nouveauRegne);

        var match = new TitleMatchRecord(
            0,
            request.TitleId,
            request.ChampionId,
            request.ChallengerId,
            request.WinnerId,
            request.LoserId,
            request.Week,
            request.ShowId,
            request.SegmentId,
            true);
        _repository.AjouterMatchTitre(match);

        var delta = CalculerDeltaPrestige(defenseReussie: false, changementChampion: true);
        _repository.AjusterPrestige(request.TitleId, delta);

        return new TitleChangeResult(true, delta, nouveauRegneId, reignClotureId);
    }

    private static int CalculerDeltaPrestige(bool defenseReussie, bool changementChampion)
    {
        if (changementChampion)
        {
            return -3;
        }

        return defenseReussie ? 2 : 0;
    }
}

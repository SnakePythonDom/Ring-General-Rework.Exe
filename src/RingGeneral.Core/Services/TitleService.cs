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

    public TitleMatchOutcome EnregistrerMatch(TitleMatchInput input)
    {
        var titre = _repository.ChargerTitre(input.TitleId)
            ?? throw new InvalidOperationException($"Titre introuvable : {input.TitleId}");

        var championActuel = input.ChampionId ?? titre.HolderWorkerId;
        var changement = !string.IsNullOrWhiteSpace(championActuel)
            && !string.Equals(championActuel, input.WinnerId, StringComparison.OrdinalIgnoreCase);

        var regneCourant = _repository.ChargerRegneCourant(input.TitleId);
        var defenses = regneCourant is null
            ? 0
            : _repository.CompterDefenses(input.TitleId, regneCourant.StartDate);

        var deltaPrestige = CalculerDeltaPrestige(championActuel, changement, defenses);

        if (string.IsNullOrWhiteSpace(championActuel) || changement)
        {
            if (regneCourant is not null)
            {
                _repository.CloreRegne(regneCourant.TitleReignId, input.Week);
            }

            var nouveauRegneId = _repository.CreerRegne(input.TitleId, input.WinnerId, input.Week);
            _repository.MettreAJourChampion(input.TitleId, input.WinnerId);
            _repository.AjouterMatch(new TitleMatchRecord(
                input.TitleId,
                input.ShowId,
                input.Week,
                championActuel,
                input.ChallengerId,
                input.WinnerId,
                true,
                deltaPrestige));
            _repository.MettreAJourPrestige(input.TitleId, deltaPrestige);
            return new TitleMatchOutcome(true, deltaPrestige, nouveauRegneId);
        }

        _repository.AjouterMatch(new TitleMatchRecord(
            input.TitleId,
            input.ShowId,
            input.Week,
            championActuel,
            input.ChallengerId,
            input.WinnerId,
            false,
            deltaPrestige));
        _repository.MettreAJourPrestige(input.TitleId, deltaPrestige);
        return new TitleMatchOutcome(false, deltaPrestige, null);
    }

    private static int CalculerDeltaPrestige(string? championActuel, bool changement, int defenses)
    {
        if (string.IsNullOrWhiteSpace(championActuel))
        {
            return 4;
        }

        if (changement)
        {
            return -2;
        }

        var bonus = Math.Clamp(defenses / 2, 0, 3);
        return 2 + bonus;
    }
}

using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class ContenderService
{
    private readonly IContenderRepository _repository;

    public ContenderService(IContenderRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<ContenderRanking> MettreAJourClassement(string titleId, int maxContenders = 5)
    {
        var titre = _repository.ChargerTitre(titleId)
            ?? throw new InvalidOperationException($"Titre introuvable : {titleId}");

        var workers = _repository.ChargerWorkersCompagnie(titre.CompanyId);
        var classement = workers
            .Where(worker => !string.Equals(worker.WorkerId, titre.HolderWorkerId, StringComparison.OrdinalIgnoreCase))
            .Where(worker => string.Equals(worker.Blessure, "AUCUNE", StringComparison.OrdinalIgnoreCase))
            .Select(worker => new
            {
                Worker = worker,
                Score = CalculerScore(worker)
            })
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Worker.Popularite)
            .Take(maxContenders)
            .Select((entry, index) => new ContenderRanking(
                titleId,
                entry.Worker.WorkerId,
                index + 1,
                entry.Score,
                $"Popularité {entry.Worker.Popularite} • Momentum {entry.Worker.Momentum}"))
            .ToList();

        _repository.EnregistrerClassement(titleId, classement);
        return classement;
    }

    private static double CalculerScore(WorkerSnapshot worker)
    {
        var technique = (worker.InRing + worker.Entertainment + worker.Story) / 3.0;
        return (worker.Popularite * 0.6) + (worker.Momentum * 0.2) + (technique * 0.2);
    }
}

using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class ContenderService
{
    public IReadOnlyList<ContenderRankingEntry> CalculerClassement(
        TitleInfo titre,
        IReadOnlyList<WorkerSnapshot> workers,
        int week,
        int max = 5)
    {
        var championId = titre.DetenteurId;
        var classement = workers
            .Where(worker => !string.Equals(worker.WorkerId, championId, StringComparison.OrdinalIgnoreCase))
            .Select(worker => new
            {
                worker.WorkerId,
                Score = CalculerScore(worker)
            })
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.WorkerId)
            .Take(max)
            .Select((entry, index) => new ContenderRankingEntry(
                titre.TitreId,
                entry.WorkerId,
                index + 1,
                entry.Score,
                week))
            .ToList();

        return classement;
    }

    public IReadOnlyList<ContenderRankingEntry> MettreAJourClassement(
        IContenderRankingRepository repository,
        TitleInfo titre,
        IReadOnlyList<WorkerSnapshot> workers,
        int week,
        int max = 5)
    {
        var classement = CalculerClassement(titre, workers, week, max);
        repository.RemplacerClassement(titre.TitreId, week, classement);
        return classement;
    }

    private static int CalculerScore(WorkerSnapshot worker)
    {
        return worker.Popularite + worker.Momentum * 2 - worker.Fatigue;
    }
}

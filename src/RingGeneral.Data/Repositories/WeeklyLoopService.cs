using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

public sealed class WeeklyLoopService
{
    private readonly GameRepository _repository;
    private readonly Random _random = new(42);

    public WeeklyLoopService(GameRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<InboxItem> PasserSemaineSuivante(string showId)
    {
        var semaine = _repository.IncrementerSemaine(showId);
        _repository.RecupererFatigueHebdo();

        var inboxItems = new List<InboxItem>();
        inboxItems.AddRange(GenererNews(semaine));
        inboxItems.AddRange(VerifierContrats(semaine));
        var scouting = GenererScouting(semaine);
        if (scouting is not null)
        {
            inboxItems.Add(scouting);
        }

        foreach (var item in inboxItems)
        {
            _repository.AjouterInboxItem(item);
        }

        return inboxItems;
    }

    private IEnumerable<InboxItem> GenererNews(int semaine)
    {
        var nouvelles = new[]
        {
            "La rumeur d'un nouveau talent circule dans les coulisses.",
            "Le public réclame plus d'intensité sur les matchs télévisés.",
            "Les sponsors surveillent de près la prochaine audience."
        };

        var total = _random.Next(1, 4);
        for (var i = 0; i < total; i++)
        {
            var contenu = nouvelles[_random.Next(0, nouvelles.Length)];
            yield return new InboxItem("news", "Actualité", contenu, semaine);
        }
    }

    private IEnumerable<InboxItem> VerifierContrats(int semaine)
    {
        var contracts = _repository.ChargerContracts();
        var noms = _repository.ChargerNomsWorkers();

        foreach (var (workerId, finSemaine) in contracts)
        {
            var semainesRestantes = finSemaine - semaine;
            if (semainesRestantes is 4 or 1)
            {
                var nom = noms.TryGetValue(workerId, out var workerNom) ? workerNom : workerId;
                var titre = semainesRestantes == 1 ? "Contrat arrive à expiration" : "Contrat bientôt à échéance";
                var contenu = $"{nom} arrive en fin de contrat dans {semainesRestantes} semaine(s).";
                yield return new InboxItem("contrat", titre, contenu, semaine);
            }
        }
    }

    private InboxItem? GenererScouting(int semaine)
    {
        if (_random.NextDouble() > 0.35)
        {
            return null;
        }

        var rapports = new[]
        {
            "Le scouting recommande de surveiller un talent high-fly de la scène indie.",
            "Un ancien champion pourrait être intéressé par un retour ponctuel.",
            "Un jeune espoir impressionne en entraînement, potentiel futur midcard."
        };

        return new InboxItem("scouting", "Rapport de scouting", rapports[_random.Next(0, rapports.Length)], semaine);
    }
}

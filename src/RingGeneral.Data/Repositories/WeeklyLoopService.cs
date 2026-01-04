using System.Diagnostics;
using System.Text.Json;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;

namespace RingGeneral.Data.Repositories;

public sealed class WeeklyLoopService
{
    private readonly GameRepository _repository;
    private readonly SeededRandomProvider _random = new(42);

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
        inboxItems.AddRange(SimulerMonde(semaine, showId));
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

    private IEnumerable<InboxItem> SimulerMonde(int semaine, string showId)
    {
        var settings = ChargerWorldSimSettings();
        _random.Reseed(settings.Seed + semaine);

        var compagnies = _repository.ChargerCompagnies();
        var compagnieJoueurId = _repository.ChargerCompagnieIdPourShow(showId);
        var scheduler = new WorldSimScheduler(_random, settings);

        var chrono = Stopwatch.StartNew();
        var plan = scheduler.Planifier(semaine, compagnieJoueurId, compagnies);
        var impacts = new List<WorldSimCompanyImpact>();

        foreach (var companyPlan in plan.Compagnies)
        {
            if (companyPlan.CompanyId == compagnieJoueurId)
            {
                continue;
            }

            var compagnie = compagnies.FirstOrDefault(item => item.CompagnieId == companyPlan.CompanyId);
            if (compagnie is null)
            {
                continue;
            }

            var (deltaPrestige, deltaTresorerie, resume) = GenererImpact(companyPlan, compagnie);
            _repository.AppliquerImpactCompagnie(companyPlan.CompanyId, deltaPrestige, deltaTresorerie);
            impacts.Add(new WorldSimCompanyImpact(companyPlan.CompanyId, deltaPrestige, deltaTresorerie, resume));
        }

        chrono.Stop();
        var budgetDepasse = chrono.ElapsedMilliseconds > settings.BudgetMsParTick;
        Debug.WriteLine($"[WorldSim] S{semaine} - {plan.Compagnies.Count} compagnies - {chrono.ElapsedMilliseconds}ms (budget {settings.BudgetMsParTick}ms)");

        foreach (var item in GenererNewsMonde(semaine, impacts, compagnies))
        {
            yield return item;
        }

        if (budgetDepasse)
        {
            yield return new InboxItem("monde", "Performance monde vivant",
                $"La simulation mondiale a dépassé le budget ({chrono.ElapsedMilliseconds}ms / {settings.BudgetMsParTick}ms).", semaine);
        }
    }

    private (int DeltaPrestige, double DeltaTresorerie, string Resume) GenererImpact(WorldSimCompanyPlan plan, CompanyState compagnie)
    {
        var (prestigeAmplitude, tresorerieAmplitude) = plan.NiveauDetail switch
        {
            WorldSimLod.Detail => (4, 6500),
            WorldSimLod.Resume => (3, 4200),
            _ => (2, 1800)
        };

        var deltaPrestige = _random.Next(-prestigeAmplitude, prestigeAmplitude + 1);
        var deltaTresorerie = (_random.NextDouble() * 2 - 1) * tresorerieAmplitude;
        var type = deltaPrestige switch
        {
            > 1 => "progression",
            < -1 => "recul",
            _ => "stabilité"
        };

        var resume = $"{compagnie.Nom} affiche une {type} (LOD {plan.NiveauDetail}).";
        return (deltaPrestige, Math.Round(deltaTresorerie, 2), resume);
    }

    private IEnumerable<InboxItem> GenererNewsMonde(
        int semaine,
        IReadOnlyList<WorldSimCompanyImpact> impacts,
        IReadOnlyList<CompanyState> compagnies)
    {
        var top = impacts
            .OrderByDescending(impact => Math.Abs(impact.DeltaPrestige))
            .ThenByDescending(impact => Math.Abs(impact.DeltaTresorerie))
            .Take(3)
            .ToList();

        foreach (var impact in top)
        {
            var compagnie = compagnies.FirstOrDefault(item => item.CompagnieId == impact.CompanyId);
            if (compagnie is null)
            {
                continue;
            }

            var variation = impact.DeltaPrestige == 0 ? "reste stable" : impact.DeltaPrestige > 0 ? "progresse" : "recule";
            var contenu = $"{compagnie.Nom} {variation} sur la scène mondiale. {impact.Resume}";
            yield return new InboxItem("monde", "Monde vivant", contenu, semaine);
        }
    }

    private static WorldSimSettings ChargerWorldSimSettings()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "models", "world-sim.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "models", "world-sim.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return WorldSimSettings.ParDefaut;
        }

        var json = File.ReadAllText(chemin);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<WorldSimSettings>(json, options) ?? WorldSimSettings.ParDefaut;
    }
}

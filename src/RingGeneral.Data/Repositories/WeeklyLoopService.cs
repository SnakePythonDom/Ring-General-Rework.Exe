using System.Diagnostics;
using System.Text.Json;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

namespace RingGeneral.Data.Repositories;

public sealed class WeeklyLoopService
{
    private readonly GameRepository _repository;
    private readonly SeededRandomProvider _random = new(42);
    private readonly SpecsReader _specsReader = new();

    public WeeklyLoopService(GameRepository repository)
    {
        _repository = repository;
        _scoutingService = new ScoutingService(_random);
    }

    public IReadOnlyList<InboxItem> PasserSemaineSuivante(string showId)
    {
        var semaine = _repository.IncrementerSemaine(showId);
        _repository.RecupererFatigueHebdo();
        AppliquerFinancesHebdo(showId, semaine);

        var inboxItems = new List<InboxItem>();
        var generation = GenererWorkers(semaine, showId);
        if (generation is not null)
        {
            foreach (var notice in generation.Notices)
            {
                inboxItems.Add(new InboxItem(notice.Type, notice.Titre, notice.Contenu, semaine));
            }
        }
        inboxItems.AddRange(SimulerBackstage(semaine, showId));
        inboxItems.AddRange(GenererNews(semaine));
        inboxItems.AddRange(VerifierContrats(semaine));
        inboxItems.AddRange(VerifierOffresExpirantes(semaine));
        inboxItems.AddRange(SimulerMonde(semaine, showId));
        var scouting = GenererScoutingHebdo(semaine);
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

    private IEnumerable<InboxItem> SimulerBackstage(int semaine, string showId)
    {
        var compagnieId = _repository.ChargerCompagnieIdPourShow(showId);
        if (string.IsNullOrWhiteSpace(compagnieId))
        {
            return Array.Empty<InboxItem>();
        }

        var roster = _repository.ChargerBackstageRoster(compagnieId);
        if (roster.Count == 0)
        {
            return Array.Empty<InboxItem>();
        }

        var incidentsSpec = ChargerIncidentsSpec();
        var definitions = incidentsSpec.Incidents
            .Select(incident => new BackstageIncidentDefinition(
                incident.Id,
                incident.Titre,
                incident.Description,
                incident.Chance,
                incident.ParticipantsMin,
                incident.ParticipantsMax,
                incident.GraviteMin,
                incident.GraviteMax,
                incident.MoraleImpactMin,
                incident.MoraleImpactMax))
            .ToList();

        var morales = _repository.ChargerMorales(compagnieId);
        var service = new BackstageService(_random);
        var resultat = service.LancerIncidents(semaine, compagnieId, roster, morales, definitions);

        foreach (var incident in resultat.Incidents)
        {
            _repository.EnregistrerBackstageIncident(incident);
        }

        _repository.AppliquerMoraleImpacts(resultat.MoraleImpacts, semaine);

        return resultat.InboxItems;
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

    private InboxItem? GenererScoutingHebdo(int semaine)
    {
        var service = new ScoutingService(_repository, new SeededRandomProvider(semaine));
        var refresh = service.RafraichirHebdo(semaine);

        if (refresh.RapportsCrees == 0 && refresh.MissionsAvancees == 0)
        {
            if (missions.Any(m => m.MissionId == mission.MissionId))
            {
                _repository.MettreAJourMission(mission);
            }
        }

        var contenu = $"Scouting: {refresh.RapportsCrees} rapport(s) créé(s), {refresh.MissionsAvancees} mission(s) avancée(s).";
        if (refresh.MissionsTerminees > 0)
        {
            contenu += $" {refresh.MissionsTerminees} mission(s) terminée(s).";
        }

        return new InboxItem("scouting", "Scouting hebdo", contenu, semaine);
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

    private IncidentLoreSpec ChargerIncidentsSpec()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "lore", "incidents.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "lore", "incidents.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return IncidentLoreSpec.ParDefaut;
        }

        return _specsReader.Charger<IncidentLoreSpec>(chemin);
    }

    private WorkerGenerationReport? GenererWorkers(int semaine, string showId)
    {
        var spec = ChargerWorkerGenerationSpec();
        var options = _repository.ChargerParametresGeneration();
        if (options.YouthMode == YouthGenerationMode.Desactivee && options.WorldMode == WorldGenerationMode.Desactivee)
        {
            return null;
        }

        var show = _repository.ChargerShowDefinition(showId);
        if (show is null)
        {
            return null;
        }
        var structures = _repository.ChargerYouthStructuresPourGeneration();
        var annee = ((semaine - 1) / 52) + 1;
        var counters = _repository.ChargerGenerationCounters(annee);
        var state = new GameState(semaine, show.CompagnieId, show.Region, options, structures, counters);
        var seed = HashCode.Combine(showId, semaine, options.YouthMode, options.WorldMode, options.SemainePivotAnnuelle ?? 0);
        var service = new WorkerGenerationService(new SeededRandomProvider(seed), spec);
        var report = service.GenerateWeekly(state, seed);
        if (report.Workers.Count > 0)
        {
            _repository.EnregistrerGeneration(report);
        }

        return report;
    }

    private IEnumerable<InboxItem> GenererProgressionYouth(int semaine)
    {
        var spec = ChargerYouthSpec();
        var trainees = _repository.ChargerYouthTraineesPourProgression();
        if (trainees.Count == 0)
        {
            yield break;
        }

        var seed = HashCode.Combine(semaine, trainees.Count);
        _random.Reseed(seed);
        var service = new YouthProgressionService(_random, spec);
        var report = service.AppliquerProgression(semaine, trainees);
        _repository.EnregistrerProgressionTrainees(report);

        foreach (var resultat in report.Resultats.Where(item => item.Diplome))
        {
            yield return new InboxItem(
                "youth",
                "Graduation Youth",
                $"{resultat.Nom} est diplômé de la structure Youth.",
                semaine);
        }
    }

    private static YouthSpec ChargerYouthSpec()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "youth", "youth-v1.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "youth", "youth-v1.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            throw new FileNotFoundException("Impossible de trouver la spec Youth v1.");
        }

        var json = File.ReadAllText(chemin);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<YouthSpec>(json, options)
               ?? throw new InvalidOperationException("Spec Youth v1 invalide.");
    }

    private static WorkerGenerationSpec ChargerWorkerGenerationSpec()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "simulation", "worker-generation.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "simulation", "worker-generation.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            throw new FileNotFoundException("Impossible de trouver la spec de génération de workers.");
        }

        var json = File.ReadAllText(chemin);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<WorkerGenerationSpec>(json, options)
               ?? throw new InvalidOperationException("Spec de génération de workers invalide.");
    }

}

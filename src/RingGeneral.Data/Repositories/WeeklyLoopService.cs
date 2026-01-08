using System.Diagnostics;
using System.Text.Json;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Services;
using RingGeneral.Core.Simulation;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

namespace RingGeneral.Data.Repositories;

public sealed class WeeklyLoopService
{
    private readonly GameRepository _repository;
    private readonly IScoutingRepository _scoutingRepository;
    private readonly IMoraleEngine? _moraleEngine;
    private readonly IRumorEngine? _rumorEngine;
    private readonly SeededRandomProvider _random = new(42);
    private readonly SpecsReader _specsReader = new();

    public WeeklyLoopService(
        GameRepository repository,
        IScoutingRepository scoutingRepository,
        IMoraleEngine? moraleEngine = null,
        IRumorEngine? rumorEngine = null)
    {
        _repository = repository;
        _scoutingRepository = scoutingRepository;
        _moraleEngine = moraleEngine;
        _rumorEngine = rumorEngine;
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

        // Progression du moral et des rumeurs (Phase 3)
        inboxItems.AddRange(ProgresserMoraleEtRumeurs(semaine, showId));

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
            .Select(incident => new IncidentDefinition(
                incident.Id,
                incident.Titre,
                incident.Description,
                _random.Next(incident.GraviteMin, incident.GraviteMax + 1),
                incident.MoraleImpactMin,
                incident.MoraleImpactMax,
                Array.Empty<string>()))
            .ToList();

        var service = new BackstageService(_random, definitions);
        var incidents = service.RollIncidents(semaine, roster);
        if (incidents.Count == 0)
        {
            return Array.Empty<InboxItem>();
        }

        var definitionLookup = definitions.ToDictionary(definition => definition.IncidentType);
        var impacts = new List<BackstageMoraleImpact>();
        var inboxItems = new List<InboxItem>();

        foreach (var incident in incidents)
        {
            _repository.EnregistrerBackstageIncident(incident);

            var definition = definitionLookup.GetValueOrDefault(incident.IncidentType);
            var moraleDelta = definition is null
                ? 0
                : _random.Next(definition.MoraleImpactMin, definition.MoraleImpactMax + 1);

            if (moraleDelta != 0)
            {
                var raison = definition is null ? "Incident backstage" : $"Incident backstage : {definition.Libelle}";
                impacts.Add(new BackstageMoraleImpact(incident.WorkerId, moraleDelta, raison, incident.IncidentId, null));
            }

            var workerName = roster.FirstOrDefault(worker => worker.WorkerId == incident.WorkerId)?.Nom ?? incident.WorkerId;
            var titre = "Incident backstage";
            var contenu = definition is null
                ? $"{workerName} est impliqué dans un incident backstage."
                : $"{workerName} : {definition.Libelle}. {definition.Description}";
            if (moraleDelta != 0)
            {
                contenu += $" Impact moral {moraleDelta:+#;-#;0}.";
            }

            inboxItems.Add(new InboxItem("backstage", titre, contenu, semaine));
        }

        _repository.AppliquerMoraleImpacts(impacts, semaine);

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

    private InboxItem? GenererScoutingHebdo(int semaine)
    {
        var service = new ScoutingService(_scoutingRepository, new SeededRandomProvider(semaine));
        var refresh = service.RafraichirHebdo(semaine);

        if (refresh.RapportsCrees == 0 && refresh.MissionsAvancees == 0 && refresh.MissionsTerminees == 0)
        {
            return null;
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

    private void AppliquerFinancesHebdo(string showId, int semaine)
    {
        var companyId = _repository.ChargerCompagnieIdPourShow(showId);
        if (string.IsNullOrWhiteSpace(companyId))
        {
            return;
        }

        var compagnie = _repository.ChargerEtatCompagnie(companyId);
        if (compagnie is null)
        {
            return;
        }

        var contrats = _repository.ChargerPaieContrats(companyId);
        var context = new WeeklyFinanceContext(companyId, semaine, compagnie.Tresorerie, contrats);
        var engine = new FinanceEngine(FinanceSettings.V1());
        var tick = new WeeklyFinanceTick(engine);
        var resultat = tick.Executer(context);

        _repository.AppliquerTransactionsFinancieres(companyId, semaine, resultat.Transactions);
        _repository.EnregistrerSnapshotFinance(companyId, semaine);
    }

    private IEnumerable<InboxItem> VerifierOffresExpirantes(int semaine)
    {
        var offres = _repository.ChargerOffresExpirant(semaine);
        if (offres.Count == 0)
        {
            return Array.Empty<InboxItem>();
        }

        var noms = _repository.ChargerNomsWorkers();
        var items = new List<InboxItem>(offres.Count);

        foreach (var offre in offres)
        {
            _repository.MettreAJourStatutOffre(offre.OfferId, "expiree");

            var nom = noms.TryGetValue(offre.WorkerId, out var workerNom) ? workerNom : offre.WorkerId;
            var contenu = $"L'offre contractuelle pour {nom} a expiré.";
            items.Add(new InboxItem("contrat", "Offre expirée", contenu, semaine));
        }

        return items;
    }

    /// <summary>
    /// Progresse le moral et les rumeurs chaque semaine (Phase 3)
    /// </summary>
    private IEnumerable<InboxItem> ProgresserMoraleEtRumeurs(int semaine, string showId)
    {
        var compagnieId = _repository.ChargerCompagnieIdPourShow(showId);
        if (string.IsNullOrWhiteSpace(compagnieId))
        {
            return Array.Empty<InboxItem>();
        }

        var items = new List<InboxItem>();

        // Progresser les rumeurs (amplification naturelle, résolution, etc.)
        if (_rumorEngine is not null)
        {
            try
            {
                _rumorEngine.ProgressRumors(compagnieId);

                // Récupérer les rumeurs widespread pour notifications
                var widespreadRumors = _rumorEngine.GetWidespreadRumors(compagnieId);
                foreach (var rumor in widespreadRumors.Take(3)) // Max 3 notifications par semaine
                {
                    var titre = $"⚠️ Rumeur répandue: {rumor.RumorType}";
                    var contenu = rumor.RumorText;
                    items.Add(new InboxItem("rumeur", titre, contenu, semaine));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WeeklyLoopService] Erreur progression rumeurs: {ex.Message}");
            }
        }

        // Détecter les signaux faibles de moral
        if (_moraleEngine is not null)
        {
            try
            {
                var weakSignals = _moraleEngine.DetectWeakSignals(compagnieId);
                foreach (var signal in weakSignals.Take(2)) // Max 2 signaux par semaine
                {
                    items.Add(new InboxItem("moral", "Signal Moral", signal, semaine));
                }

                // Recalculer le moral de compagnie
                _moraleEngine.CalculateCompanyMorale(compagnieId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WeeklyLoopService] Erreur détection moral: {ex.Message}");
            }
        }

        return items;
    }

}

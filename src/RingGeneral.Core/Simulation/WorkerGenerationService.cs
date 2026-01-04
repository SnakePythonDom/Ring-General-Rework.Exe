using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class WorkerGenerationService : IWorkerGenerationService
{
    private readonly IRandomProvider _random;
    private readonly WorkerGenerationSpec _spec;

    public WorkerGenerationService(IRandomProvider random, WorkerGenerationSpec spec)
    {
        _random = random;
        _spec = spec;
    }

    public WorkerGenerationReport GenerateWeekly(GameState state, int seed)
    {
        _random.Reseed(seed);

        if (state.Options.YouthMode == YouthGenerationMode.Desactivee && state.Options.WorldMode == WorldGenerationMode.Desactivee)
        {
            return WorkerGenerationReport.Vide(state.Semaine);
        }

        var resultatStructures = new List<WorkerGenerationResultatStructure>();
        var resultatMonde = new List<WorkerGenerationResultatMonde>();
        var workers = new List<GeneratedWorker>();
        var notices = new List<WorkerGenerationNotice>();

        var useMensuel = state.Options.YouthMode == YouthGenerationMode.Abondante
            && _spec.Modes.Abondant.Mensuel == true
            && _spec.Frequences.Mensuel.Actif;

        var semainePivot = state.Options.SemainePivotAnnuelle ?? _spec.Frequences.Annuel.SemainePivot;
        var semaineDansAnnee = ((state.Semaine - 1) % 52) + 1;
        var estSemaineGeneration = useMensuel
            ? state.Semaine % _spec.Frequences.Mensuel.IntervalleSemaines == 0
            : semaineDansAnnee == semainePivot;

        var generationYouthActive = state.Options.YouthMode != YouthGenerationMode.Desactivee && estSemaineGeneration;
        if (!generationYouthActive && state.Options.WorldMode == WorldGenerationMode.Desactivee)
        {
            return WorkerGenerationReport.Vide(state.Semaine);
        }

        var modeSpec = state.Options.YouthMode switch
        {
            YouthGenerationMode.Abondante => _spec.Modes.Abondant,
            YouthGenerationMode.Realiste => _spec.Modes.Realiste,
            _ => _spec.Modes.Desactive
        };

        var globalTraineesRestants = Math.Max(0, _spec.Caps.Trainees.GlobalAnnuel - state.Counters.GlobalTrainees);
        var globalFreeAgentsRestants = Math.Max(0, _spec.Caps.FreeAgents.GlobalAnnuel - state.Counters.GlobalFreeAgents);
        var traineesParPays = new Dictionary<string, int>(state.Counters.TraineesParPays, StringComparer.OrdinalIgnoreCase);
        var traineesParCompagnie = new Dictionary<string, int>(state.Counters.TraineesParCompagnie, StringComparer.OrdinalIgnoreCase);
        var freeAgentsParPays = new Dictionary<string, int>(state.Counters.FreeAgentsParPays, StringComparer.OrdinalIgnoreCase);

        if (generationYouthActive)
        {
            foreach (var youth in state.YouthStructures)
            {
                if (!youth.Actif)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Structure inactive."));
                    continue;
                }

                var cooldown = useMensuel ? _spec.Frequences.Mensuel.CooldownSemaines : _spec.Frequences.Annuel.CooldownSemaines;
                if (youth.DerniereGenerationSemaine.HasValue && state.Semaine - youth.DerniereGenerationSemaine.Value < cooldown)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Cooldown en cours."));
                    continue;
                }

                var capActifs = Math.Max(0, _spec.Caps.Trainees.ParStructure.MaxActifs - youth.TraineesActifs);
                if (capActifs == 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Capacité maximale atteinte."));
                    continue;
                }

                if (globalTraineesRestants == 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Cap mondial atteint."));
                    continue;
                }

                var paysUtilises = traineesParPays.TryGetValue(youth.Region, out var comptePays) ? comptePays : 0;
                var capPaysRestant = Math.Max(0, _spec.Caps.Trainees.ParPaysAnnuel - paysUtilises);
                if (capPaysRestant == 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Cap pays atteint."));
                    continue;
                }

                var companyUtilises = traineesParCompagnie.TryGetValue(youth.CompagnieId, out var compteCompagnie) ? compteCompagnie : 0;
                var capCompagnieRestant = Math.Max(0, _spec.Caps.Trainees.ParCompagnieAnnuel - companyUtilises);
                if (capCompagnieRestant == 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Cap compagnie atteint."));
                    continue;
                }

                var capPeriode = _spec.Caps.Trainees.ParStructure.MaxParPeriode;
                var limiteDisponibles = new[] { capActifs, capPeriode, globalTraineesRestants, capPaysRestant, capCompagnieRestant }.Min();
                if (limiteDisponibles <= 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Limites atteintes."));
                    continue;
                }

                var baseCount = CalculerBaseQuantite(youth) * modeSpec.MultiplicateurQuantite;
                var fluctuation = 0.85 + _random.NextDouble() * 0.3;
                var cible = (int)Math.Round(baseCount * fluctuation);
                var quantite = Math.Clamp(cible, 0, limiteDisponibles);

                if (quantite == 0)
                {
                    resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, 0, "Aucune génération cette période."));
                    continue;
                }

                for (var i = 0; i < quantite; i++)
                {
                    var worker = GenererWorker(youth, state.Semaine, i, true);
                    workers.Add(worker);
                }

                globalTraineesRestants -= quantite;
                traineesParPays[youth.Region] = (traineesParPays.TryGetValue(youth.Region, out var exist) ? exist : 0) + quantite;
                traineesParCompagnie[youth.CompagnieId] = (traineesParCompagnie.TryGetValue(youth.CompagnieId, out var company) ? company : 0) + quantite;

                var message = $"{quantite} nouveaux élèves rejoignent {youth.Nom} ({youth.Region}).";
                notices.Add(new WorkerGenerationNotice("youth", "Nouveaux trainees inscrits", message, $"youth:{youth.YouthId}"));
                resultatStructures.Add(new WorkerGenerationResultatStructure(youth.YouthId, youth.Nom, youth.Region, quantite, null));
            }
        }

        if (state.Options.WorldMode == WorldGenerationMode.Faible)
        {
            var maxMonde = Math.Min(globalFreeAgentsRestants, _spec.Monde.Faible.MaxParPeriode);
            if (maxMonde > 0 && _random.NextDouble() <= _spec.Monde.Faible.ChanceHebdo)
            {
                var region = state.Region;
                var freeAgentsPays = freeAgentsParPays.TryGetValue(region, out var count) ? count : 0;
                var capPaysRestant = Math.Max(0, _spec.Caps.FreeAgents.ParPaysAnnuel - freeAgentsPays);
                if (capPaysRestant > 0)
                {
                    var quantite = Math.Min(maxMonde, capPaysRestant);
                    for (var i = 0; i < quantite; i++)
                    {
                        var worker = GenererWorker(new YouthStructureState(
                            "WORLD",
                            "Monde",
                            state.CompagnieId,
                            region,
                            "INDEPENDANT",
                            0,
                            0,
                            1,
                            10,
                            "HYBRIDE",
                            true,
                            null,
                            0),
                            state.Semaine,
                            i,
                            false);
                        workers.Add(worker);
                    }

                    globalFreeAgentsRestants -= quantite;
                    freeAgentsParPays[region] = freeAgentsPays + quantite;
                    resultatMonde.Add(new WorkerGenerationResultatMonde(region, quantite, null));
                    notices.Add(new WorkerGenerationNotice("monde", "Nouveau catcheur sur la scène indépendante",
                        $"Un nouveau catcheur apparaît sur la scène indépendante de {region}.", $"world:{region}"));
                }
                else
                {
                    resultatMonde.Add(new WorkerGenerationResultatMonde(region, 0, "Cap pays atteint."));
                }
            }
            else
            {
                resultatMonde.Add(new WorkerGenerationResultatMonde(state.Region, 0, "Pas de génération cette semaine."));
            }
        }

        return new WorkerGenerationReport(state.Semaine, workers, resultatStructures, resultatMonde, notices);
    }

    private double CalculerBaseQuantite(YouthStructureState youth)
    {
        if (!_spec.Facteurs.TypeYouth.TryGetValue(youth.Type, out var typeFactor))
        {
            typeFactor = new WorkerGenerationTypeFactor(2, 4);
        }

        var baseCount = typeFactor.Base;
        var infraBonus = Math.Max(0, youth.NiveauEquipements - 1) * _spec.Facteurs.Infrastructure.BonusParNiveau;
        var budgetBonus = _spec.Facteurs.Budget.Paliers
            .FirstOrDefault(p => youth.BudgetAnnuel >= p.Min && youth.BudgetAnnuel <= p.Max)?.Bonus ?? 0;
        var coachingBonus = Math.Max(0, youth.QualiteCoaching - 10) * _spec.Facteurs.Coaching.BonusParPoint;

        var total = baseCount + infraBonus + budgetBonus + coachingBonus;
        return Math.Clamp(total, 0, typeFactor.Max);
    }

    private GeneratedWorker GenererWorker(YouthStructureState youth, int semaine, int index, bool estTrainee)
    {
        var profil = estTrainee ? _spec.Profils.Trainee : _spec.Profils.FreeAgent;
        var age = _random.Next(profil.AgeMin, profil.AgeMax + 1);
        var prenom = ChoisirNom(_spec.Noms.Prenoms, $"Prenom{index}");
        var nom = ChoisirNom(_spec.Noms.Noms, $"Nom{index}");
        var specialite = ChoisirSpecialite(profil.Specialites);

        var baseValue = _random.Next(4, 9);
        var (inRing, entertainment, story) = specialite switch
        {
            "inring" => (baseValue + 4, baseValue + 1, baseValue + 1),
            "divertissement" => (baseValue + 1, baseValue + 4, baseValue + 2),
            "histoire" => (baseValue + 1, baseValue + 2, baseValue + 4),
            _ => (baseValue + 2, baseValue + 2, baseValue + 2)
        };

        AppliquerBonusRegion(youth.Region, ref inRing, ref entertainment, ref story);
        AppliquerBonusPhilosophie(youth.Philosophie, ref inRing, ref entertainment, ref story);

        var attributes = new Dictionary<string, int>
        {
            ["in_ring"] = ClampAttribut(inRing),
            ["entertainment"] = ClampAttribut(entertainment),
            ["story"] = ClampAttribut(story)
        };

        var workerId = estTrainee
            ? $"TR-{youth.YouthId}-{semaine}-{index + 1}"
            : $"FA-{youth.Region}-{semaine}-{index + 1}";

        return new GeneratedWorker(
            workerId,
            prenom,
            nom,
            estTrainee ? "TRAINEE" : "CATCHEUR",
            age,
            youth.Region,
            estTrainee ? youth.CompagnieId : null,
            estTrainee ? youth.YouthId : null,
            attributes["in_ring"],
            attributes["entertainment"],
            attributes["story"],
            _spec.ValeursInitiales.Popularite,
            _spec.ValeursInitiales.Fatigue,
            _spec.ValeursInitiales.Blessure,
            _spec.ValeursInitiales.Momentum,
            estTrainee ? _spec.ValeursInitiales.RoleTrainee : _spec.ValeursInitiales.RoleFreeAgent,
            specialite,
            attributes);
    }

    private static int ClampAttribut(int value) => Math.Clamp(value, 1, 20);

    private string ChoisirNom(IReadOnlyList<string> noms, string fallback)
    {
        if (noms.Count == 0)
        {
            return fallback;
        }

        return noms[_random.Next(0, noms.Count)];
    }

    private string ChoisirSpecialite(IReadOnlyDictionary<string, double> distribution)
    {
        var total = distribution.Values.Sum();
        if (total <= 0)
        {
            return "polyvalent";
        }

        var roll = _random.NextDouble() * total;
        foreach (var (specialite, poids) in distribution)
        {
            roll -= poids;
            if (roll <= 0)
            {
                return specialite;
            }
        }

        return distribution.Keys.First();
    }

    private void AppliquerBonusPhilosophie(string philosophie, ref int inRing, ref int entertainment, ref int story)
    {
        if (!_spec.Facteurs.Philosophie.TryGetValue(philosophie, out var bonus))
        {
            return;
        }

        inRing += bonus.BonusTechnique ?? 0;
        inRing += bonus.BonusAerial ?? 0;
        entertainment += bonus.BonusPromo ?? 0;
        story += bonus.BonusGeneral ?? 0;
    }

    private void AppliquerBonusRegion(string region, ref int inRing, ref int entertainment, ref int story)
    {
        if (!_spec.Facteurs.Regions.TryGetValue(region, out var bonus))
        {
            return;
        }

        inRing += bonus.BonusTechnique ?? 0;
        inRing += bonus.BonusAerial ?? 0;
        entertainment += bonus.BonusPromo ?? 0;
    }
}

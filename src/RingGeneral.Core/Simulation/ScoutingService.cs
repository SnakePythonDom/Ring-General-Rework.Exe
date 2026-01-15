using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class ScoutingService
{
    private readonly IScoutingRepository _repository;
    private readonly IRandomProvider _random;

    public ScoutingService(IScoutingRepository repository, IRandomProvider random)
    {
        _repository = repository;
        _random = random;
    }

    public ScoutReport CreerRapport(string workerId, int semaine, string? notes = null)
    {
        var cible = _repository.ChargerCibleScouting(workerId);
        if (cible is null)
        {
            throw new InvalidOperationException($"Cible de scouting inconnue: {workerId}");
        }

        return CreerRapport(cible, semaine, notes);
    }

    public ScoutMission CreerMission(string titre, string region, string focus, int objectif, int semaine)
    {
        var mission = new ScoutMission(
            $"SCM-{region}-{semaine}-{Guid.NewGuid():N}",
            titre,
            region,
            focus,
            0,
            objectif,
            "active",
            semaine,
            semaine);

        _repository.AjouterMission(mission);
        return mission;
    }

    public ScoutingWeeklyRefresh RafraichirHebdo(int semaine)
    {
        _random.Reseed(semaine * 7919);

        var rapportsCrees = GenererRapportsHebdo(semaine).Count;
        var missions = _repository.ChargerMissionsActives();
        var missionsAvancees = 0;
        var missionsTerminees = 0;

        foreach (var mission in missions)
        {
            var increment = _random.Next(1, 4);
            var nouvelleProgression = Math.Min(mission.Objectif, mission.Progression + increment);
            var statut = nouvelleProgression >= mission.Objectif ? "terminee" : mission.Statut;

            if (nouvelleProgression != mission.Progression)
            {
                missionsAvancees++;
            }

            if (statut != mission.Statut && statut == "terminee")
            {
                missionsTerminees++;
            }

            _repository.MettreAJourMissionProgress(mission.MissionId, nouvelleProgression, statut, semaine);
        }

        return new ScoutingWeeklyRefresh(semaine, rapportsCrees, missionsAvancees, missionsTerminees);
    }

    public IReadOnlyList<ScoutReport> GenererRapportsHebdo(int semaine)
    {
        var cibles = _repository.ChargerCiblesScouting(6);
        if (cibles.Count == 0)
        {
            return [];
        }

        var maxRapports = Math.Min(2, cibles.Count);
        var rapports = new List<ScoutReport>();
        var selection = cibles.OrderBy(_ => _random.Next(0, 1000)).Take(maxRapports);

        foreach (var cible in selection)
        {
            if (_repository.RapportExiste(cible.WorkerId, semaine))
            {
                continue;
            }

            rapports.Add(CreerRapport(cible, semaine, "Rapport hebdomadaire auto-généré."));
        }

        return rapports;
    }

    private ScoutReport CreerRapport(ScoutingTarget cible, int semaine, string? notes)
    {
        var potentiel = CalculerPotentiel(cible);
        var resume = $"Observé sur la scène {cible.Region}.";

        var report = new ScoutReport(
            $"SCR-{cible.WorkerId}-{semaine}",
            cible.WorkerId,
            cible.NomComplet,
            cible.Region,
            potentiel,
            cible.InRing,
            cible.Entertainment,
            cible.Story,
            resume,
            notes ?? string.Empty,
            semaine,
            "free_agent");

        _repository.AjouterScoutReport(report);
        return report;
    }

    private int CalculerPotentiel(ScoutingTarget cible)
    {
        var baseMoyenne = (cible.InRing + cible.Entertainment + cible.Story) / 3.0;
        var variance = _random.Next(-2, 3);
        return Math.Clamp((int)Math.Round(baseMoyenne + variance), 1, 20);
    }
}

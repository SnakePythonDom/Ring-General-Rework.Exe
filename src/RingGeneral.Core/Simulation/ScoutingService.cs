using RingGeneral.Core.Models;
using RingGeneral.Core.Random;

namespace RingGeneral.Core.Simulation;

public sealed class ScoutingService
{
    private readonly SeededRandomProvider _random;

    public ScoutingService(SeededRandomProvider random)
    {
        _random = random;
    }

    public ScoutingWeeklyRefresh RafraichirSemaine(
        IReadOnlyList<ScoutMission> missions,
        IReadOnlyList<ScoutReport> rapportsExistants,
        IReadOnlyList<ScoutTargetProfile> cibles,
        int semaine)
    {
        var rapports = new List<ScoutReport>();
        var missionsMaj = new List<ScoutMission>();
        var missionsTerminees = new List<ScoutMission>();
        var dejaRapportes = new HashSet<string>(rapportsExistants.Select(r => r.WorkerId));

        foreach (var mission in missions)
        {
            if (string.Equals(mission.Statut, "TERMINEE", StringComparison.OrdinalIgnoreCase))
            {
                missionsMaj.Add(mission);
                continue;
            }

            var progression = mission.Progression + 1;
            var statut = progression >= mission.DureeSemaines ? "TERMINEE" : "EN_COURS";
            var rapportId = mission.RapportId;

            if (statut == "TERMINEE" && string.IsNullOrWhiteSpace(rapportId))
            {
                var cible = ChoisirCible(cibles, dejaRapportes, mission.Region);
                if (cible is not null)
                {
                    var rapport = CreerRapport(cible, semaine);
                    rapports.Add(rapport);
                    dejaRapportes.Add(rapport.WorkerId);
                    rapportId = rapport.ReportId;
                }
            }

            var missionMaj = mission with { Progression = progression, Statut = statut, RapportId = rapportId };
            missionsMaj.Add(missionMaj);
            if (statut == "TERMINEE")
            {
                missionsTerminees.Add(missionMaj);
            }
        }

        if (DoitGenererRapportHebdo() && cibles.Count > 0)
        {
            var cible = ChoisirCible(cibles, dejaRapportes, null);
            if (cible is not null)
            {
                var rapport = CreerRapport(cible, semaine);
                rapports.Add(rapport);
            }
        }

        return new ScoutingWeeklyRefresh(rapports, missionsMaj, missionsTerminees);
    }

    public ScoutReport CreerRapport(ScoutTargetProfile cible, int semaine)
    {
        var (force, faiblesse) = DeterminerPoints(cible);
        var note = CalculerNote(cible);
        var recommendation = note switch
        {
            >= 70 => "Cible prioritaire pour une signature rapide.",
            >= 55 => "À suivre de près, potentiel solide pour le roster.",
            _ => "Profil à garder en observation, potentiel limité."
        };

        var resume = $"{cible.NomComplet} présente un profil {DeterminerProfil(note)} avec une présence notable sur la scène {cible.Region}.";

        return new ScoutReport(
            Guid.NewGuid().ToString("N"),
            cible.WorkerId,
            cible.NomComplet,
            cible.Region,
            semaine,
            note,
            force,
            faiblesse,
            recommendation,
            resume);
    }

    private bool DoitGenererRapportHebdo()
    {
        return _random.NextDouble() <= 0.45;
    }

    private ScoutTargetProfile? ChoisirCible(
        IReadOnlyList<ScoutTargetProfile> cibles,
        HashSet<string> dejaRapportes,
        string? region)
    {
        var disponibles = cibles
            .Where(cible => !dejaRapportes.Contains(cible.WorkerId))
            .Where(cible => region is null || string.Equals(cible.Region, region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (disponibles.Count == 0)
        {
            disponibles = cibles.Where(cible => !dejaRapportes.Contains(cible.WorkerId)).ToList();
        }

        if (disponibles.Count == 0)
        {
            return null;
        }

        var index = _random.Next(0, disponibles.Count);
        return disponibles[index];
    }

    private static (string Force, string Faiblesse) DeterminerPoints(ScoutTargetProfile cible)
    {
        var scores = new Dictionary<string, int>
        {
            ["l'impact in-ring"] = cible.InRing,
            ["le divertissement"] = cible.Entertainment,
            ["le storytelling"] = cible.Story,
            ["la popularité"] = cible.Popularite,
            ["le momentum"] = cible.Momentum
        };

        var force = scores.OrderByDescending(pair => pair.Value).First();
        var faiblesse = scores.OrderBy(pair => pair.Value).First();

        return ($"Point fort : {force.Key} (niveau {force.Value}).", $"Point faible : {faiblesse.Key} (niveau {faiblesse.Value}).");
    }

    private static int CalculerNote(ScoutTargetProfile cible)
    {
        var moyenne = (cible.InRing + cible.Entertainment + cible.Story + cible.Popularite + cible.Momentum) / 5.0;
        return Math.Clamp((int)Math.Round(moyenne), 35, 95);
    }

    private static string DeterminerProfil(int note)
    {
        return note switch
        {
            >= 75 => "élite",
            >= 60 => "confirmé",
            >= 45 => "en développement",
            _ => "fragile"
        };
    }
}

public sealed record ScoutingWeeklyRefresh(
    IReadOnlyList<ScoutReport> NouveauxRapports,
    IReadOnlyList<ScoutMission> MissionsMaj,
    IReadOnlyList<ScoutMission> MissionsTerminees);

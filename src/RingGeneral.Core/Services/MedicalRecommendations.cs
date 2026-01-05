using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class MedicalRecommendations
{
    public MedicalRecommendation Recommander(int fatigue, int severite)
    {
        var fatigueClamped = Math.Clamp(fatigue, 0, 100);
        var severiteClamped = Math.Clamp(severite, 0, 100);

        var repos = (int)Math.Ceiling(severiteClamped / 25.0 + fatigueClamped / 40.0);
        if (severiteClamped == 0)
        {
            repos = Math.Max(0, repos - 1);
        }

        repos = Math.Clamp(repos, severiteClamped > 0 ? 1 : 0, 8);

        var risque = Math.Clamp((severiteClamped / 100.0) * 0.6 + (fatigueClamped / 100.0) * 0.4, 0, 1);
        var niveau = risque switch
        {
            < 0.25 => "faible",
            < 0.5 => "modéré",
            < 0.75 => "élevé",
            _ => "critique"
        };

        var conseil = repos == 0
            ? "Surveillez la charge de travail et privilégiez la récupération légère."
            : $"Repos conseillé : {repos} semaine(s), éviter les matchs intenses.";

        return new MedicalRecommendation(repos, risque, niveau, conseil);
    }
}

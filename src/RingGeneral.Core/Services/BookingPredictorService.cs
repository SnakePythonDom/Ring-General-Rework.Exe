using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de prédiction des notes et audiences pour le booking.
/// Permet au joueur d'avoir une estimation avant le show.
/// </summary>
public sealed class BookingPredictorService
{
    /// <summary>
    /// Prédit la note d'un segment basé sur les données connues des participants.
    /// </summary>
    public int PredireNoteSegment(SegmentSimulationContext segment)
    {
        if (segment.ParticipantsDetails == null || !segment.ParticipantsDetails.Any())
            return 0;

        // Formule simplifiée inspirée du ShowSimulationEngine
        // Note = (Moyenne Popularité * 0.4) + (Moyenne Skill * 0.6)

        var avgPop = segment.ParticipantsDetails.Average(p => p.Popularite);
        var avgSkill = segment.ParticipantsDetails.Average(p => (p.InRing + p.Entertainment) / 2.0);

        double noteBase;
        if (segment.TypeSegment?.ToLower() == "match")
        {
            // Les matchs favorisent légèrement le skill in-ring
            // Ajuster selon le style si présent
            var style = segment.Settings?.GetValueOrDefault("style")?.ToLower() ?? "standard";

            if (style == "technical")
                noteBase = (avgPop * 0.3) + (avgSkill * 0.7);
            else if (style == "brawl" || style == "hardcore")
                noteBase = (avgPop * 0.6) + (avgSkill * 0.4);
            else // Standard, High Flying
                noteBase = (avgPop * 0.4) + (avgSkill * 0.6);

            if (style == "squash")
                noteBase -= 15; // Les squashs sont moins bien notés techniquement
        }
        else
        {
            // Les promos favorisent la popularité et l'entertainment
            var avgEnt = segment.ParticipantsDetails.Average(p => p.Entertainment);
            noteBase = (avgPop * 0.6) + (avgEnt * 0.4);
        }

        // Impact de l'intensité
        var intensityImpact = (segment.Intensite - 50) / 10.0;
        noteBase += intensityImpact;

        // Impact de la durée (pénale pour segments trop courts ou trop longs par défaut)
        if (segment.DureeMinutes < 5) noteBase -= 5;
        if (segment.DureeMinutes > 30) noteBase -= 5;

        return Math.Clamp((int)noteBase, 0, 100);
    }

    /// <summary>
    /// Prédit la note globale d'un show.
    /// </summary>
    public int PredireNoteGlobale(IEnumerable<int> notesSegments)
    {
        var list = notesSegments.ToList();
        if (!list.Any()) return 0;

        double total = 0;
        double weightTotal = 0;

        for (int i = 0; i < list.Count; i++)
        {
            // Seul le dernier segment du show compte double (Main Event)
            var isMainEvent = i == list.Count - 1;
            var weight = isMainEvent ? 2.0 : 1.0;

            total += list[i] * weight;
            weightTotal += weight;
        }

        return Math.Clamp((int)(total / weightTotal), 0, 100);
    }

    /// <summary>
    /// Prédit l'audience basée sur la popularité des participants et l'importance du show.
    /// </summary>
    public int PredireAudience(ShowContext context, int predictedNote)
    {
        if (context.Workers == null || !context.Workers.Any()) return 1000;

        // Influence des stars (Top 3 popularité)
        var topStarsPop = context.Workers.OrderByDescending(w => w.Popularite).Take(3).Average(w => w.Popularite);

        // Base audience selon la note prédite et le star power
        var baseAudience = 1000 + (topStarsPop * 100) + (predictedNote * 50);

        return (int)baseAudience;
    }
}

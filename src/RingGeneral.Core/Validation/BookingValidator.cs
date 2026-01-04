using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Validation;

public sealed class BookingValidator : IValidator
{
    public ValidationResult ValiderBooking(BookingPlan plan)
    {
        var erreurs = new List<string>();
        var avertissements = new List<string>();

        if (plan.Segments.Count == 0)
        {
            erreurs.Add("Aucun segment n'a été booké.");
        }

        var dureeTotale = plan.Segments.Sum(segment => segment.DureeMinutes);
        if (plan.DureeShowMinutes.HasValue && dureeTotale > plan.DureeShowMinutes.Value)
        {
            erreurs.Add($"La durée totale ({dureeTotale} min) dépasse la durée du show ({plan.DureeShowMinutes.Value} min).");
        }

        var mainEvent = plan.Segments.FirstOrDefault(segment => segment.EstMainEvent);
        if (mainEvent is null && plan.Segments.Count > 0)
        {
            avertissements.Add("Aucun main event n'a été défini.");
        }
        else if (mainEvent?.ParticipantsDetails is not null && mainEvent.ParticipantsDetails.Count > 0)
        {
            var scoreMainEvent = mainEvent.ParticipantsDetails.Average(participant => participant.Popularite);
            if (scoreMainEvent < 45)
            {
                avertissements.Add("Le main event semble trop faible pour porter le show.");
            }
        }

        var promos = plan.Segments.Where(segment => segment.TypeSegment is "promo" or "angle_backstage").ToList();
        if (promos.Count > 1)
        {
            var groupes = promos.GroupBy(segment => string.Join(",", segment.Participants.OrderBy(id => id)))
                .Where(groupe => groupe.Count() > 1);
            if (groupes.Any())
            {
                avertissements.Add("Des promos répétées avec les mêmes participants ont été détectées.");
            }
        }

        if (plan.EtatWorkers is not null)
        {
            foreach (var participantId in plan.Segments.SelectMany(segment => segment.Participants).Distinct())
            {
                if (!plan.EtatWorkers.TryGetValue(participantId, out var health))
                {
                    continue;
                }

                if (health.Blessure is not "AUCUNE")
                {
                    avertissements.Add($"Attention : {participantId} est blessé ({health.Blessure}).");
                }

                if (health.Fatigue >= 70)
                {
                    avertissements.Add($"Attention : {participantId} est très fatigué ({health.Fatigue}).");
                }
            }
        }

        return new ValidationResult(erreurs.Count == 0, erreurs, avertissements);
    }
}

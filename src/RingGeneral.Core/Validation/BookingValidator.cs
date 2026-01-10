using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Validation;

public sealed class BookingValidator
{
    public ValidationResult ValiderBooking(BookingPlan plan)
    {
        var issues = new List<ValidationIssue>();

        if (plan.Segments.Count == 0)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Erreur,
                "booking.empty",
                "Aucun segment n'a été booké."));
        }

        var dureeTotale = plan.Segments.Sum(segment => segment.DureeMinutes);
        if (plan.DureeShowMinutes.HasValue && dureeTotale > plan.DureeShowMinutes.Value)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Erreur,
                "booking.duration.exceed",
                $"La durée totale ({dureeTotale} min) dépasse la durée du show ({plan.DureeShowMinutes.Value} min)."));
        }

        var mainEvent = plan.Segments.FirstOrDefault(segment => segment.EstMainEvent);
        if (mainEvent is null && plan.Segments.Count > 0)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Avertissement,
                "booking.main-event.missing",
                "Aucun main event n'a été défini."));
        }
        else if (mainEvent?.ParticipantsDetails is not null && mainEvent.ParticipantsDetails.Count > 0)
        {
            var scoreMainEvent = mainEvent.ParticipantsDetails.Average(participant => participant.Popularite);
            if (scoreMainEvent < 45)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Avertissement,
                    "booking.main-event.weak",
                    "Le main event semble trop faible pour porter le show.",
                    mainEvent.SegmentId));
            }
        }

        var promos = plan.Segments.Where(segment => segment.TypeSegment is "promo" or "angle_backstage").ToList();
        if (promos.Count > 1)
        {
            var groupes = promos.GroupBy(segment => string.Join(",", segment.Participants.OrderBy(id => id)))
                .Where(groupe => groupe.Count() > 1);
            if (groupes.Any())
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Avertissement,
                    "booking.promos.repetition",
                    "Des promos répétées avec les mêmes participants ont été détectées."));
            }
        }

        foreach (var segment in plan.Segments)
        {
            if (segment.Participants.Count == 0)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Avertissement,
                    "segment.participants.empty",
                    "Ajoutez des participants à ce segment.",
                    segment.SegmentId));
            }

            if (segment.DureeMinutes <= 0)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Avertissement,
                    "segment.duration.invalid",
                    "La durée du segment est invalide.",
                    segment.SegmentId));
            }
        }

        if (plan.EtatWorkers is not null)
        {
            foreach (var segment in plan.Segments)
            {
                foreach (var participantId in segment.Participants)
                {
                    if (!plan.EtatWorkers.TryGetValue(participantId, out var health))
                    {
                        continue;
                    }

                    if (health.Blessure is not "AUCUNE")
                    {
                        issues.Add(new ValidationIssue(
                            ValidationSeverity.Avertissement,
                            "segment.participant.injured",
                            $"Attention : {participantId} est blessé ({health.Blessure}).",
                            segment.SegmentId));
                    }

                    if (health.Fatigue >= 70)
                    {
                        issues.Add(new ValidationIssue(
                            ValidationSeverity.Avertissement,
                            "segment.participant.fatigue",
                            $"Attention : {participantId} est très fatigué ({health.Fatigue}).",
                            segment.SegmentId));
                    }
                }
            }
        }

        var erreurs = issues.Where(issue => issue.Severite == ValidationSeverity.Erreur)
            .Select(issue => issue.Message)
            .ToList();
        var avertissements = issues.Where(issue => issue.Severite == ValidationSeverity.Avertissement)
            .Select(issue => issue.Message)
            .ToList();

        return new ValidationResult(erreurs.Count == 0, erreurs, avertissements, issues);
    }
}

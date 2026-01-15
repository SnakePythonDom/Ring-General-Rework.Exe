using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;
using System;
using System.Collections.Generic;

namespace RingGeneral.Core.Services;

public class PersonalityEngine : IPersonalityEngine
{
    public int CalculateAmbition(Worker worker)
    {
        return worker.MentalAttributes?.Ambition ?? 10;
    }

    public int CalculateLoyalty(Worker worker)
    {
        return worker.MentalAttributes?.Loyalty ?? 10;
    }

    /// <summary>
    /// Volatility is the inverse of Temperament (Emotional Control).
    /// High Volatility (20) = Low Temperament (0) = Explosive
    /// Low Volatility (0) = High Temperament (20) = Zen
    /// </summary>
    public int CalculateVolatility(Worker worker)
    {
        var temp = worker.MentalAttributes?.Temperament ?? 10;
        return 20 - temp;
    }

    public string CalculatePersonalityLabel(WorkerMentalAttributes mental)
    {
        if (mental == null) return "Unknown";

        if (mental.Ego > 16) return "Egomaniac";
        if (mental.Professionalism < 6) return "Troublemaker";
        if (mental.Ambition > 16) return "Ruthless";
        if (mental.Loyalty > 16) return "Company Man";
        if (mental.Temperament > 16) return "Ice Cold";

        return "Balanced";
    }

    public void UpdateMentalAttributes(WorkerMentalAttributes mental, string eventType, int intensity)
    {
        if (mental == null) return;

        switch (eventType)
        {
            case "Push":
                // Pushes increase ego and ambition
                mental.Ego = Math.Min(20, mental.Ego + (intensity / 2));
                mental.Ambition = Math.Min(20, mental.Ambition + (intensity / 3));
                break;
            case "Burial":
                // Burials decrease loyalty, might increase or decrease determination
                mental.Loyalty = Math.Max(0, mental.Loyalty - (intensity / 2));
                break;
            case "Fine":
                // Fines decrease loyalty but might improve professionalism if low
                mental.Loyalty = Math.Max(0, mental.Loyalty - 1);
                if (mental.Professionalism < 10)
                    mental.Professionalism = Math.Min(10, mental.Professionalism + 1);
                break;
        }
    }

    public bool ShouldPersonalityChange(WorkerMentalAttributes mental, PersonalityProfile currentProfile)
    {
        // Placeholder for dynamic evolution
        return false;
    }

    public List<string> GenerateSecondaryTraits(WorkerMentalAttributes mental)
    {
        var traits = new List<string>();
        if (mental == null) return traits;

        if (mental.Ego > 15) traits.Add("Selfish");
        if (mental.Professionalism > 15) traits.Add("Leader");
        if (mental.Pressure > 15) traits.Add("Clutch");
        if (mental.Sportsmanship < 5) traits.Add("Dirty");

        return traits;
    }

    public WorkerMentalAttributes GenerateRandomMentalAttributes()
    {
        var rand = new System.Random();
        return new WorkerMentalAttributes
        {
            Ambition = rand.Next(1, 21),
            Loyalty = rand.Next(1, 21),
            Professionalism = rand.Next(1, 21),
            Pressure = rand.Next(1, 21),
            Temperament = rand.Next(1, 21),
            Ego = rand.Next(1, 21),
            Adaptability = rand.Next(1, 21),
            Influence = rand.Next(1, 6), // Start low
            Determination = rand.Next(1, 21),
            Sportsmanship = rand.Next(1, 21)
        };
    }

    public InteractionResult ProcessInteraction(Worker worker, InteractionType interactionType)
    {
        var mental = worker.MentalAttributes;
        if (mental == null)
            return InteractionResult.FromSuccess("L'interaction s'est déroulée normalement.", interactionType);

        switch (interactionType)
        {
            case InteractionType.Talk:
                if (mental.Professionnalisme >= 8)
                    return InteractionResult.FromSuccess($"{worker.Name} est resté professionnel et a écouté vos retours.", interactionType);
                else
                    return InteractionResult.FromFailure($"{worker.Name} a semblé distrait et n'a pas vraiment pris en compte vos remarques.", interactionType);

            case InteractionType.Alliance:
                int allianceScore = mental.Loyauté + (mental.Ambition / 2);
                if (allianceScore >= 12)
                {
                    string msg = mental.Ambition > 16
                        ? $"{worker.Name} accepte l'alliance, y voyant une opportunité stratégique pour sa carrière."
                        : $"{worker.Name} accepte avec enthousiasme de travailler plus étroitement avec vous.";
                    return InteractionResult.FromSuccess(msg, interactionType);
                }
                else
                {
                    return InteractionResult.FromFailure($"{worker.Name} préfère rester indépendant pour le moment.", interactionType);
                }

            case InteractionType.Rivalry:
                int volatility = 20 - mental.Tempérament;
                if (volatility >= 15)
                {
                    var result = InteractionResult.FromSuccess($"{worker.Name} a explosé de colère ! La rivalité est maintenant personnelle.", interactionType);
                    result.MoraleChange = -5; // Aggressive reaction might slightly hurt morale but boost 'heat' (not yet in model)
                    return result;
                }
                return InteractionResult.FromSuccess($"{worker.Name} a pris note de votre déclaration de rivalité avec un calme olympien.", interactionType);

            case InteractionType.GimmickEdit:
                if (mental.Adaptabilité >= 12)
                {
                    return InteractionResult.FromSuccess($"{worker.Name} est prêt à essayer ces changements pour le bien du show.", interactionType);
                }
                else if (mental.Adaptabilité <= 5)
                {
                    var result = InteractionResult.FromFailure($"{worker.Name} déteste ces changements et craint pour l'intégrité de son personnage.", interactionType);
                    result.MoraleChange = -10;
                    return result;
                }
                else
                {
                    return InteractionResult.FromSuccess($"{worker.Name} accepte les changements mais semble peu convaincu par la direction prise.", interactionType);
                }

            default:
                return InteractionResult.FromSuccess("Interaction terminée.", interactionType);
        }
    }
}

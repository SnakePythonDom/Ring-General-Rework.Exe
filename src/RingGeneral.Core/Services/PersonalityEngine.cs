using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Implémentation du moteur de personnalités.
/// Mapping complexe attributs cachés → labels visibles.
/// </summary>
public class PersonalityEngine : IPersonalityEngine
{
    private readonly System.Random _random = new();

    public string CalculatePersonalityLabel(MentalAttributes attributes)
    {
        // Algorithme de mapping complexe (attributs cachés → label visible)
        // Les seuils sont intentionnellement opaques

        // === CAS COMPLEXES (prioritaires) ===

        // Professional but Egotistic
        if (attributes.Professionalism >= 14 && attributes.Ego >= 14)
            return PersonalityLabel.ProfessionalButEgotistic.ToString();

        // Ambitious but Volatile
        if (attributes.Ambition >= 14 && attributes.Temperament <= 6)
            return PersonalityLabel.AmbitiousButVolatile.ToString();

        // Creative but Rebellious
        if (attributes.Creativity >= 14 && attributes.Loyalty <= 6)
            return PersonalityLabel.CreativeButRebellious.ToString();

        // Loyal but Unambitious
        if (attributes.Loyalty >= 15 && attributes.Ambition <= 6)
            return PersonalityLabel.LoyalButUnambitious.ToString();

        // Confident but Arrogant
        if (attributes.Ego >= 15 && attributes.SocialSkills <= 7)
            return PersonalityLabel.ConfidentButArrogant.ToString();

        // === CAS SIMPLES ===

        // Professionalism dominant
        if (attributes.Professionalism >= 15 && attributes.WorkEthic >= 15)
            return PersonalityLabel.Professional.ToString();

        // Ego dominant (Egotistic)
        if (attributes.Ego >= 16 && attributes.SocialSkills < 10)
            return PersonalityLabel.Egotistic.ToString();

        // Ambition dominante
        if (attributes.Ambition >= 16 && attributes.Professionalism >= 10)
            return PersonalityLabel.Ambitious.ToString();

        // Volatilité (Temperament très bas)
        if (attributes.Temperament <= 5)
            return PersonalityLabel.Volatile.ToString();

        // Loyauté
        if (attributes.Loyalty >= 15 && attributes.Professionalism >= 12)
            return PersonalityLabel.Loyal.ToString();

        // Créativité
        if (attributes.Creativity >= 15 && attributes.Adaptability >= 12)
            return PersonalityLabel.Creative.ToString();

        // Résilience
        if (attributes.Resilience >= 16)
            return PersonalityLabel.Resilient.ToString();

        // Adaptabilité
        if (attributes.Adaptability >= 16)
            return PersonalityLabel.Adaptable.ToString();

        // Hardworking
        if (attributes.WorkEthic >= 16)
            return PersonalityLabel.Hardworking.ToString();

        // Unmotivated (faible ambition + faible work ethic)
        if (attributes.Ambition <= 5 && attributes.WorkEthic <= 6)
            return PersonalityLabel.Unmotivated.ToString();

        // Rebellious (faible loyalty + faible professionalism)
        if (attributes.Loyalty <= 6 && attributes.Professionalism <= 7)
            return PersonalityLabel.Rebellious.ToString();

        // Opportunistic (ego moyen-haut + loyalty bas)
        if (attributes.Ego >= 12 && attributes.Loyalty <= 7)
            return PersonalityLabel.Opportunistic.ToString();

        // Reserved (social skills bas + temperament haut)
        if (attributes.SocialSkills <= 7 && attributes.Temperament >= 14)
            return PersonalityLabel.Reserved.ToString();

        // Confident (ego élevé + temperament stable)
        if (attributes.Ego >= 13 && attributes.Ego <= 15 && attributes.Temperament >= 12)
            return PersonalityLabel.Confident.ToString();

        // Default: Balanced (aucun trait dominant)
        return PersonalityLabel.Balanced.ToString();
    }

    public void UpdateMentalAttributes(
        MentalAttributes attributes,
        string eventType,
        int intensity)
    {
        // Clamp intensity entre 1 et 5
        intensity = Math.Clamp(intensity, 1, 5);

        // Événements possibles et leurs impacts
        switch (eventType)
        {
            case "MainEventPush":
                // Push vers main event → augmente Ego et Ambition
                attributes.Ego = AdjustAttribute(attributes.Ego, +intensity);
                attributes.Ambition = AdjustAttribute(attributes.Ambition, +intensity);
                // Peut diminuer Loyalty si non mérité
                if (intensity >= 4)
                    attributes.Loyalty = AdjustAttribute(attributes.Loyalty, -1);
                break;

            case "TitleWin":
                // Victoire titre → boost confiance
                attributes.Ego = AdjustAttribute(attributes.Ego, +intensity);
                attributes.Resilience = AdjustAttribute(attributes.Resilience, +1);
                break;

            case "PushFailed":
                // Push raté → diminue Resilience et Ambition
                attributes.Resilience = AdjustAttribute(attributes.Resilience, -intensity);
                attributes.Ambition = AdjustAttribute(attributes.Ambition, -Math.Max(1, intensity - 1));
                attributes.Ego = AdjustAttribute(attributes.Ego, -1);
                break;

            case "ContractDispute":
                // Conflit contractuel → diminue Loyalty et Professionalism
                attributes.Loyalty = AdjustAttribute(attributes.Loyalty, -intensity);
                attributes.Professionalism = AdjustAttribute(attributes.Professionalism, -Math.Max(1, intensity - 1));
                break;

            case "InjuryReturn":
                // Retour de blessure → augmente Resilience
                attributes.Resilience = AdjustAttribute(attributes.Resilience, +intensity);
                attributes.WorkEthic = AdjustAttribute(attributes.WorkEthic, +1);
                break;

            case "CreativeControl":
                // Obtention de contrôle créatif → augmente Creativity et Ego
                attributes.Creativity = AdjustAttribute(attributes.Creativity, +intensity);
                attributes.Ego = AdjustAttribute(attributes.Ego, +1);
                break;

            case "LockerRoomConflict":
                // Conflit vestiaire → peut affecter SocialSkills et Temperament
                attributes.SocialSkills = AdjustAttribute(attributes.SocialSkills, -intensity);
                attributes.Temperament = AdjustAttribute(attributes.Temperament, -Math.Max(1, intensity - 1));
                break;

            case "PositiveFeedback":
                // Feedback positif (du booker, owner, etc.)
                attributes.Professionalism = AdjustAttribute(attributes.Professionalism, +1);
                attributes.WorkEthic = AdjustAttribute(attributes.WorkEthic, +1);
                break;

            case "PublicCriticism":
                // Critique publique
                attributes.Ego = AdjustAttribute(attributes.Ego, -intensity);
                attributes.Temperament = AdjustAttribute(attributes.Temperament, -1);
                break;

            case "MentorshipGiven":
                // Devenu mentor d'un jeune
                attributes.SocialSkills = AdjustAttribute(attributes.SocialSkills, +1);
                attributes.Adaptability = AdjustAttribute(attributes.Adaptability, +1);
                break;

            case "LongTermSuccess":
                // Succès sur le long terme (plusieurs mois)
                attributes.Professionalism = AdjustAttribute(attributes.Professionalism, +1);
                attributes.Resilience = AdjustAttribute(attributes.Resilience, +1);
                attributes.Loyalty = AdjustAttribute(attributes.Loyalty, +1);
                break;

            default:
                // Événement non reconnu, pas de changement
                break;
        }

        attributes.LastUpdated = DateTime.Now;
    }

    private int AdjustAttribute(int currentValue, int change)
    {
        return Math.Clamp(currentValue + change, 0, 20);
    }

    public bool ShouldPersonalityChange(
        MentalAttributes currentAttributes,
        Personality currentPersonality)
    {
        var calculatedLabel = CalculatePersonalityLabel(currentAttributes);
        return calculatedLabel != currentPersonality.PersonalityLabel;
    }

    public List<string> GenerateSecondaryTraits(MentalAttributes attributes)
    {
        var traits = new List<string>();

        // Identifier les attributs secondaires élevés (>= 16)
        // Max 2 traits secondaires

        if (attributes.Resilience >= 16)
            traits.Add("Resilient");

        if (attributes.Adaptability >= 16)
            traits.Add("Adaptable");

        if (attributes.SocialSkills >= 16)
            traits.Add("Charismatic");

        if (attributes.WorkEthic >= 16)
            traits.Add("Hardworking");

        if (attributes.Creativity >= 16)
            traits.Add("Creative");

        if (attributes.Temperament >= 17)
            traits.Add("Calm");

        if (attributes.Temperament <= 4)
            traits.Add("Hot-Headed");

        if (attributes.Loyalty >= 17)
            traits.Add("Devoted");

        // Prendre max 2 traits
        return traits.Take(2).ToList();
    }

    public MentalAttributes GenerateRandomMentalAttributes()
    {
        var attributes = new MentalAttributes
        {
            Professionalism = GenerateRandomAttribute(),
            Ambition = GenerateRandomAttribute(),
            Loyalty = GenerateRandomAttribute(),
            Ego = GenerateRandomAttribute(),
            Resilience = GenerateRandomAttribute(),
            Adaptability = GenerateRandomAttribute(),
            Creativity = GenerateRandomAttribute(),
            WorkEthic = GenerateRandomAttribute(),
            SocialSkills = GenerateRandomAttribute(),
            Temperament = GenerateRandomAttribute(),
            LastUpdated = DateTime.Now
        };

        return attributes;
    }

    private int GenerateRandomAttribute()
    {
        // Distribution: 70% entre 8-12 (balanced), 30% outliers

        var roll = _random.Next(100);

        if (roll < 70)
        {
            // Balanced range (8-12)
            return _random.Next(8, 13);
        }
        else
        {
            // Outliers: soit très bas (0-7) soit très haut (13-20)
            var isHigh = _random.Next(2) == 0;
            return isHigh ? _random.Next(13, 21) : _random.Next(0, 8);
        }
    }
}

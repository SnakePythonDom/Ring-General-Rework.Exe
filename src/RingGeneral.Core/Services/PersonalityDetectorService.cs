using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service responsible for detecting personality profiles based on mental attributes.
/// Uses a priority-based algorithm to assign the most appropriate profile.
/// </summary>
public sealed class PersonalityDetectorService
{
    /// <summary>
    /// Detect personality profile from mental attributes using priority-based logic.
    /// Higher priority profiles (dangerous/elite) are checked first.
    /// </summary>
    /// <param name="mental">Worker's mental attributes</param>
    /// <returns>Detected personality profile</returns>
    public PersonalityProfile DetectProfile(WorkerMentalAttributes mental)
    {
        // ===== PRIORITY 1: PROFILS DANGEREUX (must catch early) =====

        // Poids Mort - Everything low
        if (mental.Professionnalisme <= 5 && mental.Détermination <= 5 && mental.Ambition <= 5)
            return PersonalityProfile.PoidsMort;

        // Instable Chronique - Multiple critical failures
        if (mental.Tempérament <= 5 && mental.Pression <= 5 && mental.Professionnalisme <= 8)
            return PersonalityProfile.InstableChronique;

        // Saboteur Passif - Malicious combination
        if (mental.Sportivité <= 5 && mental.Égoïsme >= 15 && mental.Influence >= 10)
            return PersonalityProfile.SaboteurPassif;

        // ===== PRIORITY 2: PROFILS ÉLITES (highest standards) =====

        // Professionnel Exemplaire - The model worker
        if (mental.Professionnalisme >= 17 && mental.Sportivité >= 15 && mental.Tempérament >= 15)
            return PersonalityProfile.ProfessionnelExemplaire;

        // Citoyen Modèle - Selfless locker room pillar
        if (mental.Loyauté >= 17 && mental.Professionnalisme >= 15 && mental.Égoïsme <= 6)
            return PersonalityProfile.CitoyenModele;

        // Machine de Guerre - Indestructible workhorse
        if (mental.Détermination >= 18 && mental.Pression >= 17 && mental.Tempérament >= 15)
            return PersonalityProfile.MachineDeGuerre;

        // ===== PRIORITY 3: PROFILS TOXIQUES (problematic) =====

        // Diva - Worst combination of ego + temperament
        if (mental.Égoïsme >= 17 && mental.Tempérament <= 6 && mental.Professionnalisme <= 10)
            return PersonalityProfile.Diva;

        // Égoïste - High ego, low sportsmanship
        if (mental.Égoïsme >= 17 && mental.Sportivité <= 6)
            return PersonalityProfile.Égoïste;

        // Paresseux - No work ethic
        if (mental.Professionnalisme <= 6 && mental.Détermination <= 6)
            return PersonalityProfile.Paresseux;

        // ===== PRIORITY 4: PROFILS AMBITIEUX (driven stars) =====

        // Leader de Vestiaire - Influential, professional leader
        if (mental.Influence >= 17 && mental.Professionnalisme >= 13 && mental.Tempérament >= 13)
            return PersonalityProfile.LeaderDeVestiaire;

        // Ambitieux - Hunger for success
        if (mental.Ambition >= 17 && mental.Détermination >= 13 && mental.Égoïsme >= 10)
            return PersonalityProfile.Ambitieux;

        // Mercenaire - Money follower
        if (mental.Loyauté <= 6 && mental.Ambition >= 13 && mental.Égoïsme >= 13)
            return PersonalityProfile.Mercenaire;

        // ===== PRIORITY 5: PROFILS INSTABLES (unreliable) =====

        // Tempérament de Feu - Explosive but talented
        if (mental.Tempérament <= 6 && mental.Professionnalisme >= 10)
            return PersonalityProfile.TempéramentDeFeu;

        // Franc-Tireur - Unpredictable loose cannon
        if (mental.Adaptabilité >= 15 && mental.Tempérament <= 8 && mental.Sportivité <= 8)
            return PersonalityProfile.FrancTireur;

        // Inconstant - Can't handle pressure
        if (mental.Pression <= 8 && mental.Détermination <= 8)
            return PersonalityProfile.Inconstant;

        // ===== PRIORITY 6: PROFILS STRATÈGES (political operators) =====

        // Politicien - Backstage power player
        if (mental.Influence >= 17 && mental.Égoïsme >= 13 && mental.Tempérament >= 13)
            return PersonalityProfile.Politicien;

        // Vétéran Rusé - Political operator
        if (mental.Adaptabilité >= 15 && mental.Influence >= 13 && mental.Sportivité <= 10)
            return PersonalityProfile.VétéranRusé;

        // Maître du Storytelling - Master craftsman
        if (mental.Adaptabilité >= 17 && mental.Professionnalisme >= 13 && mental.Pression >= 13)
            return PersonalityProfile.MaîtreDuStorytelling;

        // ===== PRIORITY 7: PROFILS COMPÉTITION (workhorses) =====

        // Accro au Ring - Lives to wrestle
        if (mental.Détermination >= 17 && mental.Professionnalisme >= 15 && mental.Ambition >= 13)
            return PersonalityProfile.AccroAuRing;

        // Pilier Fiable - Company cornerstone
        if (mental.Loyauté >= 17 && mental.Pression >= 15 && mental.Professionnalisme >= 13)
            return PersonalityProfile.PilierFiable;

        // Déterminé - Never gives up
        if (mental.Détermination >= 17 && mental.Pression >= 15)
            return PersonalityProfile.Déterminé;

        // ===== PRIORITY 8: PROFILS MÉDIATIQUES (media personalities) =====

        // Aimant à Public - Natural crowd connection
        if (mental.Sportivité >= 17 && mental.Professionnalisme >= 15 && mental.Tempérament >= 13)
            return PersonalityProfile.AimantÀPublic;

        // Charismatique Imprévisible - Wild card
        if (mental.Adaptabilité >= 15 && mental.Tempérament <= 8 && mental.Ambition >= 13)
            return PersonalityProfile.CharismatiqueImprévisible;

        // Obsédé par l'Image - Celebrity wannabe
        if (mental.Ambition >= 15 && mental.Égoïsme >= 15 && mental.Professionnalisme <= 10)
            return PersonalityProfile.ObsédéParLImage;

        // ===== DEFAULT: ÉQUILIBRÉ OR NON DÉTERMINÉ =====

        // Équilibré - All attributes in 8-13 range (average)
        if (IsBalanced(mental))
            return PersonalityProfile.Équilibré;

        // Fallback if no profile matches
        return PersonalityProfile.NonDéterminé;
    }

    /// <summary>
    /// Check if all mental attributes are in the "balanced" range (8-13)
    /// </summary>
    private static bool IsBalanced(WorkerMentalAttributes mental)
    {
        return mental.Ambition >= 8 && mental.Ambition <= 13 &&
               mental.Loyauté >= 8 && mental.Loyauté <= 13 &&
               mental.Professionnalisme >= 8 && mental.Professionnalisme <= 13 &&
               mental.Pression >= 8 && mental.Pression <= 13 &&
               mental.Tempérament >= 8 && mental.Tempérament <= 13 &&
               mental.Égoïsme >= 8 && mental.Égoïsme <= 13 &&
               mental.Détermination >= 8 && mental.Détermination <= 13 &&
               mental.Adaptabilité >= 8 && mental.Adaptabilité <= 13 &&
               mental.Influence >= 8 && mental.Influence <= 13 &&
               mental.Sportivité >= 8 && mental.Sportivité <= 13;
    }

    /// <summary>
    /// Get display name with emoji for a personality profile
    /// </summary>
    public static string GetProfileDisplayName(PersonalityProfile profile)
    {
        return profile switch
        {
            PersonalityProfile.ProfessionnelExemplaire => "⭐ Professionnel Exemplaire",
            PersonalityProfile.CitoyenModele => "🏆 Citoyen Modèle",
            PersonalityProfile.Déterminé => "💪 Déterminé",
            PersonalityProfile.Ambitieux => "🚀 Ambitieux",
            PersonalityProfile.LeaderDeVestiaire => "👑 Leader de Vestiaire",
            PersonalityProfile.Mercenaire => "💰 Mercenaire",
            PersonalityProfile.TempéramentDeFeu => "🔥 Tempérament de Feu",
            PersonalityProfile.FrancTireur => "🎲 Franc-Tireur",
            PersonalityProfile.Inconstant => "📉 Inconstant",
            PersonalityProfile.Égoïste => "😈 Égoïste",
            PersonalityProfile.Diva => "👸 Diva",
            PersonalityProfile.Paresseux => "💤 Paresseux",
            PersonalityProfile.VétéranRusé => "🦊 Vétéran Rusé",
            PersonalityProfile.MaîtreDuStorytelling => "📖 Maître du Storytelling",
            PersonalityProfile.Politicien => "🎭 Politicien",
            PersonalityProfile.AccroAuRing => "🥊 Accro au Ring",
            PersonalityProfile.PilierFiable => "🛡️ Pilier Fiable",
            PersonalityProfile.MachineDeGuerre => "⚙️ Machine de Guerre",
            PersonalityProfile.ObsédéParLImage => "📸 Obsédé par l'Image",
            PersonalityProfile.CharismatiqueImprévisible => "⚡ Charismatique Imprévisible",
            PersonalityProfile.AimantÀPublic => "🌟 Aimant à Public",
            PersonalityProfile.SaboteurPassif => "🐍 Saboteur Passif",
            PersonalityProfile.InstableChronique => "💥 Instable Chronique",
            PersonalityProfile.PoidsMort => "⚠️ Poids Mort",
            PersonalityProfile.Équilibré => "📊 Équilibré",
            _ => "❓ Non Déterminé"
        };
    }

    /// <summary>
    /// Get short description for a personality profile
    /// </summary>
    public static string GetProfileDescription(PersonalityProfile profile)
    {
        return profile switch
        {
            PersonalityProfile.ProfessionnelExemplaire =>
                "Le worker modèle. Professionnalisme exemplaire, fiable sous pression, respectueux et respecté.",
            PersonalityProfile.CitoyenModele =>
                "Pilier du vestiaire, loyal et peu égoïste. Met toujours l'entreprise avant son ego.",
            PersonalityProfile.Déterminé =>
                "Ne renonce jamais. Déterminé et fiable dans les grands moments.",
            PersonalityProfile.Ambitieux =>
                "Déterminé à atteindre le sommet. Ambition forte et détermination sans faille.",
            PersonalityProfile.LeaderDeVestiaire =>
                "Leader respecté. Influence positive, professionnel et calme.",
            PersonalityProfile.Mercenaire =>
                "Suit l'argent. Aucune loyauté, partira pour une meilleure offre.",
            PersonalityProfile.TempéramentDeFeu =>
                "Explosif mais talentueux. Risque de conflits backstage mais performances solides.",
            PersonalityProfile.FrancTireur =>
                "Imprévisible. Créatif et adaptable mais chaotique.",
            PersonalityProfile.Inconstant =>
                "Performances erratiques. Ne peut pas être compté dans les grands moments.",
            PersonalityProfile.Égoïste =>
                "Très égocentrique. Difficile à convaincre de perdre ou mettre over les autres.",
            PersonalityProfile.Diva =>
                "Problèmes constants backstage. Égo démesuré et mauvais tempérament.",
            PersonalityProfile.Paresseux =>
                "Aucune éthique de travail. Fait le minimum, ne s'améliore pas.",
            PersonalityProfile.VétéranRusé =>
                "Opérateur politique backstage. Adaptable et influent, mais peu fair-play.",
            PersonalityProfile.MaîtreDuStorytelling =>
                "Maître de la psychologie ring. Professionnel et fiable sous pression.",
            PersonalityProfile.Politicien =>
                "Joue les coulisses. Influence majeure, peut être difficile à gérer.",
            PersonalityProfile.AccroAuRing =>
                "Vit pour wrestler. Déterminé, professionnel et ambitieux.",
            PersonalityProfile.PilierFiable =>
                "Pierre angulaire de l'entreprise. Loyal, fiable, professionnel.",
            PersonalityProfile.MachineDeGuerre =>
                "Indestructible. Déterminé, fiable sous pression, tempérament d'acier.",
            PersonalityProfile.ObsédéParLImage =>
                "Veut la célébrité plus que l'excellence ring. Ambitieux mais peu professionnel.",
            PersonalityProfile.CharismatiqueImprévisible =>
                "Wild card charismatique. Peut être brillant ou désastreux.",
            PersonalityProfile.AimantÀPublic =>
                "Connexion naturelle avec la foule. Fair-play, professionnel, calme.",
            PersonalityProfile.SaboteurPassif =>
                "Dangereux backstage. Utilise son influence pour saboter les autres.",
            PersonalityProfile.InstableChronique =>
                "Risque constant. Non fiable, explosif, peu professionnel.",
            PersonalityProfile.PoidsMort =>
                "Aucun intérêt. Pas professionnel, pas déterminé, pas ambitieux.",
            PersonalityProfile.Équilibré =>
                "Profil standard. Aucun trait dominant positif ou négatif.",
            _ => "Profil en cours d'analyse."
        };
    }
}

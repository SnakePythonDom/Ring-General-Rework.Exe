using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Core.Services
{
    public static class PersonalityGenerator
    {
        public static PersonalityProfile DeterminePersonality(WorkerMentalAttributes attributes)
        {
            if (attributes == null) return PersonalityProfile.NonDéterminé;

            // LES ÉLITES
            if (attributes.Professionnalisme >= 17 && attributes.Sportivité >= 15 && attributes.Tempérament >= 15)
                return PersonalityProfile.ProfessionnelExemplaire;

            if (attributes.Loyauté >= 17 && attributes.Professionnalisme >= 15 && attributes.Égoïsme < 6)
                return PersonalityProfile.CitoyenModele;

            if (attributes.Détermination >= 17 && attributes.Pression >= 15)
                return PersonalityProfile.Déterminé;

            // LES STARS À ÉGO
            if (attributes.Ambition >= 17 && attributes.Détermination >= 13 && attributes.Égoïsme >= 10)
                return PersonalityProfile.Ambitieux;

            if (attributes.Influence >= 17 && attributes.Professionnalisme >= 13 && attributes.Tempérament >= 13)
                return PersonalityProfile.LeaderDeVestiaire;

            if (attributes.Loyauté < 6 && attributes.Ambition >= 13 && attributes.Égoïsme >= 13)
                return PersonalityProfile.Mercenaire;

            // LES INSTABLES
            if (attributes.Tempérament < 6 && attributes.Professionnalisme > 10)
                return PersonalityProfile.TempéramentDeFeu;

            if (attributes.Adaptabilité >= 15 && attributes.Tempérament < 8 && attributes.Sportivité < 8)
                return PersonalityProfile.FrancTireur;

            if (attributes.Pression < 8 && attributes.Détermination < 8)
                return PersonalityProfile.Inconstant;

            // LES TOXIQUES
            if (attributes.Égoïsme >= 17 && attributes.Sportivité < 6)
                return PersonalityProfile.Égoïste;

            if (attributes.Égoïsme >= 17 && attributes.Tempérament < 6 && attributes.Professionnalisme < 10)
                return PersonalityProfile.Diva;

            if (attributes.Professionnalisme < 6 && attributes.Détermination < 6)
                return PersonalityProfile.Paresseux;

            // LES STRATÈGES
            if (attributes.Adaptabilité >= 15 && attributes.Influence >= 13 && attributes.Sportivité < 10)
                return PersonalityProfile.VétéranRusé;

            if (attributes.Adaptabilité >= 17 && attributes.Professionnalisme >= 13 && attributes.Pression >= 13)
                return PersonalityProfile.MaîtreDuStorytelling;

            if (attributes.Influence >= 17 && attributes.Égoïsme >= 13 && attributes.Tempérament >= 13)
                return PersonalityProfile.Politicien;

            // LES BÊTES DE COMPÉTITION
            if (attributes.Détermination >= 17 && attributes.Professionnalisme >= 15 && attributes.Ambition >= 13)
                return PersonalityProfile.AccroAuRing;

            if (attributes.Loyauté >= 17 && attributes.Pression >= 15 && attributes.Professionnalisme >= 13)
                return PersonalityProfile.PilierFiable;

            if (attributes.Détermination >= 18 && attributes.Pression >= 17 && attributes.Tempérament >= 15)
                return PersonalityProfile.MachineDeGuerre;

            // LES CRÉATURES MÉDIATIQUES
            if (attributes.Ambition >= 15 && attributes.Égoïsme >= 15 && attributes.Professionnalisme < 10)
                return PersonalityProfile.ObsédéParLImage;

            if (attributes.Adaptabilité >= 15 && attributes.Tempérament < 8 && attributes.Ambition >= 13)
                return PersonalityProfile.CharismatiqueImprévisible;

            if (attributes.Sportivité >= 17 && attributes.Professionnalisme >= 15 && attributes.Tempérament >= 13)
                return PersonalityProfile.AimantÀPublic;

            // LES PROFILS DANGEREUX
            if (attributes.Sportivité < 5 && attributes.Égoïsme >= 15 && attributes.Influence >= 10)
                return PersonalityProfile.SaboteurPassif;

            if (attributes.Tempérament < 5 && attributes.Pression < 5 && attributes.Professionnalisme < 8)
                return PersonalityProfile.InstableChronique;

            if (attributes.Professionnalisme < 5 && attributes.Détermination < 5 && attributes.Ambition < 5)
                return PersonalityProfile.PoidsMort;

            // PROFILS PAR DÉFAUT
            // If all attributes are moderate (8-13), assign Balanced
            if (IsBalanced(attributes))
                return PersonalityProfile.Équilibré;

            return PersonalityProfile.NonDéterminé;
        }

        private static bool IsBalanced(WorkerMentalAttributes a)
        {
            // Helper check: All main attributes within 8-13 range
            var attrs = new[] { a.Ambition, a.Loyauté, a.Professionnalisme, a.Pression, a.Tempérament, a.Égoïsme, a.Détermination, a.Adaptabilité, a.Influence, a.Sportivité };
            foreach (var val in attrs)
            {
                if (val < 8 || val > 13) return false;
            }
            return true;
        }
    }
}

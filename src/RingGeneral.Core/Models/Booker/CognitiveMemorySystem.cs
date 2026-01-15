using System;
using RingGeneral.Core.Models.Booker; // Nécessaire pour utiliser BookerMemory

namespace RingGeneral.Core.Services.Booker;

/// <summary>
/// Service responsable de la gestion cognitive et de l'évolution de la mémoire des bookers.
/// Sépare la logique de calcul (le cerveau) des données brutes (le modèle).
/// </summary>
public class CognitiveMemorySystem
{
    /// <summary>
    /// Traite une mémoire pour une semaine de jeu, appliquant un oubli (decay) variable
    /// selon l'impact émotionnel de l'événement.
    /// </summary>
    /// <param name="memory">L'objet mémoire à mettre à jour</param>
    /// <returns>Une nouvelle instance de BookerMemory avec le RecallStrength mis à jour</returns>
    public BookerMemory ProcessWeeklyDecay(BookerMemory memory)
    {
        // LOGIQUE : Plus l'événement est impactant, plus l'oubli est lent.
        int decayAmount;
        int absImpact = Math.Abs(memory.ImpactScore);

        if (absImpact >= 80)
        {
            // Trauma ou Moment Légendaire : Reste gravé très longtemps (~2 ans)
            decayAmount = 1; 
        }
        else if (absImpact >= 50)
        {
            // Événement Majeur : Reste en mémoire quelques mois (~8 mois)
            decayAmount = 3; 
        }
        else
        {
            // Routine : Oublié rapidement (~4-5 mois)
            decayAmount = 5; 
        }

        // Utilise la méthode définie dans votre modèle BookerMemory (Record = immuable)
        return memory.ApplyDecay(decayAmount);
    }
}

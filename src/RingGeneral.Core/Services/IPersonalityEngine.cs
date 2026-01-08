using RingGeneral.Core.Models;
using RingGeneral.Core.Enums;
using System.Collections.Generic;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de calcul et évolution des personnalités.
/// Transforme les attributs mentaux cachés en labels visibles.
/// </summary>
public interface IPersonalityEngine
{
    /// <summary>
    /// Calcule le label de personnalité basé sur les attributs mentaux.
    /// IMPORTANT: Le label est incomplet et interprétatif par design.
    /// </summary>
    /// <param name="attributes">Attributs mentaux cachés</param>
    /// <returns>Label de personnalité calculé</returns>
    string CalculatePersonalityLabel(MentalAttributes attributes);

    /// <summary>
    /// Met à jour les attributs mentaux suite à un événement.
    /// </summary>
    /// <param name="attributes">Attributs actuels (seront modifiés)</param>
    /// <param name="eventType">Type d'événement (MainEventPush, PushFailed, etc.)</param>
    /// <param name="intensity">Intensité de l'impact (1-5)</param>
    void UpdateMentalAttributes(
        MentalAttributes attributes,
        string eventType,
        int intensity);

    /// <summary>
    /// Vérifie si un changement de personnalité est nécessaire.
    /// </summary>
    /// <param name="currentAttributes">Attributs mentaux actuels</param>
    /// <param name="currentPersonality">Personnalité actuelle</param>
    /// <returns>True si changement nécessaire</returns>
    bool ShouldPersonalityChange(
        MentalAttributes currentAttributes,
        Personality currentPersonality);

    /// <summary>
    /// Génère des traits secondaires basés sur les attributs.
    /// Maximum 2 traits retournés.
    /// </summary>
    /// <param name="attributes">Attributs mentaux</param>
    /// <returns>Liste de traits secondaires (max 2)</returns>
    List<string> GenerateSecondaryTraits(MentalAttributes attributes);

    /// <summary>
    /// Génère des attributs mentaux aléatoires pour une nouvelle entité.
    /// Distribution: 70% entre 8-12 (balanced), 30% outliers (0-7 ou 13-20)
    /// </summary>
    /// <returns>Nouveaux attributs mentaux aléatoires</returns>
    MentalAttributes GenerateRandomMentalAttributes();
}

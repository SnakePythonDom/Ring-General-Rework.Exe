using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le service de gestion des titres de championnat.
/// Gère les règnes, les changements de champions et le prestige des titres.
/// </summary>
public interface ITitleService
{
    /// <summary>
    /// Enregistre le résultat d'un match de titre
    /// Met à jour le champion, les règnes et le prestige du titre
    /// </summary>
    /// <param name="input">Données du match de titre</param>
    /// <returns>Résultat du match incluant si le titre a changé et le delta de prestige</returns>
    TitleMatchOutcome EnregistrerMatch(TitleMatchInput input);
}
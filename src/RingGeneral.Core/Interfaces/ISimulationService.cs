using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface consolidée pour tous les services de simulation et validation
/// Regroupe la simulation hebdomadaire et la validation des bookings
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Simule une semaine complète pour toutes les compagnies
    /// </summary>
    /// <param name="context">Contexte de simulation</param>
    /// <param name="options">Options de simulation</param>
    /// <returns>Résultat de la simulation</returns>
    SimulationResult SimulerSemaine(SimulationContext context, SimulationOptions options);

    /// <summary>
    /// Valide un plan de booking avant exécution
    /// </summary>
    /// <param name="plan">Plan de booking à valider</param>
    /// <returns>Résultat de validation</returns>
    ValidationResult ValiderBooking(BookingPlan plan);
}

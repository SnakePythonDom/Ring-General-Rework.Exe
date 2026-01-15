using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Validation;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service consolidé pour la simulation et validation.
/// Combine la simulation hebdomadaire et la validation des bookings.
/// </summary>
public sealed class SimulationService : ISimulationService
{
    private readonly IRandomProvider? _randomProvider;

    public SimulationService(IRandomProvider? randomProvider = null)
    {
        _randomProvider = randomProvider;
    }

    /// <summary>
    /// Simule une semaine complète pour toutes les compagnies
    /// </summary>
    /// <param name="context">Contexte de simulation</param>
    /// <param name="options">Options de simulation</param>
    /// <returns>Résultat de la simulation</returns>
    public SimulationResult SimulerSemaine(SimulationContext context, SimulationOptions options)
    {
        // Implémentation basique - à développer selon les besoins
        // Pour l'instant, retourne un résultat vide
        return new SimulationResult(
            Reussite: true,
            RapportHebdo: "Simulation hebdomadaire - Fonctionnalité à implémenter",
            Segments: new List<SegmentResult>());
    }

    /// <summary>
    /// Valide un plan de booking avant exécution
    /// </summary>
    /// <param name="plan">Plan de booking à valider</param>
    /// <returns>Résultat de validation</returns>
    public ValidationResult ValiderBooking(BookingPlan plan)
    {
        var validator = new BookingValidator();
        return validator.ValiderBooking(plan);
    }
}
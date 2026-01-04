using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface ISimulationEngine
{
    SimulationResult SimulerSemaine(SimulationContext context, SimulationOptions options);
}

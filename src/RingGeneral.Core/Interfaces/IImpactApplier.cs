using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IImpactApplier
{
    ImpactReport AppliquerImpacts(ImpactContext context);
}

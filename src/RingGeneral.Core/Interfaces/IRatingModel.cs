using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IRatingModel
{
    SegmentRating EvaluerSegment(SegmentSimulationContext context);
}

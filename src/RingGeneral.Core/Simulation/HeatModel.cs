using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class HeatModel
{
    public int CalculerDeltaSegment(SegmentDefinition segment, int note, int momentumMoyen)
    {
        if (string.IsNullOrWhiteSpace(segment.StorylineId))
        {
            return 0;
        }

        var qualiteDelta = note switch
        {
            >= 85 => 2,
            >= 70 => 1,
            >= 55 => 0,
            >= 40 => -1,
            _ => -2
        };

        var momentumDelta = momentumMoyen switch
        {
            >= 70 => 1,
            <= 30 => -1,
            _ => 0
        };

        var mainEventBonus = segment.EstMainEvent ? 1 : 0;
        var delta = 1 + qualiteDelta + momentumDelta + mainEventBonus;

        return Math.Clamp(delta, -3, 4);
    }

    public Dictionary<string, int> AppliquerSegment(
        SegmentDefinition segment,
        int note,
        int momentumMoyen,
        ISet<string> storylinesUtilisees,
        IDictionary<string, int> storylineHeatDelta)
    {
        var deltaLocal = new Dictionary<string, int>();
        if (string.IsNullOrWhiteSpace(segment.StorylineId))
        {
            return deltaLocal;
        }

        storylinesUtilisees.Add(segment.StorylineId);

        var delta = CalculerDeltaSegment(segment, note, momentumMoyen);
        if (delta == 0)
        {
            return deltaLocal;
        }

        storylineHeatDelta[segment.StorylineId] = storylineHeatDelta.TryGetValue(segment.StorylineId, out var total)
            ? total + delta
            : delta;
        deltaLocal[segment.StorylineId] = delta;
        return deltaLocal;
    }

    public void AppliquerOubliStorylines(
        IReadOnlyList<StorylineInfo> storylines,
        ISet<string> storylinesUtilisees,
        IDictionary<string, int> storylineHeatDelta)
    {
        foreach (var storyline in storylines)
        {
            if (storylinesUtilisees.Contains(storyline.StorylineId))
            {
                continue;
            }

            storylineHeatDelta[storyline.StorylineId] = storylineHeatDelta.TryGetValue(storyline.StorylineId, out var total)
                ? total - 1
                : -1;
        }
    }
}

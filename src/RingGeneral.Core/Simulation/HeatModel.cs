namespace RingGeneral.Core.Simulation;

public sealed class HeatModel
{
    public int CalculerDeltaSegment(int noteSegment, int segmentsPrecedents)
    {
        var baseGain = segmentsPrecedents == 0 ? 2 : 1;
        var bonusQualite = noteSegment switch
        {
            >= 80 => 2,
            >= 65 => 1,
            >= 50 => 0,
            _ => -1
        };

        return Math.Clamp(baseGain + bonusQualite, -2, 4);
    }

    public int CalculerDeltaInactif()
    {
        return -2;
    }
}

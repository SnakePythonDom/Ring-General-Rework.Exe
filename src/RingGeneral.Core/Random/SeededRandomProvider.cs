using RingGeneral.Core.Interfaces;

namespace RingGeneral.Core.Random;

public sealed class SeededRandomProvider : IRandomProvider
{
    private System.Random _random;

    public SeededRandomProvider(int seed)
    {
        _random = new System.Random(seed);
    }

    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

    public double NextDouble() => _random.NextDouble();

    public void Reseed(int seed)
    {
        _random = new System.Random(seed);
    }
}

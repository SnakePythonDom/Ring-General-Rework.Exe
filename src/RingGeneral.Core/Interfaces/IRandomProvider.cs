namespace RingGeneral.Core.Interfaces;

public interface IRandomProvider
{
    int Next(int minInclusive, int maxExclusive);
    double NextDouble();
    void Reseed(int seed);
}

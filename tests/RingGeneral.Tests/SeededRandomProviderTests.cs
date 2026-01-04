using RingGeneral.Core.Random;
using Xunit;

namespace RingGeneral.Tests;

public sealed class SeededRandomProviderTests
{
    [Fact]
    public void Reseed_reproduit_les_valeurs()
    {
        var provider = new SeededRandomProvider(42);
        var premiere = provider.Next(0, 1000);
        provider.Reseed(42);
        var seconde = provider.Next(0, 1000);

        Assert.Equal(premiere, seconde);
    }
}

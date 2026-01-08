namespace RingGeneral.Core.Models;

public sealed record RegionInfo(string RegionId, string RegionName, string CountryName)
{
    public override string ToString() => $"{RegionName}, {CountryName}";
}

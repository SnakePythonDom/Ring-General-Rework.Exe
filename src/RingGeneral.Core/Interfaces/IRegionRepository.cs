namespace RingGeneral.Core.Interfaces;

public interface IRegionRepository
{
    System.Collections.Generic.IReadOnlyList<RegionSelection> GetRegions();
}

public sealed record RegionSelection(string RegionId, string RegionName, string CountryName);

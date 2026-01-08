namespace RingGeneral.Data.Repositories;

public interface IRegionRepository
{
    IReadOnlyList<RegionSelection> GetRegions();
}

using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

public interface IRegionRepository
{
    Task<IReadOnlyList<RegionInfo>> GetAllAsync();
}

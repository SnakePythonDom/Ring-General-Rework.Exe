using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

public interface IShowReadinessService
{
    Task<ReadinessResult> CheckReadinessAsync(string companyId);
}

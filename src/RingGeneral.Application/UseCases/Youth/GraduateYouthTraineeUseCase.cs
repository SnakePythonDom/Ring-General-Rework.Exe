using RingGeneral.Core.Interfaces;
using System.Threading.Tasks;

namespace RingGeneral.Application.UseCases.Youth;

public sealed class GraduateYouthTraineeUseCase
{
    private readonly IYouthRepository _youthRepository;

    public GraduateYouthTraineeUseCase(IYouthRepository youthRepository)
    {
        _youthRepository = youthRepository;
    }

    public async Task ExecuteAsync(string workerId, int currentWeek)
    {
        await Task.Run(() => _youthRepository.DiplomerTrainee(workerId, currentWeek));
    }
}

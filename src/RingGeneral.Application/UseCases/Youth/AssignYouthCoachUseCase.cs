using RingGeneral.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Application.UseCases.Youth;

public sealed class AssignYouthCoachUseCase
{
    private readonly IYouthRepository _youthRepository;

    public AssignYouthCoachUseCase(IYouthRepository youthRepository)
    {
        _youthRepository = youthRepository;
    }

    public async Task ExecuteAsync(string structureId, string workerId, string role, int currentWeek)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(structureId) || string.IsNullOrWhiteSpace(workerId))
            return;

        await Task.Run(() => _youthRepository.AffecterCoachYouth(structureId, workerId, role, currentWeek));
    }
}

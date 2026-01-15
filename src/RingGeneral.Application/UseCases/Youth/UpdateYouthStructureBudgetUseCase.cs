using RingGeneral.Core.Interfaces;
using System.Threading.Tasks;

namespace RingGeneral.Application.UseCases.Youth;

public sealed class UpdateYouthStructureBudgetUseCase
{
    private readonly IYouthRepository _youthRepository;

    public UpdateYouthStructureBudgetUseCase(IYouthRepository youthRepository)
    {
        _youthRepository = youthRepository;
    }

    public async Task ExecuteAsync(string youthId, int newBudget)
    {
        // Validation logic can go here (e.g. min budget)
        if (newBudget < 0) return;

        await Task.Run(() => _youthRepository.ChangerBudgetYouth(youthId, newBudget));
    }
}

using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System.Threading.Tasks;

namespace RingGeneral.Application.UseCases.Youth;

public sealed class UpgradeStructureEquipmentUseCase
{
    private readonly IYouthRepository _youthRepository;

    public UpgradeStructureEquipmentUseCase(IYouthRepository youthRepository)
    {
        _youthRepository = youthRepository;
    }

    public async Task<bool> ExecuteAsync(string youthId, int currentLevel)
    {
        // 1. Validation (Rules)
        if (currentLevel >= 5)
        {
            return false;
        }

        // 2. Cost Calculation (Domain Logic)
        // Coût : 50k * niveau actuel
        // In a real Clean Architecture, this cost logic might be in a Domain Service or Aggregate
        // For now, mirroring existing logic.
        var cost = 50_000 * currentLevel;

        // TODO: Check and deduct funds from Company (GameRepository logic equivalent)
        // Assuming unlimited funds or handled elsewhere for this legacy refactor step as per original comment:
        // "// TODO: Vérifier fonds compagnie via GameRepository // Pour l'instant on suppose infini ou géré ailleurs"

        // 3. Persistence
        await Task.Run(() => _youthRepository.AmeliorerEquipements(youthId));

        return true;
    }
}

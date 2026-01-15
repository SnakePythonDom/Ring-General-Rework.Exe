using RingGeneral.Application.UseCases.Youth;
using RingGeneral.Core.Interfaces;
using System.Threading.Tasks;

namespace RingGeneral.Application.Facades;

public sealed class YouthFacade
{
    private readonly UpgradeStructureEquipmentUseCase _upgradeEquipmentUseCase;
    private readonly CreateYouthStructureUseCase _createStructureUseCase;
    private readonly UpdateYouthStructureBudgetUseCase _updateBudgetUseCase;
    private readonly AssignYouthCoachUseCase _assignCoachUseCase;
    private readonly GenerateYouthTraineesUseCase _generateTraineesUseCase;
    private readonly GraduateYouthTraineeUseCase _graduateTraineeUseCase;
    // Keep reference to repo for read operations that are not yet migrated to Use Cases
    public IYouthRepository Repository { get; }

    public YouthFacade(
        IYouthRepository youthRepository,
        IWorkerGenerationService generationService,
        IGameRepository gameRepository)
    {
        Repository = youthRepository;
        _upgradeEquipmentUseCase = new UpgradeStructureEquipmentUseCase(youthRepository);
        _createStructureUseCase = new CreateYouthStructureUseCase(youthRepository, generationService, gameRepository);
        _updateBudgetUseCase = new UpdateYouthStructureBudgetUseCase(youthRepository);
        _assignCoachUseCase = new AssignYouthCoachUseCase(youthRepository);
        _generateTraineesUseCase = new GenerateYouthTraineesUseCase(youthRepository, generationService, gameRepository);
        _graduateTraineeUseCase = new GraduateYouthTraineeUseCase(youthRepository);
    }

    public async Task<bool> UpgradeEquipmentAsync(string youthId, int currentLevel)
    {
        return await _upgradeEquipmentUseCase.ExecuteAsync(youthId, currentLevel);
    }

    public async Task<string> CreateStructureAsync(
        string companyId,
        string name,
        string? regionId,
        string type,
        decimal budget,
        int capacity,
        string philosophy,
        string genderPreference,
        string specializationPreference)
    {
        return await _createStructureUseCase.ExecuteAsync(
            companyId, name, regionId, type, budget, capacity,
            philosophy, genderPreference, specializationPreference);
    }

    public async Task UpdateBudgetAsync(string youthId, int newBudget)
    {
        await _updateBudgetUseCase.ExecuteAsync(youthId, newBudget);
    }

    public async Task AssignCoachAsync(string structureId, string workerId, string role, int currentWeek)
    {
        await _assignCoachUseCase.ExecuteAsync(structureId, workerId, role, currentWeek);
    }

    public async Task<int> GenerateTraineesAsync(string mode)
    {
        return await _generateTraineesUseCase.ExecuteAsync(mode);
    }

    public async Task GraduateTraineeAsync(string workerId, int currentWeek)
    {
        await _graduateTraineeUseCase.ExecuteAsync(workerId, currentWeek);
    }
}

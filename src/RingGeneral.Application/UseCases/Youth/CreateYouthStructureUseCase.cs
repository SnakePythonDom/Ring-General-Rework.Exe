using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Application.UseCases.Youth;

public sealed class CreateYouthStructureUseCase
{
    private readonly IYouthRepository _youthRepository;
    private readonly IWorkerGenerationService _generationService;
    private readonly IGameRepository _gameRepository;

    public CreateYouthStructureUseCase(
        IYouthRepository youthRepository,
        IWorkerGenerationService generationService,
        IGameRepository gameRepository)
    {
        _youthRepository = youthRepository;
        _generationService = generationService;
        _gameRepository = gameRepository;
    }

    public async Task<string> ExecuteAsync(
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
        var structureId = $"YS-{Guid.NewGuid():N}".ToUpperInvariant();

        // 1. Create the structure in DB
        await _youthRepository.CreateYouthStructureAsync(
            structureId,
            companyId,
            name,
            regionId,
            type,
            budget,
            capacity,
            1, // Level 1
            50, // Coaching 50
            philosophy,
            genderPreference,
            specializationPreference
        );

        // 2. Initial spawn: 2-3 trainees
        var random = new Random();
        var count = random.Next(2, 4);
        var currentWeek = 1; // Default

        // Try to get real week if possible, otherwise fallback
        // Since IGameRepository doesn't have a direct GetCurrentWeek, 
        // this is a simplified version. In a real scenario, we'd fetch it from state.

        var youthState = new YouthStructureState(
            structureId, name, companyId, regionId, type,
            (int)budget, capacity, 1, 50, philosophy,
            genderPreference, specializationPreference, true, null, 0);

        var report = _generationService.GenerateInitial(youthState, count, currentWeek, random.Next());

        // 3. Register the generated workers as trainees
        if (report.Workers.Count > 0)
        {
            await _youthRepository.EnregistrerGeneration(report.Workers, structureId, currentWeek);
        }

        return name;
    }
}

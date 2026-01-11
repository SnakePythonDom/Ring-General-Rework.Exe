using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IWorkerGenerationService
{
    WorkerGenerationReport GenerateWeekly(GameState state, int seed);
    YouthProgressionReport CalculerProgressionHebdomadaire(IEnumerable<YouthTraineeProgressionState> trainees, int semaine, int seed);
}

using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

public interface IYouthRepository
{
    IReadOnlyList<YouthStructureState> ChargerYouthStructuresPourGeneration();
    IReadOnlyList<YouthStructureState> ChargerYouthStructures();
    IReadOnlyList<YouthTraineeInfo> ChargerYouthTrainees(string youthId);
    IReadOnlyList<YouthProgramInfo> ChargerYouthPrograms(string youthId);
    IReadOnlyList<YouthStaffAssignmentInfo> ChargerYouthStaffAssignments(string youthId);
    Task CreateYouthStructureAsync(string youthStructureId, string companyId, string name, string? regionId, string type, decimal budgetAnnuel, int capaciteMax, int niveauEquipements, int qualiteCoaching, string philosophie, string genderPreference, string specializationPreference);
    void ChangerBudgetYouth(string youthId, int nouveauBudget);
    void AmeliorerEquipements(string youthId);
    void AffecterCoachYouth(string youthId, string workerId, string role, int semaine);
    void DiplomerTrainee(string workerId, int semaine);
    void LicencierTrainee(string workerId, int semaine);
    Task CreateTraineeAsync(string youthId, string name, int age, int potential, int progress);
    IReadOnlyList<YouthTraineeProgressionState> ChargerYouthTraineesPourProgression();
    void EnregistrerProgressionTrainees(YouthProgressionReport report);
    GenerationCounters ChargerGenerationCounters(int annee);
    void EnregistrerGeneration(WorkerGenerationReport report);
    Task EnregistrerGeneration(IEnumerable<GeneratedWorker> workers, string structureId, int semaine);
    IReadOnlyList<WorkerBackstageProfile> ChargerWorkersDisposPourStaff(string companyId);
    void DeleteStructure(string youthId);
}

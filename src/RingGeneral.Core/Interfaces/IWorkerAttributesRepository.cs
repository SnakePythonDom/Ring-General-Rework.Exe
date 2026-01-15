using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des attributs de workers
/// </summary>
public interface IWorkerAttributesRepository
{
    WorkerInRingAttributes? GetInRingAttributes(int workerId);
    void SaveInRingAttributes(WorkerInRingAttributes attributes);
    void UpdateInRingAttribute(int workerId, string attributeName, int value);
    
    WorkerEntertainmentAttributes? GetEntertainmentAttributes(int workerId);
    void SaveEntertainmentAttributes(WorkerEntertainmentAttributes attributes);
    void UpdateEntertainmentAttribute(int workerId, string attributeName, int value);
    
    WorkerStoryAttributes? GetStoryAttributes(int workerId);
    void SaveStoryAttributes(WorkerStoryAttributes attributes);
    void UpdateStoryAttribute(int workerId, string attributeName, int value);
    
    (WorkerInRingAttributes? InRing, WorkerEntertainmentAttributes? Entertainment, WorkerStoryAttributes? Story) GetAllAttributes(int workerId);
    void InitializeDefaultAttributes(int workerId);
    void DeleteAllAttributes(int workerId);
    
    System.Collections.Generic.List<int> GetWorkersByInRingAvg(int minAvg);
    System.Collections.Generic.List<int> GetWorkersByEntertainmentAvg(int minAvg);
    System.Collections.Generic.List<int> GetWorkersByStoryAvg(int minAvg);
    
    WorkerMentalAttributes? GetMentalAttributes(int workerId);
    void SaveMentalAttributes(WorkerMentalAttributes attributes);
    void UpdateMentalAttribute(int workerId, string attributeName, int value);
    void RevealMentalAttributes(int workerId, int scoutingLevel);
    void InitializeDefaultMentalAttributes(int workerId);
    System.Collections.Generic.List<int> GetWorkersByProfessionnalismeScore(double minScore);
    System.Collections.Generic.List<int> GetWorkersByHighEgo(int minEgo);
}

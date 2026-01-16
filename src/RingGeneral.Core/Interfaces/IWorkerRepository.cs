using RingGeneral.Core.Models;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

public interface IWorkerRepository
{
    Worker? GetWorker(string id);
    Worker? GetWorker(int id);
    void UpdateWorker(Worker worker);
    IReadOnlyList<WorkerSnapshot> ChargerWorkers(List<string> workerIds);
    WorkerSnapshot? ChargerWorker(string workerId);
    List<Worker> GetCompanyRoster(string companyId);

    // Add other common methods if needed
}

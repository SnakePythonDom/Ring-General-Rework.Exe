using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IContenderRepository
{
    TitleDetail? ChargerTitre(string titleId);
    IReadOnlyList<WorkerSnapshot> ChargerWorkersCompagnie(string companyId);
    void EnregistrerClassement(string titleId, IReadOnlyList<ContenderRanking> classements);
}

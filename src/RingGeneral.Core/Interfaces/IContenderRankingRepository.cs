using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IContenderRankingRepository
{
    void RemplacerClassement(string titleId, int week, IReadOnlyList<ContenderRankingEntry> entries);
    IReadOnlyList<ContenderRankingEntry> ChargerClassement(string titleId);
}

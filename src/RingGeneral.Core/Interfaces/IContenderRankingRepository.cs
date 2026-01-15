using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IContenderRankingRepository
{
    void RemplacerClassement(string titleId, int week, IReadOnlyList<ContenderRanking> entries);
    IReadOnlyList<ContenderRanking> ChargerClassement(string titleId);
}

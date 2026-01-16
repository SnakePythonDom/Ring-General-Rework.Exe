using RingGeneral.Core.Models.Relations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces
{
    public interface IFactionRepository
    {
        IReadOnlyList<Faction> GetAllFactions();
        Faction? GetFaction(int id);
        IReadOnlyList<FactionMember> GetFactionMembers(int factionId);
        IReadOnlyList<Faction> GetFactionsForWorker(string workerId);

        void AddFaction(Faction faction);
        void UpdateFaction(Faction faction);
        void DeleteFaction(int id);

        void AddMember(FactionMember member);
        void UpdateMember(FactionMember member);
        void RemoveMember(int memberId);
    }
}

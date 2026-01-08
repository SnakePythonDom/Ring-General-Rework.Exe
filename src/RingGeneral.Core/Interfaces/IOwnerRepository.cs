using RingGeneral.Core.Models.Owner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des propriétaires (Phase 4).
/// Gère les owners et leurs décisions stratégiques.
/// </summary>
public interface IOwnerRepository
{
    Task<Owner?> GetOwnerByIdAsync(string ownerId);
    Task<Owner?> GetOwnerByCompanyIdAsync(string companyId);
    Task<List<Owner>> GetAllOwnersAsync();
    Task SaveOwnerAsync(Owner owner);
    Task UpdateOwnerAsync(Owner owner);
}

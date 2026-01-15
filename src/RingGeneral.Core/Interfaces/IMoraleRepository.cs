using RingGeneral.Core.Models.Morale;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository du moral (Phase 3).
/// Gère le moral backstage et le moral de compagnie.
/// </summary>
public interface IMoraleRepository
{
    Task<BackstageMorale?> GetBackstageMoraleAsync(string entityId, string companyId);
    Task<List<BackstageMorale>> GetAllBackstageMoraleAsync(string companyId);
    Task<List<BackstageMorale>> GetLowMoraleEntitiesAsync(string companyId, int threshold);
    Task<List<BackstageMorale>> GetCriticalMoraleEntitiesAsync(string companyId, int threshold);
    Task SaveBackstageMoraleAsync(BackstageMorale morale);
    Task UpdateBackstageMoraleAsync(BackstageMorale morale);

    Task<CompanyMorale?> GetCompanyMoraleAsync(string companyId);
    Task SaveCompanyMoraleAsync(CompanyMorale morale);
    Task RecalculateCompanyMoraleAsync(string companyId);
}

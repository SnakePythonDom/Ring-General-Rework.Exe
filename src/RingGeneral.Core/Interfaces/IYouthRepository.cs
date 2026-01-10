using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Phase 2.3 - Interface pour le repository Youth (méthode de création)
/// </summary>
public interface IYouthRepository
{
    /// <summary>
    /// Phase 2.3 - Crée une nouvelle YouthStructure
    /// </summary>
    Task CreateYouthStructureAsync(
        string youthStructureId,
        string companyId,
        string name,
        string? regionId,
        string type,
        decimal budgetAnnuel,
        int capaciteMax,
        int niveauEquipements,
        int qualiteCoaching,
        string philosophie);
}

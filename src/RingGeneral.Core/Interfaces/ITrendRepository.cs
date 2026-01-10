using RingGeneral.Core.Models.Trends;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des tendances
/// </summary>
public interface ITrendRepository
{
    // ====================================================================
    // TREND OPERATIONS
    // ====================================================================

    Task SaveTrendAsync(Trend trend);
    Task<Trend?> GetTrendByIdAsync(string trendId);
    Task<IReadOnlyList<Trend>> GetActiveTrendsAsync(string? region = null);
    Task<IReadOnlyList<Trend>> GetTrendsByTypeAsync(string type);
    Task<IReadOnlyList<Trend>> GetTrendsByCategoryAsync(string category);
    Task UpdateTrendAsync(Trend trend);
    Task DeleteTrendAsync(string trendId);

    // ====================================================================
    // COMPATIBILITY MATRIX OPERATIONS
    // ====================================================================

    Task SaveCompatibilityMatrixAsync(CompatibilityMatrix matrix);
    Task<CompatibilityMatrix?> GetCompatibilityMatrixAsync(string companyId, string trendId);
    Task<IReadOnlyList<CompatibilityMatrix>> GetCompatibilityMatricesByCompanyIdAsync(string companyId);
    Task<IReadOnlyList<CompatibilityMatrix>> GetCompatibilityMatricesByTrendIdAsync(string trendId);
    Task UpdateCompatibilityMatrixAsync(CompatibilityMatrix matrix);
    Task DeleteCompatibilityMatrixAsync(string matrixId);
}

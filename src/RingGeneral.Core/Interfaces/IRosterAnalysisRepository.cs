using RingGeneral.Core.Models.Roster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository de l'analyse structurelle du roster
/// </summary>
public interface IRosterAnalysisRepository
{
    // ====================================================================
    // ROSTER DNA OPERATIONS
    // ====================================================================

    Task SaveRosterDNAAsync(RosterDNA dna);
    Task<RosterDNA?> GetRosterDNAByCompanyIdAsync(string companyId);
    Task UpdateRosterDNAAsync(RosterDNA dna);
    Task DeleteRosterDNAAsync(string companyId);

    // ====================================================================
    // STRUCTURAL ANALYSIS OPERATIONS
    // ====================================================================

    Task SaveStructuralAnalysisAsync(RosterStructuralAnalysis analysis);
    Task<RosterStructuralAnalysis?> GetLatestAnalysisByCompanyIdAsync(string companyId);
    Task<IReadOnlyList<RosterStructuralAnalysis>> GetAnalysesByCompanyIdAsync(string companyId, int? limit = null);
    Task<RosterStructuralAnalysis?> GetAnalysisByDateAsync(string companyId, int year, int weekNumber);
    Task DeleteOldAnalysesAsync(string companyId, int keepLastN = 10);
}

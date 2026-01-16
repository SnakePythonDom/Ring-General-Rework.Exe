using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;

namespace RingGeneral.Core.Services;

public class ReadinessResult
{
    public bool IsReady => Warnings.Count == 0;
    public List<string> Warnings { get; } = new();

    public void AddWarning(string warning) => Warnings.Add(warning);
}

public class ShowReadinessService : IShowReadinessService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICompanyRepository _companyRepository;

    public ShowReadinessService(
        IWorkerRepository workerRepository,
        IStaffRepository staffRepository,
        ICompanyRepository companyRepository)
    {
        _workerRepository = workerRepository ?? throw new ArgumentNullException(nameof(workerRepository));
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }

    public async Task<ReadinessResult> CheckReadinessAsync(string companyId)
    {
        var result = new ReadinessResult();

        // 1. Check Workers (Need at least 2 for a match)
        var roster = _workerRepository.GetCompanyRoster(companyId);
        // We filter for active and non-injured
        var activeWrestlers = roster.Where(w => w.IsActive && !w.IsInjured).ToList();

        if (activeWrestlers.Count < 2)
        {
            result.AddWarning($"Roster insuffisant : Vous avez besoin d'au moins 2 catcheurs actifs (Actuel : {activeWrestlers.Count}).");
        }

        // 2. Check Referee (Need at least 1)
        var staff = await _staffRepository.GetActiveStaffByCompanyIdAsync(companyId);
        var hasReferee = staff.Any(s => s.Role == StaffRole.Referee);

        if (!hasReferee)
        {
            result.AddWarning("Staff manquant : Vous devez avoir au moins 1 arbitre sous contrat.");
        }

        // 3. Check Solvency (Soft Lock prevention)
        // We check if treasury is dangerously low.
        var companyState = _companyRepository.ChargerEtatCompagnie(companyId);
        if (companyState != null)
        {
            // Tresorerie property check
            if (companyState.Tresorerie < 500)
            {
                result.AddWarning("Trésorerie critique : Vous n'avez pas assez de fonds pour organiser un show (Min. $500).");
            }
        }

        return result;
    }
}

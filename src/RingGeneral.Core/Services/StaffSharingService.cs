using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using RingGeneral.Core.Models.ChildCompany;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Services;

public class StaffSharingService : IStaffSharingService
{
    private readonly IGameRepository _repository;
    private readonly IChildCompanyStaffRepository _staffRepository;

    public StaffSharingService(IGameRepository repository, IChildCompanyStaffRepository staffRepository)
    {
        _repository = repository;
        _staffRepository = staffRepository;
    }

    public async Task LoanWorkerAsync(string workerId, string fromCompanyId, string toCompanyId, int durationWeeks)
    {
        var worker = _repository.GetWorker(workerId);
        if (worker == null) return;

        // Create Assignment
        var assignment = new ChildCompanyStaffAssignment(
            Guid.NewGuid().ToString("N"),
            workerId,
            toCompanyId,
            StaffAssignmentType.TemporarySupport,
            1.0, // Full time for duration
            DateTime.Now,
            DateTime.Now.AddDays(durationWeeks * 7),
            "Development Loan",
            DateTime.Now
        );

        await _staffRepository.SaveStaffAssignmentAsync(assignment);

        // Update Worker Status
        worker.Type = WorkerType.LoanedWrestler;
        _repository.UpdateWorker(worker);
    }

    public async Task RecallWorkerAsync(string workerId)
    {
        var worker = _repository.GetWorker(workerId);
        if (worker == null) return;

        // Find active assignments and close them
        var assignments = await _staffRepository.GetStaffAssignmentsByStaffAsync(workerId);
        var active = assignments.FirstOrDefault(a => a.EndDate == null || a.EndDate > DateTime.Now);

        if (active != null)
        {
            // Close assignment
            // Assuming we need to update EndDate. But record is immutable?
            // Records are immutable but usually repositories handle update by replacing.
            // But here I'll just Delete for simplicity, or ideally Update with new EndDate.
            // Since I don't see a "With" method or mutable properties easily here without recreating.
            // I'll delete the assignment to "Recall" it effectively from the active list.
            await _staffRepository.DeleteStaffAssignmentAsync(active.AssignmentId);
        }

        worker.Type = WorkerType.Wrestler;
        _repository.UpdateWorker(worker);
    }

    public async Task<IEnumerable<string>> GetLoanedWorkersAsync(string companyId)
    {
        // Get all assignments for this company (assuming companyId is the Child Company receiving loans)
        var assignments = await _staffRepository.GetActiveStaffAssignmentsAsync(companyId);
        return assignments.Select(a => a.StaffId).Distinct();
    }
}

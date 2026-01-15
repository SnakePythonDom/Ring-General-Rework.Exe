using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Staff;

namespace RingGeneral.Core.Services;

public sealed class RecruitmentService : IRecruitmentService
{
    private readonly IWorkerRepository _workerRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IContractRepository _contractRepo;
    private readonly IRandomProvider _random;

    public RecruitmentService(
        IWorkerRepository workerRepo,
        IStaffRepository staffRepo,
        IContractRepository contractRepo,
        IRandomProvider random)
    {
        _workerRepo = workerRepo;
        _staffRepo = staffRepo;
        _contractRepo = contractRepo;
        _random = random;
    }

    public async Task<RecruitmentResult> SignToMainRosterAsync(string agentId, string companyId, decimal salary)
    {
        // Try to find as Worker
        var worker = _workerRepo.GetWorker(agentId);
        if (worker != null)
        {
            if (worker.Age < 18)
            {
                return new RecruitmentResult(RecruitmentStatus.Warning, "Attention, ce talent n'est pas fini. Risque de mauvaises notes.", agentId);
            }

            worker.CompanyId = companyId;
            worker.Type = WorkerType.Wrestler;
            _workerRepo.UpdateWorker(worker);

            // Create initial contract (simplified)
            // _contractRepo.AjouterContratActif(...) 

            return new RecruitmentResult(RecruitmentStatus.Success, $"{worker.Name} a rejoint le Main Roster.", agentId);
        }

        // Try to find as Staff
        var staff = await _staffRepo.GetStaffMemberByIdAsync(agentId);
        if (staff != null)
        {
            var updatedStaff = staff with { CompanyId = companyId, IsActive = true, EmploymentStatus = "Active" };
            await _staffRepo.UpdateStaffMemberAsync(updatedStaff);
            return new RecruitmentResult(RecruitmentStatus.Success, $"{staff.Name} a rejoint le Main Roster en tant que {staff.Role}.", agentId);
        }

        return new RecruitmentResult(RecruitmentStatus.Failure, "Agent non trouvé.");
    }

    public async Task<RecruitmentResult> SignToChildCompanyAsync(string agentId, string parentCompanyId, string childCompanyId, decimal salary)
    {
        var worker = _workerRepo.GetWorker(agentId);
        if (worker != null)
        {
            worker.CompanyId = childCompanyId;
            worker.Type = WorkerType.ChildCompanyWrestler;
            _workerRepo.UpdateWorker(worker);
            return new RecruitmentResult(RecruitmentStatus.Success, $"{worker.Name} a été envoyé en développement.", agentId);
        }

        var staff = await _staffRepo.GetStaffMemberByIdAsync(agentId);
        if (staff != null)
        {
            var updatedStaff = staff with { CompanyId = childCompanyId, IsActive = true, EmploymentStatus = "Active" };
            await _staffRepo.UpdateStaffMemberAsync(updatedStaff);
            return new RecruitmentResult(RecruitmentStatus.Success, $"{staff.Name} a rejoint la fédération fille.", agentId);
        }

        return new RecruitmentResult(RecruitmentStatus.Failure, "Agent non trouvé.");
    }

    public async Task<RecruitmentResult> SignToYouthStructureAsync(string agentId, string companyId, string youthStructureId)
    {
        var worker = _workerRepo.GetWorker(agentId);
        if (worker == null) return new RecruitmentResult(RecruitmentStatus.Failure, "Seuls les lutteurs peuvent rejoindre le centre de formation.");

        if (worker.Age >= 25)
        {
            return new RecruitmentResult(RecruitmentStatus.Failure, "Ce talent est trop âgé pour le centre de formation (Max 25 ans).");
        }

        worker.CompanyId = companyId;
        worker.Type = WorkerType.Trainee;
        // Logic to link with youthStructureId would go here (e.g. Workers.YouthStructureId)
        _workerRepo.UpdateWorker(worker);

        return new RecruitmentResult(RecruitmentStatus.Success, $"{worker.Name} est maintenant élève.", agentId);
    }

    public async Task<ReconversionResult> NegotiateReconversionAsync(string workerId, string companyId, StaffRole targetRole, decimal salaryOffer)
    {
        var worker = _workerRepo.GetWorker(workerId);
        if (worker == null) return new ReconversionResult(ReconversionStatus.Denied, "Lutteur non trouvé.");

        // Algo: Ego Check
        // Default demand based on popularity or previous salary
        decimal currentDemand = (decimal)(worker.Popularity * 100); // Placeholder formula

        bool wantsToStayActive = worker.Age < 45; // Veterans >= 45 are more likely to accept

        if (wantsToStayActive && salaryOffer < currentDemand * 1.3m)
        {
            return new ReconversionResult(ReconversionStatus.Denied, "Je ne suis pas encore à la retraite ! (Offre insuffisante pour compenser l'égo)");
        }

        // Logic to convert Worker to Staff
        // 1. Create StaffMember entry
        var staffMember = new StaffMember
        {
            StaffId = $"ST-{worker.WorkerId}",
            CompanyId = companyId,
            Name = worker.Name,
            Role = targetRole,
            Department = GetDepartmentForRole(targetRole),
            ExpertiseLevel = GetExpertiseFromExperience(worker.Experience),
            SkillScore = worker.OverallRating, // Transition skill
            HireDate = DateTime.Now,
            IsActive = true,
            AnnualSalary = (double)(salaryOffer * 52)
        };
        await _staffRepo.SaveStaffMemberAsync(staffMember);

        // 2. Mark Worker as Inactive or Retired
        worker.IsActive = false;
        worker.CompanyId = null;
        _workerRepo.UpdateWorker(worker);

        return new ReconversionResult(ReconversionStatus.Accepted, "Pour ce prix-là, d'accord, je deviens entraîneur.");
    }

    private StaffDepartment GetDepartmentForRole(StaffRole role)
    {
        // Simple mapping
        if (role == StaffRole.HeadTrainer || role == StaffRole.WrestlingTrainer || role == StaffRole.PromoTrainer || role == StaffRole.StrengthCoach)
            return StaffDepartment.Training;

        return StaffDepartment.Creative; // Default
    }

    private StaffExpertiseLevel GetExpertiseFromExperience(int years)
    {
        if (years > 20) return StaffExpertiseLevel.Legend;
        if (years > 15) return StaffExpertiseLevel.Expert;
        if (years > 10) return StaffExpertiseLevel.Senior;
        if (years > 5) return StaffExpertiseLevel.MidLevel;
        return StaffExpertiseLevel.Junior;
    }
}

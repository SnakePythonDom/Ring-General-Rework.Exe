using RingGeneral.Core.Enums;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public enum RecruitmentStatus
{
    Success,
    Warning,
    Failure
}

public record RecruitmentResult(RecruitmentStatus Status, string Message, string? EntityId = null);

public enum ReconversionStatus
{
    Accepted,
    Denied,
    NegotiationNeeded
}

public record ReconversionResult(ReconversionStatus Status, string Message, decimal? CounterOffer = null);

public interface IRecruitmentService
{
    /// <summary>
    /// Signs a free agent (Wrestler or Staff) to the main roster.
    /// </summary>
    Task<RecruitmentResult> SignToMainRosterAsync(string agentId, string companyId, decimal salary);

    /// <summary>
    /// Signs a free agent to a child company.
    /// </summary>
    Task<RecruitmentResult> SignToChildCompanyAsync(string agentId, string parentCompanyId, string childCompanyId, decimal salary);

    /// <summary>
    /// Signs a young talent to the youth structure as a student.
    /// </summary>
    Task<RecruitmentResult> SignToYouthStructureAsync(string agentId, string companyId, string youthStructureId);

    /// <summary>
    /// Attempts to convince a veteran wrestler to retire and become a staff member.
    /// </summary>
    Task<ReconversionResult> NegotiateReconversionAsync(string workerId, string companyId, StaffRole targetRole, decimal salaryOffer);
}

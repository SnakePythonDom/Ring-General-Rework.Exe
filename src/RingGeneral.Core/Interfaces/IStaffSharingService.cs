using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

public interface IStaffSharingService
{
    /// <summary>
    /// Envoie un worker en prêt d'une compagnie parente vers une compagnie fille (ou vice-versa).
    /// </summary>
    Task LoanWorkerAsync(string workerId, string fromCompanyId, string toCompanyId, int durationWeeks);

    /// <summary>
    /// Rappelle un worker de son prêt.
    /// </summary>
    Task RecallWorkerAsync(string workerId);

    /// <summary>
    /// Récupère la liste des workers prêtés par/à une compagnie.
    /// </summary>
    Task<IEnumerable<string>> GetLoanedWorkersAsync(string companyId);
}

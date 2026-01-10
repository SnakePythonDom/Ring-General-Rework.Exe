using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour accéder aux données des compagnies
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Charge l'état d'une compagnie
    /// </summary>
    CompanyState? ChargerEtatCompagnie(string companyId);
}

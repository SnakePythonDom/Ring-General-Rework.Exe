namespace RingGeneral.Core.Models;

/// <summary>
/// Modèle de contrat hybride permettant trois types de contrats :
/// 1. Contrat Fixe uniquement : MonthlyWage > 0, AppearanceFee = 0
/// 2. Contrat par Apparition uniquement : MonthlyWage = 0, AppearanceFee > 0
/// 3. Contrat Hybride : MonthlyWage > 0 ET AppearanceFee > 0
/// </summary>
public sealed record HybridContract(
    string ContractId,
    string WorkerId,
    string CompanyId,
    double MonthlyWage,        // Salaire mensuel garanti (peut être 0 si contrat par apparition uniquement)
    double AppearanceFee,      // Frais par apparition (peut être 0 si contrat fixe uniquement)
    bool IsExclusive,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? LastPaymentDate,     // Dernier paiement mensuel (null si jamais payé)
    DateTime? LastAppearanceDate);  // Dernière apparition payée (null si jamais apparu)

/// <summary>
/// Extensions pour HybridContract
/// </summary>
public static class HybridContractExtensions
{
    /// <summary>
    /// Vérifie si le contrat a un salaire mensuel garanti
    /// </summary>
    public static bool HasMonthlyWage(this HybridContract contract) => contract.MonthlyWage > 0;

    /// <summary>
    /// Vérifie si le contrat a des frais d'apparition
    /// </summary>
    public static bool HasAppearanceFee(this HybridContract contract) => contract.AppearanceFee > 0;

    /// <summary>
    /// Détermine le type de contrat
    /// </summary>
    public static ContractHybridType GetContractType(this HybridContract contract)
    {
        if (contract.MonthlyWage > 0 && contract.AppearanceFee > 0)
            return ContractHybridType.Hybrid;
        if (contract.MonthlyWage > 0)
            return ContractHybridType.Fixed;
        if (contract.AppearanceFee > 0)
            return ContractHybridType.PerAppearance;
        return ContractHybridType.Unknown;
    }
}

/// <summary>
/// Type de contrat hybride
/// </summary>
public enum ContractHybridType
{
    /// <summary>
    /// Contrat fixe uniquement (mensuel garanti)
    /// </summary>
    Fixed,

    /// <summary>
    /// Contrat par apparition uniquement
    /// </summary>
    PerAppearance,

    /// <summary>
    /// Contrat hybride (mensuel + apparition)
    /// </summary>
    Hybrid,

    /// <summary>
    /// Type inconnu (erreur de données)
    /// </summary>
    Unknown
}

/// <summary>
/// Contexte pour calcul financier quotidien
/// </summary>
public sealed record DailyFinanceContext(
    string CompanyId,
    int CurrentDay,
    DateTime CurrentDate,
    IReadOnlyList<HybridContract> ActiveContracts,
    double Treasury);

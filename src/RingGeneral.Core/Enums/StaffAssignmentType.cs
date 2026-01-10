namespace RingGeneral.Core.Enums;

/// <summary>
/// Types d'assignation de staff aux Child Companies
/// </summary>
public enum StaffAssignmentType
{
    /// <summary>
    /// Partage de temps flexible entre compagnie mère et Child Company
    /// </summary>
    PartTime,

    /// <summary>
    /// Support temporaire pour mission spécifique
    /// </summary>
    TemporarySupport,

    /// <summary>
    /// Affectation dédiée avec rotation périodique
    /// </summary>
    DedicatedRotation
}
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service responsible for verifying and repairing the database schema and seeding initial data.
/// Acts as the "Annealing" mechanism to self-heal the database structure.
/// </summary>
public interface IDatabaseGeneratorService
{
    /// <summary>
    /// Ensures that the database schema is correct and all required data (tables, columns, default rows) exists.
    /// This includes:
    /// - Verifying table existence (Workers, Titles, YouthStructures, etc.)
    /// - Verifying column existence (e.g., Aura in WorkerEntertainmentAttributes)
    /// - Seeding default Company, Workers, Titles if missing.
    /// - Seeding Youth Structures if missing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureDatabaseSchemaAsync();

    /// <summary>
    /// Checks if the database needs repair or initialization.
    /// </summary>
    /// <returns>True if repair/seed is needed, false otherwise.</returns>
    Task<bool> NeedsRepairAsync();
}

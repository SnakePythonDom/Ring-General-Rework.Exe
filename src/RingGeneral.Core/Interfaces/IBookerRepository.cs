using RingGeneral.Core.Models.Booker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des bookers (Phase 4).
/// Gère les bookers, leurs mémoires et leur historique d'emploi.
/// </summary>
public interface IBookerRepository
{
    Task<Booker?> GetBookerByIdAsync(string bookerId);
    Task<Booker?> GetActiveBookerAsync(string companyId);
    Task<Booker?> GetAutoBookingBookerAsync(string companyId);
    Task<List<Booker>> GetBookersByCompanyAsync(string companyId);
    Task SaveBookerAsync(Booker booker);
    Task UpdateBookerAsync(Booker booker);

    Task<BookerMemory?> GetBookerMemoryByIdAsync(int memoryId);
    Task<List<BookerMemory>> GetRecentMemoriesAsync(string bookerId, int limit);
    Task<List<BookerMemory>> GetStrongMemoriesAsync(string bookerId);
    Task<int> CountMemoriesAsync(string bookerId);
    Task SaveBookerMemoryAsync(BookerMemory memory);
    Task UpdateBookerMemoryAsync(BookerMemory memory);
    Task DeleteWeakMemoriesAsync(string bookerId, int threshold);

    Task<List<BookerEmploymentHistory>> GetEmploymentHistoryAsync(string bookerId);
    Task SaveEmploymentHistoryAsync(BookerEmploymentHistory history);
}

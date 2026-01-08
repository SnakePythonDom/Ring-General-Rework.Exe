using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Booker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des bookers.
/// Gère Bookers, BookerMemory, et BookerEmploymentHistory.
/// </summary>
public sealed class BookerRepository : IBookerRepository
{
    private readonly string _connectionString;

    public BookerRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // BOOKER CRUD OPERATIONS
    // ====================================================================

    public async Task SaveBookerAsync(Booker booker)
    {
        if (!booker.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Booker invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Bookers (
                BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            ) VALUES (
                @BookerId, @CompanyId, @Name, @CreativityScore, @LogicScore, @BiasResistance,
                @PreferredStyle, @LikesUnderdog, @LikesVeteran, @LikesFastRise, @LikesSlowBurn,
                @IsAutoBookingEnabled, @EmploymentStatus, @HireDate, @CreatedAt
            )";

        AddBookerParameters(command, booker);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Booker?> GetBookerByIdAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE BookerId = @BookerId";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBooker(reader);
        }

        return null;
    }

    public async Task<List<Booker>> GetActiveBookersByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE CompanyId = @CompanyId AND EmploymentStatus = 'Active'
            ORDER BY Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var bookers = new List<Booker>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookers.Add(MapBooker(reader));
        }

        return bookers;
    }

    public async Task<Booker?> GetActiveBookerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE CompanyId = @CompanyId AND EmploymentStatus = 'Active'
            ORDER BY Name
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBooker(reader);
        }

        return null;
    }

    public async Task<List<Booker>> GetAllBookersByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE CompanyId = @CompanyId
            ORDER BY EmploymentStatus, Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var bookers = new List<Booker>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookers.Add(MapBooker(reader));
        }

        return bookers;
    }

    public async Task<Booker?> GetAutoBookingBookerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE CompanyId = @CompanyId
              AND EmploymentStatus = 'Active'
              AND IsAutoBookingEnabled = 1
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBooker(reader);
        }

        return null;
    }

    public async Task UpdateBookerAsync(Booker booker)
    {
        if (!booker.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Booker invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Bookers SET
                CompanyId = @CompanyId,
                Name = @Name,
                CreativityScore = @CreativityScore,
                LogicScore = @LogicScore,
                BiasResistance = @BiasResistance,
                PreferredStyle = @PreferredStyle,
                LikesUnderdog = @LikesUnderdog,
                LikesVeteran = @LikesVeteran,
                LikesFastRise = @LikesFastRise,
                LikesSlowBurn = @LikesSlowBurn,
                IsAutoBookingEnabled = @IsAutoBookingEnabled,
                EmploymentStatus = @EmploymentStatus,
                HireDate = @HireDate
            WHERE BookerId = @BookerId";

        AddBookerParameters(command, booker);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteBookerAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Bookers WHERE BookerId = @BookerId";
        command.Parameters.AddWithValue("@BookerId", bookerId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ToggleAutoBookingAsync(string bookerId, bool enabled)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Bookers
            SET IsAutoBookingEnabled = @Enabled
            WHERE BookerId = @BookerId";

        command.Parameters.AddWithValue("@BookerId", bookerId);
        command.Parameters.AddWithValue("@Enabled", enabled ? 1 : 0);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateEmploymentStatusAsync(string bookerId, string status)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Bookers
            SET EmploymentStatus = @Status
            WHERE BookerId = @BookerId";

        command.Parameters.AddWithValue("@BookerId", bookerId);
        command.Parameters.AddWithValue("@Status", status);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BOOKER MEMORY OPERATIONS
    // ====================================================================

    public async Task SaveBookerMemoryAsync(BookerMemory memory)
    {
        if (!memory.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"BookerMemory invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO BookerMemory (
                BookerId, EventType, EventDescription, ImpactScore, RecallStrength, CreatedAt
            ) VALUES (
                @BookerId, @EventType, @EventDescription, @ImpactScore, @RecallStrength, @CreatedAt
            )";

        command.Parameters.AddWithValue("@BookerId", memory.BookerId);
        command.Parameters.AddWithValue("@EventType", memory.EventType);
        command.Parameters.AddWithValue("@EventDescription", memory.EventDescription);
        command.Parameters.AddWithValue("@ImpactScore", memory.ImpactScore);
        command.Parameters.AddWithValue("@RecallStrength", memory.RecallStrength);
        command.Parameters.AddWithValue("@CreatedAt", memory.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<BookerMemory>> GetBookerMemoriesAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT MemoryId, BookerId, EventType, EventDescription, ImpactScore, RecallStrength, CreatedAt
            FROM BookerMemory
            WHERE BookerId = @BookerId
            ORDER BY CreatedAt DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var memories = new List<BookerMemory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            memories.Add(MapBookerMemory(reader));
        }

        return memories;
    }

    public async Task<List<BookerMemory>> GetStrongMemoriesAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT MemoryId, BookerId, EventType, EventDescription, ImpactScore, RecallStrength, CreatedAt
            FROM BookerMemory
            WHERE BookerId = @BookerId AND RecallStrength >= 70
            ORDER BY RecallStrength DESC, CreatedAt DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var memories = new List<BookerMemory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            memories.Add(MapBookerMemory(reader));
        }

        return memories;
    }

    public async Task<List<BookerMemory>> GetRecentMemoriesAsync(string bookerId, int weeksPast = 12)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cutoffDate = DateTime.Now.AddDays(-weeksPast * 7);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT MemoryId, BookerId, EventType, EventDescription, ImpactScore, RecallStrength, CreatedAt
            FROM BookerMemory
            WHERE BookerId = @BookerId AND CreatedAt >= @CutoffDate
            ORDER BY CreatedAt DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);
        command.Parameters.AddWithValue("@CutoffDate", cutoffDate.ToString("O"));

        var memories = new List<BookerMemory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            memories.Add(MapBookerMemory(reader));
        }

        return memories;
    }

    public async Task<List<BookerMemory>> GetMemoriesByTypeAsync(string bookerId, string eventType)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT MemoryId, BookerId, EventType, EventDescription, ImpactScore, RecallStrength, CreatedAt
            FROM BookerMemory
            WHERE BookerId = @BookerId AND EventType = @EventType
            ORDER BY RecallStrength DESC, CreatedAt DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);
        command.Parameters.AddWithValue("@EventType", eventType);

        var memories = new List<BookerMemory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            memories.Add(MapBookerMemory(reader));
        }

        return memories;
    }

    public async Task<int> CountMemoriesAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM BookerMemory
            WHERE BookerId = @BookerId";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateBookerMemoryAsync(BookerMemory memory)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE BookerMemory SET
                EventDescription = @EventDescription,
                ImpactScore = @ImpactScore,
                RecallStrength = @RecallStrength
            WHERE MemoryId = @MemoryId";

        command.Parameters.AddWithValue("@MemoryId", memory.MemoryId);
        command.Parameters.AddWithValue("@EventDescription", memory.EventDescription);
        command.Parameters.AddWithValue("@ImpactScore", memory.ImpactScore);
        command.Parameters.AddWithValue("@RecallStrength", memory.RecallStrength);

        await command.ExecuteNonQueryAsync();
    }

    public async Task CleanupWeakMemoriesAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM BookerMemory
            WHERE BookerId = @BookerId AND RecallStrength < 10";

        command.Parameters.AddWithValue("@BookerId", bookerId);
        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // EMPLOYMENT HISTORY OPERATIONS
    // ====================================================================

    public async Task SaveEmploymentHistoryAsync(BookerEmploymentHistory history)
    {
        if (!history.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"EmploymentHistory invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO BookerEmploymentHistory (
                BookerId, CompanyId, StartDate, EndDate, TerminationReason, PerformanceScore
            ) VALUES (
                @BookerId, @CompanyId, @StartDate, @EndDate, @TerminationReason, @PerformanceScore
            )";

        command.Parameters.AddWithValue("@BookerId", history.BookerId);
        command.Parameters.AddWithValue("@CompanyId", history.CompanyId);
        command.Parameters.AddWithValue("@StartDate", history.StartDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", history.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@TerminationReason", history.TerminationReason ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PerformanceScore", history.PerformanceScore ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<BookerEmploymentHistory>> GetEmploymentHistoryAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT HistoryId, BookerId, CompanyId, StartDate, EndDate, TerminationReason, PerformanceScore
            FROM BookerEmploymentHistory
            WHERE BookerId = @BookerId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var history = new List<BookerEmploymentHistory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            history.Add(MapEmploymentHistory(reader));
        }

        return history;
    }

    public async Task<BookerEmploymentHistory?> GetCurrentEmploymentAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT HistoryId, BookerId, CompanyId, StartDate, EndDate, TerminationReason, PerformanceScore
            FROM BookerEmploymentHistory
            WHERE BookerId = @BookerId AND EndDate IS NULL
            LIMIT 1";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapEmploymentHistory(reader);
        }

        return null;
    }

    public async Task UpdateEmploymentHistoryAsync(BookerEmploymentHistory history)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE BookerEmploymentHistory SET
                EndDate = @EndDate,
                TerminationReason = @TerminationReason,
                PerformanceScore = @PerformanceScore
            WHERE HistoryId = @HistoryId";

        command.Parameters.AddWithValue("@HistoryId", history.HistoryId);
        command.Parameters.AddWithValue("@EndDate", history.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@TerminationReason", history.TerminationReason ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PerformanceScore", history.PerformanceScore ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<BookerEmploymentHistory>> GetCompanyEmploymentHistoryAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT HistoryId, BookerId, CompanyId, StartDate, EndDate, TerminationReason, PerformanceScore
            FROM BookerEmploymentHistory
            WHERE CompanyId = @CompanyId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var history = new List<BookerEmploymentHistory>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            history.Add(MapEmploymentHistory(reader));
        }

        return history;
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<int> CountActiveBookersAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM Bookers
            WHERE CompanyId = @CompanyId AND EmploymentStatus = 'Active'";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> CompanyHasActiveBookerAsync(string companyId)
    {
        var count = await CountActiveBookersAsync(companyId);
        return count > 0;
    }

    public async Task<List<Booker>> GetAllAutoBookingBookersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT BookerId, CompanyId, Name, CreativityScore, LogicScore, BiasResistance,
                   PreferredStyle, LikesUnderdog, LikesVeteran, LikesFastRise, LikesSlowBurn,
                   IsAutoBookingEnabled, EmploymentStatus, HireDate, CreatedAt
            FROM Bookers
            WHERE EmploymentStatus = 'Active' AND IsAutoBookingEnabled = 1
            ORDER BY CompanyId, Name";

        var bookers = new List<Booker>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookers.Add(MapBooker(reader));
        }

        return bookers;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static void AddBookerParameters(SqliteCommand command, Booker booker)
    {
        command.Parameters.AddWithValue("@BookerId", booker.BookerId);
        command.Parameters.AddWithValue("@CompanyId", booker.CompanyId);
        command.Parameters.AddWithValue("@Name", booker.Name);
        command.Parameters.AddWithValue("@CreativityScore", booker.CreativityScore);
        command.Parameters.AddWithValue("@LogicScore", booker.LogicScore);
        command.Parameters.AddWithValue("@BiasResistance", booker.BiasResistance);
        command.Parameters.AddWithValue("@PreferredStyle", booker.PreferredStyle);
        command.Parameters.AddWithValue("@LikesUnderdog", booker.LikesUnderdog ? 1 : 0);
        command.Parameters.AddWithValue("@LikesVeteran", booker.LikesVeteran ? 1 : 0);
        command.Parameters.AddWithValue("@LikesFastRise", booker.LikesFastRise ? 1 : 0);
        command.Parameters.AddWithValue("@LikesSlowBurn", booker.LikesSlowBurn ? 1 : 0);
        command.Parameters.AddWithValue("@IsAutoBookingEnabled", booker.IsAutoBookingEnabled ? 1 : 0);
        command.Parameters.AddWithValue("@EmploymentStatus", booker.EmploymentStatus);
        command.Parameters.AddWithValue("@HireDate", booker.HireDate.ToString("O"));
        command.Parameters.AddWithValue("@CreatedAt", booker.CreatedAt.ToString("O"));
    }

    private static Booker MapBooker(SqliteDataReader reader)
    {
        return new Booker
        {
            BookerId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Name = reader.GetString(2),
            CreativityScore = reader.GetInt32(3),
            LogicScore = reader.GetInt32(4),
            BiasResistance = reader.GetInt32(5),
            PreferredStyle = reader.GetString(6),
            LikesUnderdog = reader.GetInt32(7) == 1,
            LikesVeteran = reader.GetInt32(8) == 1,
            LikesFastRise = reader.GetInt32(9) == 1,
            LikesSlowBurn = reader.GetInt32(10) == 1,
            IsAutoBookingEnabled = reader.GetInt32(11) == 1,
            EmploymentStatus = reader.GetString(12),
            HireDate = DateTime.Parse(reader.GetString(13)),
            CreatedAt = DateTime.Parse(reader.GetString(14))
        };
    }

    private static BookerMemory MapBookerMemory(SqliteDataReader reader)
    {
        return new BookerMemory
        {
            MemoryId = reader.GetInt32(0),
            BookerId = reader.GetString(1),
            EventType = reader.GetString(2),
            EventDescription = reader.GetString(3),
            ImpactScore = reader.GetInt32(4),
            RecallStrength = reader.GetInt32(5),
            CreatedAt = DateTime.Parse(reader.GetString(6))
        };
    }

    private static BookerEmploymentHistory MapEmploymentHistory(SqliteDataReader reader)
    {
        return new BookerEmploymentHistory
        {
            HistoryId = reader.GetInt32(0),
            BookerId = reader.GetString(1),
            CompanyId = reader.GetString(2),
            StartDate = DateTime.Parse(reader.GetString(3)),
            EndDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
            TerminationReason = reader.IsDBNull(5) ? null : reader.GetString(5),
            PerformanceScore = reader.IsDBNull(6) ? null : reader.GetInt32(6)
        };
    }
}

using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des ères et transitions.
/// </summary>
public sealed class EraRepository : IEraRepository
{
    private readonly string _connectionString;

    public EraRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // ERA CRUD OPERATIONS
    // ====================================================================

    public async Task SaveEraAsync(Era era)
    {
        if (!era.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Era invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Eras (
                EraId, CompanyId, Type, CustomName, StartDate, EndDate, Intensity,
                PreferredMatchDuration, PreferredSegmentCount, MatchToSegmentRatio,
                DominantMatchTypes, AudienceExpectations, IsCurrentEra, CreatedAt
            ) VALUES (
                @EraId, @CompanyId, @Type, @CustomName, @StartDate, @EndDate, @Intensity,
                @PreferredMatchDuration, @PreferredSegmentCount, @MatchToSegmentRatio,
                @DominantMatchTypes, @AudienceExpectations, @IsCurrentEra, @CreatedAt
            )";

        command.Parameters.AddWithValue("@EraId", era.EraId);
        command.Parameters.AddWithValue("@CompanyId", era.CompanyId);
        command.Parameters.AddWithValue("@Type", era.Type.ToString());
        command.Parameters.AddWithValue("@CustomName", era.CustomName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", era.StartDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", era.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Intensity", era.Intensity);
        command.Parameters.AddWithValue("@PreferredMatchDuration", era.PreferredMatchDuration);
        command.Parameters.AddWithValue("@PreferredSegmentCount", era.PreferredSegmentCount);
        command.Parameters.AddWithValue("@MatchToSegmentRatio", era.MatchToSegmentRatio);
        command.Parameters.AddWithValue("@DominantMatchTypes", era.DominantMatchTypes);
        command.Parameters.AddWithValue("@AudienceExpectations", era.AudienceExpectations);
        command.Parameters.AddWithValue("@IsCurrentEra", era.IsCurrentEra ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", era.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Era?> GetEraByIdAsync(string eraId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EraId, CompanyId, Type, CustomName, StartDate, EndDate, Intensity,
                   PreferredMatchDuration, PreferredSegmentCount, MatchToSegmentRatio,
                   DominantMatchTypes, AudienceExpectations, IsCurrentEra, CreatedAt
            FROM Eras
            WHERE EraId = @EraId";

        command.Parameters.AddWithValue("@EraId", eraId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapEra(reader);
        }

        return null;
    }

    public async Task<List<Era>> GetErasByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EraId, CompanyId, Type, CustomName, StartDate, EndDate, Intensity,
                   PreferredMatchDuration, PreferredSegmentCount, MatchToSegmentRatio,
                   DominantMatchTypes, AudienceExpectations, IsCurrentEra, CreatedAt
            FROM Eras
            WHERE CompanyId = @CompanyId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var eras = new List<Era>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            eras.Add(MapEra(reader));
        }

        return eras;
    }

    public async Task<Era?> GetCurrentEraAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EraId, CompanyId, Type, CustomName, StartDate, EndDate, Intensity,
                   PreferredMatchDuration, PreferredSegmentCount, MatchToSegmentRatio,
                   DominantMatchTypes, AudienceExpectations, IsCurrentEra, CreatedAt
            FROM Eras
            WHERE CompanyId = @CompanyId AND IsCurrentEra = 1
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapEra(reader);
        }

        return null;
    }

    public async Task UpdateEraAsync(Era era)
    {
        if (!era.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Era invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Eras SET
                Type = @Type,
                CustomName = @CustomName,
                EndDate = @EndDate,
                Intensity = @Intensity,
                PreferredMatchDuration = @PreferredMatchDuration,
                PreferredSegmentCount = @PreferredSegmentCount,
                MatchToSegmentRatio = @MatchToSegmentRatio,
                DominantMatchTypes = @DominantMatchTypes,
                AudienceExpectations = @AudienceExpectations,
                IsCurrentEra = @IsCurrentEra
            WHERE EraId = @EraId";

        command.Parameters.AddWithValue("@EraId", era.EraId);
        command.Parameters.AddWithValue("@Type", era.Type.ToString());
        command.Parameters.AddWithValue("@CustomName", era.CustomName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", era.EndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Intensity", era.Intensity);
        command.Parameters.AddWithValue("@PreferredMatchDuration", era.PreferredMatchDuration);
        command.Parameters.AddWithValue("@PreferredSegmentCount", era.PreferredSegmentCount);
        command.Parameters.AddWithValue("@MatchToSegmentRatio", era.MatchToSegmentRatio);
        command.Parameters.AddWithValue("@DominantMatchTypes", era.DominantMatchTypes);
        command.Parameters.AddWithValue("@AudienceExpectations", era.AudienceExpectations);
        command.Parameters.AddWithValue("@IsCurrentEra", era.IsCurrentEra ? 1 : 0);

        await command.ExecuteNonQueryAsync();
    }

    public async Task EndEraAsync(string eraId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Eras
            SET EndDate = @EndDate, IsCurrentEra = 0
            WHERE EraId = @EraId";

        command.Parameters.AddWithValue("@EraId", eraId);
        command.Parameters.AddWithValue("@EndDate", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task SetCurrentEraAsync(string companyId, string eraId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Désactiver toutes les ères actuelles
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "UPDATE Eras SET IsCurrentEra = 0 WHERE CompanyId = @CompanyId";
                command.Parameters.AddWithValue("@CompanyId", companyId);
                await command.ExecuteNonQueryAsync();
            }

            // Activer la nouvelle ère
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "UPDATE Eras SET IsCurrentEra = 1 WHERE EraId = @EraId";
                command.Parameters.AddWithValue("@EraId", eraId);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ====================================================================
    // ERA TRANSITION OPERATIONS
    // ====================================================================

    public async Task SaveEraTransitionAsync(EraTransition transition)
    {
        if (!transition.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"EraTransition invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO EraTransitions (
                TransitionId, CompanyId, FromEraId, ToEraId, StartDate, PlannedEndDate, ActualEndDate,
                ProgressPercentage, Speed, MoraleImpact, AudienceImpact, ChangeResistance,
                InitiatedByBookerId, IsActive, Notes, CreatedAt
            ) VALUES (
                @TransitionId, @CompanyId, @FromEraId, @ToEraId, @StartDate, @PlannedEndDate, @ActualEndDate,
                @ProgressPercentage, @Speed, @MoraleImpact, @AudienceImpact, @ChangeResistance,
                @InitiatedByBookerId, @IsActive, @Notes, @CreatedAt
            )";

        command.Parameters.AddWithValue("@TransitionId", transition.TransitionId);
        command.Parameters.AddWithValue("@CompanyId", transition.CompanyId);
        command.Parameters.AddWithValue("@FromEraId", transition.FromEraId);
        command.Parameters.AddWithValue("@ToEraId", transition.ToEraId);
        command.Parameters.AddWithValue("@StartDate", transition.StartDate.ToString("O"));
        command.Parameters.AddWithValue("@PlannedEndDate", transition.PlannedEndDate.ToString("O"));
        command.Parameters.AddWithValue("@ActualEndDate", transition.ActualEndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ProgressPercentage", transition.ProgressPercentage);
        command.Parameters.AddWithValue("@Speed", transition.Speed.ToString());
        command.Parameters.AddWithValue("@MoraleImpact", transition.MoraleImpact);
        command.Parameters.AddWithValue("@AudienceImpact", transition.AudienceImpact);
        command.Parameters.AddWithValue("@ChangeResistance", transition.ChangeResistance);
        command.Parameters.AddWithValue("@InitiatedByBookerId", transition.InitiatedByBookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", transition.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Notes", transition.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", transition.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<EraTransition?> GetTransitionByIdAsync(string transitionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TransitionId, CompanyId, FromEraId, ToEraId, StartDate, PlannedEndDate, ActualEndDate,
                   ProgressPercentage, Speed, MoraleImpact, AudienceImpact, ChangeResistance,
                   InitiatedByBookerId, IsActive, Notes, CreatedAt
            FROM EraTransitions
            WHERE TransitionId = @TransitionId";

        command.Parameters.AddWithValue("@TransitionId", transitionId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapTransition(reader);
        }

        return null;
    }

    public async Task<EraTransition?> GetActiveTransitionAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TransitionId, CompanyId, FromEraId, ToEraId, StartDate, PlannedEndDate, ActualEndDate,
                   ProgressPercentage, Speed, MoraleImpact, AudienceImpact, ChangeResistance,
                   InitiatedByBookerId, IsActive, Notes, CreatedAt
            FROM EraTransitions
            WHERE CompanyId = @CompanyId AND IsActive = 1
            LIMIT 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapTransition(reader);
        }

        return null;
    }

    public async Task<List<EraTransition>> GetTransitionsByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TransitionId, CompanyId, FromEraId, ToEraId, StartDate, PlannedEndDate, ActualEndDate,
                   ProgressPercentage, Speed, MoraleImpact, AudienceImpact, ChangeResistance,
                   InitiatedByBookerId, IsActive, Notes, CreatedAt
            FROM EraTransitions
            WHERE CompanyId = @CompanyId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var transitions = new List<EraTransition>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            transitions.Add(MapTransition(reader));
        }

        return transitions;
    }

    public async Task<List<EraTransition>> GetTransitionsByBookerAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TransitionId, CompanyId, FromEraId, ToEraId, StartDate, PlannedEndDate, ActualEndDate,
                   ProgressPercentage, Speed, MoraleImpact, AudienceImpact, ChangeResistance,
                   InitiatedByBookerId, IsActive, Notes, CreatedAt
            FROM EraTransitions
            WHERE InitiatedByBookerId = @BookerId
            ORDER BY StartDate DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        var transitions = new List<EraTransition>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            transitions.Add(MapTransition(reader));
        }

        return transitions;
    }

    public async Task UpdateEraTransitionAsync(EraTransition transition)
    {
        if (!transition.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"EraTransition invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE EraTransitions SET
                ProgressPercentage = @ProgressPercentage,
                ActualEndDate = @ActualEndDate,
                MoraleImpact = @MoraleImpact,
                AudienceImpact = @AudienceImpact,
                IsActive = @IsActive,
                Notes = @Notes
            WHERE TransitionId = @TransitionId";

        command.Parameters.AddWithValue("@TransitionId", transition.TransitionId);
        command.Parameters.AddWithValue("@ProgressPercentage", transition.ProgressPercentage);
        command.Parameters.AddWithValue("@ActualEndDate", transition.ActualEndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MoraleImpact", transition.MoraleImpact);
        command.Parameters.AddWithValue("@AudienceImpact", transition.AudienceImpact);
        command.Parameters.AddWithValue("@IsActive", transition.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Notes", transition.Notes ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task CompleteTransitionAsync(string transitionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE EraTransitions
            SET ActualEndDate = @ActualEndDate, IsActive = 0, ProgressPercentage = 100
            WHERE TransitionId = @TransitionId";

        command.Parameters.AddWithValue("@TransitionId", transitionId);
        command.Parameters.AddWithValue("@ActualEndDate", DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<bool> HasActiveTransitionAsync(string companyId)
    {
        var transition = await GetActiveTransitionAsync(companyId);
        return transition != null;
    }

    public async Task<int> CountErasAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Eras WHERE CompanyId = @CompanyId";
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Era>> GetEraHistoryAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EraId, CompanyId, Type, CustomName, StartDate, EndDate, Intensity,
                   PreferredMatchDuration, PreferredSegmentCount, MatchToSegmentRatio,
                   DominantMatchTypes, AudienceExpectations, IsCurrentEra, CreatedAt
            FROM Eras
            WHERE CompanyId = @CompanyId AND EndDate IS NOT NULL
            ORDER BY EndDate DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var eras = new List<Era>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            eras.Add(MapEra(reader));
        }

        return eras;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static Era MapEra(SqliteDataReader reader)
    {
        return new Era
        {
            EraId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Type = Enum.Parse<EraType>(reader.GetString(2)),
            CustomName = reader.IsDBNull(3) ? null : reader.GetString(3),
            StartDate = DateTime.Parse(reader.GetString(4)),
            EndDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            Intensity = reader.GetInt32(6),
            PreferredMatchDuration = reader.GetInt32(7),
            PreferredSegmentCount = reader.GetInt32(8),
            MatchToSegmentRatio = reader.GetInt32(9),
            DominantMatchTypes = reader.GetString(10),
            AudienceExpectations = reader.GetString(11),
            IsCurrentEra = reader.GetInt32(12) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(13))
        };
    }

    private static EraTransition MapTransition(SqliteDataReader reader)
    {
        return new EraTransition
        {
            TransitionId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            FromEraId = reader.GetString(2),
            ToEraId = reader.GetString(3),
            StartDate = DateTime.Parse(reader.GetString(4)),
            PlannedEndDate = DateTime.Parse(reader.GetString(5)),
            ActualEndDate = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
            ProgressPercentage = reader.GetInt32(7),
            Speed = Enum.Parse<EraTransitionSpeed>(reader.GetString(8)),
            MoraleImpact = reader.GetInt32(9),
            AudienceImpact = reader.GetInt32(10),
            ChangeResistance = reader.GetInt32(11),
            InitiatedByBookerId = reader.IsDBNull(12) ? null : reader.GetString(12),
            IsActive = reader.GetInt32(13) == 1,
            Notes = reader.IsDBNull(14) ? null : reader.GetString(14),
            CreatedAt = DateTime.Parse(reader.GetString(15))
        };
    }
}

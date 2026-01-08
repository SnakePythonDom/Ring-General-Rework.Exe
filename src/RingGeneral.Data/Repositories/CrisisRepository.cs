using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Crisis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des crises.
/// Gère Crises, Communications, et CommunicationOutcomes.
/// </summary>
public sealed class CrisisRepository : ICrisisRepository
{
    private readonly string _connectionString;

    public CrisisRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // CRISIS OPERATIONS
    // ====================================================================

    public async Task SaveCrisisAsync(Crisis crisis)
    {
        if (!crisis.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Crisis invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Crises (
                CompanyId, CrisisType, Stage, Severity, Description,
                AffectedWorkers, EscalationScore, ResolutionAttempts, CreatedAt, ResolvedAt
            ) VALUES (
                @CompanyId, @CrisisType, @Stage, @Severity, @Description,
                @AffectedWorkers, @EscalationScore, @ResolutionAttempts, @CreatedAt, @ResolvedAt
            )";

        command.Parameters.AddWithValue("@CompanyId", crisis.CompanyId);
        command.Parameters.AddWithValue("@CrisisType", crisis.CrisisType);
        command.Parameters.AddWithValue("@Stage", crisis.Stage);
        command.Parameters.AddWithValue("@Severity", crisis.Severity);
        command.Parameters.AddWithValue("@Description", crisis.Description);
        command.Parameters.AddWithValue("@AffectedWorkers", crisis.AffectedWorkers ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@EscalationScore", crisis.EscalationScore);
        command.Parameters.AddWithValue("@ResolutionAttempts", crisis.ResolutionAttempts);
        command.Parameters.AddWithValue("@CreatedAt", crisis.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@ResolvedAt", crisis.ResolvedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Crisis?> GetCrisisByIdAsync(int crisisId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CrisisId, CompanyId, CrisisType, Stage, Severity, Description,
                   AffectedWorkers, EscalationScore, ResolutionAttempts, CreatedAt, ResolvedAt
            FROM Crises
            WHERE CrisisId = @CrisisId";

        command.Parameters.AddWithValue("@CrisisId", crisisId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCrisis(reader);
        }

        return null;
    }

    public async Task<List<Crisis>> GetActiveCrisesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CrisisId, CompanyId, CrisisType, Stage, Severity, Description,
                   AffectedWorkers, EscalationScore, ResolutionAttempts, CreatedAt, ResolvedAt
            FROM Crises
            WHERE CompanyId = @CompanyId
              AND Stage NOT IN ('Resolved', 'Ignored')
            ORDER BY Severity DESC, EscalationScore DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var crises = new List<Crisis>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            crises.Add(MapCrisis(reader));
        }

        return crises;
    }

    public async Task<List<Crisis>> GetCrisesByStageAsync(string companyId, string stage)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CrisisId, CompanyId, CrisisType, Stage, Severity, Description,
                   AffectedWorkers, EscalationScore, ResolutionAttempts, CreatedAt, ResolvedAt
            FROM Crises
            WHERE CompanyId = @CompanyId AND Stage = @Stage
            ORDER BY EscalationScore DESC, CreatedAt DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Stage", stage);

        var crises = new List<Crisis>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            crises.Add(MapCrisis(reader));
        }

        return crises;
    }

    public async Task<List<Crisis>> GetCriticalCrisesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CrisisId, CompanyId, CrisisType, Stage, Severity, Description,
                   AffectedWorkers, EscalationScore, ResolutionAttempts, CreatedAt, ResolvedAt
            FROM Crises
            WHERE CompanyId = @CompanyId
              AND (Severity >= 4 OR Stage = 'Declared')
              AND Stage NOT IN ('Resolved', 'Ignored')
            ORDER BY Severity DESC, EscalationScore DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var crises = new List<Crisis>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            crises.Add(MapCrisis(reader));
        }

        return crises;
    }

    public async Task UpdateCrisisAsync(Crisis crisis)
    {
        if (!crisis.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Crisis invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Crises SET
                CrisisType = @CrisisType,
                Stage = @Stage,
                Severity = @Severity,
                Description = @Description,
                AffectedWorkers = @AffectedWorkers,
                EscalationScore = @EscalationScore,
                ResolutionAttempts = @ResolutionAttempts,
                ResolvedAt = @ResolvedAt
            WHERE CrisisId = @CrisisId";

        command.Parameters.AddWithValue("@CrisisId", crisis.CrisisId);
        command.Parameters.AddWithValue("@CrisisType", crisis.CrisisType);
        command.Parameters.AddWithValue("@Stage", crisis.Stage);
        command.Parameters.AddWithValue("@Severity", crisis.Severity);
        command.Parameters.AddWithValue("@Description", crisis.Description);
        command.Parameters.AddWithValue("@AffectedWorkers", crisis.AffectedWorkers ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@EscalationScore", crisis.EscalationScore);
        command.Parameters.AddWithValue("@ResolutionAttempts", crisis.ResolutionAttempts);
        command.Parameters.AddWithValue("@ResolvedAt", crisis.ResolvedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteCrisisAsync(int crisisId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Crises WHERE CrisisId = @CrisisId";
        command.Parameters.AddWithValue("@CrisisId", crisisId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountActiveCrisesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM Crises
            WHERE CompanyId = @CompanyId
              AND Stage NOT IN ('Resolved', 'Ignored')";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // ====================================================================
    // COMMUNICATION OPERATIONS
    // ====================================================================

    public async Task SaveCommunicationAsync(Communication communication)
    {
        if (!communication.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Communication invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Communications (
                CompanyId, CrisisId, CommunicationType, InitiatorId, TargetId,
                Message, Tone, SuccessChance, CreatedAt
            ) VALUES (
                @CompanyId, @CrisisId, @CommunicationType, @InitiatorId, @TargetId,
                @Message, @Tone, @SuccessChance, @CreatedAt
            )";

        command.Parameters.AddWithValue("@CompanyId", communication.CompanyId);
        command.Parameters.AddWithValue("@CrisisId", communication.CrisisId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CommunicationType", communication.CommunicationType);
        command.Parameters.AddWithValue("@InitiatorId", communication.InitiatorId);
        command.Parameters.AddWithValue("@TargetId", communication.TargetId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Message", communication.Message);
        command.Parameters.AddWithValue("@Tone", communication.Tone);
        command.Parameters.AddWithValue("@SuccessChance", communication.SuccessChance);
        command.Parameters.AddWithValue("@CreatedAt", communication.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Communication?> GetCommunicationByIdAsync(int communicationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CommunicationId, CompanyId, CrisisId, CommunicationType, InitiatorId,
                   TargetId, Message, Tone, SuccessChance, CreatedAt
            FROM Communications
            WHERE CommunicationId = @CommunicationId";

        command.Parameters.AddWithValue("@CommunicationId", communicationId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCommunication(reader);
        }

        return null;
    }

    public async Task<List<Communication>> GetCommunicationsForCrisisAsync(int crisisId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CommunicationId, CompanyId, CrisisId, CommunicationType, InitiatorId,
                   TargetId, Message, Tone, SuccessChance, CreatedAt
            FROM Communications
            WHERE CrisisId = @CrisisId
            ORDER BY CreatedAt DESC";

        command.Parameters.AddWithValue("@CrisisId", crisisId);

        var communications = new List<Communication>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            communications.Add(MapCommunication(reader));
        }

        return communications;
    }

    public async Task<List<Communication>> GetRecentCommunicationsAsync(string companyId, int limit = 10)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CommunicationId, CompanyId, CrisisId, CommunicationType, InitiatorId,
                   TargetId, Message, Tone, SuccessChance, CreatedAt
            FROM Communications
            WHERE CompanyId = @CompanyId
            ORDER BY CreatedAt DESC
            LIMIT @Limit";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Limit", limit);

        var communications = new List<Communication>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            communications.Add(MapCommunication(reader));
        }

        return communications;
    }

    public async Task<List<Communication>> GetCommunicationsByTypeAsync(string companyId, string communicationType)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CommunicationId, CompanyId, CrisisId, CommunicationType, InitiatorId,
                   TargetId, Message, Tone, SuccessChance, CreatedAt
            FROM Communications
            WHERE CompanyId = @CompanyId AND CommunicationType = @Type
            ORDER BY CreatedAt DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Type", communicationType);

        var communications = new List<Communication>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            communications.Add(MapCommunication(reader));
        }

        return communications;
    }

    public async Task UpdateCommunicationAsync(Communication communication)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Communications SET
                Message = @Message,
                Tone = @Tone,
                SuccessChance = @SuccessChance
            WHERE CommunicationId = @CommunicationId";

        command.Parameters.AddWithValue("@CommunicationId", communication.CommunicationId);
        command.Parameters.AddWithValue("@Message", communication.Message);
        command.Parameters.AddWithValue("@Tone", communication.Tone);
        command.Parameters.AddWithValue("@SuccessChance", communication.SuccessChance);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteCommunicationAsync(int communicationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Communications WHERE CommunicationId = @CommunicationId";
        command.Parameters.AddWithValue("@CommunicationId", communicationId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // COMMUNICATION OUTCOME OPERATIONS
    // ====================================================================

    public async Task SaveCommunicationOutcomeAsync(CommunicationOutcome outcome)
    {
        if (!outcome.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"CommunicationOutcome invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO CommunicationOutcomes (
                CommunicationId, WasSuccessful, MoraleImpact, RelationshipImpact,
                CrisisEscalationChange, Feedback, CreatedAt
            ) VALUES (
                @CommunicationId, @WasSuccessful, @MoraleImpact, @RelationshipImpact,
                @CrisisEscalationChange, @Feedback, @CreatedAt
            )";

        command.Parameters.AddWithValue("@CommunicationId", outcome.CommunicationId);
        command.Parameters.AddWithValue("@WasSuccessful", outcome.WasSuccessful ? 1 : 0);
        command.Parameters.AddWithValue("@MoraleImpact", outcome.MoraleImpact);
        command.Parameters.AddWithValue("@RelationshipImpact", outcome.RelationshipImpact);
        command.Parameters.AddWithValue("@CrisisEscalationChange", outcome.CrisisEscalationChange);
        command.Parameters.AddWithValue("@Feedback", outcome.Feedback ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", outcome.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<CommunicationOutcome?> GetCommunicationOutcomeAsync(int communicationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OutcomeId, CommunicationId, WasSuccessful, MoraleImpact,
                   RelationshipImpact, CrisisEscalationChange, Feedback, CreatedAt
            FROM CommunicationOutcomes
            WHERE CommunicationId = @CommunicationId";

        command.Parameters.AddWithValue("@CommunicationId", communicationId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCommunicationOutcome(reader);
        }

        return null;
    }

    public async Task<List<CommunicationOutcome>> GetOutcomesForCrisisAsync(int crisisId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT o.OutcomeId, o.CommunicationId, o.WasSuccessful, o.MoraleImpact,
                   o.RelationshipImpact, o.CrisisEscalationChange, o.Feedback, o.CreatedAt
            FROM CommunicationOutcomes o
            INNER JOIN Communications c ON o.CommunicationId = c.CommunicationId
            WHERE c.CrisisId = @CrisisId
            ORDER BY o.CreatedAt DESC";

        command.Parameters.AddWithValue("@CrisisId", crisisId);

        var outcomes = new List<CommunicationOutcome>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            outcomes.Add(MapCommunicationOutcome(reader));
        }

        return outcomes;
    }

    public async Task<List<CommunicationOutcome>> GetSuccessfulOutcomesAsync(string companyId, int limit = 10)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT o.OutcomeId, o.CommunicationId, o.WasSuccessful, o.MoraleImpact,
                   o.RelationshipImpact, o.CrisisEscalationChange, o.Feedback, o.CreatedAt
            FROM CommunicationOutcomes o
            INNER JOIN Communications c ON o.CommunicationId = c.CommunicationId
            WHERE c.CompanyId = @CompanyId AND o.WasSuccessful = 1
            ORDER BY o.CreatedAt DESC
            LIMIT @Limit";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Limit", limit);

        var outcomes = new List<CommunicationOutcome>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            outcomes.Add(MapCommunicationOutcome(reader));
        }

        return outcomes;
    }

    public async Task UpdateCommunicationOutcomeAsync(CommunicationOutcome outcome)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CommunicationOutcomes SET
                WasSuccessful = @WasSuccessful,
                MoraleImpact = @MoraleImpact,
                RelationshipImpact = @RelationshipImpact,
                CrisisEscalationChange = @CrisisEscalationChange,
                Feedback = @Feedback
            WHERE OutcomeId = @OutcomeId";

        command.Parameters.AddWithValue("@OutcomeId", outcome.OutcomeId);
        command.Parameters.AddWithValue("@WasSuccessful", outcome.WasSuccessful ? 1 : 0);
        command.Parameters.AddWithValue("@MoraleImpact", outcome.MoraleImpact);
        command.Parameters.AddWithValue("@RelationshipImpact", outcome.RelationshipImpact);
        command.Parameters.AddWithValue("@CrisisEscalationChange", outcome.CrisisEscalationChange);
        command.Parameters.AddWithValue("@Feedback", outcome.Feedback ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<double> CalculateCommunicationSuccessRateAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                COUNT(*) as Total,
                SUM(CASE WHEN o.WasSuccessful = 1 THEN 1 ELSE 0 END) as Successful
            FROM CommunicationOutcomes o
            INNER JOIN Communications c ON o.CommunicationId = c.CommunicationId
            WHERE c.CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var total = reader.GetInt32(0);
            var successful = reader.GetInt32(1);

            return total > 0 ? (successful * 100.0) / total : 0.0;
        }

        return 0.0;
    }

    public async Task<(Crisis Crisis, List<Communication> Communications, List<CommunicationOutcome> Outcomes)?> GetCrisisHistoryAsync(int crisisId)
    {
        var crisis = await GetCrisisByIdAsync(crisisId);
        if (crisis == null)
            return null;

        var communications = await GetCommunicationsForCrisisAsync(crisisId);
        var outcomes = await GetOutcomesForCrisisAsync(crisisId);

        return (crisis, communications, outcomes);
    }

    public async Task CleanupOldCrisesAsync(string companyId, int daysToKeep = 90)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM Crises
            WHERE CompanyId = @CompanyId
              AND Stage IN ('Resolved', 'Ignored')
              AND ResolvedAt < @CutoffDate";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@CutoffDate", cutoffDate.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static Crisis MapCrisis(SqliteDataReader reader)
    {
        return new Crisis
        {
            CrisisId = reader.GetInt32(0),
            CompanyId = reader.GetString(1),
            CrisisType = reader.GetString(2),
            Stage = reader.GetString(3),
            Severity = reader.GetInt32(4),
            Description = reader.GetString(5),
            AffectedWorkers = reader.IsDBNull(6) ? null : reader.GetString(6),
            EscalationScore = reader.GetInt32(7),
            ResolutionAttempts = reader.GetInt32(8),
            CreatedAt = DateTime.Parse(reader.GetString(9)),
            ResolvedAt = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10))
        };
    }

    private static Communication MapCommunication(SqliteDataReader reader)
    {
        return new Communication
        {
            CommunicationId = reader.GetInt32(0),
            CompanyId = reader.GetString(1),
            CrisisId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            CommunicationType = reader.GetString(3),
            InitiatorId = reader.GetString(4),
            TargetId = reader.IsDBNull(5) ? null : reader.GetString(5),
            Message = reader.GetString(6),
            Tone = reader.GetString(7),
            SuccessChance = reader.GetInt32(8),
            CreatedAt = DateTime.Parse(reader.GetString(9))
        };
    }

    private static CommunicationOutcome MapCommunicationOutcome(SqliteDataReader reader)
    {
        return new CommunicationOutcome
        {
            OutcomeId = reader.GetInt32(0),
            CommunicationId = reader.GetInt32(1),
            WasSuccessful = reader.GetInt32(2) == 1,
            MoraleImpact = reader.GetInt32(3),
            RelationshipImpact = reader.GetInt32(4),
            CrisisEscalationChange = reader.GetInt32(5),
            Feedback = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7))
        };
    }
}

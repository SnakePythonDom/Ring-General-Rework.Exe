using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des propriétaires (Owners).
/// </summary>
public sealed class OwnerRepository : RepositoryBase, RingGeneral.Core.Interfaces.IOwnerRepository
{
    private readonly string _connectionString;

    public OwnerRepository(SqliteConnectionFactory factory) : base(factory)
    {
        _connectionString = factory.GetConnectionString();
    }

    public async Task SaveOwnerAsync(Owner owner)
    {
        if (!owner.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Owner invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Owners (
                OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                PreferredProductType, ShowFrequencyPreference,
                TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                Satisfaction, CreatedAt
            ) VALUES (
                @OwnerId, @CompanyId, @Name, @VisionType, @RiskTolerance,
                @PreferredProductType, @ShowFrequencyPreference,
                @TalentDevelopmentFocus, @FinancialPriority, @FanSatisfactionPriority,
                @Satisfaction, @CreatedAt
            )";

        command.Parameters.AddWithValue("@OwnerId", owner.OwnerId);
        command.Parameters.AddWithValue("@CompanyId", owner.CompanyId);
        command.Parameters.AddWithValue("@Name", owner.Name);
        command.Parameters.AddWithValue("@VisionType", owner.VisionType);
        command.Parameters.AddWithValue("@RiskTolerance", owner.RiskTolerance);
        command.Parameters.AddWithValue("@PreferredProductType", owner.PreferredProductType);
        command.Parameters.AddWithValue("@ShowFrequencyPreference", owner.ShowFrequencyPreference);
        command.Parameters.AddWithValue("@TalentDevelopmentFocus", owner.TalentDevelopmentFocus);
        command.Parameters.AddWithValue("@FinancialPriority", owner.FinancialPriority);
        command.Parameters.AddWithValue("@FanSatisfactionPriority", owner.FanSatisfactionPriority);
        command.Parameters.AddWithValue("@Satisfaction", owner.Satisfaction);
        command.Parameters.AddWithValue("@CreatedAt", owner.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Owner?> GetOwnerByIdAsync(string ownerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   Satisfaction, CreatedAt
            FROM Owners
            WHERE OwnerId = @OwnerId";

        command.Parameters.AddWithValue("@OwnerId", ownerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapOwner(reader);
        }

        return null;
    }

    public async Task<Owner?> GetOwnerByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   Satisfaction, CreatedAt
            FROM Owners
            WHERE CompanyId = @CompanyId";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapOwner(reader);
        }

        return null;
    }

    public async Task<List<Owner>> GetAllOwnersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   Satisfaction, CreatedAt
            FROM Owners
            ORDER BY Name";

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task<List<Owner>> GetOwnersByVisionTypeAsync(string visionType)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   Satisfaction, CreatedAt
            FROM Owners
            WHERE VisionType = @VisionType
            ORDER BY Name";

        command.Parameters.AddWithValue("@VisionType", visionType);

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task<List<Owner>> GetOwnersWithRiskToleranceAboveAsync(int minRiskTolerance)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT OwnerId, CompanyId, Name, VisionType, RiskTolerance,
                   PreferredProductType, ShowFrequencyPreference,
                   TalentDevelopmentFocus, FinancialPriority, FanSatisfactionPriority,
                   Satisfaction, CreatedAt
            FROM Owners
            WHERE RiskTolerance >= @MinRiskTolerance
            ORDER BY RiskTolerance DESC";

        command.Parameters.AddWithValue("@MinRiskTolerance", minRiskTolerance);

        var owners = new List<Owner>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            owners.Add(MapOwner(reader));
        }

        return owners;
    }

    public async Task UpdateOwnerAsync(Owner owner)
    {
        if (!owner.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Owner invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Owners SET
                CompanyId = @CompanyId,
                Name = @Name,
                VisionType = @VisionType,
                RiskTolerance = @RiskTolerance,
                PreferredProductType = @PreferredProductType,
                ShowFrequencyPreference = @ShowFrequencyPreference,
                TalentDevelopmentFocus = @TalentDevelopmentFocus,
                FinancialPriority = @FinancialPriority,
                FanSatisfactionPriority = @FanSatisfactionPriority,
                Satisfaction = @Satisfaction
            WHERE OwnerId = @OwnerId";

        command.Parameters.AddWithValue("@OwnerId", owner.OwnerId);
        command.Parameters.AddWithValue("@CompanyId", owner.CompanyId);
        command.Parameters.AddWithValue("@Name", owner.Name);
        command.Parameters.AddWithValue("@VisionType", owner.VisionType);
        command.Parameters.AddWithValue("@RiskTolerance", owner.RiskTolerance);
        command.Parameters.AddWithValue("@PreferredProductType", owner.PreferredProductType);
        command.Parameters.AddWithValue("@ShowFrequencyPreference", owner.ShowFrequencyPreference);
        command.Parameters.AddWithValue("@TalentDevelopmentFocus", owner.TalentDevelopmentFocus);
        command.Parameters.AddWithValue("@FinancialPriority", owner.FinancialPriority);
        command.Parameters.AddWithValue("@FanSatisfactionPriority", owner.FanSatisfactionPriority);
        command.Parameters.AddWithValue("@Satisfaction", owner.Satisfaction);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteOwnerAsync(string ownerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Owners WHERE OwnerId = @OwnerId";
        command.Parameters.AddWithValue("@OwnerId", ownerId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountOwnersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Owners";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> CompanyHasOwnerAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Owners WHERE CompanyId = @CompanyId";
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // Goal Management Implementation

    public async Task<List<OwnerGoal>> GetGoalsAsync(string ownerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT GoalId, OwnerId, Description, Metric, TargetValue, 
                   CurrentValue, Deadline, Status, TargetEntityId, Importance
            FROM OwnerGoals
            WHERE OwnerId = @OwnerId";
        command.Parameters.AddWithValue("@OwnerId", ownerId);

        var goals = new List<OwnerGoal>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            goals.Add(MapGoal(reader));
        }
        return goals;
    }

    public async Task AddGoalAsync(OwnerGoal goal)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO OwnerGoals (
                GoalId, OwnerId, Description, Metric, TargetValue, 
                CurrentValue, Deadline, Status, TargetEntityId, Importance
            ) VALUES (
                @GoalId, @OwnerId, @Description, @Metric, @TargetValue, 
                @CurrentValue, @Deadline, @Status, @TargetEntityId, @Importance
            )";

        command.Parameters.AddWithValue("@GoalId", goal.GoalId);
        command.Parameters.AddWithValue("@OwnerId", goal.OwnerId);
        command.Parameters.AddWithValue("@Description", goal.Description);
        command.Parameters.AddWithValue("@Metric", goal.Metric.ToString());
        command.Parameters.AddWithValue("@TargetValue", goal.TargetValue);
        command.Parameters.AddWithValue("@CurrentValue", goal.CurrentValue);
        command.Parameters.AddWithValue("@Deadline", goal.Deadline.ToString("O"));
        command.Parameters.AddWithValue("@Status", goal.Status.ToString());
        command.Parameters.AddWithValue("@TargetEntityId", (object?)goal.TargetEntityId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Importance", goal.Importance);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateGoalAsync(OwnerGoal goal)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE OwnerGoals SET
                CurrentValue = @CurrentValue,
                Status = @Status
            WHERE GoalId = @GoalId";

        command.Parameters.AddWithValue("@GoalId", goal.GoalId);
        command.Parameters.AddWithValue("@CurrentValue", goal.CurrentValue);
        command.Parameters.AddWithValue("@Status", goal.Status.ToString());

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static Owner MapOwner(SqliteDataReader reader)
    {
        return new Owner
        {
            OwnerId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            Name = reader.GetString(2),
            VisionType = reader.GetString(3),
            RiskTolerance = reader.GetInt32(4),
            PreferredProductType = reader.GetString(5),
            ShowFrequencyPreference = reader.GetString(6),
            TalentDevelopmentFocus = reader.GetInt32(7),
            FinancialPriority = reader.GetInt32(8),
            FanSatisfactionPriority = reader.GetInt32(9),
            Satisfaction = reader.GetInt32(10),
            CreatedAt = DateTime.Parse(reader.GetString(11))
        };
    }

    private static OwnerGoal MapGoal(SqliteDataReader reader)
    {
        return new OwnerGoal
        {
            GoalId = reader.GetString(0),
            OwnerId = reader.GetString(1),
            Description = reader.GetString(2),
            Metric = Enum.Parse<GoalMetric>(reader.GetString(3)),
            TargetValue = reader.GetDouble(4),
            CurrentValue = reader.GetDouble(5),
            Deadline = DateTime.Parse(reader.GetString(6)),
            Status = Enum.Parse<GoalStatus>(reader.GetString(7)),
            TargetEntityId = reader.IsDBNull(8) ? null : reader.GetString(8),
            Importance = reader.GetInt32(9)
        };
    }
}

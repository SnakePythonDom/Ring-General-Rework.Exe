using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Staff;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository du staff (StaffMember, CreativeStaff, StructuralStaff, Trainer).
/// </summary>
public sealed class StaffRepository : IStaffRepository
{
    private readonly string _connectionString;

    public StaffRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // STAFF MEMBER CRUD OPERATIONS
    // ====================================================================

    public async Task SaveStaffMemberAsync(StaffMember staffMember)
    {
        if (!staffMember.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StaffMember invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO StaffMembers (
                StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            ) VALUES (
                @StaffId, @CompanyId, @BrandId, @Name, @Role, @Department, @ExpertiseLevel,
                @YearsOfExperience, @SkillScore, @PersonalityScore, @AnnualSalary, @HireDate,
                @ContractEndDate, @EmploymentStatus, @IsActive, @Notes, @CreatedAt
            )";

        command.Parameters.AddWithValue("@StaffId", staffMember.StaffId);
        command.Parameters.AddWithValue("@CompanyId", staffMember.CompanyId);
        command.Parameters.AddWithValue("@BrandId", staffMember.BrandId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", staffMember.Name);
        command.Parameters.AddWithValue("@Role", staffMember.Role.ToString());
        command.Parameters.AddWithValue("@Department", staffMember.Department.ToString());
        command.Parameters.AddWithValue("@ExpertiseLevel", staffMember.ExpertiseLevel.ToString());
        command.Parameters.AddWithValue("@YearsOfExperience", staffMember.YearsOfExperience);
        command.Parameters.AddWithValue("@SkillScore", staffMember.SkillScore);
        command.Parameters.AddWithValue("@PersonalityScore", staffMember.PersonalityScore);
        command.Parameters.AddWithValue("@AnnualSalary", staffMember.AnnualSalary);
        command.Parameters.AddWithValue("@HireDate", staffMember.HireDate.ToString("O"));
        command.Parameters.AddWithValue("@ContractEndDate", staffMember.ContractEndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@EmploymentStatus", staffMember.EmploymentStatus);
        command.Parameters.AddWithValue("@IsActive", staffMember.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Notes", staffMember.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", staffMember.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<StaffMember?> GetStaffMemberByIdAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapStaffMember(reader);
        }

        return null;
    }

    public async Task<List<StaffMember>> GetStaffByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE CompanyId = @CompanyId
            ORDER BY Department, Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadStaffMembers(command);
    }

    public async Task<List<StaffMember>> GetActiveStaffByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE CompanyId = @CompanyId AND IsActive = 1 AND EmploymentStatus = 'Active'
            ORDER BY Department, Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadStaffMembers(command);
    }

    public async Task<List<StaffMember>> GetStaffByDepartmentAsync(string companyId, string department)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE CompanyId = @CompanyId AND Department = @Department AND IsActive = 1
            ORDER BY Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Department", department);

        return await ReadStaffMembers(command);
    }

    public async Task<List<StaffMember>> GetStaffByRoleAsync(string companyId, string role)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE CompanyId = @CompanyId AND Role = @Role AND IsActive = 1
            ORDER BY Name";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Role", role);

        return await ReadStaffMembers(command);
    }

    public async Task<List<StaffMember>> GetStaffByBrandIdAsync(string brandId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE BrandId = @BrandId AND IsActive = 1
            ORDER BY Department, Name";

        command.Parameters.AddWithValue("@BrandId", brandId);

        return await ReadStaffMembers(command);
    }

    public async Task UpdateStaffMemberAsync(StaffMember staffMember)
    {
        if (!staffMember.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StaffMember invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE StaffMembers SET
                BrandId = @BrandId,
                Name = @Name,
                Role = @Role,
                Department = @Department,
                ExpertiseLevel = @ExpertiseLevel,
                YearsOfExperience = @YearsOfExperience,
                SkillScore = @SkillScore,
                PersonalityScore = @PersonalityScore,
                AnnualSalary = @AnnualSalary,
                ContractEndDate = @ContractEndDate,
                EmploymentStatus = @EmploymentStatus,
                IsActive = @IsActive,
                Notes = @Notes
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffMember.StaffId);
        command.Parameters.AddWithValue("@BrandId", staffMember.BrandId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", staffMember.Name);
        command.Parameters.AddWithValue("@Role", staffMember.Role.ToString());
        command.Parameters.AddWithValue("@Department", staffMember.Department.ToString());
        command.Parameters.AddWithValue("@ExpertiseLevel", staffMember.ExpertiseLevel.ToString());
        command.Parameters.AddWithValue("@YearsOfExperience", staffMember.YearsOfExperience);
        command.Parameters.AddWithValue("@SkillScore", staffMember.SkillScore);
        command.Parameters.AddWithValue("@PersonalityScore", staffMember.PersonalityScore);
        command.Parameters.AddWithValue("@AnnualSalary", staffMember.AnnualSalary);
        command.Parameters.AddWithValue("@ContractEndDate", staffMember.ContractEndDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@EmploymentStatus", staffMember.EmploymentStatus);
        command.Parameters.AddWithValue("@IsActive", staffMember.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Notes", staffMember.Notes ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateEmploymentStatusAsync(string staffId, string status)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE StaffMembers SET EmploymentStatus = @Status WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@Status", status);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteStaffMemberAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StaffMembers WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // CREATIVE STAFF OPERATIONS
    // ====================================================================

    public async Task SaveCreativeStaffAsync(CreativeStaff creativeStaff)
    {
        if (!creativeStaff.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"CreativeStaff invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO CreativeStaff (
                StaffId, CompanyId, BookerId, CreativityScore, ConsistencyScore, PreferredStyle,
                WorkerBias, LongTermStorylinePreference, CreativeRiskTolerance, BookerCompatibilityScore,
                GimmickPreferences, CanRuinStorylines, ProposedStorylines, ProposalAcceptanceRate,
                Specialty, CreatedAt
            ) VALUES (
                @StaffId, @CompanyId, @BookerId, @CreativityScore, @ConsistencyScore, @PreferredStyle,
                @WorkerBias, @LongTermStorylinePreference, @CreativeRiskTolerance, @BookerCompatibilityScore,
                @GimmickPreferences, @CanRuinStorylines, @ProposedStorylines, @ProposalAcceptanceRate,
                @Specialty, @CreatedAt
            )";

        command.Parameters.AddWithValue("@StaffId", creativeStaff.StaffId);
        command.Parameters.AddWithValue("@CompanyId", creativeStaff.CompanyId);
        command.Parameters.AddWithValue("@BookerId", creativeStaff.BookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreativityScore", creativeStaff.CreativityScore);
        command.Parameters.AddWithValue("@ConsistencyScore", creativeStaff.ConsistencyScore);
        command.Parameters.AddWithValue("@PreferredStyle", creativeStaff.PreferredStyle.ToString());
        command.Parameters.AddWithValue("@WorkerBias", creativeStaff.WorkerBias.ToString());
        command.Parameters.AddWithValue("@LongTermStorylinePreference", creativeStaff.LongTermStorylinePreference);
        command.Parameters.AddWithValue("@CreativeRiskTolerance", creativeStaff.CreativeRiskTolerance);
        command.Parameters.AddWithValue("@BookerCompatibilityScore", creativeStaff.BookerCompatibilityScore);
        command.Parameters.AddWithValue("@GimmickPreferences", creativeStaff.GimmickPreferences);
        command.Parameters.AddWithValue("@CanRuinStorylines", creativeStaff.CanRuinStorylines ? 1 : 0);
        command.Parameters.AddWithValue("@ProposedStorylines", creativeStaff.ProposedStorylines ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ProposalAcceptanceRate", creativeStaff.ProposalAcceptanceRate);
        command.Parameters.AddWithValue("@Specialty", creativeStaff.Specialty ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", creativeStaff.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<CreativeStaff?> GetCreativeStaffByIdAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BookerId, CreativityScore, ConsistencyScore, PreferredStyle,
                   WorkerBias, LongTermStorylinePreference, CreativeRiskTolerance, BookerCompatibilityScore,
                   GimmickPreferences, CanRuinStorylines, ProposedStorylines, ProposalAcceptanceRate,
                   Specialty, CreatedAt
            FROM CreativeStaff
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCreativeStaff(reader);
        }

        return null;
    }

    public async Task<List<CreativeStaff>> GetCreativeStaffByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT cs.StaffId, cs.CompanyId, cs.BookerId, cs.CreativityScore, cs.ConsistencyScore, cs.PreferredStyle,
                   cs.WorkerBias, cs.LongTermStorylinePreference, cs.CreativeRiskTolerance, cs.BookerCompatibilityScore,
                   cs.GimmickPreferences, cs.CanRuinStorylines, cs.ProposedStorylines, cs.ProposalAcceptanceRate,
                   cs.Specialty, cs.CreatedAt
            FROM CreativeStaff cs
            INNER JOIN StaffMembers sm ON cs.StaffId = sm.StaffId
            WHERE cs.CompanyId = @CompanyId AND sm.IsActive = 1
            ORDER BY cs.BookerCompatibilityScore DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadCreativeStaff(command);
    }

    public async Task<List<CreativeStaff>> GetCreativeStaffByBookerIdAsync(string bookerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BookerId, CreativityScore, ConsistencyScore, PreferredStyle,
                   WorkerBias, LongTermStorylinePreference, CreativeRiskTolerance, BookerCompatibilityScore,
                   GimmickPreferences, CanRuinStorylines, ProposedStorylines, ProposalAcceptanceRate,
                   Specialty, CreatedAt
            FROM CreativeStaff
            WHERE BookerId = @BookerId
            ORDER BY BookerCompatibilityScore DESC";

        command.Parameters.AddWithValue("@BookerId", bookerId);

        return await ReadCreativeStaff(command);
    }

    public async Task UpdateCreativeStaffAsync(CreativeStaff creativeStaff)
    {
        if (!creativeStaff.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"CreativeStaff invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CreativeStaff SET
                BookerId = @BookerId,
                CreativityScore = @CreativityScore,
                ConsistencyScore = @ConsistencyScore,
                PreferredStyle = @PreferredStyle,
                WorkerBias = @WorkerBias,
                LongTermStorylinePreference = @LongTermStorylinePreference,
                CreativeRiskTolerance = @CreativeRiskTolerance,
                BookerCompatibilityScore = @BookerCompatibilityScore,
                GimmickPreferences = @GimmickPreferences,
                CanRuinStorylines = @CanRuinStorylines,
                ProposedStorylines = @ProposedStorylines,
                ProposalAcceptanceRate = @ProposalAcceptanceRate,
                Specialty = @Specialty
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", creativeStaff.StaffId);
        command.Parameters.AddWithValue("@BookerId", creativeStaff.BookerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreativityScore", creativeStaff.CreativityScore);
        command.Parameters.AddWithValue("@ConsistencyScore", creativeStaff.ConsistencyScore);
        command.Parameters.AddWithValue("@PreferredStyle", creativeStaff.PreferredStyle.ToString());
        command.Parameters.AddWithValue("@WorkerBias", creativeStaff.WorkerBias.ToString());
        command.Parameters.AddWithValue("@LongTermStorylinePreference", creativeStaff.LongTermStorylinePreference);
        command.Parameters.AddWithValue("@CreativeRiskTolerance", creativeStaff.CreativeRiskTolerance);
        command.Parameters.AddWithValue("@BookerCompatibilityScore", creativeStaff.BookerCompatibilityScore);
        command.Parameters.AddWithValue("@GimmickPreferences", creativeStaff.GimmickPreferences);
        command.Parameters.AddWithValue("@CanRuinStorylines", creativeStaff.CanRuinStorylines ? 1 : 0);
        command.Parameters.AddWithValue("@ProposedStorylines", creativeStaff.ProposedStorylines ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ProposalAcceptanceRate", creativeStaff.ProposalAcceptanceRate);
        command.Parameters.AddWithValue("@Specialty", creativeStaff.Specialty ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateCompatibilityScoreAsync(string staffId, int compatibilityScore)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE CreativeStaff SET BookerCompatibilityScore = @Score WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@Score", compatibilityScore);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // STRUCTURAL STAFF OPERATIONS (Suite dans un autre message si besoin)
    // ====================================================================

    public async Task SaveStructuralStaffAsync(StructuralStaff structuralStaff)
    {
        if (!structuralStaff.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StructuralStaff invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO StructuralStaff (
                StaffId, CompanyId, EfficiencyScore, ProactivityScore, ExpertiseDomain, GlobalImpactAreas,
                InjuryRecoveryBonus, InjuryPreventionScore, CrisisManagementScore, ReputationBonus,
                DealNegotiationScore, CostReductionBonus, TalentDiscoveryScore, IndustryNetworkScore,
                MoraleBonus, ConflictResolutionScore, LitigationManagementScore, ContractNegotiationScore,
                SuccessfulInterventions, TotalInterventions, CreatedAt
            ) VALUES (
                @StaffId, @CompanyId, @EfficiencyScore, @ProactivityScore, @ExpertiseDomain, @GlobalImpactAreas,
                @InjuryRecoveryBonus, @InjuryPreventionScore, @CrisisManagementScore, @ReputationBonus,
                @DealNegotiationScore, @CostReductionBonus, @TalentDiscoveryScore, @IndustryNetworkScore,
                @MoraleBonus, @ConflictResolutionScore, @LitigationManagementScore, @ContractNegotiationScore,
                @SuccessfulInterventions, @TotalInterventions, @CreatedAt
            )";

        command.Parameters.AddWithValue("@StaffId", structuralStaff.StaffId);
        command.Parameters.AddWithValue("@CompanyId", structuralStaff.CompanyId);
        command.Parameters.AddWithValue("@EfficiencyScore", structuralStaff.EfficiencyScore);
        command.Parameters.AddWithValue("@ProactivityScore", structuralStaff.ProactivityScore);
        command.Parameters.AddWithValue("@ExpertiseDomain", structuralStaff.ExpertiseDomain);
        command.Parameters.AddWithValue("@GlobalImpactAreas", structuralStaff.GlobalImpactAreas);
        command.Parameters.AddWithValue("@InjuryRecoveryBonus", structuralStaff.InjuryRecoveryBonus);
        command.Parameters.AddWithValue("@InjuryPreventionScore", structuralStaff.InjuryPreventionScore);
        command.Parameters.AddWithValue("@CrisisManagementScore", structuralStaff.CrisisManagementScore);
        command.Parameters.AddWithValue("@ReputationBonus", structuralStaff.ReputationBonus);
        command.Parameters.AddWithValue("@DealNegotiationScore", structuralStaff.DealNegotiationScore);
        command.Parameters.AddWithValue("@CostReductionBonus", structuralStaff.CostReductionBonus);
        command.Parameters.AddWithValue("@TalentDiscoveryScore", structuralStaff.TalentDiscoveryScore);
        command.Parameters.AddWithValue("@IndustryNetworkScore", structuralStaff.IndustryNetworkScore);
        command.Parameters.AddWithValue("@MoraleBonus", structuralStaff.MoraleBonus);
        command.Parameters.AddWithValue("@ConflictResolutionScore", structuralStaff.ConflictResolutionScore);
        command.Parameters.AddWithValue("@LitigationManagementScore", structuralStaff.LitigationManagementScore);
        command.Parameters.AddWithValue("@ContractNegotiationScore", structuralStaff.ContractNegotiationScore);
        command.Parameters.AddWithValue("@SuccessfulInterventions", structuralStaff.SuccessfulInterventions);
        command.Parameters.AddWithValue("@TotalInterventions", structuralStaff.TotalInterventions);
        command.Parameters.AddWithValue("@CreatedAt", structuralStaff.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<StructuralStaff?> GetStructuralStaffByIdAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, EfficiencyScore, ProactivityScore, ExpertiseDomain, GlobalImpactAreas,
                   InjuryRecoveryBonus, InjuryPreventionScore, CrisisManagementScore, ReputationBonus,
                   DealNegotiationScore, CostReductionBonus, TalentDiscoveryScore, IndustryNetworkScore,
                   MoraleBonus, ConflictResolutionScore, LitigationManagementScore, ContractNegotiationScore,
                   SuccessfulInterventions, TotalInterventions, CreatedAt
            FROM StructuralStaff
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapStructuralStaff(reader);
        }

        return null;
    }

    public async Task<List<StructuralStaff>> GetStructuralStaffByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ss.StaffId, ss.CompanyId, ss.EfficiencyScore, ss.ProactivityScore, ss.ExpertiseDomain, ss.GlobalImpactAreas,
                   ss.InjuryRecoveryBonus, ss.InjuryPreventionScore, ss.CrisisManagementScore, ss.ReputationBonus,
                   ss.DealNegotiationScore, ss.CostReductionBonus, ss.TalentDiscoveryScore, ss.IndustryNetworkScore,
                   ss.MoraleBonus, ss.ConflictResolutionScore, ss.LitigationManagementScore, ss.ContractNegotiationScore,
                   ss.SuccessfulInterventions, ss.TotalInterventions, ss.CreatedAt
            FROM StructuralStaff ss
            INNER JOIN StaffMembers sm ON ss.StaffId = sm.StaffId
            WHERE ss.CompanyId = @CompanyId AND sm.IsActive = 1
            ORDER BY ss.ExpertiseDomain";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadStructuralStaff(command);
    }

    public async Task<List<StructuralStaff>> GetStructuralStaffByExpertiseAsync(string companyId, string expertiseDomain)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ss.StaffId, ss.CompanyId, ss.EfficiencyScore, ss.ProactivityScore, ss.ExpertiseDomain, ss.GlobalImpactAreas,
                   ss.InjuryRecoveryBonus, ss.InjuryPreventionScore, ss.CrisisManagementScore, ss.ReputationBonus,
                   ss.DealNegotiationScore, ss.CostReductionBonus, ss.TalentDiscoveryScore, ss.IndustryNetworkScore,
                   ss.MoraleBonus, ss.ConflictResolutionScore, ss.LitigationManagementScore, ss.ContractNegotiationScore,
                   ss.SuccessfulInterventions, ss.TotalInterventions, ss.CreatedAt
            FROM StructuralStaff ss
            INNER JOIN StaffMembers sm ON ss.StaffId = sm.StaffId
            WHERE ss.CompanyId = @CompanyId AND ss.ExpertiseDomain = @ExpertiseDomain AND sm.IsActive = 1";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@ExpertiseDomain", expertiseDomain);

        return await ReadStructuralStaff(command);
    }

    public async Task UpdateStructuralStaffAsync(StructuralStaff structuralStaff)
    {
        if (!structuralStaff.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"StructuralStaff invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE StructuralStaff SET
                EfficiencyScore = @EfficiencyScore,
                ProactivityScore = @ProactivityScore,
                ExpertiseDomain = @ExpertiseDomain,
                GlobalImpactAreas = @GlobalImpactAreas,
                InjuryRecoveryBonus = @InjuryRecoveryBonus,
                InjuryPreventionScore = @InjuryPreventionScore,
                CrisisManagementScore = @CrisisManagementScore,
                ReputationBonus = @ReputationBonus,
                DealNegotiationScore = @DealNegotiationScore,
                CostReductionBonus = @CostReductionBonus,
                TalentDiscoveryScore = @TalentDiscoveryScore,
                IndustryNetworkScore = @IndustryNetworkScore,
                MoraleBonus = @MoraleBonus,
                ConflictResolutionScore = @ConflictResolutionScore,
                LitigationManagementScore = @LitigationManagementScore,
                ContractNegotiationScore = @ContractNegotiationScore,
                SuccessfulInterventions = @SuccessfulInterventions,
                TotalInterventions = @TotalInterventions
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", structuralStaff.StaffId);
        command.Parameters.AddWithValue("@EfficiencyScore", structuralStaff.EfficiencyScore);
        command.Parameters.AddWithValue("@ProactivityScore", structuralStaff.ProactivityScore);
        command.Parameters.AddWithValue("@ExpertiseDomain", structuralStaff.ExpertiseDomain);
        command.Parameters.AddWithValue("@GlobalImpactAreas", structuralStaff.GlobalImpactAreas);
        command.Parameters.AddWithValue("@InjuryRecoveryBonus", structuralStaff.InjuryRecoveryBonus);
        command.Parameters.AddWithValue("@InjuryPreventionScore", structuralStaff.InjuryPreventionScore);
        command.Parameters.AddWithValue("@CrisisManagementScore", structuralStaff.CrisisManagementScore);
        command.Parameters.AddWithValue("@ReputationBonus", structuralStaff.ReputationBonus);
        command.Parameters.AddWithValue("@DealNegotiationScore", structuralStaff.DealNegotiationScore);
        command.Parameters.AddWithValue("@CostReductionBonus", structuralStaff.CostReductionBonus);
        command.Parameters.AddWithValue("@TalentDiscoveryScore", structuralStaff.TalentDiscoveryScore);
        command.Parameters.AddWithValue("@IndustryNetworkScore", structuralStaff.IndustryNetworkScore);
        command.Parameters.AddWithValue("@MoraleBonus", structuralStaff.MoraleBonus);
        command.Parameters.AddWithValue("@ConflictResolutionScore", structuralStaff.ConflictResolutionScore);
        command.Parameters.AddWithValue("@LitigationManagementScore", structuralStaff.LitigationManagementScore);
        command.Parameters.AddWithValue("@ContractNegotiationScore", structuralStaff.ContractNegotiationScore);
        command.Parameters.AddWithValue("@SuccessfulInterventions", structuralStaff.SuccessfulInterventions);
        command.Parameters.AddWithValue("@TotalInterventions", structuralStaff.TotalInterventions);

        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementInterventionsAsync(string staffId, bool successful)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = successful
            ? "UPDATE StructuralStaff SET SuccessfulInterventions = SuccessfulInterventions + 1, TotalInterventions = TotalInterventions + 1 WHERE StaffId = @StaffId"
            : "UPDATE StructuralStaff SET TotalInterventions = TotalInterventions + 1 WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        await command.ExecuteNonQueryAsync();
    }

    // Continues in next message with Trainer operations and helper methods...

    // ====================================================================
    // TRAINER OPERATIONS (Part 2)
    // ====================================================================

    public async Task SaveTrainerAsync(Trainer trainer)
    {
        if (!trainer.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Trainer invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Trainers (
                StaffId, CompanyId, InfrastructureId, TrainingSpecialization, TrainingEfficiency,
                ProgressionBonus, YouthDevelopmentScore, WrestlingExperience, WrestlingStyle,
                Reputation, CurrentStudents, MaxStudentCapacity, GraduatedStudents, FailedStudents,
                TeachingSpecialty, CanDevelopStars, CreatedAt
            ) VALUES (
                @StaffId, @CompanyId, @InfrastructureId, @TrainingSpecialization, @TrainingEfficiency,
                @ProgressionBonus, @YouthDevelopmentScore, @WrestlingExperience, @WrestlingStyle,
                @Reputation, @CurrentStudents, @MaxStudentCapacity, @GraduatedStudents, @FailedStudents,
                @TeachingSpecialty, @CanDevelopStars, @CreatedAt
            )";

        command.Parameters.AddWithValue("@StaffId", trainer.StaffId);
        command.Parameters.AddWithValue("@CompanyId", trainer.CompanyId);
        command.Parameters.AddWithValue("@InfrastructureId", trainer.InfrastructureId);
        command.Parameters.AddWithValue("@TrainingSpecialization", trainer.TrainingSpecialization);
        command.Parameters.AddWithValue("@TrainingEfficiency", trainer.TrainingEfficiency);
        command.Parameters.AddWithValue("@ProgressionBonus", trainer.ProgressionBonus);
        command.Parameters.AddWithValue("@YouthDevelopmentScore", trainer.YouthDevelopmentScore);
        command.Parameters.AddWithValue("@WrestlingExperience", trainer.WrestlingExperience);
        command.Parameters.AddWithValue("@WrestlingStyle", trainer.WrestlingStyle);
        command.Parameters.AddWithValue("@Reputation", trainer.Reputation);
        command.Parameters.AddWithValue("@CurrentStudents", trainer.CurrentStudents);
        command.Parameters.AddWithValue("@MaxStudentCapacity", trainer.MaxStudentCapacity);
        command.Parameters.AddWithValue("@GraduatedStudents", trainer.GraduatedStudents);
        command.Parameters.AddWithValue("@FailedStudents", trainer.FailedStudents);
        command.Parameters.AddWithValue("@TeachingSpecialty", trainer.TeachingSpecialty ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CanDevelopStars", trainer.CanDevelopStars ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", trainer.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Trainer?> GetTrainerByIdAsync(string staffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, InfrastructureId, TrainingSpecialization, TrainingEfficiency,
                   ProgressionBonus, YouthDevelopmentScore, WrestlingExperience, WrestlingStyle,
                   Reputation, CurrentStudents, MaxStudentCapacity, GraduatedStudents, FailedStudents,
                   TeachingSpecialty, CanDevelopStars, CreatedAt
            FROM Trainers
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapTrainer(reader);
        }

        return null;
    }

    public async Task<List<Trainer>> GetTrainersByCompanyIdAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.StaffId, t.CompanyId, t.InfrastructureId, t.TrainingSpecialization, t.TrainingEfficiency,
                   t.ProgressionBonus, t.YouthDevelopmentScore, t.WrestlingExperience, t.WrestlingStyle,
                   t.Reputation, t.CurrentStudents, t.MaxStudentCapacity, t.GraduatedStudents, t.FailedStudents,
                   t.TeachingSpecialty, t.CanDevelopStars, t.CreatedAt
            FROM Trainers t
            INNER JOIN StaffMembers sm ON t.StaffId = sm.StaffId
            WHERE t.CompanyId = @CompanyId AND sm.IsActive = 1
            ORDER BY t.Reputation DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadTrainers(command);
    }

    public async Task<List<Trainer>> GetTrainersByInfrastructureIdAsync(string infrastructureId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.StaffId, t.CompanyId, t.InfrastructureId, t.TrainingSpecialization, t.TrainingEfficiency,
                   t.ProgressionBonus, t.YouthDevelopmentScore, t.WrestlingExperience, t.WrestlingStyle,
                   t.Reputation, t.CurrentStudents, t.MaxStudentCapacity, t.GraduatedStudents, t.FailedStudents,
                   t.TeachingSpecialty, t.CanDevelopStars, t.CreatedAt
            FROM Trainers t
            INNER JOIN StaffMembers sm ON t.StaffId = sm.StaffId
            WHERE t.InfrastructureId = @InfrastructureId AND sm.IsActive = 1
            ORDER BY t.Reputation DESC";

        command.Parameters.AddWithValue("@InfrastructureId", infrastructureId);

        return await ReadTrainers(command);
    }

    public async Task<List<Trainer>> GetTrainersBySpecializationAsync(string companyId, string specialization)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.StaffId, t.CompanyId, t.InfrastructureId, t.TrainingSpecialization, t.TrainingEfficiency,
                   t.ProgressionBonus, t.YouthDevelopmentScore, t.WrestlingExperience, t.WrestlingStyle,
                   t.Reputation, t.CurrentStudents, t.MaxStudentCapacity, t.GraduatedStudents, t.FailedStudents,
                   t.TeachingSpecialty, t.CanDevelopStars, t.CreatedAt
            FROM Trainers t
            INNER JOIN StaffMembers sm ON t.StaffId = sm.StaffId
            WHERE t.CompanyId = @CompanyId AND t.TrainingSpecialization = @Specialization AND sm.IsActive = 1
            ORDER BY t.Reputation DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Specialization", specialization);

        return await ReadTrainers(command);
    }

    public async Task UpdateTrainerAsync(Trainer trainer)
    {
        if (!trainer.IsValid(out var errorMessage))
        {
            throw new ArgumentException($"Trainer invalide: {errorMessage}");
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Trainers SET
                InfrastructureId = @InfrastructureId,
                TrainingSpecialization = @TrainingSpecialization,
                TrainingEfficiency = @TrainingEfficiency,
                ProgressionBonus = @ProgressionBonus,
                YouthDevelopmentScore = @YouthDevelopmentScore,
                WrestlingExperience = @WrestlingExperience,
                WrestlingStyle = @WrestlingStyle,
                Reputation = @Reputation,
                CurrentStudents = @CurrentStudents,
                MaxStudentCapacity = @MaxStudentCapacity,
                GraduatedStudents = @GraduatedStudents,
                FailedStudents = @FailedStudents,
                TeachingSpecialty = @TeachingSpecialty,
                CanDevelopStars = @CanDevelopStars
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", trainer.StaffId);
        command.Parameters.AddWithValue("@InfrastructureId", trainer.InfrastructureId);
        command.Parameters.AddWithValue("@TrainingSpecialization", trainer.TrainingSpecialization);
        command.Parameters.AddWithValue("@TrainingEfficiency", trainer.TrainingEfficiency);
        command.Parameters.AddWithValue("@ProgressionBonus", trainer.ProgressionBonus);
        command.Parameters.AddWithValue("@YouthDevelopmentScore", trainer.YouthDevelopmentScore);
        command.Parameters.AddWithValue("@WrestlingExperience", trainer.WrestlingExperience);
        command.Parameters.AddWithValue("@WrestlingStyle", trainer.WrestlingStyle);
        command.Parameters.AddWithValue("@Reputation", trainer.Reputation);
        command.Parameters.AddWithValue("@CurrentStudents", trainer.CurrentStudents);
        command.Parameters.AddWithValue("@MaxStudentCapacity", trainer.MaxStudentCapacity);
        command.Parameters.AddWithValue("@GraduatedStudents", trainer.GraduatedStudents);
        command.Parameters.AddWithValue("@FailedStudents", trainer.FailedStudents);
        command.Parameters.AddWithValue("@TeachingSpecialty", trainer.TeachingSpecialty ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CanDevelopStars", trainer.CanDevelopStars ? 1 : 0);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateStudentCountAsync(string staffId, int currentStudents)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Trainers SET CurrentStudents = @CurrentStudents WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@CurrentStudents", currentStudents);

        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementGraduationStatsAsync(string staffId, bool graduated)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = graduated
            ? "UPDATE Trainers SET GraduatedStudents = GraduatedStudents + 1, CurrentStudents = CurrentStudents - 1 WHERE StaffId = @StaffId"
            : "UPDATE Trainers SET FailedStudents = FailedStudents + 1, CurrentStudents = CurrentStudents - 1 WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);

        await command.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // BUSINESS QUERIES
    // ====================================================================

    public async Task<int> CountActiveStaffByDepartmentAsync(string companyId, string department)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM StaffMembers
            WHERE CompanyId = @CompanyId AND Department = @Department AND IsActive = 1 AND EmploymentStatus = 'Active'";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Department", department);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<StaffMember>> GetExpiringContractsAsync(string companyId, int daysThreshold = 90)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var thresholdDate = DateTime.Now.AddDays(daysThreshold).ToString("O");

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt
            FROM StaffMembers
            WHERE CompanyId = @CompanyId
              AND IsActive = 1
              AND ContractEndDate IS NOT NULL
              AND ContractEndDate <= @ThresholdDate
            ORDER BY ContractEndDate";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@ThresholdDate", thresholdDate);

        return await ReadStaffMembers(command);
    }

    public async Task<double> CalculateMonthlyStaffCostAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT SUM(AnnualSalary) / 12.0
            FROM StaffMembers
            WHERE CompanyId = @CompanyId AND IsActive = 1 AND EmploymentStatus = 'Active'";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? 0.0 : Convert.ToDouble(result);
    }

    public async Task<List<CreativeStaff>> GetDangerousCreativeStaffAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT cs.StaffId, cs.CompanyId, cs.BookerId, cs.CreativityScore, cs.ConsistencyScore, cs.PreferredStyle,
                   cs.WorkerBias, cs.LongTermStorylinePreference, cs.CreativeRiskTolerance, cs.BookerCompatibilityScore,
                   cs.GimmickPreferences, cs.CanRuinStorylines, cs.ProposedStorylines, cs.ProposalAcceptanceRate,
                   cs.Specialty, cs.CreatedAt
            FROM CreativeStaff cs
            INNER JOIN StaffMembers sm ON cs.StaffId = sm.StaffId
            WHERE cs.CompanyId = @CompanyId
              AND sm.IsActive = 1
              AND cs.CanRuinStorylines = 1
            ORDER BY cs.BookerCompatibilityScore";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadCreativeStaff(command);
    }

    // ====================================================================
    // CHILD STAFF SYSTEM EXTENSIONS
    // ====================================================================

    public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffForSharingAsync(string companyId, DateTime period)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt,
                   CanBeShared, MobilityRating, SharingPreferences, ChildSpecializations
            FROM StaffMembers
            WHERE CompanyId = @CompanyId
              AND IsActive = 1
              AND EmploymentStatus = 'Active'
              AND CanBeShared = 1
              AND SkillScore >= 60
              AND (ContractEndDate IS NULL OR ContractEndDate > @Period)
            ORDER BY SkillScore DESC, YearsOfExperience DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@Period", period.ToString("O"));

        return await ReadStaffMembersWithChildExtensions(command);
    }

    public async Task<IReadOnlyList<StaffMember>> GetStaffWithSharingCapabilitiesAsync(string companyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT StaffId, CompanyId, BrandId, Name, Role, Department, ExpertiseLevel,
                   YearsOfExperience, SkillScore, PersonalityScore, AnnualSalary, HireDate,
                   ContractEndDate, EmploymentStatus, IsActive, Notes, CreatedAt,
                   CanBeShared, MobilityRating, SharingPreferences, ChildSpecializations
            FROM StaffMembers
            WHERE CompanyId = @CompanyId
              AND IsActive = 1
              AND EmploymentStatus = 'Active'
              AND CanBeShared = 1
            ORDER BY MobilityRating DESC, SkillScore DESC";

        command.Parameters.AddWithValue("@CompanyId", companyId);

        return await ReadStaffMembersWithChildExtensions(command);
    }

    public async Task UpdateStaffSharingPropertiesAsync(string staffId, bool canBeShared, StaffMobilityRating mobilityRating, string? sharingPreferences, string? childSpecializations)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE StaffMembers
            SET CanBeShared = @CanBeShared,
                MobilityRating = @MobilityRating,
                SharingPreferences = @SharingPreferences,
                ChildSpecializations = @ChildSpecializations
            WHERE StaffId = @StaffId";

        command.Parameters.AddWithValue("@StaffId", staffId);
        command.Parameters.AddWithValue("@CanBeShared", canBeShared ? 1 : 0);
        command.Parameters.AddWithValue("@MobilityRating", mobilityRating.ToString());
        command.Parameters.AddWithValue("@SharingPreferences", sharingPreferences ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ChildSpecializations", childSpecializations ?? (object)DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"StaffMember avec ID {staffId} introuvable");
        }
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static async Task<List<StaffMember>> ReadStaffMembers(SqliteCommand command)
    {
        var staffMembers = new List<StaffMember>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            staffMembers.Add(MapStaffMember(reader));
        }
        return staffMembers;
    }

    private static async Task<List<CreativeStaff>> ReadCreativeStaff(SqliteCommand command)
    {
        var creativeStaff = new List<CreativeStaff>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            creativeStaff.Add(MapCreativeStaff(reader));
        }
        return creativeStaff;
    }

    private static async Task<List<StructuralStaff>> ReadStructuralStaff(SqliteCommand command)
    {
        var structuralStaff = new List<StructuralStaff>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            structuralStaff.Add(MapStructuralStaff(reader));
        }
        return structuralStaff;
    }

    private static async Task<List<Trainer>> ReadTrainers(SqliteCommand command)
    {
        var trainers = new List<Trainer>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trainers.Add(MapTrainer(reader));
        }
        return trainers;
    }

    private static async Task<IReadOnlyList<StaffMember>> ReadStaffMembersWithChildExtensions(SqliteCommand command)
    {
        var staffMembers = new List<StaffMember>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            staffMembers.Add(MapStaffMemberWithChildExtensions(reader));
        }
        return staffMembers;
    }

    private static StaffMember MapStaffMember(SqliteDataReader reader)
    {
        return new StaffMember
        {
            StaffId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            BrandId = reader.IsDBNull(2) ? null : reader.GetString(2),
            Name = reader.GetString(3),
            Role = Enum.Parse<StaffRole>(reader.GetString(4)),
            Department = Enum.Parse<StaffDepartment>(reader.GetString(5)),
            ExpertiseLevel = Enum.Parse<StaffExpertiseLevel>(reader.GetString(6)),
            YearsOfExperience = reader.GetInt32(7),
            SkillScore = reader.GetInt32(8),
            PersonalityScore = reader.GetInt32(9),
            AnnualSalary = reader.GetDouble(10),
            HireDate = DateTime.Parse(reader.GetString(11)),
            ContractEndDate = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
            EmploymentStatus = reader.GetString(13),
            IsActive = reader.GetInt32(14) == 1,
            Notes = reader.IsDBNull(15) ? null : reader.GetString(15),
            CreatedAt = DateTime.Parse(reader.GetString(16)),
            CanBeShared = reader.GetInt32(17) == 1,
            MobilityRating = Enum.Parse<StaffMobilityRating>(reader.GetString(18)),
            SharingPreferences = reader.IsDBNull(19) ? null : reader.GetString(19),
            ChildSpecializations = reader.IsDBNull(20) ? null : reader.GetString(20)
        };
    }

    private static StaffMember MapStaffMemberWithChildExtensions(SqliteDataReader reader)
    {
        // Même mapping que MapStaffMember mais avec toutes les colonnes explicites
        return MapStaffMember(reader);
    }

    private static CreativeStaff MapCreativeStaff(SqliteDataReader reader)
    {
        return new CreativeStaff
        {
            StaffId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            BookerId = reader.IsDBNull(2) ? null : reader.GetString(2),
            CreativityScore = reader.GetInt32(3),
            ConsistencyScore = reader.GetInt32(4),
            PreferredStyle = Enum.Parse<ProductStyle>(reader.GetString(5)),
            WorkerBias = Enum.Parse<WorkerTypeBias>(reader.GetString(6)),
            LongTermStorylinePreference = reader.GetInt32(7),
            CreativeRiskTolerance = reader.GetInt32(8),
            BookerCompatibilityScore = reader.GetInt32(9),
            GimmickPreferences = reader.GetString(10),
            CanRuinStorylines = reader.GetInt32(11) == 1,
            ProposedStorylines = reader.IsDBNull(12) ? null : reader.GetString(12),
            ProposalAcceptanceRate = reader.GetInt32(13),
            Specialty = reader.IsDBNull(14) ? null : reader.GetString(14),
            CreatedAt = DateTime.Parse(reader.GetString(15))
        };
    }

    private static StructuralStaff MapStructuralStaff(SqliteDataReader reader)
    {
        return new StructuralStaff
        {
            StaffId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            EfficiencyScore = reader.GetInt32(2),
            ProactivityScore = reader.GetInt32(3),
            ExpertiseDomain = reader.GetString(4),
            GlobalImpactAreas = reader.GetString(5),
            InjuryRecoveryBonus = reader.GetInt32(6),
            InjuryPreventionScore = reader.GetInt32(7),
            CrisisManagementScore = reader.GetInt32(8),
            ReputationBonus = reader.GetInt32(9),
            DealNegotiationScore = reader.GetInt32(10),
            CostReductionBonus = reader.GetInt32(11),
            TalentDiscoveryScore = reader.GetInt32(12),
            IndustryNetworkScore = reader.GetInt32(13),
            MoraleBonus = reader.GetInt32(14),
            ConflictResolutionScore = reader.GetInt32(15),
            LitigationManagementScore = reader.GetInt32(16),
            ContractNegotiationScore = reader.GetInt32(17),
            SuccessfulInterventions = reader.GetInt32(18),
            TotalInterventions = reader.GetInt32(19),
            CreatedAt = DateTime.Parse(reader.GetString(20))
        };
    }

    private static Trainer MapTrainer(SqliteDataReader reader)
    {
        return new Trainer
        {
            StaffId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            InfrastructureId = reader.GetString(2),
            TrainingSpecialization = reader.GetString(3),
            TrainingEfficiency = reader.GetInt32(4),
            ProgressionBonus = reader.GetInt32(5),
            YouthDevelopmentScore = reader.GetInt32(6),
            WrestlingExperience = reader.GetInt32(7),
            WrestlingStyle = reader.GetString(8),
            Reputation = reader.GetInt32(9),
            CurrentStudents = reader.GetInt32(10),
            MaxStudentCapacity = reader.GetInt32(11),
            GraduatedStudents = reader.GetInt32(12),
            FailedStudents = reader.GetInt32(13),
            TeachingSpecialty = reader.IsDBNull(14) ? null : reader.GetString(14),
            CanDevelopStars = reader.GetInt32(15) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(16))
        };
    }
}

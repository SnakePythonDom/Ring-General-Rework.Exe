using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des filiales étendues
/// </summary>
public class ChildCompanyExtendedRepository : RepositoryBase, IChildCompanyExtendedRepository
{
    public ChildCompanyExtendedRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    public async Task SaveChildCompanyExtendedAsync(ChildCompanyExtended childCompany)
    {
        await Task.Run(() =>
        {
            if (!childCompany.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"ChildCompanyExtended invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO ChildCompaniesExtended (
                    ChildCompanyId, ParentCompanyId, Objective, HasFullAutonomy,
                    AssignedBookerId, IsLaboratory, TestStyle, NicheType,
                    CreatedAt, IsActive
                ) VALUES (
                    $childCompanyId, $parentCompanyId, $objective, $hasFullAutonomy,
                    $assignedBookerId, $isLaboratory, $testStyle, $nicheType,
                    $createdAt, $isActive
                )
                ON CONFLICT(ChildCompanyId) DO UPDATE SET
                    ParentCompanyId = $parentCompanyId, Objective = $objective,
                    HasFullAutonomy = $hasFullAutonomy, AssignedBookerId = $assignedBookerId,
                    IsLaboratory = $isLaboratory, TestStyle = $testStyle,
                    NicheType = $nicheType, IsActive = $isActive;";

            AjouterParametre(command, "$childCompanyId", childCompany.ChildCompanyId);
            AjouterParametre(command, "$parentCompanyId", childCompany.ParentCompanyId);
            AjouterParametre(command, "$objective", childCompany.Objective.ToString());
            AjouterParametre(command, "$hasFullAutonomy", childCompany.HasFullAutonomy ? 1 : 0);
            AjouterParametre(command, "$assignedBookerId", childCompany.AssignedBookerId);
            AjouterParametre(command, "$isLaboratory", childCompany.IsLaboratory ? 1 : 0);
            AjouterParametre(command, "$testStyle", childCompany.TestStyle);
            AjouterParametre(command, "$nicheType", childCompany.NicheType?.ToString());
            AjouterParametre(command, "$createdAt", childCompany.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$isActive", childCompany.IsActive ? 1 : 0);

            command.ExecuteNonQuery();
        });
    }

    public async Task<ChildCompanyExtended?> GetChildCompanyExtendedByIdAsync(string childCompanyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ChildCompanyId, ParentCompanyId, Objective, HasFullAutonomy,
                       AssignedBookerId, IsLaboratory, TestStyle, NicheType,
                       CreatedAt, IsActive
                FROM ChildCompaniesExtended
                WHERE ChildCompanyId = $childCompanyId
                LIMIT 1;";

            AjouterParametre(command, "$childCompanyId", childCompanyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapChildCompanyExtended(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<ChildCompanyExtended>> GetChildCompaniesByParentIdAsync(string parentCompanyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ChildCompanyId, ParentCompanyId, Objective, HasFullAutonomy,
                       AssignedBookerId, IsLaboratory, TestStyle, NicheType,
                       CreatedAt, IsActive
                FROM ChildCompaniesExtended
                WHERE ParentCompanyId = $parentCompanyId AND IsActive = 1
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$parentCompanyId", parentCompanyId);

            using var reader = command.ExecuteReader();
            var results = new List<ChildCompanyExtended>();

            while (reader.Read())
            {
                results.Add(MapChildCompanyExtended(reader));
            }

            return results;
        });
    }

    public async Task<IReadOnlyList<ChildCompanyExtended>> GetChildCompaniesByObjectiveAsync(string objective)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ChildCompanyId, ParentCompanyId, Objective, HasFullAutonomy,
                       AssignedBookerId, IsLaboratory, TestStyle, NicheType,
                       CreatedAt, IsActive
                FROM ChildCompaniesExtended
                WHERE Objective = $objective AND IsActive = 1
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$objective", objective);

            using var reader = command.ExecuteReader();
            var results = new List<ChildCompanyExtended>();

            while (reader.Read())
            {
                results.Add(MapChildCompanyExtended(reader));
            }

            return results;
        });
    }

    public async Task UpdateChildCompanyExtendedAsync(ChildCompanyExtended childCompany)
    {
        await SaveChildCompanyExtendedAsync(childCompany); // Utilise l'upsert
    }

    public async Task DeleteChildCompanyExtendedAsync(string childCompanyId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "UPDATE ChildCompaniesExtended SET IsActive = 0 WHERE ChildCompanyId = $childCompanyId;";
            AjouterParametre(command, "$childCompanyId", childCompanyId);
            command.ExecuteNonQuery();
        });
    }

    private static ChildCompanyExtended MapChildCompanyExtended(SqliteDataReader reader)
    {
        return new ChildCompanyExtended
        {
            ChildCompanyId = reader.GetString(0),
            ParentCompanyId = reader.GetString(1),
            Objective = Enum.Parse<ChildCompanyObjective>(reader.GetString(2)),
            HasFullAutonomy = reader.GetInt32(3) == 1,
            AssignedBookerId = reader.IsDBNull(4) ? null : reader.GetString(4),
            IsLaboratory = reader.GetInt32(5) == 1,
            TestStyle = reader.IsDBNull(6) ? null : reader.GetString(6),
            NicheType = reader.IsDBNull(7) ? null : Enum.Parse<NicheType>(reader.GetString(7)),
            CreatedAt = DateTime.Parse(reader.GetString(8)),
            IsActive = reader.GetInt32(9) == 1
        };
    }
}

using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Morale;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour la gestion des rumeurs backstage.
/// </summary>
public class RumorRepository : RepositoryBase, IRumorRepository
{
    public RumorRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    public async Task<Rumor?> GetRumorByIdAsync(int rumorId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, RumorType, RumorText,
                       Stage, Severity, AmplificationScore, CreatedAt
                FROM Rumors
                WHERE Id = $rumorId
                LIMIT 1;";

            AjouterParametre(command, "$rumorId", rumorId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRumor(reader);
            }

            return null;
        });
    }

    public async Task SaveRumorAsync(Rumor rumor)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO Rumors (
                    CompanyId, RumorType, RumorText,
                    Stage, Severity, AmplificationScore, CreatedAt
                ) VALUES (
                    $companyId, $rumorType, $rumorText,
                    $stage, $severity, $amplificationScore, $createdAt
                );
                SELECT last_insert_rowid();";

            AjouterParametre(command, "$companyId", rumor.CompanyId);
            AjouterParametre(command, "$rumorType", rumor.RumorType);
            AjouterParametre(command, "$rumorText", rumor.RumorText);
            AjouterParametre(command, "$stage", rumor.Stage);
            AjouterParametre(command, "$severity", rumor.Severity);
            AjouterParametre(command, "$amplificationScore", rumor.AmplificationScore);
            AjouterParametre(command, "$createdAt", rumor.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            rumor.Id = Convert.ToInt32(command.ExecuteScalar());
        });
    }

    public async Task UpdateRumorAsync(Rumor rumor)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                UPDATE Rumors
                SET CompanyId = $companyId,
                    RumorType = $rumorType,
                    RumorText = $rumorText,
                    Stage = $stage,
                    Severity = $severity,
                    AmplificationScore = $amplificationScore,
                    CreatedAt = $createdAt
                WHERE Id = $rumorId;";

            AjouterParametre(command, "$rumorId", rumor.Id);
            AjouterParametre(command, "$companyId", rumor.CompanyId);
            AjouterParametre(command, "$rumorType", rumor.RumorType);
            AjouterParametre(command, "$rumorText", rumor.RumorText);
            AjouterParametre(command, "$stage", rumor.Stage);
            AjouterParametre(command, "$severity", rumor.Severity);
            AjouterParametre(command, "$amplificationScore", rumor.AmplificationScore);
            AjouterParametre(command, "$createdAt", rumor.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<List<Rumor>> GetActiveRumorsAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, RumorType, RumorText,
                       Stage, Severity, AmplificationScore, CreatedAt
                FROM Rumors
                WHERE CompanyId = $companyId
                  AND Stage NOT IN ('Resolved', 'Ignored')
                ORDER BY Severity DESC, AmplificationScore DESC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var rumors = new List<Rumor>();

            while (reader.Read())
            {
                rumors.Add(MapRumor(reader));
            }

            return rumors;
        });
    }

    public async Task<List<Rumor>> GetAllRumorsByCompanyAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, RumorType, RumorText,
                       Stage, Severity, AmplificationScore, CreatedAt
                FROM Rumors
                WHERE CompanyId = $companyId
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var rumors = new List<Rumor>();

            while (reader.Read())
            {
                rumors.Add(MapRumor(reader));
            }

            return rumors;
        });
    }

    public async Task<List<Rumor>> GetRumorsByStageAsync(string companyId, string stage)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, RumorType, RumorText,
                       Stage, Severity, AmplificationScore, CreatedAt
                FROM Rumors
                WHERE CompanyId = $companyId
                  AND Stage = $stage
                ORDER BY Severity DESC, AmplificationScore DESC;";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$stage", stage);

            using var reader = command.ExecuteReader();
            var rumors = new List<Rumor>();

            while (reader.Read())
            {
                rumors.Add(MapRumor(reader));
            }

            return rumors;
        });
    }

    public async Task<List<Rumor>> GetWidespreadRumorsAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, CompanyId, RumorType, RumorText,
                       Stage, Severity, AmplificationScore, CreatedAt
                FROM Rumors
                WHERE CompanyId = $companyId
                  AND AmplificationScore >= 70
                  AND Stage NOT IN ('Resolved', 'Ignored')
                ORDER BY AmplificationScore DESC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var rumors = new List<Rumor>();

            while (reader.Read())
            {
                rumors.Add(MapRumor(reader));
            }

            return rumors;
        });
    }

    public async Task CleanupOldRumorsAsync(string companyId, int daysToKeep = 90)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

            command.CommandText = @"
                DELETE FROM Rumors
                WHERE CompanyId = $companyId
                  AND Stage IN ('Resolved', 'Ignored')
                  AND CreatedAt < $cutoffDate;";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$cutoffDate", cutoffDate.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    // === Mapping Methods ===

    private Rumor MapRumor(SqliteDataReader reader)
    {
        return new Rumor
        {
            Id = reader.GetInt32(0),
            CompanyId = reader.GetString(1),
            RumorType = reader.GetString(2),
            RumorText = reader.GetString(3),
            Stage = reader.GetString(4),
            Severity = reader.GetInt32(5),
            AmplificationScore = reader.GetInt32(6),
            CreatedAt = DateTime.Parse(reader.GetString(7))
        };
    }
}

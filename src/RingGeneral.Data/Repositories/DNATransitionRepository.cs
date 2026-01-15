using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des transitions d'ADN
/// </summary>
public class DNATransitionRepository : RepositoryBase, IDNATransitionRepository
{
    public DNATransitionRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    public async Task SaveDNATransitionAsync(DNATransition transition)
    {
        await Task.Run(() =>
        {
            if (!transition.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"DNATransition invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO DNATransitions (
                    TransitionId, CompanyId, StartDNAId, TargetDNAId,
                    CurrentWeek, TotalWeeks, ProgressPercentage, InertiaScore,
                    StartedAt, CompletedAt, IsActive
                ) VALUES (
                    $transitionId, $companyId, $startDNAId, $targetDNAId,
                    $currentWeek, $totalWeeks, $progressPercentage, $inertiaScore,
                    $startedAt, $completedAt, $isActive
                )
                ON CONFLICT(TransitionId) DO UPDATE SET
                    CurrentWeek = $currentWeek, ProgressPercentage = $progressPercentage,
                    CompletedAt = $completedAt, IsActive = $isActive;";

            AjouterParametre(command, "$transitionId", transition.TransitionId);
            AjouterParametre(command, "$companyId", transition.CompanyId);
            AjouterParametre(command, "$startDNAId", transition.StartDNAId);
            AjouterParametre(command, "$targetDNAId", transition.TargetDNAId);
            AjouterParametre(command, "$currentWeek", transition.CurrentWeek);
            AjouterParametre(command, "$totalWeeks", transition.TotalWeeks);
            AjouterParametre(command, "$progressPercentage", transition.ProgressPercentage);
            AjouterParametre(command, "$inertiaScore", transition.InertiaScore);
            AjouterParametre(command, "$startedAt", transition.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$completedAt", transition.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$isActive", transition.IsActive ? 1 : 0);

            command.ExecuteNonQuery();
        });
    }

    public async Task<DNATransition?> GetDNATransitionByIdAsync(string transitionId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TransitionId, CompanyId, StartDNAId, TargetDNAId,
                       CurrentWeek, TotalWeeks, ProgressPercentage, InertiaScore,
                       StartedAt, CompletedAt, IsActive
                FROM DNATransitions
                WHERE TransitionId = $transitionId
                LIMIT 1;";

            AjouterParametre(command, "$transitionId", transitionId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapDNATransition(reader);
            }

            return null;
        });
    }

    public async Task<DNATransition?> GetActiveTransitionByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TransitionId, CompanyId, StartDNAId, TargetDNAId,
                       CurrentWeek, TotalWeeks, ProgressPercentage, InertiaScore,
                       StartedAt, CompletedAt, IsActive
                FROM DNATransitions
                WHERE CompanyId = $companyId AND IsActive = 1
                ORDER BY StartedAt DESC
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapDNATransition(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<DNATransition>> GetTransitionsByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT TransitionId, CompanyId, StartDNAId, TargetDNAId,
                       CurrentWeek, TotalWeeks, ProgressPercentage, InertiaScore,
                       StartedAt, CompletedAt, IsActive
                FROM DNATransitions
                WHERE CompanyId = $companyId
                ORDER BY StartedAt DESC;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var results = new List<DNATransition>();

            while (reader.Read())
            {
                results.Add(MapDNATransition(reader));
            }

            return results;
        });
    }

    public async Task UpdateDNATransitionAsync(DNATransition transition)
    {
        await SaveDNATransitionAsync(transition); // Utilise l'upsert
    }

    public async Task DeleteDNATransitionAsync(string transitionId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "DELETE FROM DNATransitions WHERE TransitionId = $transitionId;";
            AjouterParametre(command, "$transitionId", transitionId);
            command.ExecuteNonQuery();
        });
    }

    private static DNATransition MapDNATransition(SqliteDataReader reader)
    {
        return new DNATransition
        {
            TransitionId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            StartDNAId = reader.GetString(2),
            TargetDNAId = reader.GetString(3),
            CurrentWeek = reader.GetInt32(4),
            TotalWeeks = reader.GetInt32(5),
            ProgressPercentage = reader.GetDouble(6),
            InertiaScore = reader.GetDouble(7),
            StartedAt = DateTime.Parse(reader.GetString(8)),
            CompletedAt = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
            IsActive = reader.GetInt32(10) == 1
        };
    }
}

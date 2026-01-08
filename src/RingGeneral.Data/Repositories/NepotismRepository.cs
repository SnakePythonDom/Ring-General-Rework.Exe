using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour la gestion du népotisme et des décisions biaisées.
/// </summary>
public class NepotismRepository : RepositoryBase, INepotismRepository
{
    public NepotismRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    // === WorkerRelation Nepotism Extensions ===

    public async Task UpdateRelationNepotismAttributesAsync(int relationId, bool isHidden, int biasStrength, string? originEvent, string? lastImpact)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                UPDATE WorkerRelations
                SET IsHidden = $isHidden,
                    BiasStrength = $biasStrength,
                    OriginEvent = $originEvent,
                    LastImpact = $lastImpact
                WHERE Id = $relationId;";

            AjouterParametre(command, "$relationId", relationId);
            AjouterParametre(command, "$isHidden", isHidden);
            AjouterParametre(command, "$biasStrength", biasStrength);
            AjouterParametre(command, "$originEvent", originEvent);
            AjouterParametre(command, "$lastImpact", lastImpact);

            command.ExecuteNonQuery();
        });
    }

    public async Task<List<WorkerRelation>> GetStrongBiasRelationsAsync(string decisionMakerId, int minBiasStrength = 70)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // Convert decisionMakerId string to int for WorkerId comparison
            if (!int.TryParse(decisionMakerId, out int decisionMakerIntId))
            {
                return new List<WorkerRelation>();
            }

            command.CommandText = @"
                SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes,
                       IsPublic, CreatedDate, IsHidden, BiasStrength, OriginEvent, LastImpact
                FROM WorkerRelations
                WHERE (WorkerId1 = $decisionMakerId OR WorkerId2 = $decisionMakerId)
                  AND BiasStrength >= $minBiasStrength
                ORDER BY BiasStrength DESC;";

            AjouterParametre(command, "$decisionMakerId", decisionMakerIntId);
            AjouterParametre(command, "$minBiasStrength", minBiasStrength);

            using var reader = command.ExecuteReader();
            var relations = new List<WorkerRelation>();

            while (reader.Read())
            {
                relations.Add(MapWorkerRelation(reader));
            }

            return relations;
        });
    }

    // === NepotismImpact ===

    public async Task SaveNepotismImpactAsync(NepotismImpact impact)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO NepotismImpacts (
                    RelationId, ImpactType, TargetEntityId, DecisionMakerId,
                    Severity, IsVisible, Description, CreatedAt
                ) VALUES (
                    $relationId, $impactType, $targetEntityId, $decisionMakerId,
                    $severity, $isVisible, $description, $createdAt
                );
                SELECT last_insert_rowid();";

            AjouterParametre(command, "$relationId", impact.RelationId);
            AjouterParametre(command, "$impactType", impact.ImpactType);
            AjouterParametre(command, "$targetEntityId", impact.TargetEntityId);
            AjouterParametre(command, "$decisionMakerId", impact.DecisionMakerId);
            AjouterParametre(command, "$severity", impact.Severity);
            AjouterParametre(command, "$isVisible", impact.IsVisible);
            AjouterParametre(command, "$description", impact.Description);
            AjouterParametre(command, "$createdAt", impact.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            impact.Id = Convert.ToInt32(command.ExecuteScalar());
        });
    }

    public async Task<List<NepotismImpact>> GetImpactsByRelationAsync(int relationId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, RelationId, ImpactType, TargetEntityId, DecisionMakerId,
                       Severity, IsVisible, Description, CreatedAt
                FROM NepotismImpacts
                WHERE RelationId = $relationId
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$relationId", relationId);

            using var reader = command.ExecuteReader();
            var impacts = new List<NepotismImpact>();

            while (reader.Read())
            {
                impacts.Add(MapNepotismImpact(reader));
            }

            return impacts;
        });
    }

    public async Task<List<NepotismImpact>> GetVisibleImpactsByCompanyAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // For now, return all visible impacts (companyId filtering would need workers table join)
            command.CommandText = @"
                SELECT Id, RelationId, ImpactType, TargetEntityId, DecisionMakerId,
                       Severity, IsVisible, Description, CreatedAt
                FROM NepotismImpacts
                WHERE IsVisible = 1
                ORDER BY CreatedAt DESC;";

            using var reader = command.ExecuteReader();
            var impacts = new List<NepotismImpact>();

            while (reader.Read())
            {
                impacts.Add(MapNepotismImpact(reader));
            }

            return impacts;
        });
    }

    public async Task<List<NepotismImpact>> GetRecentImpactsAsync(string companyId, int daysBack = 30)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            var cutoffDate = DateTime.Now.AddDays(-daysBack);

            command.CommandText = @"
                SELECT Id, RelationId, ImpactType, TargetEntityId, DecisionMakerId,
                       Severity, IsVisible, Description, CreatedAt
                FROM NepotismImpacts
                WHERE CreatedAt >= $cutoffDate
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$cutoffDate", cutoffDate.ToString("yyyy-MM-dd HH:mm:ss"));

            using var reader = command.ExecuteReader();
            var impacts = new List<NepotismImpact>();

            while (reader.Read())
            {
                impacts.Add(MapNepotismImpact(reader));
            }

            return impacts;
        });
    }

    // === BiasedDecision ===

    public async Task SaveBiasedDecisionAsync(BiasedDecision decision)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO BiasedDecisions (
                    DecisionType, TargetEntityId, DecisionMakerId,
                    IsBiased, BiasReason, Justification, CreatedAt
                ) VALUES (
                    $decisionType, $targetEntityId, $decisionMakerId,
                    $isBiased, $biasReason, $justification, $createdAt
                );
                SELECT last_insert_rowid();";

            AjouterParametre(command, "$decisionType", decision.DecisionType);
            AjouterParametre(command, "$targetEntityId", decision.TargetEntityId);
            AjouterParametre(command, "$decisionMakerId", decision.DecisionMakerId);
            AjouterParametre(command, "$isBiased", decision.IsBiased);
            AjouterParametre(command, "$biasReason", decision.BiasReason);
            AjouterParametre(command, "$justification", decision.Justification);
            AjouterParametre(command, "$createdAt", decision.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            decision.Id = Convert.ToInt32(command.ExecuteScalar());
        });
    }

    public async Task<List<BiasedDecision>> GetDecisionsByTargetAsync(string targetEntityId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, DecisionType, TargetEntityId, DecisionMakerId,
                       IsBiased, BiasReason, Justification, CreatedAt
                FROM BiasedDecisions
                WHERE TargetEntityId = $targetEntityId
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$targetEntityId", targetEntityId);

            using var reader = command.ExecuteReader();
            var decisions = new List<BiasedDecision>();

            while (reader.Read())
            {
                decisions.Add(MapBiasedDecision(reader));
            }

            return decisions;
        });
    }

    public async Task<List<BiasedDecision>> GetBiasedDecisionsByMakerAsync(string decisionMakerId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, DecisionType, TargetEntityId, DecisionMakerId,
                       IsBiased, BiasReason, Justification, CreatedAt
                FROM BiasedDecisions
                WHERE DecisionMakerId = $decisionMakerId
                  AND IsBiased = 1
                ORDER BY CreatedAt DESC;";

            AjouterParametre(command, "$decisionMakerId", decisionMakerId);

            using var reader = command.ExecuteReader();
            var decisions = new List<BiasedDecision>();

            while (reader.Read())
            {
                decisions.Add(MapBiasedDecision(reader));
            }

            return decisions;
        });
    }

    public async Task<int> CountRecentBiasedDecisionsAsync(string decisionMakerId, int daysBack = 14)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            var cutoffDate = DateTime.Now.AddDays(-daysBack);

            command.CommandText = @"
                SELECT COUNT(*)
                FROM BiasedDecisions
                WHERE DecisionMakerId = $decisionMakerId
                  AND IsBiased = 1
                  AND CreatedAt >= $cutoffDate;";

            AjouterParametre(command, "$decisionMakerId", decisionMakerId);
            AjouterParametre(command, "$cutoffDate", cutoffDate.ToString("yyyy-MM-dd HH:mm:ss"));

            return Convert.ToInt32(command.ExecuteScalar());
        });
    }

    // === Mapping Methods ===

    private WorkerRelation MapWorkerRelation(SqliteDataReader reader)
    {
        return new WorkerRelation
        {
            Id = reader.GetInt32(0),
            WorkerId1 = reader.GetInt32(1),
            WorkerId2 = reader.GetInt32(2),
            RelationType = (RelationType)reader.GetInt32(3),
            RelationStrength = reader.GetInt32(4),
            Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsPublic = reader.GetBoolean(6),
            CreatedDate = DateTime.Parse(reader.GetString(7)),
            IsHidden = reader.GetBoolean(8),
            BiasStrength = reader.GetInt32(9),
            OriginEvent = reader.IsDBNull(10) ? null : reader.GetString(10),
            LastImpact = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    private NepotismImpact MapNepotismImpact(SqliteDataReader reader)
    {
        return new NepotismImpact
        {
            Id = reader.GetInt32(0),
            RelationId = reader.GetInt32(1),
            ImpactType = reader.GetString(2),
            TargetEntityId = reader.GetString(3),
            DecisionMakerId = reader.GetString(4),
            Severity = reader.GetInt32(5),
            IsVisible = reader.GetBoolean(6),
            Description = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = DateTime.Parse(reader.GetString(8))
        };
    }

    private BiasedDecision MapBiasedDecision(SqliteDataReader reader)
    {
        return new BiasedDecision
        {
            Id = reader.GetInt32(0),
            DecisionType = reader.GetString(1),
            TargetEntityId = reader.GetString(2),
            DecisionMakerId = reader.GetString(3),
            IsBiased = reader.GetBoolean(4),
            BiasReason = reader.IsDBNull(5) ? null : reader.GetString(5),
            Justification = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7))
        };
    }
}

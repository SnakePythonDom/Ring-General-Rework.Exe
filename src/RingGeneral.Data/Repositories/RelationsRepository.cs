using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository for managing worker relations and factions.
/// </summary>
public sealed class RelationsRepository : RepositoryBase, RingGeneral.Core.Interfaces.IRelationsRepository
{
    private readonly Dictionary<string, List<List<string>>> _cliqueCache = new();
    private readonly Dictionary<string, DateTime> _lastCliqueUpdate = new();

    public RelationsRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public List<List<string>> GetCliques(string companyId, List<string>? relevantWorkerIds = null)
    {
        if (_cliqueCache.TryGetValue(companyId, out var cached) &&
            _lastCliqueUpdate.TryGetValue(companyId, out var lastUpdate) &&
            (DateTime.Now - lastUpdate).TotalMinutes < 60)
        {
            return cached;
        }

        RefreshCliques(companyId, relevantWorkerIds);
        return _cliqueCache[companyId];
    }

    public void RefreshCliques(string companyId, List<string>? relevantWorkerIds = null)
    {
        // On récupère toutes les relations fortes de la compagnie
        var relations = GetAllStrongRelations();

        // Filter by roster if provided
        if (relevantWorkerIds != null && relevantWorkerIds.Any())
        {
            var workerSet = new HashSet<string>(relevantWorkerIds);
            relations = relations
                .Where(r => workerSet.Contains(r.WorkerId1) && workerSet.Contains(r.WorkerId2))
                .ToList();
        }

        var cliques = DetectCliques(relations);
        _cliqueCache[companyId] = cliques;
        _lastCliqueUpdate[companyId] = DateTime.Now;
    }

    private List<WorkerRelation> GetAllStrongRelations()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations
            WHERE RelationStrength >= 70";

        var relations = new List<WorkerRelation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relations.Add(MapRelation(reader));
        }

        return relations;
    }

    public List<WorkerRelation> GetAllRelations()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations";

        var relations = new List<WorkerRelation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relations.Add(MapRelation(reader));
        }

        return relations;
    }

    private List<List<string>> DetectCliques(List<WorkerRelation> relations)
    {
        // Simple algorithm: Group workers who have a strong relation
        var adjacency = new Dictionary<string, HashSet<string>>();
        foreach (var rel in relations)
        {
            if (!adjacency.ContainsKey(rel.WorkerId1)) adjacency[rel.WorkerId1] = new HashSet<string>();
            if (!adjacency.ContainsKey(rel.WorkerId2)) adjacency[rel.WorkerId2] = new HashSet<string>();
            adjacency[rel.WorkerId1].Add(rel.WorkerId2);
            adjacency[rel.WorkerId2].Add(rel.WorkerId1);
        }

        var cliques = new List<List<string>>();
        var visited = new HashSet<string>();

        foreach (var workerId in adjacency.Keys)
        {
            if (visited.Contains(workerId)) continue;

            var currentClique = new List<string>();
            var queue = new Queue<string>();
            queue.Enqueue(workerId);
            visited.Add(workerId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                currentClique.Add(current);

                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            cliques.Add(currentClique);
        }

        return cliques;
    }

    // ====================================================================
    // WORKER RELATIONS
    // ====================================================================

    public WorkerRelation? GetRelation(int relationId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations
            WHERE Id = $id";

        AjouterParametre(command, "$id", relationId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapRelation(reader);
        }

        return null;
    }

    public List<WorkerRelation> GetRelationsForWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations
            WHERE WorkerId1 = $workerId OR WorkerId2 = $workerId
            ORDER BY RelationStrength DESC";

        AjouterParametre(command, "$workerId", workerId);

        var relations = new List<WorkerRelation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relations.Add(MapRelation(reader));
        }

        return relations;
    }

    public List<WorkerRelation> GetRelationsByType(string workerId, RelationType relationType)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations
            WHERE (WorkerId1 = $workerId OR WorkerId2 = $workerId)
              AND RelationType = $relationType
            ORDER BY RelationStrength DESC";

        AjouterParametre(command, "$workerId", workerId);
        AjouterParametre(command, "$relationType", relationType.ToString());

        var relations = new List<WorkerRelation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relations.Add(MapRelation(reader));
        }

        return relations;
    }

    public void AddOrUpdateRelation(WorkerRelation relation)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();

        // Ensure proper ordering for lookup (string comparison)
        var w1 = relation.WorkerId1;
        var w2 = relation.WorkerId2;
        if (string.Compare(w1, w2) > 0)
        {
            (w1, w2) = (w2, w1);
        }

        command.CommandText = "SELECT Id FROM WorkerRelations WHERE WorkerId1 = $w1 AND WorkerId2 = $w2";
        AjouterParametre(command, "$w1", w1);
        AjouterParametre(command, "$w2", w2);

        var existingId = command.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value)
        {
            relation.Id = Convert.ToInt32(existingId);
            UpdateRelation(relation);
        }
        else
        {
            CreateRelation(relation);
        }
    }

    public int CreateRelation(WorkerRelation relation)
    {
        // Ensure WorkerId1 < WorkerId2 for consistency (string comparison)
        if (string.Compare(relation.WorkerId1, relation.WorkerId2) > 0)
        {
            (relation.WorkerId1, relation.WorkerId2) = (relation.WorkerId2, relation.WorkerId1);
        }

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                                       IsHidden, BiasStrength, OriginEvent, LastImpact)
            VALUES ($workerId1, $workerId2, $relationType, $relationStrength, $notes, $isPublic, $createdDate,
                    $isHidden, $biasStrength, $originEvent, $lastImpact);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId1", relation.WorkerId1);
        AjouterParametre(command, "$workerId2", relation.WorkerId2);
        AjouterParametre(command, "$relationType", relation.RelationType.ToString());
        AjouterParametre(command, "$relationStrength", relation.RelationStrength);
        AjouterParametre(command, "$notes", relation.Notes);
        AjouterParametre(command, "$isPublic", relation.IsPublic ? 1 : 0);
        AjouterParametre(command, "$createdDate", relation.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
        AjouterParametre(command, "$isHidden", relation.IsHidden ? 1 : 0);
        AjouterParametre(command, "$biasStrength", relation.BiasStrength);
        AjouterParametre(command, "$originEvent", relation.OriginEvent);
        AjouterParametre(command, "$lastImpact", relation.LastImpact);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateRelation(WorkerRelation relation)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE WorkerRelations
            SET RelationType = $relationType,
                RelationStrength = $relationStrength,
                Notes = $notes,
                IsPublic = $isPublic,
                IsHidden = $isHidden,
                BiasStrength = $biasStrength,
                OriginEvent = $originEvent,
                LastImpact = $lastImpact
            WHERE Id = $id";

        AjouterParametre(command, "$id", relation.Id);
        AjouterParametre(command, "$relationType", relation.RelationType.ToString());
        AjouterParametre(command, "$relationStrength", relation.RelationStrength);
        AjouterParametre(command, "$notes", relation.Notes);
        AjouterParametre(command, "$isPublic", relation.IsPublic ? 1 : 0);
        AjouterParametre(command, "$isHidden", relation.IsHidden ? 1 : 0);
        AjouterParametre(command, "$biasStrength", relation.BiasStrength);
        AjouterParametre(command, "$originEvent", relation.OriginEvent);
        AjouterParametre(command, "$lastImpact", relation.LastImpact);

        command.ExecuteNonQuery();
    }

    public void DeleteRelation(int relationId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM WorkerRelations WHERE Id = $id";
        AjouterParametre(command, "$id", relationId);
        command.ExecuteNonQuery();
    }

    public bool RelationExists(string workerId1, string workerId2)
    {
        // Ensure proper ordering (string comparison)
        if (string.Compare(workerId1, workerId2) > 0)
        {
            (workerId1, workerId2) = (workerId2, workerId1);
        }

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM WorkerRelations
            WHERE WorkerId1 = $workerId1 AND WorkerId2 = $workerId2";

        AjouterParametre(command, "$workerId1", workerId1);
        AjouterParametre(command, "$workerId2", workerId2);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public List<WorkerRelation> GetStrongRelations(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate,
                   IsHidden, BiasStrength, OriginEvent, LastImpact
            FROM WorkerRelations
            WHERE (WorkerId1 = $workerId OR WorkerId2 = $workerId)
              AND RelationStrength >= 70
            ORDER BY RelationStrength DESC";

        AjouterParametre(command, "$workerId", workerId);

        var relations = new List<WorkerRelation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relations.Add(MapRelation(reader));
        }

        return relations;
    }

    // ====================================================================
    // FACTIONS
    // ====================================================================

    public Faction? GetFaction(int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear, DisbandedWeek, DisbandedYear
            FROM Factions
            WHERE Id = $id";

        AjouterParametre(command, "$id", factionId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapFaction(reader);
        }

        return null;
    }

    public List<Faction> GetActiveFactions()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear, DisbandedWeek, DisbandedYear
            FROM Factions
            WHERE Status = 'Active'
            ORDER BY CreatedYear DESC, CreatedWeek DESC";

        var factions = new List<Faction>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factions.Add(MapFaction(reader));
        }

        return factions;
    }

    public List<Faction> GetAllFactions()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear, DisbandedWeek, DisbandedYear
            FROM Factions
            ORDER BY CreatedYear DESC, CreatedWeek DESC";

        var factions = new List<Faction>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factions.Add(MapFaction(reader));
        }

        return factions;
    }

    public int CreateFaction(Faction faction)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO Factions (Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear)
            VALUES ($name, $factionType, $leaderId, $status, $createdWeek, $createdYear);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$name", faction.Name);
        AjouterParametre(command, "$factionType", faction.FactionType.ToString());
        AjouterParametre(command, "$leaderId", faction.LeaderId);
        AjouterParametre(command, "$status", faction.Status.ToString());
        AjouterParametre(command, "$createdWeek", faction.CreatedWeek);
        AjouterParametre(command, "$createdYear", faction.CreatedYear);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateFaction(Faction faction)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE Factions
            SET Name = $name,
                FactionType = $factionType,
                LeaderId = $leaderId,
                Status = $status
            WHERE Id = $id";

        AjouterParametre(command, "$id", faction.Id);
        AjouterParametre(command, "$name", faction.Name);
        AjouterParametre(command, "$factionType", faction.FactionType.ToString());
        AjouterParametre(command, "$leaderId", faction.LeaderId);
        AjouterParametre(command, "$status", faction.Status.ToString());

        command.ExecuteNonQuery();
    }

    public void DeleteFaction(int factionId)
    {
        WithTransaction((conn, trans) =>
        {
            // First delete all faction members
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = "DELETE FROM FactionMembers WHERE FactionId = $factionId";
                AjouterParametre(cmd, "$factionId", factionId);
                cmd.ExecuteNonQuery();
            }

            // Then delete the faction
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = "DELETE FROM Factions WHERE Id = $id";
                AjouterParametre(cmd, "$id", factionId);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public void DisbandFaction(int factionId, int week, int year)
    {
        WithTransaction((conn, trans) =>
        {
            // Update faction status
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = @"
                    UPDATE Factions
                    SET Status = 'Disbanded',
                        DisbandedWeek = $week,
                        DisbandedYear = $year
                    WHERE Id = $id";
                AjouterParametre(cmd, "$id", factionId);
                AjouterParametre(cmd, "$week", week);
                AjouterParametre(cmd, "$year", year);
                cmd.ExecuteNonQuery();
            }

            // Remove all active members
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = @"
                    UPDATE FactionMembers
                    SET LeftWeek = $week,
                        LeftYear = $year
                    WHERE FactionId = $factionId
                      AND LeftWeek IS NULL";
                AjouterParametre(cmd, "$factionId", factionId);
                AjouterParametre(cmd, "$week", week);
                AjouterParametre(cmd, "$year", year);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public void SetFactionInactive(int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Factions SET Status = 'Inactive' WHERE Id = $id";
        AjouterParametre(command, "$id", factionId);
        command.ExecuteNonQuery();
    }

    public void ReactivateFaction(int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE Factions SET Status = 'Active' WHERE Id = $id";
        AjouterParametre(command, "$id", factionId);
        command.ExecuteNonQuery();
    }

    // ====================================================================
    // FACTION MEMBERS
    // ====================================================================

    public FactionMember? GetFactionMember(int memberId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear
            FROM FactionMembers
            WHERE Id = $id";

        AjouterParametre(command, "$id", memberId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapFactionMember(reader);
        }

        return null;
    }

    public List<FactionMember> GetFactionMembers(int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear
            FROM FactionMembers
            WHERE FactionId = $factionId
            ORDER BY JoinedYear, JoinedWeek";

        AjouterParametre(command, "$factionId", factionId);

        var members = new List<FactionMember>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            members.Add(MapFactionMember(reader));
        }

        return members;
    }

    public List<FactionMember> GetActiveFactionMembers(int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear
            FROM FactionMembers
            WHERE FactionId = $factionId AND LeftWeek IS NULL
            ORDER BY JoinedYear, JoinedWeek";

        AjouterParametre(command, "$factionId", factionId);

        var members = new List<FactionMember>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            members.Add(MapFactionMember(reader));
        }

        return members;
    }

    public List<FactionMember> GetWorkerFactionHistory(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear
            FROM FactionMembers
            WHERE WorkerId = $workerId
            ORDER BY JoinedYear DESC, JoinedWeek DESC";

        AjouterParametre(command, "$workerId", workerId);

        var memberships = new List<FactionMember>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            memberships.Add(MapFactionMember(reader));
        }

        return memberships;
    }

    public List<FactionMember> GetCurrentFactionMemberships(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear
            FROM FactionMembers
            WHERE WorkerId = $workerId AND LeftWeek IS NULL
            ORDER BY JoinedYear DESC, JoinedWeek DESC";

        AjouterParametre(command, "$workerId", workerId);

        var memberships = new List<FactionMember>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            memberships.Add(MapFactionMember(reader));
        }

        return memberships;
    }

    public int AddFactionMember(FactionMember member)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO FactionMembers (FactionId, WorkerId, JoinedWeek, JoinedYear, LeftWeek, LeftYear)
            VALUES ($factionId, $workerId, $joinedWeek, $joinedYear, $leftWeek, $leftYear);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$factionId", member.FactionId);
        AjouterParametre(command, "$workerId", member.WorkerId);
        AjouterParametre(command, "$joinedWeek", member.JoinedWeek);
        AjouterParametre(command, "$joinedYear", member.JoinedYear);
        AjouterParametre(command, "$leftWeek", member.LeftWeek);
        AjouterParametre(command, "$leftYear", member.LeftYear);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void RemoveFactionMember(int memberId, int week, int year)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            UPDATE FactionMembers
            SET LeftWeek = $week,
                LeftYear = $year
            WHERE Id = $id";

        AjouterParametre(command, "$id", memberId);
        AjouterParametre(command, "$week", week);
        AjouterParametre(command, "$year", year);

        command.ExecuteNonQuery();
    }

    public bool IsWorkerInFaction(string workerId, int factionId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM FactionMembers
            WHERE WorkerId = $workerId AND FactionId = $factionId AND LeftWeek IS NULL";

        AjouterParametre(command, "$workerId", workerId);
        AjouterParametre(command, "$factionId", factionId);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public List<Faction> GetFactionsLedByWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, FactionType, LeaderId, Status, CreatedWeek, CreatedYear, DisbandedWeek, DisbandedYear
            FROM Factions
            WHERE LeaderId = $workerId
            ORDER BY CreatedYear DESC, CreatedWeek DESC";

        AjouterParametre(command, "$workerId", workerId);

        var factions = new List<Faction>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factions.Add(MapFaction(reader));
        }

        return factions;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private static WorkerRelation MapRelation(SqliteDataReader reader)
    {
        return new WorkerRelation
        {
            Id = reader.GetInt32(0),
            WorkerId1 = reader.GetString(1),
            WorkerId2 = reader.GetString(2),
            RelationType = Enum.Parse<RelationType>(reader.GetString(3)),
            RelationStrength = reader.GetInt32(4),
            Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsPublic = reader.GetInt32(6) == 1,
            CreatedDate = DateTime.Parse(reader.GetString(7)),
            IsHidden = reader.FieldCount > 8 && !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
            BiasStrength = reader.FieldCount > 9 && !reader.IsDBNull(9) ? reader.GetInt32(9) : 0,
            OriginEvent = reader.FieldCount > 10 && !reader.IsDBNull(10) ? reader.GetString(10) : null,
            LastImpact = reader.FieldCount > 11 && !reader.IsDBNull(11) ? reader.GetString(11) : null
        };
    }

    private static Faction MapFaction(SqliteDataReader reader)
    {
        return new Faction
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            FactionType = Enum.Parse<FactionType>(reader.GetString(2)),
            LeaderId = reader.IsDBNull(3) ? null : reader.GetString(3),
            Status = Enum.Parse<FactionStatus>(reader.GetString(4)),
            CreatedWeek = reader.GetInt32(5),
            CreatedYear = reader.GetInt32(6),
            DisbandedWeek = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            DisbandedYear = reader.IsDBNull(8) ? null : reader.GetInt32(8)
        };
    }

    private static FactionMember MapFactionMember(SqliteDataReader reader)
    {
        return new FactionMember
        {
            Id = reader.GetInt32(0),
            FactionId = reader.GetInt32(1),
            WorkerId = reader.GetString(2),
            JoinedWeek = reader.GetInt32(3),
            JoinedYear = reader.GetInt32(4),
            LeftWeek = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            LeftYear = reader.IsDBNull(6) ? null : reader.GetInt32(6)
        };
    }
}

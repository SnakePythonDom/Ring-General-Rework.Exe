using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Relations;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository for managing worker relations and factions.
/// </summary>
public sealed class RelationsRepository : RepositoryBase, IRelationsRepository
{
    public RelationsRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    // ====================================================================
    // WORKER RELATIONS
    // ====================================================================

    public WorkerRelation? GetRelation(int relationId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate
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

    public List<WorkerRelation> GetRelationsForWorker(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate
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

    public List<WorkerRelation> GetRelationsByType(int workerId, RelationType relationType)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate
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

    public int CreateRelation(WorkerRelation relation)
    {
        // Ensure WorkerId1 < WorkerId2 for consistency
        if (relation.WorkerId1 > relation.WorkerId2)
        {
            (relation.WorkerId1, relation.WorkerId2) = (relation.WorkerId2, relation.WorkerId1);
        }

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO WorkerRelations (WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate)
            VALUES ($workerId1, $workerId2, $relationType, $relationStrength, $notes, $isPublic, $createdDate);
            SELECT last_insert_rowid()";

        AjouterParametre(command, "$workerId1", relation.WorkerId1);
        AjouterParametre(command, "$workerId2", relation.WorkerId2);
        AjouterParametre(command, "$relationType", relation.RelationType.ToString());
        AjouterParametre(command, "$relationStrength", relation.RelationStrength);
        AjouterParametre(command, "$notes", relation.Notes);
        AjouterParametre(command, "$isPublic", relation.IsPublic ? 1 : 0);
        AjouterParametre(command, "$createdDate", relation.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

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
                IsPublic = $isPublic
            WHERE Id = $id";

        AjouterParametre(command, "$id", relation.Id);
        AjouterParametre(command, "$relationType", relation.RelationType.ToString());
        AjouterParametre(command, "$relationStrength", relation.RelationStrength);
        AjouterParametre(command, "$notes", relation.Notes);
        AjouterParametre(command, "$isPublic", relation.IsPublic ? 1 : 0);

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

    public bool RelationExists(int workerId1, int workerId2)
    {
        // Ensure proper ordering
        if (workerId1 > workerId2)
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

    public List<WorkerRelation> GetStrongRelations(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkerId1, WorkerId2, RelationType, RelationStrength, Notes, IsPublic, CreatedDate
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

    public List<FactionMember> GetWorkerFactionHistory(int workerId)
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

    public List<FactionMember> GetCurrentFactionMemberships(int workerId)
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

    public bool IsWorkerInFaction(int workerId, int factionId)
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

    public List<Faction> GetFactionsLedByWorker(int workerId)
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
            WorkerId1 = reader.GetInt32(1),
            WorkerId2 = reader.GetInt32(2),
            RelationType = Enum.Parse<RelationType>(reader.GetString(3)),
            RelationStrength = reader.GetInt32(4),
            Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsPublic = reader.GetInt32(6) == 1,
            CreatedDate = DateTime.Parse(reader.GetString(7))
        };
    }

    private static Faction MapFaction(SqliteDataReader reader)
    {
        return new Faction
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            FactionType = Enum.Parse<FactionType>(reader.GetString(2)),
            LeaderId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
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
            WorkerId = reader.GetInt32(2),
            JoinedWeek = reader.GetInt32(3),
            JoinedYear = reader.GetInt32(4),
            LeftWeek = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            LeftYear = reader.IsDBNull(6) ? null : reader.GetInt32(6)
        };
    }
}

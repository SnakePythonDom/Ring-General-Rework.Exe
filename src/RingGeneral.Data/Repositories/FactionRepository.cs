using Dapper;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Relations;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Data.Repositories
{
    public class FactionRepository : IFactionRepository
    {
        private readonly string _connectionString;

        public FactionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IReadOnlyList<Faction> GetAllFactions()
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Factions";
            var factions = connection.Query<Faction>(sql).ToList();

            foreach (var faction in factions)
            {
                faction.Members = GetFactionMembers(faction.Id).ToList();
            }

            return factions;
        }

        public Faction? GetFaction(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM Factions WHERE Id = @Id";
            var faction = connection.QueryFirstOrDefault<Faction>(sql, new { Id = id });

            if (faction != null)
            {
                faction.Members = GetFactionMembers(faction.Id).ToList();
            }

            return faction;
        }

        public IReadOnlyList<FactionMember> GetFactionMembers(int factionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM FactionMembers WHERE FactionId = @FactionId";
            return connection.Query<FactionMember>(sql, new { FactionId = factionId }).ToList();
        }

        public IReadOnlyList<Faction> GetFactionsForWorker(string workerId)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                SELECT f.* FROM Factions f
                JOIN FactionMembers fm ON f.Id = fm.FactionId
                WHERE fm.WorkerId = @WorkerId AND (fm.LeftWeek IS NULL OR fm.LeftYear IS NULL)";

            var factions = connection.Query<Faction>(sql, new { WorkerId = workerId }).ToList();
            foreach (var faction in factions)
            {
                faction.Members = GetFactionMembers(faction.Id).ToList();
            }
            return factions;
        }

        public void AddFaction(Faction faction)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Factions (Name, LeaderId, FactionType, Status, CreatedWeek, CreatedYear)
                VALUES (@Name, @LeaderId, @FactionType, @Status, @CreatedWeek, @CreatedYear);
                SELECT last_insert_rowid();";

            faction.Id = connection.ExecuteScalar<int>(sql, faction);
        }

        public void UpdateFaction(Faction faction)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                UPDATE Factions 
                SET Name = @Name, LeaderId = @LeaderId, FactionType = @FactionType, 
                    Status = @Status, DisbandedWeek = @DisbandedWeek, DisbandedYear = @DisbandedYear
                WHERE Id = @Id";
            connection.Execute(sql, faction);
        }

        public void DeleteFaction(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Execute("DELETE FROM FactionMembers WHERE FactionId = @Id", new { Id = id });
            connection.Execute("DELETE FROM Factions WHERE Id = @Id", new { Id = id });
        }

        public void AddMember(FactionMember member)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO FactionMembers (FactionId, WorkerId, JoinedWeek, JoinedYear)
                VALUES (@FactionId, @WorkerId, @JoinedWeek, @JoinedYear);
                SELECT last_insert_rowid();";
            member.Id = connection.ExecuteScalar<int>(sql, member);
        }

        public void UpdateMember(FactionMember member)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                UPDATE FactionMembers 
                SET JoinedWeek = @JoinedWeek, JoinedYear = @JoinedYear, 
                    LeftWeek = @LeftWeek, LeftYear = @LeftYear
                WHERE Id = @Id";
            connection.Execute(sql, member);
        }

        public void RemoveMember(int memberId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Execute("DELETE FROM FactionMembers WHERE Id = @Id", new { Id = memberId });
        }
    }
}

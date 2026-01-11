using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class WorkerRepository : RepositoryBase
{
    public WorkerRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<WorkerBackstageProfile> ChargerBackstageRoster(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, nom || ' ' || prenom FROM workers WHERE company_id = $companyId;";
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var roster = new List<WorkerBackstageProfile>();
        while (reader.Read())
        {
            roster.Add(new WorkerBackstageProfile(reader.GetString(0), reader.GetString(1)));
        }

        return roster;
    }

    public IReadOnlyDictionary<string, int> ChargerMorales(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, morale FROM workers WHERE company_id = $companyId;";
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var morales = new Dictionary<string, int>();
        while (reader.Read())
        {
            morales[reader.GetString(0)] = reader.GetInt32(1);
        }

        return morales;
    }

    public int ChargerMorale(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT morale FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyDictionary<string, string> ChargerNomsWorkers()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT worker_id, nom || ' ' || prenom FROM workers;";
        using var reader = command.ExecuteReader();
        var noms = new Dictionary<string, string>();
        while (reader.Read())
        {
            noms[reader.GetString(0)] = reader.GetString(1);
        }

        return noms;
    }

    public int ChargerFatigueWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT fatigue FROM workers WHERE worker_id = $workerId;";
        command.Parameters.AddWithValue("$workerId", workerId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void RecupererFatigueHebdo()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "UPDATE workers SET fatigue = MAX(0, fatigue - 12);";
        command.ExecuteNonQuery();
    }

    public WorkerSnapshot? ChargerWorker(string workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv, Morale
            FROM Workers
            WHERE WorkerId = $workerId;
            """;
        command.Parameters.AddWithValue("$workerId", workerId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerSnapshot(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10));
        }
        return null;
    }

    public Worker? GetWorker(int id)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT WorkerId, Name, RingName, Nationality, Gender, BirthDate, RoleTv, InjuryStatus
            FROM Workers
            WHERE WorkerId = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var worker = new Worker
            {
                Id = id,
                Name = reader.IsDBNull(2) ? reader.GetString(1) : reader.GetString(2), // Use RingName if available, else Name
                RealName = reader.GetString(1),
                BirthCountry = reader.GetString(3),
                // ResidenceCountry could be set to same or left null
                TvRole = 50, // Default
                IsActive = true,
                IsInjured = !string.Equals(reader.GetString(7), "AUCUNE", StringComparison.OrdinalIgnoreCase)
            };

            // Gender
            if (!reader.IsDBNull(4))
            {
                var genderStr = reader.GetString(4);
                if (Enum.TryParse<Gender>(genderStr, true, out var gender))
                {
                    worker.Gender = gender;
                }
            }

            // Age from BirthDate
            if (!reader.IsDBNull(5))
            {
                if (DateTime.TryParse(reader.GetString(5), out var birthDate))
                {
                    worker.DateOfBirth = birthDate;
                    var today = DateTime.Today; // Should use Game Date ideally
                    var age = today.Year - birthDate.Year;
                    if (birthDate.Date > today.AddYears(-age)) age--;
                    worker.Age = age;
                }
            }
            else
            {
                worker.Age = 25; // Default age
            }

            // Defaults for missing columns
            worker.Height = 180;
            worker.Weight = 100;

            // Map RoleTv to PushLevel (Approximate)
            if (!reader.IsDBNull(6))
            {
                var role = reader.GetString(6);
                // Simple mapping logic
                if (role.Contains("Main", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.MainEvent;
                else if (role.Contains("Upper", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.UpperMid;
                else if (role.Contains("Mid", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.MidCard;
                else if (role.Contains("Lower", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.LowerMid;
                else if (role.Contains("Job", StringComparison.OrdinalIgnoreCase)) worker.PushLevel = PushLevel.Jobber;
            }

            return worker;
        }

        return null;
    }
}

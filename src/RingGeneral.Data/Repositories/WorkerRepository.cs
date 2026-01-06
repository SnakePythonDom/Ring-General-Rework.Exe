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
}

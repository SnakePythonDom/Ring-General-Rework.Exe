using Microsoft.Data.Sqlite;
using RingGeneral.Data.Database;
using Xunit;

namespace RingGeneral.Tests;

/// <summary>
/// Classe de base pour les tests d'intégration des repositories
/// Configure une base de données SQLite en mémoire pour chaque test
/// </summary>
public abstract class RepositoryTestBase : IDisposable
{
    protected readonly SqliteConnectionFactory ConnectionFactory;
    protected readonly SqliteConnection Connection;

    protected RepositoryTestBase()
    {
        // Créer une connexion SQLite en mémoire
        var connectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";
        Connection = new SqliteConnection(connectionString);
        Connection.Open();

        ConnectionFactory = new SqliteConnectionFactory(() => Connection);

        // Initialiser la base de données
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Créer les tables nécessaires pour les tests
        using var command = Connection.CreateCommand();

        // Table workers
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS workers (
                worker_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                nom TEXT NOT NULL,
                prenom TEXT NOT NULL,
                morale INTEGER DEFAULT 50,
                popularity INTEGER DEFAULT 50,
                health INTEGER DEFAULT 100
            );";
        command.ExecuteNonQuery();

        // Table companies
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS companies (
                company_id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                prestige INTEGER DEFAULT 50,
                reach INTEGER DEFAULT 50,
                audience_moyenne INTEGER DEFAULT 50
            );";
        command.ExecuteNonQuery();

        // Table shows
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS shows (
                show_id TEXT PRIMARY KEY,
                company_id TEXT NOT NULL,
                name TEXT NOT NULL,
                attendance INTEGER DEFAULT 0,
                revenue REAL DEFAULT 0.0,
                date TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Méthode utilitaire pour insérer des données de test
    /// </summary>
    protected void InsertTestData(string tableName, Dictionary<string, object> data)
    {
        using var command = Connection.CreateCommand();

        var columns = string.Join(", ", data.Keys);
        var parameters = string.Join(", ", data.Keys.Select(k => $"${k}"));
        var values = data.Keys.Select(k => data[k]).ToArray();

        command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

        for (int i = 0; i < data.Keys.Count; i++)
        {
            var key = data.Keys.ElementAt(i);
            command.Parameters.AddWithValue($"${key}", values[i]);
        }

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Méthode utilitaire pour nettoyer les données de test
    /// </summary>
    protected void ClearTestData(string tableName)
    {
        using var command = Connection.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName}";
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        Connection?.Dispose();
    }
}
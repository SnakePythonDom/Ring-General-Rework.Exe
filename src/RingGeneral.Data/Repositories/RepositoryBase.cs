using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public abstract class RepositoryBase
{
    protected readonly SqliteConnectionFactory _factory;

    // Cache pour l'existence des tables (clé: "DataSource::TableName")
    private static readonly ConcurrentDictionary<string, bool> _tableExistenceCache = new();

    // JsonSerializerOptions static readonly (explicitement immuable)
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected RepositoryBase(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    protected SqliteConnection OpenConnection()
    {
        return _factory.CreateGeneralConnection();
    }

    protected void WithTransaction(Action<SqliteConnection, SqliteTransaction> action)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        try
        {
            action(connexion, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    protected T WithTransaction<T>(Func<SqliteConnection, SqliteTransaction, T> func)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        try
        {
            var result = func(connexion, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    protected static void AjouterParametre(SqliteCommand commande, string nom, object? valeur)
    {
        commande.Parameters.AddWithValue(nom, valeur ?? DBNull.Value);
    }

    // === Helpers techniques (Catégorie A) ===

    protected static bool TableExiste(SqliteConnection connexion, string table)
    {
        // Ne pas mettre en cache pour les bases en mémoire (tests unitaires ou temporaires)
        if (string.IsNullOrEmpty(connexion.DataSource) ||
            connexion.DataSource.Contains(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return VerifierExistenceTable(connexion, table);
        }

        var key = $"{connexion.DataSource}::{table}";

        // Optimisation : Si on sait déjà que la table existe, on retourne true immédiatement.
        // On ne cache JAMAIS le résultat 'false' car la table peut être créée juste après le check.
        if (_tableExistenceCache.TryGetValue(key, out var exists) && exists)
        {
            return true;
        }

        // Vérification réelle en base
        var existsInDb = VerifierExistenceTable(connexion, table);

        // Si la table existe, on met en cache cette information (on suppose que les tables ne sont pas supprimées au runtime)
        if (existsInDb)
        {
            _tableExistenceCache.TryAdd(key, true);
        }

        return existsInDb;
    }

    private static bool VerifierExistenceTable(SqliteConnection connexion, string table)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return command.ExecuteScalar() is not null;
    }

    protected static bool ColonneExiste(SqliteConnection connexion, string table, string colonne)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), colonne, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    protected static void AjouterColonneSiAbsente(SqliteConnection connexion, string table, string colonne, string type)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1).Equals(colonne, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        using var alterCommand = connexion.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE {table} ADD COLUMN {colonne} {type};";
        alterCommand.ExecuteNonQuery();
    }

    protected static decimal LireDecimal(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? 0m : Convert.ToDecimal(reader.GetDouble(index));
    }

    protected static T? LireJson<T>(SqliteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(reader.GetString(index), JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    protected static string? SerializeJson<T>(T value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }

    protected static PayrollFrequency ConvertFrequence(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return PayrollFrequency.Hebdomadaire;
        }

        return valeur.Trim().ToLowerInvariant() switch
        {
            "mensuelle" => PayrollFrequency.Mensuelle,
            "mensuel" => PayrollFrequency.Mensuelle,
            "monthly" => PayrollFrequency.Mensuelle,
            _ => PayrollFrequency.Hebdomadaire
        };
    }
}

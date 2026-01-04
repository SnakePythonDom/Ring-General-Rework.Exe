using Microsoft.Data.Sqlite;

namespace RingGeneral.Data.Database;

public sealed class DbInitializer
{
    public const int SchemaVersionActuelle = 1;

    public void CreateDatabaseIfMissing(string cheminDb)
    {
        if (string.IsNullOrWhiteSpace(cheminDb))
        {
            throw new InvalidOperationException("Chemin de base de données invalide.");
        }

        var dossier = Path.GetDirectoryName(cheminDb);
        if (!string.IsNullOrWhiteSpace(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        if (!File.Exists(cheminDb))
        {
            using var connexion = new SqliteConnection($"Data Source={cheminDb}");
            connexion.Open();
        }

        ApplyMigrations(cheminDb);
    }

    public void ApplyMigrations(string cheminDb)
    {
        if (string.IsNullOrWhiteSpace(cheminDb))
        {
            throw new InvalidOperationException("Chemin de base de données invalide.");
        }

        using var connexion = new SqliteConnection($"Data Source={cheminDb}");
        connexion.Open();

        ActiverForeignKeys(connexion);
        AssurerTableVersion(connexion);

        var migrations = ChargerMigrations();
        var versionsAppliquees = ChargerVersionsAppliquees(connexion);

        foreach (var migration in migrations.OrderBy(m => m.Version))
        {
            if (versionsAppliquees.Contains(migration.Version))
            {
                continue;
            }

            using var transaction = connexion.BeginTransaction();
            using var command = connexion.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = migration.Sql;
            command.ExecuteNonQuery();

            using var versionCommand = connexion.CreateCommand();
            versionCommand.Transaction = transaction;
            versionCommand.CommandText = "INSERT INTO SchemaVersion (Version, AppliedAt) VALUES ($version, $appliedAt);";
            versionCommand.Parameters.AddWithValue("$version", migration.Version);
            versionCommand.Parameters.AddWithValue("$appliedAt", DateTimeOffset.UtcNow.ToString("O"));
            versionCommand.ExecuteNonQuery();

            transaction.Commit();
        }
    }

    private static void ActiverForeignKeys(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
    }

    private static void AssurerTableVersion(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS SchemaVersion (Version INTEGER PRIMARY KEY, AppliedAt TEXT NOT NULL);";
        command.ExecuteNonQuery();
    }

    private static HashSet<int> ChargerVersionsAppliquees(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Version FROM SchemaVersion;";
        using var reader = command.ExecuteReader();
        var versions = new HashSet<int>();
        while (reader.Read())
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private static IReadOnlyList<MigrationEntry> ChargerMigrations()
    {
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "data", "migrations"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "migrations")
        };

        var dossier = chemins.FirstOrDefault(Directory.Exists);
        if (dossier is null)
        {
            throw new InvalidOperationException("Impossible de trouver les scripts de migration (data/migrations).");
        }

        var fichiers = Directory.GetFiles(dossier, "*.sql", SearchOption.TopDirectoryOnly);
        var migrations = new List<MigrationEntry>();
        foreach (var fichier in fichiers)
        {
            var nom = Path.GetFileNameWithoutExtension(fichier);
            var version = ExtraireVersion(nom);
            if (version is null)
            {
                continue;
            }

            migrations.Add(new MigrationEntry(version.Value, File.ReadAllText(fichier)));
        }

        return migrations;
    }

    private static int? ExtraireVersion(string nom)
    {
        var prefix = new string(nom.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(prefix, out var version) ? version : null;
    }

    private sealed record MigrationEntry(int Version, string Sql);
}

using Microsoft.Data.Sqlite;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;

namespace RingGeneral.Data.Database;

public sealed class DbInitializer 
{
    public const int SchemaVersionActuelle = 3;

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
        var migrationsAppliquees = ChargerMigrationsAppliquees(connexion);

        foreach (var migration in migrations.OrderBy(m => m.Version).ThenBy(m => m.Nom))
        {
            if (migrationsAppliquees.Contains(migration.Nom))
            {
                continue;
            }

            using var transaction = connexion.BeginTransaction();
            try
            {
                using var command = connexion.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = migration.Sql;
                command.ExecuteNonQuery();

                using var versionCommand = connexion.CreateCommand();
                versionCommand.Transaction = transaction;
                versionCommand.CommandText = "INSERT INTO MigrationHistory (Name, Version, AppliedAt) VALUES ($name, $version, $appliedAt);";
                versionCommand.Parameters.AddWithValue("$name", migration.Nom);
                versionCommand.Parameters.AddWithValue("$version", migration.Version);
                versionCommand.Parameters.AddWithValue("$appliedAt", DateTimeOffset.UtcNow.ToString("O"));
                versionCommand.ExecuteNonQuery();

                transaction.Commit();
                Console.WriteLine($"Migration appliquée avec succès : {migration.Nom}");
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("duplicate column name"))
            {
                // Gestion gracieuse des colonnes dupliquées
                transaction.Rollback();
                Console.WriteLine($"Migration {migration.Nom} contient des colonnes déjà existantes, marquage comme appliquée");

                // Marquer la migration comme appliquée même si elle a échoué sur des colonnes dupliquées
                try
                {
                    using var versionCommand = connexion.CreateCommand();
                    versionCommand.CommandText = "INSERT INTO MigrationHistory (Name, Version, AppliedAt) VALUES ($name, $version, $appliedAt);";
                    versionCommand.Parameters.AddWithValue("$name", migration.Nom);
                    versionCommand.Parameters.AddWithValue("$version", migration.Version);
                    versionCommand.Parameters.AddWithValue("$appliedAt", DateTimeOffset.UtcNow.ToString("O"));
                    versionCommand.ExecuteNonQuery();
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Erreur lors du marquage de la migration {migration.Nom} comme appliquée : {innerEx.Message}");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"ERREUR lors de la migration {migration.Nom} : {ex.Message}");
                // On arrête tout pour éviter des incohérences.
                throw;
            }
        }

        // Correction d'urgence pour la migration 005 si elle a été sautée (doit être fait avant le seed)
        AssurerColonnesIdentity(connexion);

        // Seed des données de démonstration si la DB est vide
        DbSeeder.SeedIfEmpty(connexion);
    }

    private static void AssurerColonnesIdentity(SqliteConnection connexion)
    {
        string[] colonnes = { 
            "FoundedYear INTEGER DEFAULT 2024",
            "CompanySize TEXT DEFAULT 'Local'",
            "CurrentEra TEXT DEFAULT 'Foundation Era'",
            "CatchStyleId TEXT",
            "IsPlayerControlled INTEGER DEFAULT 0",
            "MonthlyBurnRate REAL DEFAULT 0.0"
        };

        foreach (var colDef in colonnes)
        {
            var nomColonne = colDef.Split(' ')[0];
            try
            {
                using var checkCmd = connexion.CreateCommand();
                checkCmd.CommandText = $"SELECT {nomColonne} FROM Companies LIMIT 0";
                checkCmd.ExecuteNonQuery();
            }
            catch
            {
                try
                {
                    using var alterCmd = connexion.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE Companies ADD COLUMN {colDef}";
                    alterCmd.ExecuteNonQuery();
                    Console.WriteLine($"Ajout de la colonne manquante : {nomColonne}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'ajout de la colonne {nomColonne} : {ex.Message}");
                }
            }
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
        // Création de la nouvelle table de suivi si elle n'existe pas
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS MigrationHistory (
                Name TEXT PRIMARY KEY,
                Version INTEGER NOT NULL,
                AppliedAt TEXT NOT NULL
            );";
        command.ExecuteNonQuery();

        // Migration des données depuis l'ancienne table SchemaVersion si elle existe
        try
        {
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SchemaVersion';";
            var hasOldTable = Convert.ToInt32(command.ExecuteScalar()) > 0;
            if (hasOldTable)
            {
                command.CommandText = @"
                    INSERT OR IGNORE INTO MigrationHistory (Name, Version, AppliedAt)
                    SELECT 'Migration_v' || Version, Version, AppliedAt FROM SchemaVersion;";
                command.ExecuteNonQuery();
            }
        }
        catch { /* Ignorer les erreurs de migration de table de version */ }
    }

    private static HashSet<string> ChargerMigrationsAppliquees(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Name FROM MigrationHistory;";
        using var reader = command.ExecuteReader();
        var versions = new HashSet<string>();
        while (reader.Read())
        {
            versions.Add(reader.GetString(0));
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
            Console.WriteLine("WARNING: migrations directory not found. Skipping migrations.");
            return Array.Empty<MigrationEntry>();
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

            migrations.Add(new MigrationEntry(version.Value, nom, File.ReadAllText(fichier)));
        }

        return migrations;
    }

    private static int? ExtraireVersion(string nom)
    {
        var prefix = new string(nom.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(prefix, out var version) ? version : null;
    }

    private sealed record MigrationEntry(int Version, string Nom, string Sql);
}

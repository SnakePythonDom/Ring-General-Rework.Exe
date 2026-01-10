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

        // NE PLUS appeler ApplyMigrations() automatiquement
        // Les migrations sont maintenant appliquées uniquement via DbManager tool
    }

    /// <summary>
    /// Initialise une base de données en copiant depuis un template statique pré-rempli.
    /// Utilisé pour créer ring_general.db au premier lancement.
    /// </summary>
    /// <param name="cheminDb">Chemin de destination de la base de données</param>
    /// <param name="templatePath">Chemin vers le template statique (ring_general_static.db)</param>
    public void InitializeFromStaticTemplate(string cheminDb, string templatePath)
    {
        if (string.IsNullOrWhiteSpace(cheminDb))
        {
            throw new InvalidOperationException("Chemin de base de données invalide.");
        }

        if (string.IsNullOrWhiteSpace(templatePath))
        {
            throw new InvalidOperationException("Chemin du template invalide.");
        }

        // Si la base existe déjà, ne rien faire
        if (File.Exists(cheminDb))
        {
            return;
        }

        // Créer le répertoire de destination si nécessaire
        var dossier = Path.GetDirectoryName(cheminDb);
        if (!string.IsNullOrWhiteSpace(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        // Vérifier que le template existe
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"Template statique introuvable : {templatePath}\n" +
                "Veuillez créer la base statique via DbManager tool avant le premier lancement.",
                templatePath);
        }

        // Copier le template vers la destination
        try
        {
            File.Copy(templatePath, cheminDb, overwrite: false);
            
            // Après la copie, vérifier si la base est vide et la remplir avec des données génériques
            using var connection = new SqliteConnection($"Data Source={cheminDb}");
            connection.Open();
            
            // Vérifier si la base est vide (pas de Companies ou Workers)
            if (IsDatabaseEmpty(connection))
            {
                DbSeeder.SeedIfEmpty(connection);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Impossible de copier le template vers {cheminDb}.\n" +
                $"Erreur : {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Vérifie si la base de données est vide (pas de Companies ou Workers)
    /// </summary>
    private static bool IsDatabaseEmpty(SqliteConnection connection)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            
            // Vérifier Companies
            cmd.CommandText = "SELECT COUNT(*) FROM Companies";
            var companiesCount = Convert.ToInt64(cmd.ExecuteScalar());
            
            // Vérifier Workers
            cmd.CommandText = "SELECT COUNT(*) FROM Workers";
            var workersCount = Convert.ToInt64(cmd.ExecuteScalar());
            
            return companiesCount == 0 || workersCount == 0;
        }
        catch
        {
            // Si erreur (table n'existe pas), considérer comme vide
            return true;
        }
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
                // #region agent log
                var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DbInitializer.cs:58\",\"message\":\"Before migration execution\",\"data\":{{\"migrationName\":\"{migration.Nom}\",\"migrationVersion\":{migration.Version},\"sqlPreview\":\"{migration.Sql.Substring(0, Math.Min(100, migration.Sql.Length)).Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion
                
                using var command = connexion.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = migration.Sql;
                command.ExecuteNonQuery();
                
                // #region agent log
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DbInitializer.cs:64\",\"message\":\"Migration executed successfully\",\"data\":{{\"migrationName\":\"{migration.Nom}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion

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
                // #region agent log
                var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DbInitializer.cs:98\",\"message\":\"Migration failed\",\"data\":{{\"migrationName\":\"{migration.Nom}\",\"exceptionType\":\"{ex.GetType().Name}\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\",\"sqliteErrorCode\":{(ex is Microsoft.Data.Sqlite.SqliteException sqliteEx ? sqliteEx.SqliteErrorCode.ToString() : "N/A")}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion
                
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
        // #region agent log
        var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
        File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:128\",\"message\":\"AssurerColonnesIdentity entry\",\"data\":{{}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
        // #endregion
        
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
            
            // #region agent log
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:142\",\"message\":\"Checking column\",\"data\":{{\"columnName\":\"{nomColonne}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion
            
            try
            {
                using var checkCmd = connexion.CreateCommand();
                checkCmd.CommandText = $"SELECT {nomColonne} FROM Companies LIMIT 0";
                checkCmd.ExecuteNonQuery();
                
                // #region agent log
                File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:146\",\"message\":\"Column exists\",\"data\":{{\"columnName\":\"{nomColonne}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                // #endregion
            }
            catch
            {
                try
                {
                    // #region agent log
                    File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:150\",\"message\":\"Column missing, adding\",\"data\":{{\"columnName\":\"{nomColonne}\",\"columnDef\":\"{colDef.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                    // #endregion
                    
                    using var alterCmd = connexion.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE Companies ADD COLUMN {colDef}";
                    alterCmd.ExecuteNonQuery();
                    Console.WriteLine($"Ajout de la colonne manquante : {nomColonne}");
                    
                    // #region agent log
                    File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:156\",\"message\":\"Column added successfully\",\"data\":{{\"columnName\":\"{nomColonne}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                    // #endregion
                }
                catch (Exception ex)
                {
                    // #region agent log
                    File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"DbInitializer.cs:160\",\"message\":\"Failed to add column\",\"data\":{{\"columnName\":\"{nomColonne}\",\"exceptionType\":\"{ex.GetType().Name}\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
                    // #endregion
                    
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

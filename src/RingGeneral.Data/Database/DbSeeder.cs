using Microsoft.Data.Sqlite;
using RingGeneral.Core.Services;

namespace RingGeneral.Data.Database;

/// <summary>
/// Service de seed de la base de données pour environnement de test/démo
/// </summary>
public static class DbSeeder
{
    private static ILoggingService? _logger;

    /// <summary>
    /// Configure le logger pour DbSeeder (optionnel)
    /// </summary>
    public static void SetLogger(ILoggingService logger) => _logger = logger;

    /// <summary>
    /// Seed la base de données si elle est vide
    /// </summary>
    public static void SeedIfEmpty(SqliteConnection connection)
    {
        if (!IsDatabaseEmpty(connection))
        {
            Log(LogLevel.Debug, "Base de données déjà peuplée. Seed ignoré.");
            return;
        }

        Log(LogLevel.Info, "Base de données vide détectée. Démarrage du seed...");

        // Chercher BAKI1.1.db dans plusieurs emplacements possibles
        string? bakiDbPath = FindBakiDatabase();

        if (bakiDbPath != null)
        {
            Log(LogLevel.Info, $"BAKI1.1.db trouvé à: {bakiDbPath}");
            Log(LogLevel.Info, "Import des données réelles depuis BAKI1.1.db...");

            try
            {
                DbBakiImporter.ImportFromBaki(connection, bakiDbPath);
                Log(LogLevel.Info, "Import BAKI terminé avec succès");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Erreur lors de l'import BAKI: {ex.Message}");
                Log(LogLevel.Warning, "Fallback vers données de démonstration...");
                SeedDemoData(connection);
            }
        }
        else
        {
            Log(LogLevel.Info, "BAKI1.1.db introuvable, utilisation des données de démonstration");
            SeedDemoData(connection);
        }

        Log(LogLevel.Info, "Seed terminé avec succès.");
    }

    /// <summary>
    /// Recherche BAKI1.1.db dans plusieurs emplacements
    /// </summary>
    private static string? FindBakiDatabase()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "BAKI1.1.db"),
            Path.Combine(AppContext.BaseDirectory, "BAKI1.1.db"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "BAKI1.1.db"),
            "BAKI1.1.db"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            catch
            {
                // Ignorer les erreurs de path
            }
        }

        return null;
    }

    /// <summary>
    /// Vérifie si la base de données est vide (aucun worker)
    /// </summary>
    private static bool IsDatabaseEmpty(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Workers";

        try
        {
            var count = Convert.ToInt64(cmd.ExecuteScalar());
            return count == 0;
        }
        catch
        {
            // Si la table n'existe pas encore, considérer comme vide
            return true;
        }
    }

    /// <summary>
    /// Génère des données de démonstration
    /// </summary>
    private static void SeedDemoData(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Créer une compagnie
            var companyId = SeedCompany(connection);
            Log(LogLevel.Debug, $"Compagnie créée: {companyId}");

            // 2. Créer des workers
            var workerIds = SeedWorkers(connection, companyId);
            Log(LogLevel.Debug, $"{workerIds.Count} workers créés");

            // 3. Créer des titres
            var titleIds = SeedTitles(connection, companyId, workerIds);
            Log(LogLevel.Debug, $"{titleIds.Count} titres créés");

            // 4. Créer un show
            var showId = SeedShow(connection, companyId);
            Log(LogLevel.Debug, $"Show créé: {showId}");

            transaction.Commit();
            Log(LogLevel.Debug, "Transaction committée.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Error.WriteLine($"[DbSeeder] Erreur lors du seed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Crée une compagnie de démo
    /// </summary>
    private static string SeedCompany(SqliteConnection connection)
    {
        var companyId = "COMP_WWE";

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Companies (CompanyId, Name, Region, Prestige, Treasury, CurrentWeek, IsPlayerControlled)
            VALUES (@id, @name, @region, @prestige, @treasury, @week, @player)";

        cmd.Parameters.AddWithValue("@id", companyId);
        cmd.Parameters.AddWithValue("@name", "World Wrestling Entertainment");
        cmd.Parameters.AddWithValue("@region", "USA");
        cmd.Parameters.AddWithValue("@prestige", 95);
        cmd.Parameters.AddWithValue("@treasury", 10_000_000.0);
        cmd.Parameters.AddWithValue("@week", 1);
        cmd.Parameters.AddWithValue("@player", 1);

        cmd.ExecuteNonQuery();

        return companyId;
    }

    /// <summary>
    /// Crée des workers de démo
    /// </summary>
    private static List<string> SeedWorkers(SqliteConnection connection, string companyId)
    {
        var workers = new List<(string id, string name, int inRing, int entertainment, int story, int popularity, string role)>
        {
            ("W_CENA", "John Cena", 85, 92, 88, 95, "Main Eventer"),
            ("W_ORTON", "Randy Orton", 88, 85, 86, 92, "Main Eventer"),
            ("W_PUNK", "CM Punk", 90, 88, 90, 88, "Upper Midcard"),
            ("W_ROCK", "The Rock", 82, 95, 92, 98, "Main Eventer"),
            ("W_AUSTIN", "Stone Cold Steve Austin", 86, 90, 89, 96, "Main Eventer"),
            ("W_TAKER", "The Undertaker", 88, 87, 91, 94, "Main Eventer"),
            ("W_HHH", "Triple H", 87, 86, 88, 90, "Main Eventer"),
            ("W_HBK", "Shawn Michaels", 92, 88, 87, 91, "Main Eventer"),
            ("W_ANGLE", "Kurt Angle", 95, 82, 85, 87, "Main Eventer"),
            ("W_EDGE", "Edge", 86, 84, 88, 86, "Upper Midcard"),
            ("W_JERICHO", "Chris Jericho", 88, 87, 89, 85, "Upper Midcard"),
            ("W_BENOIT", "Chris Benoit", 96, 78, 82, 84, "Upper Midcard"),
            ("W_EDDIE", "Eddie Guerrero", 91, 86, 87, 85, "Upper Midcard"),
            ("W_REY", "Rey Mysterio", 89, 82, 80, 83, "Midcard"),
            ("W_KANE", "Kane", 82, 80, 84, 82, "Upper Midcard"),
            ("W_SHOW", "Big Show", 78, 76, 79, 80, "Midcard"),
            ("W_BATISTA", "Batista", 80, 82, 81, 84, "Upper Midcard"),
            ("W_LESNAR", "Brock Lesnar", 88, 79, 83, 89, "Main Eventer"),
            ("W_RVD", "Rob Van Dam", 88, 84, 79, 82, "Midcard"),
            ("W_BOOKER", "Booker T", 84, 83, 82, 81, "Midcard")
        };

        var workerIds = new List<string>();

        foreach (var worker in workers)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Workers (WorkerId, FullName, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, Morale, TvRole)
                VALUES (@id, @name, @companyId, @inRing, @entertainment, @story, @popularity, @fatigue, @morale, @role)";

            cmd.Parameters.AddWithValue("@id", worker.id);
            cmd.Parameters.AddWithValue("@name", worker.name);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@inRing", worker.inRing);
            cmd.Parameters.AddWithValue("@entertainment", worker.entertainment);
            cmd.Parameters.AddWithValue("@story", worker.story);
            cmd.Parameters.AddWithValue("@popularity", worker.popularity);
            cmd.Parameters.AddWithValue("@fatigue", new Random().Next(10, 40));
            cmd.Parameters.AddWithValue("@morale", new Random().Next(70, 95));
            cmd.Parameters.AddWithValue("@role", worker.role);

            cmd.ExecuteNonQuery();
            workerIds.Add(worker.id);
        }

        return workerIds;
    }

    /// <summary>
    /// Crée des titres de démo
    /// </summary>
    private static List<string> SeedTitles(SqliteConnection connection, string companyId, List<string> workerIds)
    {
        var titles = new List<(string id, string name, int prestige, string? championId)>
        {
            ("T_WWE", "WWE Championship", 95, workerIds.Count > 0 ? workerIds[0] : null),
            ("T_WORLD", "World Heavyweight Championship", 92, workerIds.Count > 1 ? workerIds[1] : null),
            ("T_IC", "Intercontinental Championship", 78, null), // Vacant
            ("T_US", "United States Championship", 75, workerIds.Count > 2 ? workerIds[2] : null),
            ("T_TAG", "Tag Team Championship", 72, null)
        };

        var titleIds = new List<string>();

        foreach (var title in titles)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Titles (TitleId, Name, CompanyId, Prestige, CurrentChampionId)
                VALUES (@id, @name, @companyId, @prestige, @championId)";

            cmd.Parameters.AddWithValue("@id", title.id);
            cmd.Parameters.AddWithValue("@name", title.name);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@prestige", title.prestige);
            cmd.Parameters.AddWithValue("@championId", (object?)title.championId ?? DBNull.Value);

            cmd.ExecuteNonQuery();
            titleIds.Add(title.id);

            // Créer un reign actif si champion
            if (title.championId != null)
            {
                using var reignCmd = connection.CreateCommand();
                reignCmd.CommandText = @"
                    INSERT INTO TitleReigns (TitleId, WorkerId, StartWeek, DefenseCount, IsActive)
                    VALUES (@titleId, @workerId, @startWeek, @defenseCount, @isActive)";

                reignCmd.Parameters.AddWithValue("@titleId", title.id);
                reignCmd.Parameters.AddWithValue("@workerId", title.championId);
                reignCmd.Parameters.AddWithValue("@startWeek", 1);
                reignCmd.Parameters.AddWithValue("@defenseCount", new Random().Next(0, 5));
                reignCmd.Parameters.AddWithValue("@isActive", 1);

                reignCmd.ExecuteNonQuery();
            }
        }

        return titleIds;
    }

    /// <summary>
    /// Crée un show de démo
    /// </summary>
    private static string SeedShow(SqliteConnection connection, string companyId)
    {
        var showId = "SHOW_RAW_W1";

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Shows (ShowId, Name, CompanyId, Week, DurationMinutes, Location, Broadcast)
            VALUES (@id, @name, @companyId, @week, @duration, @location, @broadcast)";

        cmd.Parameters.AddWithValue("@id", showId);
        cmd.Parameters.AddWithValue("@name", "Monday Night Raw");
        cmd.Parameters.AddWithValue("@companyId", companyId);
        cmd.Parameters.AddWithValue("@week", 1);
        cmd.Parameters.AddWithValue("@duration", 180);
        cmd.Parameters.AddWithValue("@location", "Madison Square Garden, New York");
        cmd.Parameters.AddWithValue("@broadcast", "USA Network");

        cmd.ExecuteNonQuery();

        return showId;
    }

    /// <summary>
    /// Helper pour logger avec fallback vers Console si pas de logger configuré
    /// </summary>
    private static void Log(LogLevel level, string message)
    {
        if (_logger != null)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.Debug(message);
                    break;
                case LogLevel.Info:
                    _logger.Info(message);
                    break;
                case LogLevel.Warning:
                    _logger.Warning(message);
                    break;
                case LogLevel.Error:
                    _logger.Error(message);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }
        else
        {
            // Fallback to Console if no logger configured
            Console.WriteLine($"[DbSeeder] [{level}] {message}");
        }
    }
}

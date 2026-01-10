using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using System.IO;

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
    /// Seed la base de données si elle est vide avec des données génériques de test
    /// </summary>
    public static void SeedIfEmpty(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Vérification si la base de données nécessite un seeding...");
        
        if (!IsDatabaseEmpty(connection))
        {
            Log(LogLevel.Info, "Base de données n'est pas vide, seeding ignoré");
            return;
        }

        Log(LogLevel.Info, "Base de données vide détectée, démarrage du seeding...");
        // Toujours utiliser les données génériques de démo (plus d'import BAKI)
        SeedDemoData(connection);
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
            Log(LogLevel.Debug, $"Nombre de workers dans la base : {count}");
            return count == 0;
        }
        catch (Exception ex)
        {
            // Si la table n'existe pas encore, considérer comme vide
            Log(LogLevel.Debug, $"Table Workers n'existe pas encore ou erreur lors du comptage : {ex.Message}");
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
            // 0. S'assurer que les pays et régions de base existent
            SeedBaseGeography(connection);

            // 1. Créer une compagnie
            var companyId = SeedCompany(connection);

            // 2. Créer des workers
            var workerIds = SeedWorkers(connection, companyId);

            // 3. Créer des titres
            var titleIds = SeedTitles(connection, companyId, workerIds);

            // 4. Créer un show (optionnel - peut échouer si la table n'existe pas)
            try
            {
                var showId = SeedShow(connection, companyId);
                Log(LogLevel.Info, $"Show créé : {showId}");
            }
            catch (Exception ex)
            {
                // Ne pas faire échouer tout le seeding si le show échoue
                Log(LogLevel.Warning, $"Impossible de créer le show (table Shows peut ne pas exister) : {ex.Message}");
            }

            transaction.Commit();
            
            // Vérification finale : compter les workers créés
            using var verifyCmd = connection.CreateCommand();
            verifyCmd.CommandText = "SELECT COUNT(*) FROM Workers";
            var finalWorkerCount = Convert.ToInt64(verifyCmd.ExecuteScalar());
            
            verifyCmd.CommandText = "SELECT COUNT(*) FROM Companies";
            var finalCompanyCount = Convert.ToInt64(verifyCmd.ExecuteScalar());
            
            verifyCmd.CommandText = "SELECT COUNT(*) FROM Titles";
            var finalTitleCount = Convert.ToInt64(verifyCmd.ExecuteScalar());
            
            Log(LogLevel.Info, $"✅ Seeding terminé avec succès :");
            Log(LogLevel.Info, $"   - {finalCompanyCount} compagnie(s)");
            Log(LogLevel.Info, $"   - {finalWorkerCount} worker(s)");
            Log(LogLevel.Info, $"   - {finalTitleCount} titre(s)");
            
            if (finalWorkerCount == 0)
            {
                Log(LogLevel.Warning, "⚠️ Aucun worker créé ! Vérifiez que la table Workers existe et a la bonne structure.");
            }
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Log(LogLevel.Error, $"❌ Erreur lors du seeding : {ex.Message}");
            Log(LogLevel.Error, $"   StackTrace : {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// S'assure que les données géographiques de base existent
    /// </summary>
    private static void SeedBaseGeography(SqliteConnection connection)
    {
        try
        {
            // Vérifier que les tables existent
            if (!TableExists(connection, "Countries"))
            {
                Log(LogLevel.Warning, "Table Countries n'existe pas, création ignorée pour les pays");
                return;
            }

            if (!TableExists(connection, "Regions"))
            {
                Log(LogLevel.Warning, "Table Regions n'existe pas, création ignorée pour les régions");
                return;
            }

            using var cmd = connection.CreateCommand();
            
            // Insérer pays par défaut
            cmd.CommandText = "INSERT OR IGNORE INTO Countries (CountryId, Code, Name) VALUES ('COUNTRY_DEFAULT', 'WLD', 'World')";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT OR IGNORE INTO Countries (CountryId, Code, Name) VALUES ('COUNTRY_UNITED_STATES', 'USA', 'United States')";
            cmd.ExecuteNonQuery();

            // Insérer régions par défaut
            cmd.CommandText = "INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name) VALUES ('REGION_DEFAULT', 'COUNTRY_DEFAULT', 'Global')";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name) VALUES ('REGION_USA_DEFAULT', 'COUNTRY_UNITED_STATES', 'USA East')";
            cmd.ExecuteNonQuery();

            Log(LogLevel.Info, "Données géographiques de base créées");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Warning, $"Erreur lors de la création des données géographiques : {ex.Message}");
            // Ne pas faire échouer le seeding pour ça
        }
    }

    /// <summary>
    /// Crée une compagnie de démo
    /// </summary>
    private static string SeedCompany(SqliteConnection connection)
    {
        if (!TableExists(connection, "Companies"))
        {
            throw new InvalidOperationException("La table Companies n'existe pas. Le schéma doit être initialisé avant le seeding.");
        }

        var companyId = "COMP_WWE";

        // Vérifier si la compagnie existe déjà
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Companies WHERE CompanyId = @id";
        checkCmd.Parameters.AddWithValue("@id", companyId);
        var exists = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

        if (exists)
        {
            Log(LogLevel.Info, $"Compagnie {companyId} existe déjà, réutilisation");
            return companyId;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, FoundedYear, IsPlayerControlled)
            VALUES (@id, @name, @countryId, @regionId, @prestige, @treasury, @foundedYear, @player)";

        cmd.Parameters.AddWithValue("@id", companyId);
        cmd.Parameters.AddWithValue("@name", "World Wrestling Entertainment");
        cmd.Parameters.AddWithValue("@countryId", "COUNTRY_UNITED_STATES");
        cmd.Parameters.AddWithValue("@regionId", "REGION_USA_DEFAULT");
        cmd.Parameters.AddWithValue("@prestige", 95);
        cmd.Parameters.AddWithValue("@treasury", 10_000_000.0);
        cmd.Parameters.AddWithValue("@foundedYear", 2024);
        cmd.Parameters.AddWithValue("@player", 1);

        cmd.ExecuteNonQuery();
        Log(LogLevel.Info, $"Compagnie créée : {companyId}");

        return companyId;
    }

    /// <summary>
    /// Crée des workers de démo
    /// </summary>
    private static List<string> SeedWorkers(SqliteConnection connection, string companyId)
    {
        if (!TableExists(connection, "Workers"))
        {
            throw new InvalidOperationException("La table Workers n'existe pas. Le schéma doit être initialisé avant le seeding.");
        }

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
        var random = new Random();

        foreach (var worker in workers)
        {
            try
            {
                // Vérifier si le worker existe déjà
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Workers WHERE WorkerId = @id";
                checkCmd.Parameters.AddWithValue("@id", worker.id);
                var exists = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

                if (exists)
                {
                    Log(LogLevel.Debug, $"Worker {worker.id} existe déjà, ignoré");
                    workerIds.Add(worker.id);
                    continue;
                }

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv)
                    VALUES (@id, @name, @companyId, @nationality, @inRing, @entertainment, @story, @popularity, @fatigue, @role)";

                cmd.Parameters.AddWithValue("@id", worker.id);
                cmd.Parameters.AddWithValue("@name", worker.name);
                cmd.Parameters.AddWithValue("@companyId", companyId);
                cmd.Parameters.AddWithValue("@nationality", "USA"); // Default for demo
                cmd.Parameters.AddWithValue("@inRing", worker.inRing);
                cmd.Parameters.AddWithValue("@entertainment", worker.entertainment);
                cmd.Parameters.AddWithValue("@story", worker.story);
                cmd.Parameters.AddWithValue("@popularity", worker.popularity);
                cmd.Parameters.AddWithValue("@fatigue", random.Next(10, 40));
                cmd.Parameters.AddWithValue("@role", worker.role);

                cmd.ExecuteNonQuery();
                workerIds.Add(worker.id);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Erreur lors de la création du worker {worker.id} ({worker.name}) : {ex.Message}");
                // Continuer avec les autres workers même si un échoue
            }
        }

        Log(LogLevel.Info, $"{workerIds.Count}/{workers.Count} workers créés avec succès");
        return workerIds;
    }

    /// <summary>
    /// Crée des titres de démo
    /// </summary>
    private static List<string> SeedTitles(SqliteConnection connection, string companyId, List<string> workerIds)
    {
        if (!TableExists(connection, "Titles"))
        {
            Log(LogLevel.Warning, "Table Titles n'existe pas, création des titres ignorée");
            return new List<string>();
        }

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
            try
            {
                // Vérifier si le titre existe déjà
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Titles WHERE TitleId = @id";
                checkCmd.Parameters.AddWithValue("@id", title.id);
                var exists = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

                if (exists)
                {
                    Log(LogLevel.Debug, $"Titre {title.id} existe déjà, ignoré");
                    titleIds.Add(title.id);
                    continue;
                }

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Titles (TitleId, Name, CompanyId, Prestige, HolderWorkerId)
                    VALUES (@id, @name, @companyId, @prestige, @championId)";

                cmd.Parameters.AddWithValue("@id", title.id);
                cmd.Parameters.AddWithValue("@name", title.name);
                cmd.Parameters.AddWithValue("@companyId", companyId);
                cmd.Parameters.AddWithValue("@prestige", title.prestige);
                cmd.Parameters.AddWithValue("@championId", (object?)title.championId ?? DBNull.Value);

                cmd.ExecuteNonQuery();
                titleIds.Add(title.id);

                // Créer un reign actif si champion et si la table TitleReigns existe
                if (title.championId != null && TableExists(connection, "TitleReigns"))
                {
                    try
                    {
                        using var reignCmd = connection.CreateCommand();
                        reignCmd.CommandText = @"
                            INSERT INTO TitleReigns (TitleId, WorkerId, StartDate, IsCurrent)
                            VALUES (@titleId, @workerId, @startDate, @isCurrent)";

                        reignCmd.Parameters.AddWithValue("@titleId", title.id);
                        reignCmd.Parameters.AddWithValue("@workerId", title.championId);
                        reignCmd.Parameters.AddWithValue("@startDate", (int)(DateTime.Now.AddMonths(-3) - new DateTime(1970, 1, 1)).TotalDays);
                        reignCmd.Parameters.AddWithValue("@isCurrent", 1);

                        reignCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log(LogLevel.Warning, $"Impossible de créer le reign pour {title.id} : {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Erreur lors de la création du titre {title.id} : {ex.Message}");
                // Continuer avec les autres titres même si un échoue
            }
        }

        Log(LogLevel.Info, $"{titleIds.Count}/{titles.Count} titres créés avec succès");
        return titleIds;
    }

    /// <summary>
    /// Crée un show de démo
    /// </summary>
    private static string SeedShow(SqliteConnection connection, string companyId)
    {
        // Vérifier d'abord si la table Shows existe et a la bonne structure
        if (!TableExists(connection, "Shows"))
        {
            throw new InvalidOperationException("La table Shows n'existe pas. Assurez-vous que le schéma est initialisé avant le seeding.");
        }

        // Vérifier que la colonne ShowId existe
        var hasShowId = false;
        try
        {
            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(Shows);";
            using var reader = pragmaCmd.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                if (columnName.Equals("ShowId", StringComparison.OrdinalIgnoreCase))
                {
                    hasShowId = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Impossible de vérifier la structure de la table Shows : {ex.Message}");
        }

        if (!hasShowId)
        {
            throw new InvalidOperationException("La table Shows existe mais n'a pas la colonne ShowId. Le schéma doit être mis à jour.");
        }

        var showId = "SHOW_RAW_001";
        var showDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Shows (ShowId, Name, CompanyId, Week, Date, RegionId, DurationMinutes)
            VALUES (@id, @name, @companyId, @week, @date, @regionId, @duration)";

        cmd.Parameters.AddWithValue("@id", showId);
        cmd.Parameters.AddWithValue("@name", "Monday Night Raw");
        cmd.Parameters.AddWithValue("@companyId", companyId);
        // Calculer la semaine à partir de la date (simplifié: semaine 1 pour la première date)
        cmd.Parameters.AddWithValue("@week", 1);
        cmd.Parameters.AddWithValue("@date", showDate);
        cmd.Parameters.AddWithValue("@regionId", "REGION_USA_DEFAULT");
        cmd.Parameters.AddWithValue("@duration", 180);

        cmd.ExecuteNonQuery();

        return showId;
    }

    /// <summary>
    /// Vérifie si une table existe dans la base de données
    /// </summary>
    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT name FROM sqlite_master 
                WHERE type='table' AND name = @tableName COLLATE NOCASE";
            cmd.Parameters.AddWithValue("@tableName", tableName);
            var result = cmd.ExecuteScalar();
            return result != null;
        }
        catch
        {
            return false;
        }
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

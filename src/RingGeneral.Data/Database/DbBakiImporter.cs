using Microsoft.Data.Sqlite;
using RingGeneral.Core.Services;

namespace RingGeneral.Data.Database;

/// <summary>
/// Importateur de données depuis BAKI1.1.db
/// </summary>
public static class DbBakiImporter
{
    private static ILoggingService? _logger;

    public static void SetLogger(ILoggingService logger) => _logger = logger;

    /// <summary>
    /// Importe les données depuis BAKI1.1.db vers la base Ring General
    /// </summary>
    public static void ImportFromBaki(SqliteConnection targetConnection, string bakiDbPath)
    {
        if (!File.Exists(bakiDbPath))
        {
            Log(LogLevel.Warning, $"BAKI1.1.db not found: {bakiDbPath}");
            return;
        }

        Log(LogLevel.Info, $"Starting import from {bakiDbPath}");

        using var transaction = targetConnection.BeginTransaction();

        try
        {
            // Attacher la base BAKI comme base secondaire
            using (var attachCmd = targetConnection.CreateCommand())
            {
                attachCmd.CommandText = $"ATTACH DATABASE '{bakiDbPath}' AS baki";
                attachCmd.ExecuteNonQuery();
            }

            // 1. Importer les countries
            var countriesImported = ImportCountries(targetConnection);
            Log(LogLevel.Info, $"{countriesImported} pays importés");

            // 2. Importer les regions
            var regionsImported = ImportRegions(targetConnection);
            Log(LogLevel.Info, $"{regionsImported} régions importées");

            // 3. Importer les compagnies (promotions)
            var companiesImported = ImportCompanies(targetConnection);
            Log(LogLevel.Info, $"{companiesImported} compagnies importées");

            // 4. Importer les workers avec leurs contrats
            var (workersImported, contractsImported, freeAgents) = ImportWorkersAndContracts(targetConnection);
            Log(LogLevel.Info, $"{workersImported} workers importés");
            Log(LogLevel.Info, $"{contractsImported} contrats importés");
            Log(LogLevel.Info, $"{freeAgents} free agents (sans contrat)");

            // 5. Importer les titres
            var titlesImported = ImportTitles(targetConnection);
            Log(LogLevel.Info, $"{titlesImported} titres importés");

            // Détacher la base BAKI
            using (var detachCmd = targetConnection.CreateCommand())
            {
                detachCmd.CommandText = "DETACH DATABASE baki";
                detachCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            Log(LogLevel.Info, "Import terminé avec succès");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Log(LogLevel.Error, $"Erreur lors de l'import: {ex.Message}");
            Log(LogLevel.Error, $"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Importe les countries depuis BAKI
    /// </summary>
    private static int ImportCountries(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Import des countries...");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Countries (CountryId, Code, Name)
            SELECT
                'COUNTRY_' || countryID,
                COALESCE(SUBSTR(countryName, 1, 3), 'UNK'),
                countryName
            FROM baki.countries
            WHERE countryName IS NOT NULL AND countryName != ''";

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Importe les regions depuis BAKI
    /// </summary>
    private static int ImportRegions(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Import des regions...");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name)
            SELECT
                'REGION_' || r.regionID,
                'COUNTRY_' || r.regionParent,
                r.regionName
            FROM baki.regions r
            INNER JOIN baki.countries c ON c.countryID = r.regionParent
            WHERE r.regionName IS NOT NULL AND r.regionName != ''";

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Importe les companies (promotions) depuis BAKI
    /// </summary>
    private static int ImportCompanies(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Import des companies...");

        // Récupérer les promotions avec leurs pays et régions
        using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT
                p.promotionID,
                p.fullName,
                p.prestige,
                p.money,
                p.basedInCountry,
                p.basedInRegion,
                c.countryID as countryId,
                r.regionID as regionId
            FROM baki.promotions p
            LEFT JOIN baki.countries c ON c.countryName = p.basedInCountry
            LEFT JOIN baki.regions r ON r.regionName = p.basedInRegion AND r.regionParent = c.countryID
            WHERE p.fullName IS NOT NULL
            LIMIT 50";

        var promotions = new List<(int id, string name, double prestige, int money, int? countryId, int? regionId)>();

        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var prestige = reader.IsDBNull(2) ? 50.0 : reader.GetDouble(2);
                var money = reader.IsDBNull(3) ? 1000000 : reader.GetInt32(3);
                var countryId = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6);
                var regionId = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);

                promotions.Add((id, name, prestige, money, countryId, regionId));
            }
        }

        // Si aucune promotion trouvée ou pas de région valide, créer une région par défaut
        var hasDefaultRegion = false;
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM Regions WHERE RegionId = 'REGION_DEFAULT'";
            hasDefaultRegion = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
        }

        if (!hasDefaultRegion)
        {
            // Créer un pays par défaut si nécessaire
            using (var insertCountryCmd = connection.CreateCommand())
            {
                insertCountryCmd.CommandText = @"
                    INSERT OR IGNORE INTO Countries (CountryId, Code, Name)
                    VALUES ('COUNTRY_DEFAULT', 'WLD', 'World')";
                insertCountryCmd.ExecuteNonQuery();
            }

            // Créer une région par défaut
            using (var insertRegionCmd = connection.CreateCommand())
            {
                insertRegionCmd.CommandText = @"
                    INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name)
                    VALUES ('REGION_DEFAULT', 'COUNTRY_DEFAULT', 'Global')";
                insertRegionCmd.ExecuteNonQuery();
            }
        }

        // Insérer les compagnies
        int count = 0;
        foreach (var promo in promotions)
        {
            using var insertCmd = connection.CreateCommand();

            var countryIdStr = promo.countryId.HasValue ? $"COUNTRY_{promo.countryId.Value}" : "COUNTRY_DEFAULT";
            var regionIdStr = promo.regionId.HasValue ? $"REGION_{promo.regionId.Value}" : "REGION_DEFAULT";

            insertCmd.CommandText = @"
                INSERT OR IGNORE INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury)
                VALUES (@id, @name, @countryId, @regionId, @prestige, @treasury)";

            insertCmd.Parameters.AddWithValue("@id", $"COMP_{promo.id}");
            insertCmd.Parameters.AddWithValue("@name", promo.name);
            insertCmd.Parameters.AddWithValue("@countryId", countryIdStr);
            insertCmd.Parameters.AddWithValue("@regionId", regionIdStr);
            insertCmd.Parameters.AddWithValue("@prestige", (int)promo.prestige);
            insertCmd.Parameters.AddWithValue("@treasury", (double)promo.money);

            count += insertCmd.ExecuteNonQuery();
        }

        return count;
    }

    /// <summary>
    /// Importe les workers et leurs contrats depuis BAKI
    /// </summary>
    /// <returns>Tuple (workers importés, contrats importés, free agents)</returns>
    private static (int workers, int contracts, int freeAgents) ImportWorkersAndContracts(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Import des workers et contrats...");

        // Vérifier si les tables existent dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='workers'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Log(LogLevel.Info, "Table workers absente dans BAKI");
                return (0, 0, 0);
            }
        }

        // Créer un mapping des promotionID vers CompanyId
        var promotionMapping = new Dictionary<int, string>();
        using (var mapCmd = connection.CreateCommand())
        {
            mapCmd.CommandText = @"
                SELECT
                    CAST(SUBSTR(CompanyId, 6) AS INTEGER) as PromotionId,
                    CompanyId
                FROM Companies
                WHERE CompanyId LIKE 'COMP_%'";

            using var reader = mapCmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    var promotionId = reader.GetInt32(0);
                    var companyId = reader.GetString(1);
                    promotionMapping[promotionId] = companyId;
                }
            }
        }

        // Récupérer tous les workers avec leurs contrats (s'ils en ont)
        using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT
                w.workerID,
                w.fullName,
                w.nationality,
                w.technique,
                w.charisme,
                w.psychologie,
                w.popularite,
                c.promotionID,
                c.role,
                c.exclusive,
                c.expiryDate,
                c.wagePerMonth
            FROM baki.workers w
            LEFT JOIN baki.contracts c ON c.workerID = w.workerID
            WHERE w.fullName IS NOT NULL
            LIMIT 400";

        var workerData = new List<(
            int id,
            string name,
            string nationality,
            int technique,
            int charisme,
            int psychologie,
            int popularite,
            int? promotionId,
            string? role,
            int? exclusive,
            string? expiryDate,
            int? salary
        )>();

        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var nationality = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2);
                var technique = reader.IsDBNull(3) ? 50 : reader.GetInt32(3);
                var charisme = reader.IsDBNull(4) ? 50 : reader.GetInt32(4);
                var psychologie = reader.IsDBNull(5) ? 50 : reader.GetInt32(5);
                var popularite = reader.IsDBNull(6) ? 50 : reader.GetInt32(6);
                var promotionId = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);
                var role = reader.IsDBNull(8) ? null : reader.GetString(8);
                var exclusive = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);
                var expiryDate = reader.IsDBNull(10) ? null : reader.GetString(10);
                var salary = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11);

                workerData.Add((id, name, nationality, technique, charisme, psychologie, popularite,
                               promotionId, role, exclusive, expiryDate, salary));
            }
        }

        // Insérer les workers et leurs contrats
        int workersCount = 0;
        int contractsCount = 0;
        int freeAgentsCount = 0;

        foreach (var worker in workerData)
        {
            // Déterminer le CompanyId du worker
            string? companyId = null;
            if (worker.promotionId.HasValue && promotionMapping.ContainsKey(worker.promotionId.Value))
            {
                companyId = promotionMapping[worker.promotionId.Value];
            }

            // Si pas de contrat, le worker est un free agent (CompanyId = null)
            if (!worker.promotionId.HasValue)
            {
                freeAgentsCount++;
            }

            // Insérer le worker
            using (var insertWorkerCmd = connection.CreateCommand())
            {
                insertWorkerCmd.CommandText = @"
                    INSERT OR IGNORE INTO Workers (
                        WorkerId, Name, CompanyId, Nationality,
                        InRing, Entertainment, Story, Popularity, Fatigue
                    )
                    VALUES (@id, @name, @companyId, @nationality, @inRing, @entertainment, @story, @popularity, @fatigue)";

                insertWorkerCmd.Parameters.AddWithValue("@id", $"W_{worker.id}");
                insertWorkerCmd.Parameters.AddWithValue("@name", worker.name);
                insertWorkerCmd.Parameters.AddWithValue("@companyId", (object?)companyId ?? DBNull.Value);
                insertWorkerCmd.Parameters.AddWithValue("@nationality", worker.nationality);
                // Convertir les stats BAKI (0-500) en stats Ring General (0-100)
                insertWorkerCmd.Parameters.AddWithValue("@inRing", (int)(worker.technique * 0.2));
                insertWorkerCmd.Parameters.AddWithValue("@entertainment", (int)(worker.charisme * 0.2));
                insertWorkerCmd.Parameters.AddWithValue("@story", (int)(worker.psychologie * 0.2));
                insertWorkerCmd.Parameters.AddWithValue("@popularity", (int)(worker.popularite * 0.2));
                insertWorkerCmd.Parameters.AddWithValue("@fatigue", 0);

                workersCount += insertWorkerCmd.ExecuteNonQuery();
            }

            // Insérer le contrat si le worker en a un
            if (worker.promotionId.HasValue && companyId != null)
            {
                using var insertContractCmd = connection.CreateCommand();

                // Calculer une date de fin (aujourd'hui + 1 an si pas de date d'expiration valide)
                var endDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(worker.expiryDate) &&
                    worker.expiryDate != "No Expiry" &&
                    DateTime.TryParse(worker.expiryDate, out var parsedDate))
                {
                    // Convertir en timestamp (jours depuis epoch)
                    endDate = ((int)(parsedDate - new DateTime(1970, 1, 1)).TotalDays).ToString();
                }
                else
                {
                    // Pas d'expiration = contrat longue durée (3 ans)
                    endDate = ((int)(DateTime.Now.AddYears(3) - new DateTime(1970, 1, 1)).TotalDays).ToString();
                }

                insertContractCmd.CommandText = @"
                    INSERT OR IGNORE INTO Contracts (
                        WorkerId, CompanyId, StartDate, EndDate, Salary, IsExclusive
                    )
                    VALUES (@workerId, @companyId, @startDate, @endDate, @salary, @isExclusive)";

                insertContractCmd.Parameters.AddWithValue("@workerId", $"W_{worker.id}");
                insertContractCmd.Parameters.AddWithValue("@companyId", companyId);
                insertContractCmd.Parameters.AddWithValue("@startDate", (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalDays);
                insertContractCmd.Parameters.AddWithValue("@endDate", endDate);
                insertContractCmd.Parameters.AddWithValue("@salary", worker.salary ?? 0);
                insertContractCmd.Parameters.AddWithValue("@isExclusive", worker.exclusive ?? 0);

                contractsCount += insertContractCmd.ExecuteNonQuery();
            }
        }

        return (workersCount, contractsCount, freeAgentsCount);
    }

    /// <summary>
    /// Importe les titles depuis BAKI
    /// </summary>
    private static int ImportTitles(SqliteConnection connection)
    {
        Log(LogLevel.Info, "Import des titles...");

        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='titles'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Log(LogLevel.Info, "Table titles absente dans BAKI");
                return 0;
            }
        }

        // Récupérer le premier CompanyId disponible
        string? firstCompanyId = null;
        using (var companyCmd = connection.CreateCommand())
        {
            companyCmd.CommandText = "SELECT CompanyId FROM Companies LIMIT 1";
            using var reader = companyCmd.ExecuteReader();
            if (reader.Read())
            {
                firstCompanyId = reader.GetString(0);
            }
        }

        if (firstCompanyId == null)
        {
            Log(LogLevel.Info, "Aucune compagnie disponible pour associer les titres");
            return 0;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Titles (TitleId, Name, CompanyId, Prestige)
            SELECT
                'T_' || titleID,
                titleName,
                @companyId,
                CAST(COALESCE(prestige, 50) * 0.2 AS INTEGER)
            FROM baki.titles
            WHERE titleName IS NOT NULL
            LIMIT 50";

        cmd.Parameters.AddWithValue("@companyId", firstCompanyId);

        return cmd.ExecuteNonQuery();
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
            Console.WriteLine($"[DbBakiImporter] [{level}] {message}");
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace RingGeneral.Data.Database;

/// <summary>
/// Importateur de données depuis BAKI1.1.db
/// </summary>
public static class DbBakiImporter
{
    /// <summary>
    /// Importe les données depuis BAKI1.1.db vers la base Ring General
    /// </summary>
    public static void ImportFromBaki(SqliteConnection targetConnection, string bakiDbPath)
    {
        if (!File.Exists(bakiDbPath))
        {
            Console.WriteLine($"[DbBakiImporter] Fichier BAKI1.1.db introuvable: {bakiDbPath}");
            return;
        }

        Console.WriteLine($"[DbBakiImporter] Démarrage de l'import depuis {bakiDbPath}");

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
            Console.WriteLine($"[DbBakiImporter] {countriesImported} pays importés");

            // 2. Importer les regions
            var regionsImported = ImportRegions(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {regionsImported} régions importées");

            // 3. Importer les compagnies (promotions)
            var companiesImported = ImportCompanies(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {companiesImported} compagnies importées");

            // 4. Importer les workers
            var workersImported = ImportWorkers(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {workersImported} workers importés");

            // 5. Importer les titres
            var titlesImported = ImportTitles(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {titlesImported} titres importés");

            // Détacher la base BAKI
            using (var detachCmd = targetConnection.CreateCommand())
            {
                detachCmd.CommandText = "DETACH DATABASE baki";
                detachCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            Console.WriteLine("[DbBakiImporter] Import terminé avec succès");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Error.WriteLine($"[DbBakiImporter] Erreur lors de l'import: {ex.Message}");
            Console.Error.WriteLine($"[DbBakiImporter] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Importe les countries depuis BAKI
    /// </summary>
    private static int ImportCountries(SqliteConnection connection)
    {
        Console.WriteLine("[DbBakiImporter] Import des countries...");

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
        Console.WriteLine("[DbBakiImporter] Import des regions...");

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
        Console.WriteLine("[DbBakiImporter] Import des companies...");

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
    /// Importe les workers depuis BAKI
    /// </summary>
    private static int ImportWorkers(SqliteConnection connection)
    {
        Console.WriteLine("[DbBakiImporter] Import des workers...");

        // Vérifier si la table workers existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='workers'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table workers absente dans BAKI");
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
            Console.WriteLine("[DbBakiImporter] Aucune compagnie disponible pour associer les workers");
            return 0;
        }

        // Récupérer les workers depuis BAKI
        using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT
                workerID,
                fullName,
                nationality,
                technique,
                charisme,
                psychologie,
                popularite
            FROM baki.workers
            WHERE fullName IS NOT NULL
            LIMIT 200";

        var workers = new List<(int id, string name, string nationality, int technique, int charisme, int psychologie, int popularite)>();

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

                workers.Add((id, name, nationality, technique, charisme, psychologie, popularite));
            }
        }

        // Insérer les workers
        int count = 0;
        foreach (var worker in workers)
        {
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT OR IGNORE INTO Workers (
                    WorkerId, Name, CompanyId, Nationality,
                    InRing, Entertainment, Story, Popularity, Fatigue
                )
                VALUES (@id, @name, @companyId, @nationality, @inRing, @entertainment, @story, @popularity, @fatigue)";

            insertCmd.Parameters.AddWithValue("@id", $"W_{worker.id}");
            insertCmd.Parameters.AddWithValue("@name", worker.name);
            insertCmd.Parameters.AddWithValue("@companyId", firstCompanyId);
            insertCmd.Parameters.AddWithValue("@nationality", worker.nationality);
            // Convertir les stats BAKI (0-500) en stats Ring General (0-100)
            insertCmd.Parameters.AddWithValue("@inRing", (int)(worker.technique * 0.2));
            insertCmd.Parameters.AddWithValue("@entertainment", (int)(worker.charisme * 0.2));
            insertCmd.Parameters.AddWithValue("@story", (int)(worker.psychologie * 0.2));
            insertCmd.Parameters.AddWithValue("@popularity", (int)(worker.popularite * 0.2));
            insertCmd.Parameters.AddWithValue("@fatigue", 0);

            count += insertCmd.ExecuteNonQuery();
        }

        return count;
    }

    /// <summary>
    /// Importe les titles depuis BAKI
    /// </summary>
    private static int ImportTitles(SqliteConnection connection)
    {
        Console.WriteLine("[DbBakiImporter] Import des titles...");

        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='titles'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table titles absente dans BAKI");
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
            Console.WriteLine("[DbBakiImporter] Aucune compagnie disponible pour associer les titres");
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
}

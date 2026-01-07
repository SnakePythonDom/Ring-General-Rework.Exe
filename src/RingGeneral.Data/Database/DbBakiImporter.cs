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

            // 1. Importer les compagnies
            var companiesImported = ImportCompanies(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {companiesImported} compagnies importées");

            // 2. Importer les workers
            var workersImported = ImportWorkers(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {workersImported} workers importés");

            // 3. Importer les titres
            var titlesImported = ImportTitles(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {titlesImported} titres importés");

            // 4. Importer les shows
            var showsImported = ImportShows(targetConnection);
            Console.WriteLine($"[DbBakiImporter] {showsImported} shows importés");

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
            throw;
        }
    }

    private static int ImportCompanies(SqliteConnection connection)
    {
        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='Companies'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table Companies absente dans BAKI, création d'une compagnie par défaut");
                // Créer une compagnie par défaut
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Companies (CompanyId, Name, Region, Prestige, Treasury, CurrentWeek, IsPlayerControlled)
                    VALUES ('COMP_WWE', 'World Wrestling Entertainment', 'USA', 95, 10000000.0, 1, 1)";
                insertCmd.ExecuteNonQuery();
                return 1;
            }
        }

        // Importer depuis BAKI
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Companies (CompanyId, Name, Region, Prestige, Treasury, CurrentWeek, IsPlayerControlled)
            SELECT
                COALESCE(CompanyId, 'COMP_' || CAST(rowid AS TEXT)),
                Name,
                COALESCE(Region, 'Unknown'),
                COALESCE(Prestige, 50),
                COALESCE(Treasury, 1000000.0),
                1,
                1
            FROM baki.Companies
            LIMIT 10";

        return cmd.ExecuteNonQuery();
    }

    private static int ImportWorkers(SqliteConnection connection)
    {
        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='Workers' OR name='workers'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table Workers absente dans BAKI, pas d'import");
                return 0;
            }
        }

        // Déterminer le nom de la table (Workers ou workers)
        string tableName = "Workers";
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT name FROM baki.sqlite_master WHERE type='table' AND (name='Workers' OR name='workers')";
            using var reader = checkCmd.ExecuteReader();
            if (reader.Read())
            {
                tableName = reader.GetString(0);
            }
        }

        // Importer les workers avec mapping des attributs BAKI -> Ring General
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            INSERT INTO Workers (WorkerId, FullName, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, Morale, TvRole)
            SELECT
                COALESCE(WorkerId, 'W_' || CAST(rowid AS TEXT)),
                COALESCE(FullName, Name, 'Unknown Worker'),
                'COMP_WWE',
                CAST(COALESCE(InRing, technique, 50) * 0.2 AS INTEGER),
                CAST(COALESCE(Entertainment, charisme, 50) * 0.2 AS INTEGER),
                CAST(COALESCE(Story, psychologie, 50) * 0.2 AS INTEGER),
                CAST(COALESCE(Popularity, popularite, 50) * 0.2 AS INTEGER),
                COALESCE(Fatigue, 0),
                COALESCE(Morale, 75),
                COALESCE(TvRole, 'Midcard')
            FROM baki.{tableName}
            LIMIT 200";

        return cmd.ExecuteNonQuery();
    }

    private static int ImportTitles(SqliteConnection connection)
    {
        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='Titles'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table Titles absente dans BAKI, pas d'import");
                return 0;
            }
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Titles (TitleId, Name, CompanyId, Prestige, CurrentChampionId)
            SELECT
                COALESCE(TitleId, 'T_' || CAST(rowid AS TEXT)),
                Name,
                'COMP_WWE',
                COALESCE(Prestige, 50),
                CurrentChampionId
            FROM baki.Titles
            LIMIT 20";

        return cmd.ExecuteNonQuery();
    }

    private static int ImportShows(SqliteConnection connection)
    {
        // Vérifier si la table existe dans BAKI
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM baki.sqlite_master WHERE type='table' AND name='Shows'";
            var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!tableExists)
            {
                Console.WriteLine("[DbBakiImporter] Table Shows absente dans BAKI, pas d'import");
                return 0;
            }
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Shows (ShowId, Name, CompanyId, Week, DurationMinutes, Location, Broadcast)
            SELECT
                COALESCE(ShowId, 'SHOW_' || CAST(rowid AS TEXT)),
                Name,
                'COMP_WWE',
                COALESCE(Week, 1),
                COALESCE(DurationMinutes, 120),
                COALESCE(Location, 'Unknown'),
                COALESCE(Broadcast, 'TV')
            FROM baki.Shows
            LIMIT 50";

        return cmd.ExecuteNonQuery();
    }
}

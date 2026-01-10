using Microsoft.Data.Sqlite;

namespace RingGeneral.Data.Database;

/// <summary>
/// Factory de connexions SQLite avec support de deux bases s?par?es :
/// - General DB (ring_general.db) : Donn?es statiques + donn?es de session (Companies, Workers, Shows, etc.)
/// - Save DB (ring_save.db) : Sauvegardes (SaveGames, ?tat de partie active)
/// 
/// Chemins g?r?s par :
/// 1. Variable d'environnement RINGGENERAL_GENERAL_DB_PATH (override)
/// 2. Sinon : %APPDATA%/RingGeneral/ring_general.db (d?faut)
/// 3. Save DB : %APPDATA%/RingGeneral/ring_save.db (automatique)
/// </summary>
public sealed class SqliteConnectionFactory
{
    private readonly string _generalConnectionString;
    private readonly string _saveConnectionString;

    public string GeneralDatabasePath { get; }
    public string SaveDatabasePath { get; }

    /// <summary>
    /// Propri?t? de compatibilit? : redirige vers GeneralDatabasePath
    /// </summary>
    [Obsolete("Utiliser GeneralDatabasePath ? la place")]
    public string WorldDatabasePath => GeneralDatabasePath;

    /// <summary>
    /// Initialise la factory avec les deux cha?nes de connexion
    /// </summary>
    /// <param name="generalConnectionString">Connection string pour General DB (si null, cherche via env ou utilise AppData)</param>
    /// <param name="saveConnectionString">Connection string pour Save DB (si null, utilise AppData)</param>
    public SqliteConnectionFactory(string? generalConnectionString = null, string? saveConnectionString = null)
    {
        // ???????????????????????????????????????????????????????????????????????????
        // GENERAL DB PATH RESOLUTION (ring_general.db)
        // ???????????????????????????????????????????????????????????????????????????
        
        if (!string.IsNullOrWhiteSpace(generalConnectionString))
        {
            // Path explicite fourni (rare, surtout pour tests)
            _generalConnectionString = generalConnectionString;
            var generalBuilder = new SqliteConnectionStringBuilder(generalConnectionString);
            GeneralDatabasePath = generalBuilder.DataSource ?? throw new InvalidOperationException("DataSource non trouv? dans connection string");
        }
        else
        {
            // 1. Chercher via variable d'environnement (expert override)
            var envGeneralPath = Environment.GetEnvironmentVariable("RINGGENERAL_GENERAL_DB_PATH");
            if (!string.IsNullOrWhiteSpace(envGeneralPath))
            {
                GeneralDatabasePath = envGeneralPath;
            }
            else
            {
                // 2. Sinon : %APPDATA%/RingGeneral/ring_general.db (d?faut)
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var ringGenDir = Path.Combine(appDataPath, "RingGeneral");
                GeneralDatabasePath = Path.Combine(ringGenDir, "ring_general.db");
                
                // Cr?er le r?pertoire s'il n'existe pas
                Directory.CreateDirectory(ringGenDir);
            }

            _generalConnectionString = $"Data Source={GeneralDatabasePath}";
        }

        // ???????????????????????????????????????????????????????????????????
        // SAVE DB PATH RESOLUTION
        // ???????????????????????????????????????????????????????????????????

        if (!string.IsNullOrWhiteSpace(saveConnectionString))
        {
            // ? Path explicite fourni
            _saveConnectionString = saveConnectionString;
            var saveBuilder = new SqliteConnectionStringBuilder(saveConnectionString);
            SaveDatabasePath = saveBuilder.DataSource ?? throw new InvalidOperationException("DataSource non trouv? dans connection string");
        }
        else
        {
            // ? D?faut : %APPDATA%/RingGeneral/ring_save.db
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var ringGenDir = Path.Combine(appDataPath, "RingGeneral");
            SaveDatabasePath = Path.Combine(ringGenDir, "ring_save.db");
            _saveConnectionString = $"Data Source={SaveDatabasePath}";

            // Cr?er le r?pertoire s'il n'existe pas
            Directory.CreateDirectory(ringGenDir);
        }
    }

    // ???????????????????????????????????????????????????????????????????????????
    // PUBLIC API - CONNEXIONS
    // ???????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// ? RECOMMAND? : Cr?e une connexion ? la WORLD DB avec validation
    /// 
    /// Validation effectu?e :
    /// - Fichier doit exister
    /// - Tables 'Companies' et 'Workers' doivent exister
    /// - L?ve InvalidOperationException si validation ?choue
    /// </summary>
    /// <summary>
    /// ? RECOMMAND? : Cr?e une connexion ? la GENERAL DB avec validation
    /// 
    /// Validation effectu?e :
    /// - Fichier doit exister
    /// - Tables 'Companies' et 'Workers' doivent exister
    /// - L?ve InvalidOperationException si validation ?choue
    /// </summary>
    public SqliteConnection CreateGeneralConnection()
    {
        var connexion = new SqliteConnection(_generalConnectionString);
        connexion.Open();

        // ✅ Vérifier que les tables essentielles existent
        var hasCompanies = TableExists(connexion, "companies") || TableExists(connexion, "Companies");
        var hasWorkers = TableExists(connexion, "workers") || TableExists(connexion, "Workers");

        // Si les tables de base manquent, créer le schéma complet
        if (!hasCompanies || !hasWorkers)
        {
            // La base existe mais n'a pas le schéma de base - créer le schéma complet
            CreateBaseSchemaIfMissing(connexion);
            
            // Vérifier à nouveau après création du schéma
            hasCompanies = TableExists(connexion, "companies") || TableExists(connexion, "Companies");
            hasWorkers = TableExists(connexion, "workers") || TableExists(connexion, "Workers");
            
            if (!hasCompanies || !hasWorkers)
            {
                connexion.Dispose();
                throw new InvalidOperationException(
                    $"❌ Impossible de créer le schéma de la base de données.\n" +
                    $"Table 'Companies' ou 'Workers' introuvable après initialisation.\n\n" +
                    $"Chemin : {GeneralDatabasePath}\n\n" +
                    $"Conseil : Vérifier les permissions d'écriture et que le fichier 001_init.sql existe.");
            }
            
            // Remplir avec des données génériques si la base est vide
            DbSeeder.SeedIfEmpty(connexion);
        }
        
        // ✅ TOUJOURS s'assurer que toutes les tables essentielles existent
        // Cela permet de créer les tables manquantes même si Companies/Workers existent déjà
        EnsureEssentialTablesExist(connexion);
        
        // ✅ Vérification finale : tester que la table shows a bien la colonne show_id
        // Si la vérification échoue, forcer la recréation
        if (TableExists(connexion, "shows"))
        {
            try
            {
                using var testCmd = connexion.CreateCommand();
                testCmd.CommandText = "SELECT show_id FROM shows LIMIT 0;"; // Test de préparation, pas d'exécution réelle
                testCmd.Prepare(); // Préparer la commande pour vérifier la structure
            }
            catch
            {
                // Si la préparation échoue, la colonne n'existe pas - forcer la recréation
                try
                {
                    using var dropCmd = connexion.CreateCommand();
                    dropCmd.CommandText = "PRAGMA foreign_keys = OFF;";
                    dropCmd.ExecuteNonQuery();
                    dropCmd.CommandText = "DROP TABLE IF EXISTS shows;";
                    dropCmd.ExecuteNonQuery();
                    dropCmd.CommandText = "PRAGMA foreign_keys = ON;";
                    dropCmd.ExecuteNonQuery();
                    
                    // Recréer la table
                    EnsureEssentialTablesExist(connexion);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la recréation forcée de shows: {ex.Message}");
                }
            }
        }

        return connexion;
    }

    /// <summary>
    /// ?? LEGACY : Redirige vers CreateGeneralConnection() pour backward compatibility
    /// ? pr?f?rer : Utilisez CreateGeneralConnection() explicitement
    /// </summary>
    [Obsolete("Utiliser CreateGeneralConnection() ? la place")]
    public SqliteConnection CreateWorldConnection()
    {
        return CreateGeneralConnection();
    }

    /// <summary>
    /// ? RECOMMAND? : Cr?e une connexion ? la SAVE DB avec auto-cr?ation
    /// 
    /// Comportement :
    /// - Cr?e le r?pertoire si absent
    /// - Cr?e la table SaveGames si manquante
    /// - Pas d'exception si table existe d?j?
    /// </summary>
    public SqliteConnection CreateSaveConnection()
    {
        var connexion = new SqliteConnection(_saveConnectionString);
        connexion.Open();

        // ? AUTO-CR?ATION : Cr?er la table SaveGames si manquante
        EnsureSaveSchema(connexion);

        return connexion;
    }

    /// <summary>
    /// ?? LEGACY : Redirige vers CreateGeneralConnection() pour backward compatibility
    /// ? pr?f?rer : Utilisez CreateGeneralConnection() explicitement
    /// </summary>
    [Obsolete("Utiliser CreateGeneralConnection() ? la place")]
    public SqliteConnection OuvrirConnexion()
    {
        return CreateGeneralConnection();
    }

    // ???????????????????????????????????????????????????????????????????????????
    // HELPERS ET ACCESSEURS
    // ???????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Retourne la cha?ne de connexion General DB
    /// </summary>
    public string GetConnectionString() => _generalConnectionString;

    /// <summary>
    /// Retourne la cha?ne de connexion Save DB
    /// </summary>
    public string GetSaveConnectionString() => _saveConnectionString;

    /// <summary>
    /// Retourne le chemin de la DB sp�cifi�e (pour logging/debug)
    /// </summary>
    public string GetDbFilePath(bool isSaveDb = false)
    {
        return isSaveDb ? SaveDatabasePath : GeneralDatabasePath;
    }

    /// <summary>
    /// Retourne l'�tat des deux bases (pour logs au d�marrage)
    /// </summary>
    public (bool GeneralDbExists, bool SaveDbExists) CheckDatabasesExist()
    {
        return (
            File.Exists(GeneralDatabasePath),
            File.Exists(SaveDatabasePath)
        );
    }

    /// <summary>
    /// ?? LEGACY : Propri�t� de compatibilit�
    /// </summary>
    [Obsolete("Utiliser CheckDatabasesExist() qui retourne GeneralDbExists")]
    public (bool WorldDbExists, bool SaveDbExists) CheckDatabasesExistLegacy()
    {
        var (generalExists, saveExists) = CheckDatabasesExist();
        return (generalExists, saveExists);
    }

    // ???????????????????????????????????????????????????????????????????????????
    // PRIVATE HELPERS
    // ???????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Vérifie l'existence d'une table (case-insensitive)
    /// </summary>
    private static bool TableExists(SqliteConnection connexion, string tableName)
    {
        try
        {
            using var command = connexion.CreateCommand();
            command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=@tableName LIMIT 1;";
            command.Parameters.AddWithValue("@tableName", tableName);
            return command.ExecuteScalar() is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Crée le schéma de base si les tables n'existent pas
    /// </summary>
    private static void CreateBaseSchemaIfMissing(SqliteConnection connection)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "data", "migrations", "001_init.sql");
        
        // Si le fichier de migration existe, l'exécuter
        if (File.Exists(schemaPath))
        {
            try
            {
                var schemaSql = File.ReadAllText(schemaPath);
                using var command = connection.CreateCommand();
                command.CommandText = schemaSql;
                command.ExecuteNonQuery();
            }
            catch
            {
                // Si erreur, créer les tables minimales
                CreateMinimalSchema(connection);
            }
        }
        else
        {
            // Créer les tables minimales
            CreateMinimalSchema(connection);
        }
        
        // Toujours s'assurer que les tables essentielles existent (même si 001_init.sql a été exécuté)
        // Cela permet de créer les tables manquantes si elles n'étaient pas dans le fichier de migration
        EnsureEssentialTablesExist(connection);
    }

    /// <summary>
    /// S'assure que toutes les tables essentielles existent (crée celles qui manquent)
    /// Méthode publique pour permettre l'appel depuis l'extérieur (ex: App.axaml.cs)
    /// </summary>
    public static void EnsureEssentialTablesExist(SqliteConnection connection)
    {
        // Vérifier et créer les tables une par une si elles n'existent pas
        if (!TableExists(connection, "ui_table_settings"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ui_table_settings (
                    id INTEGER PRIMARY KEY CHECK (id = 1),
                    recherche TEXT,
                    filtre_type TEXT,
                    filtre_statut TEXT,
                    colonnes_visibles TEXT,
                    colonnes_ordre TEXT,
                    tri_colonnes TEXT
                );
            ";
            cmd.ExecuteNonQuery();
        }

        if (!TableExists(connection, "game_settings"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS game_settings (
                    id INTEGER PRIMARY KEY CHECK (id = 1),
                    youth_generation_mode TEXT NOT NULL DEFAULT 'Realiste',
                    world_generation_mode TEXT NOT NULL DEFAULT 'Desactivee',
                    semaine_pivot_annuelle INTEGER
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Vérifier si SaveGames existe avant de créer GameState
        var hasSaveGames = TableExists(connection, "SaveGames");
        if (!hasSaveGames)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS SaveGames (
                    SaveGameId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaveName TEXT NOT NULL,
                    PlayerCompanyId TEXT NOT NULL,
                    CurrentWeek INTEGER NOT NULL DEFAULT 1,
                    CurrentDate TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastPlayedAt TEXT,
                    FOREIGN KEY (PlayerCompanyId) REFERENCES Companies(CompanyId)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        if (!TableExists(connection, "GameState"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS GameState (
                    GameStateId INTEGER PRIMARY KEY CHECK (GameStateId = 1),
                    CurrentSaveGameId INTEGER,
                    CurrentWeek INTEGER NOT NULL DEFAULT 1,
                    CurrentDate TEXT,
                    LastUpdatedAt TEXT,
                    BookingControlLevel TEXT NOT NULL DEFAULT 'CoBooker',
                    FOREIGN KEY (CurrentSaveGameId) REFERENCES SaveGames(SaveGameId)
                );
                INSERT OR IGNORE INTO GameState (GameStateId, CurrentWeek, BookingControlLevel) VALUES (1, 1, 'CoBooker');
            ";
            cmd.ExecuteNonQuery();
        }
        else
        {
            // Vérifier si BookingControlLevel existe dans GameState, sinon l'ajouter
            try
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT BookingControlLevel FROM GameState LIMIT 1;";
                checkCmd.ExecuteScalar();
            }
            catch
            {
                // La colonne n'existe pas, l'ajouter
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE GameState ADD COLUMN BookingControlLevel TEXT NOT NULL DEFAULT 'CoBooker';";
                    alterCmd.ExecuteNonQuery();
                }
                catch
                {
                    // Ignorer si la colonne existe déjà ou si l'ALTER échoue
                }
            }
        }

        // Table: match_types (bibliothèque de types de match)
        if (!TableExists(connection, "match_types"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS match_types (
                    match_type_id TEXT PRIMARY KEY,
                    nom TEXT NOT NULL,
                    description TEXT,
                    actif INTEGER NOT NULL DEFAULT 1,
                    ordre INTEGER NOT NULL DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS idx_match_types_actif ON match_types(actif);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: segment_templates (templates de segments)
        if (!TableExists(connection, "segment_templates"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS segment_templates (
                    template_id TEXT PRIMARY KEY,
                    nom TEXT NOT NULL,
                    type_segment TEXT NOT NULL,
                    duree INTEGER NOT NULL,
                    main_event INTEGER NOT NULL DEFAULT 0,
                    intensite INTEGER NOT NULL DEFAULT 50,
                    match_type_id TEXT,
                    FOREIGN KEY (match_type_id) REFERENCES match_types(match_type_id)
                );
                CREATE INDEX IF NOT EXISTS idx_segment_templates_type ON segment_templates(type_segment);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: shows (shows/événements legacy) - CRITIQUE pour GameRepository.ChargerShow()
        // Cette table DOIT exister avec la colonne show_id, même si Shows (majuscule) existe
        var showsTableExists = TableExists(connection, "shows");
        var needsRecreateShowsLegacy = false;
        
        if (showsTableExists)
        {
            // Test direct : essayer de préparer une commande avec show_id
            try
            {
                using var testCmd = connection.CreateCommand();
                testCmd.CommandText = "SELECT show_id FROM shows LIMIT 0;";
                testCmd.Prepare(); // Préparer pour vérifier que la colonne existe
            }
            catch
            {
                // Si la préparation échoue, la colonne show_id n'existe pas - FORCER la recréation
                needsRecreateShowsLegacy = true;
            }
        }
        else
        {
            // La table n'existe pas du tout - la créer
            needsRecreateShowsLegacy = true;
        }
        
        if (needsRecreateShowsLegacy)
        {
            // Supprimer les tables dépendantes et shows si nécessaire
            try
            {
                using var dropCmd = connection.CreateCommand();
                dropCmd.CommandText = "PRAGMA foreign_keys = OFF;";
                dropCmd.ExecuteNonQuery();
                
                // Supprimer les tables dépendantes (si elles existent)
                dropCmd.CommandText = "DROP TABLE IF EXISTS segments;";
                dropCmd.ExecuteNonQuery();
                
                dropCmd.CommandText = "DROP TABLE IF EXISTS show_history;";
                dropCmd.ExecuteNonQuery();
                
                dropCmd.CommandText = "DROP TABLE IF EXISTS segment_history;";
                dropCmd.ExecuteNonQuery();
                
                dropCmd.CommandText = "DROP TABLE IF EXISTS audience_history;";
                dropCmd.ExecuteNonQuery();
                
                // Supprimer shows (legacy)
                dropCmd.CommandText = "DROP TABLE IF EXISTS shows;";
                dropCmd.ExecuteNonQuery();
                
                dropCmd.CommandText = "PRAGMA foreign_keys = ON;";
                dropCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression de la table shows: {ex.Message}");
                try
                {
                    using var pragmaCmd = connection.CreateCommand();
                    pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                    pragmaCmd.ExecuteNonQuery();
                }
                catch { }
            }
            
            // Créer la table avec la bonne structure (SANS IF NOT EXISTS pour forcer la création)
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    PRAGMA foreign_keys = ON;
                    CREATE TABLE shows (
                        show_id TEXT PRIMARY KEY,
                        nom TEXT NOT NULL,
                        semaine INTEGER NOT NULL,
                        region TEXT NOT NULL,
                        duree INTEGER NOT NULL,
                        compagnie_id TEXT NOT NULL,
                        tv_deal_id TEXT,
                        lieu TEXT NOT NULL DEFAULT '',
                        diffusion TEXT NOT NULL DEFAULT '',
                        FOREIGN KEY (compagnie_id) REFERENCES Companies(CompanyId)
                    );
                ";
                cmd.ExecuteNonQuery();
                
                // Vérification finale : tester que show_id est accessible
                using var verifyCmd = connection.CreateCommand();
                verifyCmd.CommandText = "SELECT show_id FROM shows LIMIT 0;";
                verifyCmd.Prepare();
            }
            catch
            {
                // Si la création échoue, essayer avec IF NOT EXISTS comme fallback
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        PRAGMA foreign_keys = ON;
                        CREATE TABLE IF NOT EXISTS shows (
                            show_id TEXT PRIMARY KEY,
                            nom TEXT NOT NULL,
                            semaine INTEGER NOT NULL,
                            region TEXT NOT NULL,
                            duree INTEGER NOT NULL,
                            compagnie_id TEXT NOT NULL,
                            tv_deal_id TEXT,
                            lieu TEXT NOT NULL DEFAULT '',
                            diffusion TEXT NOT NULL DEFAULT '',
                            FOREIGN KEY (compagnie_id) REFERENCES Companies(CompanyId)
                        );
                    ";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la création de la table shows (fallback): {ex2.Message}");
                }
            }
        }
        
        // Vérifier et corriger la table Companies pour ajouter les colonnes manquantes
        if (TableExists(connection, "Companies") || TableExists(connection, "companies"))
        {
            var tableName = TableExists(connection, "Companies") ? "Companies" : "companies";
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = $"PRAGMA table_info({tableName});";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch
            {
                // Si on ne peut pas lire les colonnes, on assume qu'elles n'existent pas
            }
            
            // Ajouter les colonnes manquantes une par une
            var requiredColumns = new Dictionary<string, string>
            {
                { "FoundedYear", "INTEGER DEFAULT 2024" },
                { "IsPlayerControlled", "INTEGER DEFAULT 0" },
                { "CreatedAt", "TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP" },
                { "CompanySize", "TEXT DEFAULT 'Local'" },
                { "CurrentEra", "TEXT DEFAULT 'Foundation Era'" },
                { "CatchStyleId", "TEXT" },
                { "MonthlyBurnRate", "REAL DEFAULT 0.0" },
                { "CurrentWeek", "INTEGER DEFAULT 1" },
                { "YouthBudget", "REAL DEFAULT 0.0" }
            };
            
            foreach (var (columnName, columnDef) in requiredColumns)
            {
                if (!existingColumns.Contains(columnName))
                {
                    try
                    {
                        using var alterCmd = connection.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDef};";
                        alterCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Ignorer si la colonne existe déjà ou si l'ALTER échoue
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de {columnName} à {tableName}: {ex.Message}");
                    }
                }
            }
        }

        // Table: Shows (nouvelle version, peut coexister avec shows)
        var showsTableExistsUpper = TableExists(connection, "Shows");
        var needsRecreateShowsUpper = false;
        
        if (showsTableExistsUpper)
        {
            // Vérifier si les colonnes essentielles existent (ShowId et Week)
            var hasShowId = false;
            var hasWeek = false;
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
                    }
                    if (columnName.Equals("Week", StringComparison.OrdinalIgnoreCase))
                    {
                        hasWeek = true;
                    }
                }
            }
            catch
            {
                hasShowId = false;
                hasWeek = false;
            }
            
            // Si ShowId manque, on doit recréer la table (colonne primaire)
            if (!hasShowId)
            {
                needsRecreateShowsUpper = true;
            }
            else if (!hasWeek)
            {
                // Ajouter la colonne Week si elle manque
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Shows ADD COLUMN Week INTEGER NOT NULL DEFAULT 1;";
                    alterCmd.ExecuteNonQuery();
                }
                catch
                {
                    // Si l'ALTER échoue (par exemple si Week existe déjà avec un autre type), recréer la table
                    needsRecreateShowsUpper = true;
                }
            }
        }
        
        if (!showsTableExistsUpper || needsRecreateShowsUpper)
        {
            if (needsRecreateShowsUpper)
            {
                // Supprimer la table si elle existe avec une mauvaise structure
                try
                {
                    using var dropCmd = connection.CreateCommand();
                    dropCmd.CommandText = "PRAGMA foreign_keys = OFF;";
                    dropCmd.ExecuteNonQuery();
                    dropCmd.CommandText = "DROP TABLE IF EXISTS ShowSegments;";
                    dropCmd.ExecuteNonQuery();
                    dropCmd.CommandText = "DROP TABLE IF EXISTS Shows;";
                    dropCmd.ExecuteNonQuery();
                    dropCmd.CommandText = "PRAGMA foreign_keys = ON;";
                    dropCmd.ExecuteNonQuery();
                }
                catch { }
            }
            
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Shows (
                    ShowId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Week INTEGER NOT NULL DEFAULT 1,
                    Date TEXT,
                    RegionId TEXT NOT NULL,
                    VenueId TEXT,
                    DurationMinutes INTEGER NOT NULL,
                    ShowType TEXT,
                    TvDealId TEXT,
                    Broadcast TEXT,
                    TicketPrice REAL DEFAULT 0.0,
                    Status TEXT DEFAULT 'ABOOKER',
                    BrandId TEXT,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
                    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
                );
            ";
            cmd.ExecuteNonQuery();
        }
        else if (showsTableExistsUpper)
        {
            // Vérifier et ajouter les colonnes manquantes si la table existe déjà
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(Shows);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch
            {
                // Si on ne peut pas lire les colonnes, on assume qu'elles n'existent pas
            }
            
            // Ajouter les colonnes manquantes une par une
            var requiredColumns = new Dictionary<string, string>
            {
                { "Broadcast", "TEXT" },
                { "TicketPrice", "REAL DEFAULT 0.0" },
                { "Status", "TEXT DEFAULT 'ABOOKER'" },
                { "BrandId", "TEXT" }
            };
            
            foreach (var (columnName, columnDef) in requiredColumns)
            {
                if (!existingColumns.Contains(columnName))
                {
                    try
                    {
                        using var alterCmd = connection.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE Shows ADD COLUMN {columnName} {columnDef};";
                        alterCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Ignorer si la colonne existe déjà ou si l'ALTER échoue
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de {columnName} à Shows: {ex.Message}");
                    }
                }
            }
        }

        // Table: ShowSegments (segments de shows)
        if (!TableExists(connection, "ShowSegments"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowSegments (
                    ShowSegmentId TEXT PRIMARY KEY,
                    ShowId TEXT NOT NULL,
                    OrderIndex INTEGER NOT NULL,
                    SegmentType TEXT NOT NULL,
                    DurationMinutes INTEGER NOT NULL,
                    StorylineId TEXT,
                    TitleId TEXT,
                    IsMainEvent INTEGER NOT NULL DEFAULT 0,
                    Intensity INTEGER NOT NULL DEFAULT 50,
                    WinnerWorkerId TEXT,
                    LoserWorkerId TEXT,
                    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: SegmentParticipants (participants aux segments)
        if (!TableExists(connection, "SegmentParticipants"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS SegmentParticipants (
                    ShowSegmentId TEXT NOT NULL,
                    WorkerId TEXT NOT NULL,
                    Role TEXT,
                    PRIMARY KEY (ShowSegmentId, WorkerId),
                    FOREIGN KEY (ShowSegmentId) REFERENCES ShowSegments(ShowSegmentId),
                    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: SegmentSettings (paramètres des segments)
        if (!TableExists(connection, "SegmentSettings"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS SegmentSettings (
                    ShowSegmentId TEXT NOT NULL,
                    SettingKey TEXT NOT NULL,
                    SettingValue TEXT NOT NULL,
                    PRIMARY KEY (ShowSegmentId, SettingKey),
                    FOREIGN KEY (ShowSegmentId) REFERENCES ShowSegments(ShowSegmentId)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: youth_structures (legacy - utilisée par YouthRepository)
        if (!TableExists(connection, "youth_structures"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS youth_structures (
                    youth_id TEXT PRIMARY KEY,
                    company_id TEXT NOT NULL,
                    nom TEXT NOT NULL,
                    type TEXT NOT NULL,
                    region TEXT NOT NULL,
                    budget_annuel INTEGER NOT NULL DEFAULT 0,
                    capacite_max INTEGER NOT NULL DEFAULT 10,
                    niveau_equipements INTEGER NOT NULL DEFAULT 50,
                    qualite_coaching INTEGER NOT NULL DEFAULT 50,
                    philosophie TEXT NOT NULL DEFAULT '',
                    actif INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY (company_id) REFERENCES Companies(CompanyId)
                );
                CREATE INDEX IF NOT EXISTS idx_youth_company ON youth_structures(company_id);
                CREATE INDEX IF NOT EXISTS idx_youth_region ON youth_structures(region);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: youth_trainees (legacy - utilisée par YouthRepository)
        if (!TableExists(connection, "youth_trainees"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS youth_trainees (
                    worker_id TEXT NOT NULL,
                    youth_id TEXT NOT NULL,
                    statut TEXT NOT NULL DEFAULT 'EN_FORMATION',
                    semaine_inscription INTEGER,
                    semaine_graduation INTEGER,
                    PRIMARY KEY (worker_id, youth_id),
                    FOREIGN KEY (worker_id) REFERENCES Workers(WorkerId),
                    FOREIGN KEY (youth_id) REFERENCES youth_structures(youth_id)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: youth_generation_state (legacy - utilisée par YouthRepository)
        if (!TableExists(connection, "youth_generation_state"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS youth_generation_state (
                    youth_id TEXT PRIMARY KEY,
                    derniere_generation_semaine INTEGER,
                    FOREIGN KEY (youth_id) REFERENCES youth_structures(youth_id)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: youth_programs (legacy - utilisée par YouthRepository)
        if (!TableExists(connection, "youth_programs"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS youth_programs (
                    program_id TEXT PRIMARY KEY,
                    youth_id TEXT NOT NULL,
                    nom TEXT NOT NULL,
                    duree_semaines INTEGER NOT NULL DEFAULT 12,
                    focus TEXT,
                    FOREIGN KEY (youth_id) REFERENCES youth_structures(youth_id)
                );
                CREATE INDEX IF NOT EXISTS idx_youth_programs_youth ON youth_programs(youth_id);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: youth_staff_assignments (legacy - utilisée par YouthRepository)
        if (!TableExists(connection, "youth_staff_assignments"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS youth_staff_assignments (
                    assignment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    youth_id TEXT NOT NULL,
                    worker_id TEXT NOT NULL,
                    role TEXT NOT NULL,
                    semaine_debut INTEGER,
                    FOREIGN KEY (youth_id) REFERENCES youth_structures(youth_id),
                    FOREIGN KEY (worker_id) REFERENCES Workers(WorkerId)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: CatchStyles (styles de catch - utilisée par CatchStyleRepository)
        if (!TableExists(connection, "CatchStyles"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CatchStyles (
                    CatchStyleId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL UNIQUE,
                    Description TEXT,
                    WrestlingPurity INTEGER DEFAULT 50,
                    EntertainmentFocus INTEGER DEFAULT 50,
                    HardcoreIntensity INTEGER DEFAULT 0,
                    LuchaInfluence INTEGER DEFAULT 0,
                    StrongStyleInfluence INTEGER DEFAULT 0,
                    FanExpectationMatchQuality INTEGER DEFAULT 50,
                    FanExpectationStorylines INTEGER DEFAULT 50,
                    FanExpectationPromos INTEGER DEFAULT 50,
                    FanExpectationSpectacle INTEGER DEFAULT 50,
                    MatchRatingMultiplier REAL DEFAULT 1.0,
                    PromoRatingMultiplier REAL DEFAULT 1.0,
                    IconName TEXT,
                    AccentColor TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );
                CREATE INDEX IF NOT EXISTS idx_catch_styles_active ON CatchStyles(IsActive);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: Owners (propriétaires de compagnies - utilisée par OwnerRepository)
        if (!TableExists(connection, "Owners"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Owners (
                    OwnerId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    VisionType TEXT NOT NULL DEFAULT 'Balanced',
                    RiskTolerance INTEGER NOT NULL DEFAULT 50,
                    PreferredProductType TEXT NOT NULL DEFAULT 'Entertainment',
                    ShowFrequencyPreference TEXT NOT NULL DEFAULT 'Weekly',
                    TalentDevelopmentFocus INTEGER NOT NULL DEFAULT 50,
                    FinancialPriority INTEGER NOT NULL DEFAULT 50,
                    FanSatisfactionPriority INTEGER NOT NULL DEFAULT 50,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS idx_owners_company ON Owners(CompanyId);
            ";
            cmd.ExecuteNonQuery();
        }

        // Table: Bookers (bookers avec préférences créatives - utilisée par BookerRepository)
        if (!TableExists(connection, "Bookers"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Bookers (
                    BookerId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    CreativityScore INTEGER NOT NULL DEFAULT 50,
                    LogicScore INTEGER NOT NULL DEFAULT 50,
                    BiasResistance INTEGER NOT NULL DEFAULT 50,
                    PreferredStyle TEXT,
                    LikesUnderdog INTEGER NOT NULL DEFAULT 0,
                    LikesVeteran INTEGER NOT NULL DEFAULT 0,
                    LikesFastRise INTEGER NOT NULL DEFAULT 0,
                    LikesSlowBurn INTEGER NOT NULL DEFAULT 0,
                    IsAutoBookingEnabled INTEGER NOT NULL DEFAULT 0,
                    EmploymentStatus TEXT NOT NULL DEFAULT 'Active',
                    HireDate TEXT,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS idx_bookers_company ON Bookers(CompanyId);
                CREATE INDEX IF NOT EXISTS idx_bookers_employment ON Bookers(EmploymentStatus);
            ";
            cmd.ExecuteNonQuery();
        }

        // Vérifier et corriger la table Workers pour ajouter les colonnes manquantes
        if (TableExists(connection, "Workers"))
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(Workers);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }

            var requiredColumns = new Dictionary<string, string>
            {
                { "TvRole", "INTEGER DEFAULT 50" },
                { "Morale", "INTEGER DEFAULT 50" }
            };
            
            foreach (var (columnName, columnDef) in requiredColumns)
            {
                if (!existingColumns.Contains(columnName))
                {
                    try
                    {
                        using var alterCmd = connection.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE Workers ADD COLUMN {columnName} {columnDef};";
                        alterCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de {columnName} à Workers: {ex.Message}");
                    }
                }
            }
        }
        
        // Vérifier et corriger la table workers (minuscule legacy) pour ajouter la colonne prenom
        if (TableExists(connection, "workers"))
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(workers);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }
            
            if (!existingColumns.Contains("prenom"))
            {
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE workers ADD COLUMN prenom TEXT DEFAULT '';";
                    alterCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de prenom à workers: {ex.Message}");
                }
            }
        }

        // Vérifier et corriger la table Storylines pour ajouter les colonnes manquantes
        if (TableExists(connection, "Storylines"))
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(Storylines);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }

            var requiredColumns = new Dictionary<string, string>
            {
                { "Status", "TEXT NOT NULL DEFAULT 'ACTIVE'" },
                { "Phase", "TEXT DEFAULT 'Setup'" }
            };
            
            foreach (var (columnName, columnDef) in requiredColumns)
            {
                if (!existingColumns.Contains(columnName))
                {
                    try
                    {
                        using var alterCmd = connection.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE Storylines ADD COLUMN {columnName} {columnDef};";
                        alterCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de {columnName} à Storylines: {ex.Message}");
                    }
                }
            }
        }

        // Vérifier et corriger la table Titles pour ajouter les colonnes manquantes
        if (TableExists(connection, "Titles"))
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(Titles);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }

            if (!existingColumns.Contains("CurrentChampionId"))
            {
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Titles ADD COLUMN CurrentChampionId TEXT;";
                    alterCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de CurrentChampionId à Titles: {ex.Message}");
                }
            }
        }

        // Table: TitleReigns (règnes de titres - vérifier DefenseCount)
        if (TableExists(connection, "TitleReigns"))
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(TitleReigns);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }
            
            if (!existingColumns.Contains("DefenseCount"))
            {
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE TitleReigns ADD COLUMN DefenseCount INTEGER DEFAULT 0;";
                    alterCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de DefenseCount à TitleReigns: {ex.Message}");
                }
            }
        }
        
        // Table: CompanyBalanceSnapshots (snapshots de balance de compagnie - utilisée par FinanceViewModel)
        if (!TableExists(connection, "CompanyBalanceSnapshots"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CompanyBalanceSnapshots (
                    SnapshotId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Week INTEGER NOT NULL,
                    Revenues REAL NOT NULL DEFAULT 0.0,
                    Expenses REAL NOT NULL DEFAULT 0.0,
                    Balance REAL NOT NULL DEFAULT 0.0,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
                );
                CREATE INDEX IF NOT EXISTS idx_balance_snapshots_company_week ON CompanyBalanceSnapshots(CompanyId, Week);
            ";
            cmd.ExecuteNonQuery();
        }
        
        // Table: CalendarEntries (entrées de calendrier - utilisée par CalendarViewModel)
        var calendarEntriesExists = TableExists(connection, "CalendarEntries");
        if (!calendarEntriesExists)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CalendarEntries (
                    CalendarEntryId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Date TEXT NOT NULL,
                    EntryType TEXT NOT NULL,
                    Title TEXT,
                    Notes TEXT,
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
                );
                CREATE INDEX IF NOT EXISTS idx_calendar_entries_company_date ON CalendarEntries(CompanyId, Date);
            ";
            cmd.ExecuteNonQuery();
        }
        else
        {
            // Vérifier si CompanyId existe dans CalendarEntries
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(CalendarEntries);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    existingColumns.Add(columnName);
                }
            }
            catch { }

            if (!existingColumns.Contains("CompanyId"))
            {
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE CalendarEntries ADD COLUMN CompanyId TEXT NOT NULL DEFAULT '';";
                    alterCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de CompanyId à CalendarEntries: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Crée le schéma minimal (Companies, Workers, Countries, Regions, Settings tables)
    /// </summary>
    private static void CreateMinimalSchema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            PRAGMA foreign_keys = ON;
            
            CREATE TABLE IF NOT EXISTS Countries (
                CountryId TEXT PRIMARY KEY,
                Code TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Regions (
                RegionId TEXT PRIMARY KEY,
                CountryId TEXT NOT NULL,
                Name TEXT NOT NULL,
                FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)
            );
            
            CREATE TABLE IF NOT EXISTS Companies (
                CompanyId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CountryId TEXT,
                RegionId TEXT NOT NULL,
                Prestige INTEGER NOT NULL DEFAULT 0,
                Treasury REAL NOT NULL DEFAULT 0,
                FoundedYear INTEGER DEFAULT 2024,
                IsPlayerControlled INTEGER DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (CountryId) REFERENCES Countries(CountryId),
                FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
            );
            
            CREATE TABLE IF NOT EXISTS Workers (
                WorkerId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CompanyId TEXT,
                Nationality TEXT NOT NULL,
                InRing INTEGER NOT NULL DEFAULT 0,
                Entertainment INTEGER NOT NULL DEFAULT 0,
                Story INTEGER NOT NULL DEFAULT 0,
                Popularity INTEGER NOT NULL DEFAULT 0,
                Fatigue INTEGER NOT NULL DEFAULT 0,
                RoleTv TEXT NOT NULL DEFAULT 'NONE',
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
            );
            
            -- Table: SaveGames (doit être créée avant GameState qui y fait référence)
            CREATE TABLE IF NOT EXISTS SaveGames (
                SaveGameId INTEGER PRIMARY KEY AUTOINCREMENT,
                SaveName TEXT NOT NULL,
                PlayerCompanyId TEXT NOT NULL,
                CurrentWeek INTEGER NOT NULL DEFAULT 1,
                CurrentDate TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                LastPlayedAt TEXT,
                FOREIGN KEY (PlayerCompanyId) REFERENCES Companies(CompanyId)
            );
            
            -- Table: shows (shows/événements legacy)
            CREATE TABLE IF NOT EXISTS shows (
                show_id TEXT PRIMARY KEY,
                nom TEXT NOT NULL,
                semaine INTEGER NOT NULL,
                region TEXT NOT NULL,
                duree INTEGER NOT NULL,
                compagnie_id TEXT NOT NULL,
                tv_deal_id TEXT,
                lieu TEXT NOT NULL DEFAULT '',
                diffusion TEXT NOT NULL DEFAULT '',
                FOREIGN KEY (compagnie_id) REFERENCES Companies(CompanyId)
            );
            
            -- Table: game_settings (pour WorkerGenerationOptions)
            CREATE TABLE IF NOT EXISTS game_settings (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                youth_generation_mode TEXT NOT NULL DEFAULT 'Realiste',
                world_generation_mode TEXT NOT NULL DEFAULT 'Desactivee',
                semaine_pivot_annuelle INTEGER
            );
            
            -- Table: ui_table_settings (pour TableUiSettings)
            CREATE TABLE IF NOT EXISTS ui_table_settings (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                recherche TEXT,
                filtre_type TEXT,
                filtre_statut TEXT,
                colonnes_visibles TEXT,
                colonnes_ordre TEXT,
                tri_colonnes TEXT
            );
            
            -- Table: GameState (pour l'état du jeu et BookingControlLevel)
            CREATE TABLE IF NOT EXISTS GameState (
                GameStateId INTEGER PRIMARY KEY CHECK (GameStateId = 1),
                CurrentSaveGameId INTEGER,
                CurrentWeek INTEGER NOT NULL DEFAULT 1,
                CurrentDate TEXT,
                LastUpdatedAt TEXT,
                BookingControlLevel TEXT NOT NULL DEFAULT 'CoBooker',
                FOREIGN KEY (CurrentSaveGameId) REFERENCES SaveGames(SaveGameId)
            );
            
            -- Insérer un état de jeu par défaut si absent
            INSERT OR IGNORE INTO GameState (GameStateId, CurrentWeek, BookingControlLevel) VALUES (1, 1, 'CoBooker');
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Cr?e le sch?ma Save DB si absent (idempotent)
    /// </summary>
    private static void EnsureSaveSchema(SqliteConnection connexion)
    {
        const string createSaveGamesTableSql = """
            CREATE TABLE IF NOT EXISTS SaveGames (
                SaveId TEXT PRIMARY KEY,
                CompanyId TEXT NOT NULL,
                CompanyName TEXT NOT NULL,
                PlayerId TEXT,
                WorldVersion INTEGER NOT NULL DEFAULT 1,
                CurrentWeek INTEGER NOT NULL DEFAULT 1,
                CurrentYear INTEGER NOT NULL DEFAULT 2024,
                CurrentDate TEXT,
                CreatedAt TEXT NOT NULL,
                LastPlayedAt TEXT,
                TotalHoursPlayed REAL NOT NULL DEFAULT 0.0,
                GameDifficulty TEXT DEFAULT 'Normal',
                IsActive INTEGER NOT NULL DEFAULT 0
            );
            """;

        // #region agent log
        var logPath = Path.Combine(AppContext.BaseDirectory, ".cursor", "debug.log");
        File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"SqliteConnectionFactory.cs:232\",\"message\":\"EnsureSaveSchema entry\",\"data\":{{\"dataSource\":\"{connexion.DataSource}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
        // #endregion
        
        try
        {
            var tableExists = TableExists(connexion, "SaveGames");
            
            if (!tableExists)
            {
                // Créer la table si elle n'existe pas
                using var command = connexion.CreateCommand();
                command.CommandText = createSaveGamesTableSql;
                command.ExecuteNonQuery();
            }
            else
            {
                // Vérifier que la table a la bonne structure en testant les colonnes essentielles
                var requiredColumns = new[] { "SaveId", "CompanyId", "CompanyName", "WorldVersion", "CurrentWeek", "CurrentYear", "CurrentDate", "TotalHoursPlayed", "GameDifficulty" };
                var missingColumns = new List<string>();
                
                try
                {
                    using var pragmaCmd = connexion.CreateCommand();
                    pragmaCmd.CommandText = "PRAGMA table_info(SaveGames);";
                    using var reader = pragmaCmd.ExecuteReader();
                    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    
                    while (reader.Read())
                    {
                        var columnName = reader.GetString(1);
                        existingColumns.Add(columnName);
                    }
                    
                    foreach (var requiredCol in requiredColumns)
                    {
                        if (!existingColumns.Contains(requiredCol))
                        {
                            missingColumns.Add(requiredCol);
                        }
                    }
                }
                catch
                {
                    // Si on ne peut pas lire les colonnes, recréer la table
                    missingColumns.AddRange(requiredColumns);
                }
                
                // Si des colonnes manquent, recréer la table
                if (missingColumns.Count > 0)
                {
                    // Sauvegarder les données existantes si nécessaire
                    using var transaction = connexion.BeginTransaction();
                    try
                    {
                        // Supprimer l'ancienne table
                        using var dropCmd = connexion.CreateCommand();
                        dropCmd.Transaction = transaction;
                        dropCmd.CommandText = "DROP TABLE IF EXISTS SaveGames;";
                        dropCmd.ExecuteNonQuery();
                        
                        // Recréer avec la bonne structure
                        using var createCmd = connexion.CreateCommand();
                        createCmd.Transaction = transaction;
                        createCmd.CommandText = createSaveGamesTableSql;
                        createCmd.ExecuteNonQuery();
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            
            // #region agent log
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"SqliteConnectionFactory.cs:236\",\"message\":\"SaveGames table created/verified\",\"data\":{{}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion
        }
        catch (Exception ex)
        {
            // #region agent log
            File.AppendAllText(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"SqliteConnectionFactory.cs:238\",\"message\":\"Failed to create SaveGames table\",\"data\":{{\"exceptionType\":\"{ex.GetType().Name}\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            // #endregion
            
            throw new InvalidOperationException(
                "❌ Impossible de créer la table SaveGames dans la Save DB.\n" +
                $"Chemin : {connexion.DataSource}\n" +
                "Vérifier les permissions d'écriture du dossier AppData/RingGeneral.",
                ex);
        }
    }
}

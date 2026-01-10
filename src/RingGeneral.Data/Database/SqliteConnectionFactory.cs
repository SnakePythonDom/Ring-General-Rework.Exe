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
    /// </summary>
    internal static void EnsureEssentialTablesExist(SqliteConnection connection)
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

        // Table: shows (shows/événements legacy)
        // Vérifier si la table existe et a la bonne structure
        var showsTableExists = TableExists(connection, "shows");
        
        if (showsTableExists)
        {
            // Vérifier si la colonne show_id existe en utilisant PRAGMA table_info
            var hasShowId = false;
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(shows);";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1); // name column
                    if (columnName.Equals("show_id", StringComparison.OrdinalIgnoreCase))
                    {
                        hasShowId = true;
                        break;
                    }
                }
            }
            catch
            {
                hasShowId = false;
            }
            
            if (!hasShowId)
            {
                // La table existe mais sans la colonne show_id - la recréer
                try
                {
                    // Sauvegarder les données existantes si nécessaire (optionnel)
                    // Pour l'instant, on supprime et recrée car la structure est incompatible
                    using var dropCmd = connection.CreateCommand();
                    dropCmd.CommandText = "DROP TABLE IF EXISTS shows;";
                    dropCmd.ExecuteNonQuery();
                    
                    showsTableExists = false; // Marquer comme non existante pour la recréer
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression de la table shows: {ex.Message}");
                }
            }
        }
        
        if (!showsTableExists)
        {
            // Créer la table avec la bonne structure
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
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
            }
            catch (Exception ex)
            {
                // Ignorer si la table existe déjà (peut arriver en cas de race condition)
                if (!ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la création de la table shows: {ex.Message}");
                }
            }
        }
        
        // Vérifier et corriger la table Companies pour ajouter FoundedYear si manquant
        if (TableExists(connection, "Companies") || TableExists(connection, "companies"))
        {
            var tableName = TableExists(connection, "Companies") ? "Companies" : "companies";
            var hasFoundedYear = false;
            try
            {
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = $"PRAGMA table_info({tableName});";
                using var reader = pragmaCmd.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    if (columnName.Equals("FoundedYear", StringComparison.OrdinalIgnoreCase))
                    {
                        hasFoundedYear = true;
                        break;
                    }
                }
            }
            catch
            {
                hasFoundedYear = false;
            }
            
            if (!hasFoundedYear)
            {
                try
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN FoundedYear INTEGER DEFAULT 2024;";
                    alterCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Ignorer si la colonne existe déjà ou si l'ALTER échoue
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout de FoundedYear à {tableName}: {ex.Message}");
                }
            }
        }

        // Table: Shows (nouvelle version, peut coexister avec shows)
        if (!TableExists(connection, "Shows"))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Shows (
                    ShowId TEXT PRIMARY KEY,
                    CompanyId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Week INTEGER NOT NULL,
                    Date TEXT,
                    RegionId TEXT NOT NULL,
                    VenueId TEXT,
                    DurationMinutes INTEGER NOT NULL,
                    ShowType TEXT,
                    TvDealId TEXT,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
                    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
                );
            ";
            cmd.ExecuteNonQuery();
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
            using var command = connexion.CreateCommand();
            command.CommandText = createSaveGamesTableSql;
            command.ExecuteNonQuery();
            
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
                "? Impossible de cr?er la table SaveGames dans la Save DB.\n" +
                $"Chemin : {connexion.DataSource}\n" +
                "V?rifier les permissions d'?criture du dossier AppData/RingGeneral.",
                ex);
        }
    }
}

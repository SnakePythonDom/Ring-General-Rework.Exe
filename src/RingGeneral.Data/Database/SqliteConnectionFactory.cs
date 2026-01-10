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

        // ? VALIDATION CRITIQUE : V?rifier que c'est la bonne DB
        if (!TableExists(connexion, "companies") && !TableExists(connexion, "Companies"))
        {
            connexion.Dispose();
            throw new InvalidOperationException(
                $"? Mauvaise base de donn?es charg?e.\n" +
                $"Table 'Companies' introuvable.\n\n" +
                $"Chemin attendu : {GeneralDatabasePath}\n\n" +
                $"Conseil :\n" +
                $"  1. V?rifier que ring_general.db existe et n'est pas vide\n" +
                $"  2. V?rifier que ring_general.db contient les tables Companies et Workers\n" +
                $"  3. Chercher la variable d'env RINGGENERAL_GENERAL_DB_PATH (si d?finie)");
        }

        if (!TableExists(connexion, "workers") && !TableExists(connexion, "Workers"))
        {
            connexion.Dispose();
            throw new InvalidOperationException(
                $"? Mauvaise base de donn?es charg?e.\n" +
                $"Table 'Workers' introuvable.\n\n" +
                $"Chemin attendu : {GeneralDatabasePath}\n\n" +
                $"Conseil : V?rifier que ring_general.db est la bonne base de donn?es.");
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
    /// Retourne le chemin de la DB spécifiée (pour logging/debug)
    /// </summary>
    public string GetDbFilePath(bool isSaveDb = false)
    {
        return isSaveDb ? SaveDatabasePath : GeneralDatabasePath;
    }

    /// <summary>
    /// Retourne l'état des deux bases (pour logs au démarrage)
    /// </summary>
    public (bool GeneralDbExists, bool SaveDbExists) CheckDatabasesExist()
    {
        return (
            File.Exists(GeneralDatabasePath),
            File.Exists(SaveDatabasePath)
        );
    }

    /// <summary>
    /// ?? LEGACY : Propriété de compatibilité
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
    /// V?rifie l'existence d'une table (case-insensitive)
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

using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace RingGeneral.Data.Database;

/// <summary>
/// Gestionnaire des sauvegardes de jeu avec orchestration des deux bases SQLite.
/// 
/// Architecture :
// - WORLD DB : ring_world.db (données monde statiques)
// - SAVE DB : ring_save.db (table SaveGames + état partie active)
/// 
/// Responsabilités :
// - CRUD dans SAVE DB (SaveGames table)
// - Activation/déactivation de sauvegarde unique
// - Calcul WorldVersion pour compatibilité
// - Listing et nettoyage des sauvegardes
// - Fallback sur fichiers .db pour backward compatibility
/// </summary>
public sealed class SaveGameManager
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly DbInitializer _initializer;
    private readonly IDbValidator _validator;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    // ✅ NOUVEAU : Chemin Save DB dans AppData
    public string SaveDatabasePath => _connectionFactory.SaveDatabasePath;
    public string SavesDirectory { get; }

    /// <summary>
    /// Initialise le manager avec les deux bases.
    /// </summary>
    public SaveGameManager(
        SqliteConnectionFactory connectionFactory,
        DbInitializer initializer,
        IDbValidator validator)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        // ✅ Dossier legacy pour backward compatibility (fichiers .db)
        SavesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RingGeneral",
            "Saves");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API - SAVE DB (NOUVEAU)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ✅ Crée une nouvelle sauvegarde dans SAVE DB et la marque comme active.
    /// Désactive toutes les autres sauvegardes pour ce joueur.
    /// </summary>
    public SaveGameInfo CreerNouvellePartie(string companyId, string companyName, string? playerId = null, string difficulty = "Normal")
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentException("companyId est requis", nameof(companyId));
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("companyName est requis", nameof(companyName));

        var saveId = GenerateSaveId(companyId);
        var worldVersion = CalculateWorldVersion();
        var now = DateTime.UtcNow;

        using var conn = _connectionFactory.CreateSaveConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1️⃣ Désactiver les anciennes sauvegardes
            using (var deactivateCmd = conn.CreateCommand())
            {
                deactivateCmd.Transaction = transaction;
                deactivateCmd.CommandText = "UPDATE SaveGames SET IsActive = 0 WHERE PlayerId = @playerId;";
                deactivateCmd.Parameters.AddWithValue("@playerId", (object?)playerId ?? DBNull.Value);
                deactivateCmd.ExecuteNonQuery();
            }

            // 2️⃣ Créer la nouvelle sauvegarde
            using (var insertCmd = conn.CreateCommand())
            {
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = """
                    INSERT INTO SaveGames (
                        SaveId, CompanyId, CompanyName, PlayerId, WorldVersion,
                        CurrentWeek, CurrentYear, CreatedAt, LastPlayedAt,
                        TotalHoursPlayed, GameDifficulty, IsActive
                    ) VALUES (
                        @saveId, @companyId, @companyName, @playerId, @worldVersion,
                        @week, @year, @createdAt, @lastPlayedAt,
                        @totalHours, @difficulty, 1
                    );
                    """;

                insertCmd.Parameters.AddWithValue("@saveId", saveId);
                insertCmd.Parameters.AddWithValue("@companyId", companyId);
                insertCmd.Parameters.AddWithValue("@companyName", companyName);
                insertCmd.Parameters.AddWithValue("@playerId", (object?)playerId ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@worldVersion", worldVersion);
                insertCmd.Parameters.AddWithValue("@week", 1);
                insertCmd.Parameters.AddWithValue("@year", 2024);
                insertCmd.Parameters.AddWithValue("@createdAt", now.ToString("O"));
                insertCmd.Parameters.AddWithValue("@lastPlayedAt", now.ToString("O"));
                insertCmd.Parameters.AddWithValue("@totalHours", 0.0);
                insertCmd.Parameters.AddWithValue("@difficulty", difficulty);

                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();

            return new SaveGameInfo(
                SaveId: saveId,
                Name: companyName,
                FullPath: SaveDatabasePath,
                LastWriteTime: now,
                CompanyId: companyId,
                WorldVersion: worldVersion,
                IsFromSaveDb: true);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new InvalidOperationException($"Erreur lors de la création de la sauvegarde pour {companyName}.", ex);
        }
    }

    /// <summary>
    /// ✅ Charge la sauvegarde active depuis SAVE DB (s'il en existe une).
    /// </summary>
    public SaveGameInfo? ChargerSauvegardeActive()
    {
        using var conn = _connectionFactory.CreateSaveConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT SaveId, CompanyId, CompanyName, WorldVersion, CreatedAt, IsActive
            FROM SaveGames
            WHERE IsActive = 1
            LIMIT 1;
            """;

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        var saveId = reader.GetString(0);
        var companyId = reader.GetString(1);
        var companyName = reader.GetString(2);
        var worldVersion = reader.GetInt32(3);
        var createdAt = DateTime.Parse(reader.GetString(4));

        // ⚠️ Vérification compatibilité WorldVersion
        if (!IsWorldVersionCompatible(worldVersion))
        {
            throw new InvalidOperationException(
                $"❌ Sauvegarde incompatible.\n" +
                $"Monde version {worldVersion}, application version {CalculateWorldVersion()}.\n" +
                $"Veuillez mettre à jour la sauvegarde ou réinstaller.");
        }

        return new SaveGameInfo(
            SaveId: saveId,
            Name: companyName,
            FullPath: SaveDatabasePath,
            LastWriteTime: createdAt,
            CompanyId: companyId,
            WorldVersion: worldVersion,
            IsFromSaveDb: true);
    }

    /// <summary>
    /// ✅ Met à jour une sauvegarde active (appelé après chaque semaine de jeu).
    /// </summary>
    public void MettreAJourSauvegarde(string saveId, int currentWeek, int currentYear, double totalHours)
    {
        using var conn = _connectionFactory.CreateSaveConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE SaveGames SET
                CurrentWeek = @week,
                CurrentYear = @year,
                LastPlayedAt = @lastPlayed,
                TotalHoursPlayed = @totalHours
            WHERE SaveId = @saveId;
            """;

        cmd.Parameters.AddWithValue("@week", currentWeek);
        cmd.Parameters.AddWithValue("@year", currentYear);
        cmd.Parameters.AddWithValue("@lastPlayed", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("@totalHours", totalHours);
        cmd.Parameters.AddWithValue("@saveId", saveId);

        var rowsAffected = cmd.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Sauvegarde {saveId} introuvable.");
    }

    /// <summary>
    /// ✅ Liste toutes les sauvegardes actives du SAVE DB.
    /// </summary>
    public IReadOnlyList<SaveGameInfo> ListerSauvegardesActives()
    {
        var saves = new List<SaveGameInfo>();

        using var conn = _connectionFactory.CreateSaveConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT SaveId, CompanyId, CompanyName, WorldVersion, CreatedAt
            FROM SaveGames
            WHERE IsActive = 0
            ORDER BY CreatedAt DESC
            LIMIT 100;
            """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            saves.Add(new SaveGameInfo(
                SaveId: reader.GetString(0),
                Name: reader.GetString(2),
                FullPath: SaveDatabasePath,
                LastWriteTime: DateTime.Parse(reader.GetString(4)),
                CompanyId: reader.GetString(1),
                WorldVersion: reader.GetInt32(3),
                IsFromSaveDb: true));
        }

        return saves;
    }

    /// <summary>
    /// ✅ Supprime une sauvegarde du SAVE DB.
    /// </summary>
    public void SupprimerSauvegardeDb(string saveId)
    {
        if (string.IsNullOrWhiteSpace(saveId))
            throw new ArgumentException("saveId est requis", nameof(saveId));

        using var conn = _connectionFactory.CreateSaveConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM SaveGames WHERE SaveId = @saveId;";
        cmd.Parameters.AddWithValue("@saveId", saveId);
        cmd.ExecuteNonQuery();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API - LEGACY (BACKWARD COMPATIBILITY)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ⚠️ LEGACY : Liste les fichiers .db dans le dossier Saves (backward compat).
    /// </summary>
    public IReadOnlyList<SaveGameInfo> ListerSaves()
    {
        if (!Directory.Exists(SavesDirectory))
        {
            return Array.Empty<SaveGameInfo>();
        }

        return Directory.GetFiles(SavesDirectory, "*.db", SearchOption.TopDirectoryOnly)
            .Select(chemin => new FileInfo(chemin))
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .Select(info => new SaveGameInfo(
                SaveId: Path.GetFileNameWithoutExtension(info.Name),
                Name: Path.GetFileNameWithoutExtension(info.Name),
                FullPath: info.FullName,
                LastWriteTime: info.LastWriteTime,
                CompanyId: null,
                WorldVersion: 1,
                IsFromSaveDb: false))
            .ToList();
    }

    /// <summary>
    /// ⚠️ LEGACY : Crée une nouvelle partie (fichier .db).
    /// </summary>
    public SaveGameInfo CreerNouvellePartieDepuisFichier(string? nom)
    {
        Directory.CreateDirectory(SavesDirectory);
        var nomFinal = string.IsNullOrWhiteSpace(nom) ? $"sauvegarde_{DateTime.UtcNow:yyyyMMdd_HHmmss}" : nom.Trim();
        var nomNettoye = NettoyerNomFichier(nomFinal);
        var chemin = ObtenirCheminUnique(nomNettoye);

        _initializer.CreateDatabaseIfMissing(chemin);
        
        var info = new FileInfo(chemin);
        return new SaveGameInfo(
            SaveId: Path.GetFileNameWithoutExtension(info.Name),
            Name: Path.GetFileNameWithoutExtension(info.Name),
            FullPath: info.FullName,
            LastWriteTime: info.LastWriteTime,
            CompanyId: null,
            WorldVersion: 1,
            IsFromSaveDb: false);
    }

    /// <summary>
    /// ⚠️ LEGACY : Importe une base (fichier .db).
    /// </summary>
    public SaveGameInfo ImporterBase(string cheminSource)
    {
        if (string.IsNullOrWhiteSpace(cheminSource))
        {
            throw new InvalidOperationException("Chemin d'import manquant.");
        }

        if (!File.Exists(cheminSource))
        {
            throw new InvalidOperationException("Le fichier à importer est introuvable.");
        }

        if (!string.Equals(Path.GetExtension(cheminSource), ".db", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Le fichier sélectionné n'est pas une base SQLite (.db).");
        }

        Directory.CreateDirectory(SavesDirectory);
        var nom = Path.GetFileNameWithoutExtension(cheminSource);
        var cheminDestination = ObtenirCheminUnique(nom);
        File.Copy(cheminSource, cheminDestination, overwrite: false);

        var validation = _validator.Valider(cheminDestination);
        if (!validation.EstValide)
        {
            File.Delete(cheminDestination);
            var details = string.Join("\n", validation.Erreurs);
            throw new InvalidOperationException($"Base importée invalide :\n{details}");
        }

        var info = new FileInfo(cheminDestination);
        return new SaveGameInfo(
            SaveId: Path.GetFileNameWithoutExtension(info.Name),
            Name: Path.GetFileNameWithoutExtension(info.Name),
            FullPath: info.FullName,
            LastWriteTime: info.LastWriteTime,
            CompanyId: null,
            WorldVersion: 1,
            IsFromSaveDb: false);
    }

    /// <summary>
    /// ⚠️ LEGACY : Duplique une sauvegarde (fichier .db).
    /// </summary>
    public SaveGameInfo DupliquerSauvegarde(string cheminSource)
    {
        if (string.IsNullOrWhiteSpace(cheminSource))
        {
            throw new InvalidOperationException("Sélectionnez une sauvegarde à dupliquer.");
        }

        if (!File.Exists(cheminSource))
        {
            throw new InvalidOperationException("La sauvegarde sélectionnée est introuvable.");
        }

        Directory.CreateDirectory(SavesDirectory);
        var nom = Path.GetFileNameWithoutExtension(cheminSource);
        var cheminDestination = ObtenirCheminUnique($"{nom}_copie");
        File.Copy(cheminSource, cheminDestination, overwrite: false);
        
        var info = new FileInfo(cheminDestination);
        return new SaveGameInfo(
            SaveId: Path.GetFileNameWithoutExtension(info.Name),
            Name: Path.GetFileNameWithoutExtension(info.Name),
            FullPath: info.FullName,
            LastWriteTime: info.LastWriteTime,
            CompanyId: null,
            WorldVersion: 1,
            IsFromSaveDb: false);
    }

    /// <summary>
    /// ⚠️ LEGACY : Supprime un fichier .db.
    /// </summary>
    public void SupprimerSauvegarde(string cheminSource)
    {
        if (string.IsNullOrWhiteSpace(cheminSource))
        {
            throw new InvalidOperationException("Sélectionnez une sauvegarde à supprimer.");
        }

        if (!File.Exists(cheminSource))
        {
            throw new InvalidOperationException("La sauvegarde sélectionnée est introuvable.");
        }

        File.Delete(cheminSource);
    }

    /// <summary>
    /// Valide une sauvegarde (fichier .db).
    /// </summary>
    public DbValidationResult ValiderSauvegarde(string cheminDb)
        => _validator.Valider(cheminDb);

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Génère un SaveId unique basé sur companyId + timestamp.
    /// </summary>
    private static string GenerateSaveId(string companyId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"SAVE_{companyId}_{timestamp}";
    }

    /// <summary>
    /// Calcule la version du monde basée sur World DB.
    /// </summary>
    private int CalculateWorldVersion()
    {
        try
        {
            var worldPath = _connectionFactory.GeneralDatabasePath;
            if (!File.Exists(worldPath))
                return 1;

            var fileInfo = new FileInfo(worldPath);
            var hashCode = (fileInfo.Length ^ fileInfo.LastWriteTimeUtc.Ticks).GetHashCode();
            return Math.Abs(hashCode % 10000) + 1; // Range: 1-10001
        }
        catch
        {
            return 1;
        }
    }

    /// <summary>
    /// Vérifie la compatibilité du WorldVersion.
    /// </summary>
    private bool IsWorldVersionCompatible(int saveWorldVersion)
    {
        var currentVersion = CalculateWorldVersion();
        var diff = Math.Abs(saveWorldVersion - currentVersion);
        return diff <= 1; // Tolérance de 1 version
    }

    private string ObtenirCheminUnique(string nomBase)
    {
        var nomNettoye = NettoyerNomFichier(nomBase);
        var chemin = Path.Combine(SavesDirectory, $"{nomNettoye}.db");
        if (!File.Exists(chemin))
        {
            return chemin;
        }

        var compteur = 1;
        string cheminUnique;
        do
        {
            cheminUnique = Path.Combine(SavesDirectory, $"{nomNettoye}_{compteur}.db");
            compteur++;
        } while (File.Exists(cheminUnique));

        return cheminUnique;
    }

    private static string NettoyerNomFichier(string nom)
    {
        var caracteresInvalides = Path.GetInvalidFileNameChars();
        var nettoye = new string(nom.Select(c => caracteresInvalides.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(nettoye) ? "sauvegarde" : nettoye;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// DTO - METADONNÉES SAUVEGARDE
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Information sur une sauvegarde de jeu.
/// </summary>
public sealed record SaveGameInfo(
    string SaveId,
    string Name,
    string FullPath,
    DateTime LastWriteTime,
    string? CompanyId,
    int WorldVersion,
    bool IsFromSaveDb)
{
    /// <summary>
    /// Affichage formaté pour l'UI.
    /// </summary>
    public string DisplayName => $"{Name} ({(IsFromSaveDb ? "SAVE DB" : "Fichier")})";

    /// <summary>
    /// Affichage formaté de la date.
    /// </summary>
    public string LastPlayedDisplay => $"Dernière modification : {LastWriteTime:yyyy-MM-dd HH:mm:ss}";

    /// <summary>
    /// Retourne le temps écoulé depuis la création.
    /// </summary>
    public TimeSpan Age => DateTime.Now - LastWriteTime;
}

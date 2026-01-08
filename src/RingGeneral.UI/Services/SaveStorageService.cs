using System.IO.Compression;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.Services;

public sealed record SaveInfo(string Nom, string Chemin, DateTime DerniereModification);

public sealed record DatabaseValidationResult(bool EstValide, string Message);

public sealed class SaveStorageService
{
    private static readonly string[] TablesRequises =
    [
        "companies",
        "workers",
        "titles",
        "storylines",
        "storyline_participants",
        "shows",
        "segments",
        "chimies",
        "finances",
        "show_history",
        "segment_history",
        "inbox_items",
        "contracts",
        "popularity_regionale"
    ];

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SaveStorageService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        CheminRacine = Path.Combine(appData, "RingGeneral", "Saves");
    }

    public string CheminRacine { get; }

    public void AssurerDossier()
    {
        Directory.CreateDirectory(CheminRacine);
    }

    public IReadOnlyList<SaveInfo> ListerSauvegardes()
    {
        AssurerDossier();
        return Directory
            .EnumerateFiles(CheminRacine, "*.db", SearchOption.TopDirectoryOnly)
            .Select(ConstruireInfo)
            .OrderByDescending(info => info.DerniereModification)
            .ToList();
    }

    public SaveInfo CreerSauvegarde(string? nom)
    {
        AssurerDossier();
        var nomFinal = string.IsNullOrWhiteSpace(nom)
            ? $"Sauvegarde {DateTime.Now:yyyy-MM-dd HHmm}"
            : nom;
        var chemin = ObtenirCheminUnique(nomFinal);
        new DbInitializer().CreateDatabaseIfMissing(chemin);
        var factory = new SqliteConnectionFactory($"Data Source={chemin}");
        RepositoryFactory.CreateRepositories(factory);
        return ConstruireInfo(chemin);
    }

    public SaveInfo ImporterBase(string cheminSource)
    {
        AssurerDossier();
        var validation = ValiderBase(cheminSource);
        if (!validation.EstValide)
        {
            throw new InvalidOperationException(validation.Message);
        }

        var nom = Path.GetFileNameWithoutExtension(cheminSource);
        var chemin = ObtenirCheminUnique(nom);
        File.Copy(cheminSource, chemin, true);
        return ConstruireInfo(chemin);
    }

    public SaveInfo ImporterPack(string cheminPack)
    {
        AssurerDossier();
        string? tempDb = null;
        try
        {
            using var archive = ZipFile.OpenRead(cheminPack);
            var entry = archive.Entries
                .FirstOrDefault(e => e.FullName.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                throw new InvalidOperationException("Le pack ne contient pas de base de données .db.");
            }

            tempDb = Path.Combine(Path.GetTempPath(), $"ringgeneral_{Guid.NewGuid():N}.db");
            entry.ExtractToFile(tempDb, true);

            var validation = ValiderBase(tempDb);
            if (!validation.EstValide)
            {
                throw new InvalidOperationException(validation.Message);
            }

            var nom = ChargerNomPack(archive) ?? Path.GetFileNameWithoutExtension(cheminPack);
            var chemin = ObtenirCheminUnique(nom);
            File.Copy(tempDb, chemin, true);
            return ConstruireInfo(chemin);
        }
        finally
        {
            if (tempDb is not null && File.Exists(tempDb))
            {
                File.Delete(tempDb);
            }
        }
    }

    public void ExporterPack(SaveInfo sauvegarde, string cheminDestination)
    {
        if (File.Exists(cheminDestination))
        {
            File.Delete(cheminDestination);
        }

        using var archive = ZipFile.Open(cheminDestination, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(sauvegarde.Chemin, $"{sauvegarde.Nom}.db");

        var manifest = new SavePackManifest(sauvegarde.Nom, DateTimeOffset.Now);
        var entry = archive.CreateEntry("manifest.json");
        using var writer = new StreamWriter(entry.Open());
        writer.Write(JsonSerializer.Serialize(manifest, _jsonOptions));
    }

    public void ExporterBase(SaveInfo sauvegarde, string cheminDestination)
    {
        if (File.Exists(cheminDestination))
        {
            File.Delete(cheminDestination);
        }

        File.Copy(sauvegarde.Chemin, cheminDestination);
    }

    public DatabaseValidationResult ValiderBase(string cheminBase)
    {
        if (!File.Exists(cheminBase))
        {
            return new DatabaseValidationResult(false, "Le fichier sélectionné est introuvable.");
        }

        try
        {
            using var connection = new SqliteConnection($"Data Source={cheminBase}");
            connection.Open();

            using var integrityCommand = connection.CreateCommand();
            integrityCommand.CommandText = "PRAGMA integrity_check;";
            var integrity = Convert.ToString(integrityCommand.ExecuteScalar());
            if (!string.Equals(integrity, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return new DatabaseValidationResult(false,
                    "La base de données est corrompue (integrity_check KO).");
            }

            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            using var reader = tableCommand.ExecuteReader();
            var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            var missing = TablesRequises.Where(table => !tables.Contains(table)).ToList();
            if (missing.Count > 0)
            {
                return new DatabaseValidationResult(false,
                    $"La base importée n'est pas une sauvegarde Ring General valide. Tables manquantes : {string.Join(", ", missing)}.");
            }

            return new DatabaseValidationResult(true, "Base de données valide.");
        }
        catch (SqliteException)
        {
            return new DatabaseValidationResult(false,
                "Le fichier fourni n'est pas une base SQLite lisible.");
        }
        catch (Exception ex)
        {
            return new DatabaseValidationResult(false,
                $"Impossible de valider la base : {ex.Message}");
        }
    }

    private SaveInfo ConstruireInfo(string chemin)
    {
        var info = new FileInfo(chemin);
        return new SaveInfo(Path.GetFileNameWithoutExtension(chemin), chemin, info.LastWriteTime);
    }

    private string ObtenirCheminUnique(string nom)
    {
        var baseName = NettoyerNom(nom);
        var chemin = Path.Combine(CheminRacine, $"{baseName}.db");
        var index = 1;
        while (File.Exists(chemin))
        {
            chemin = Path.Combine(CheminRacine, $"{baseName} ({index}).db");
            index += 1;
        }

        return chemin;
    }

    private static string NettoyerNom(string nom)
    {
        var invalides = Path.GetInvalidFileNameChars();
        var nettoye = new string(nom.Select(ch => invalides.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(nettoye) ? "Sauvegarde" : nettoye.Trim();
    }

    private string? ChargerNomPack(ZipArchive archive)
    {
        var entry = archive.GetEntry("manifest.json");
        if (entry is null)
        {
            return null;
        }

        try
        {
            using var reader = new StreamReader(entry.Open());
            var json = reader.ReadToEnd();
            var manifest = JsonSerializer.Deserialize<SavePackManifest>(json, _jsonOptions);
            return manifest?.Nom;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record SavePackManifest(string Nom, DateTimeOffset ExporteLe);
}

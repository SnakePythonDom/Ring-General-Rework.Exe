using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using RingGeneral.Data.Models;

namespace RingGeneral.Data.Repositories;

public sealed class SettingsRepository : RepositoryBase
{
    public SettingsRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public WorkerGenerationOptions ChargerParametresGeneration()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT youth_generation_mode, world_generation_mode, semaine_pivot_annuelle FROM game_settings WHERE id = 1;";
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var youthMode = Enum.TryParse<YouthGenerationMode>(reader.GetString(0), out var ym)
                ? ym
                : YouthGenerationMode.Realiste;
            var worldMode = Enum.TryParse<WorldGenerationMode>(reader.GetString(1), out var wm)
                ? wm
                : WorldGenerationMode.Desactivee;
            int? pivot = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
            return new WorkerGenerationOptions(youthMode, worldMode, pivot);
        }

        return new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, null);
    }

    public void SauvegarderParametresGeneration(WorkerGenerationOptions options)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO game_settings (id, youth_generation_mode, world_generation_mode, semaine_pivot_annuelle)
            VALUES (1, $youthMode, $worldMode, $pivot)
            ON CONFLICT(id) DO UPDATE SET
                youth_generation_mode = excluded.youth_generation_mode,
                world_generation_mode = excluded.world_generation_mode,
                semaine_pivot_annuelle = excluded.semaine_pivot_annuelle;
            """;
        command.Parameters.AddWithValue("$youthMode", options.YouthMode.ToString());
        command.Parameters.AddWithValue("$worldMode", options.WorldMode.ToString());
        command.Parameters.AddWithValue("$pivot", (object?)options.SemainePivotAnnuelle ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public TableUiSettings ChargerTableUiSettings()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT recherche, filtre_type, filtre_statut, colonnes_visibles, colonnes_ordre, tri_colonnes
            FROM ui_table_settings
            WHERE id = 1;
            """;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new TableUiSettings(null, null, null, new Dictionary<string, bool>(), new List<string>(), new List<TableSortSetting>());
        }

        var colonnesVisibles = LireJson<Dictionary<string, bool>>(reader, 3) ?? new Dictionary<string, bool>();
        var colonnesOrdre = LireJson<List<string>>(reader, 4) ?? new List<string>();
        var tris = LireJson<List<TableSortSetting>>(reader, 5) ?? new List<TableSortSetting>();
        return new TableUiSettings(
            reader.IsDBNull(0) ? null : reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            colonnesVisibles,
            colonnesOrdre,
            tris);
    }

    public void SauvegarderTableUiSettings(TableUiSettings settings)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO ui_table_settings (
                id,
                recherche,
                filtre_type,
                filtre_statut,
                colonnes_visibles,
                colonnes_ordre,
                tri_colonnes
            )
            VALUES (1, $recherche, $filtreType, $filtreStatut, $colonnesVisibles, $colonnesOrdre, $triColonnes)
            ON CONFLICT(id) DO UPDATE SET
                recherche = excluded.recherche,
                filtre_type = excluded.filtre_type,
                filtre_statut = excluded.filtre_statut,
                colonnes_visibles = excluded.colonnes_visibles,
                colonnes_ordre = excluded.colonnes_ordre,
                tri_colonnes = excluded.tri_colonnes;
            """;
        command.Parameters.AddWithValue("$recherche", (object?)settings.Recherche ?? DBNull.Value);
        command.Parameters.AddWithValue("$filtreType", (object?)settings.FiltreType ?? DBNull.Value);
        command.Parameters.AddWithValue("$filtreStatut", (object?)settings.FiltreStatut ?? DBNull.Value);
        command.Parameters.AddWithValue("$colonnesVisibles", SerializeJson(settings.ColonnesVisibles));
        command.Parameters.AddWithValue("$colonnesOrdre", SerializeJson(settings.ColonnesOrdre));
        command.Parameters.AddWithValue("$triColonnes", SerializeJson(settings.Tris));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Phase 1.2 - Charge le niveau de contrôle du booking depuis GameState
    /// </summary>
    public string ChargerBookingControlLevel()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT BookingControlLevel FROM GameState WHERE GameStateId = 1;";
        var result = command.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            return result.ToString() ?? "CoBooker";
        }
        return "CoBooker"; // Valeur par défaut
    }

    /// <summary>
    /// Phase 1.2 - Sauvegarde le niveau de contrôle du booking dans GameState
    /// </summary>
    public void SauvegarderBookingControlLevel(string controlLevel)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE GameState 
            SET BookingControlLevel = $controlLevel 
            WHERE GameStateId = 1;
            """;
        command.Parameters.AddWithValue("$controlLevel", controlLevel);
        command.ExecuteNonQuery();
    }
}

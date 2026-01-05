using Microsoft.Data.Sqlite;

namespace RingGeneral.Data.Database;

public sealed class DbValidator : IDbValidator
{
    private static readonly IReadOnlyDictionary<string, string[]> ColonnesObligatoires =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["SchemaVersion"] = new[] { "Version", "AppliedAt" },
            ["Countries"] = new[] { "CountryId", "Code", "Name" },
            ["Regions"] = new[] { "RegionId", "CountryId", "Name" },
            ["Companies"] = new[] { "CompanyId", "Name", "RegionId", "SimLevel", "LastSimulatedAt" },
            ["CompanyCustomization"] = new[] { "CompanyCustomizationId", "CompanyId" },
            ["NetworkRelations"] = new[] { "NetworkRelationId", "CompanyId", "TargetCompanyId" },
            ["Workers"] = new[] { "WorkerId", "Name", "Nationality", "CompanyId", "SimLevel", "LastSimulatedAt" },
            ["WorkerAttributes"] = new[] { "WorkerId" },
            ["WorkerPopularityByRegion"] = new[] { "WorkerId", "RegionId", "Popularity" },
            ["Contracts"] = new[] { "ContractId", "WorkerId", "CompanyId", "EndDate" },
            ["Titles"] = new[] { "TitleId", "CompanyId", "Name" },
            ["TitleReigns"] = new[] { "TitleReignId", "TitleId", "WorkerId" },
            ["Storylines"] = new[] { "StorylineId", "CompanyId", "Name" },
            ["StorylineParticipants"] = new[] { "StorylineId", "WorkerId" },
            ["Shows"] = new[] { "ShowId", "CompanyId", "Date" },
            ["ShowSegments"] = new[] { "ShowSegmentId", "ShowId", "OrderIndex" },
            ["SegmentParticipants"] = new[] { "ShowSegmentId", "WorkerId" },
            ["SegmentResults"] = new[] { "SegmentResultId", "ShowSegmentId" },
            ["Injuries"] = new[] { "InjuryId", "WorkerId" },
            ["Fatigue"] = new[] { "WorkerId", "Value" },
            ["FinanceTransactions"] = new[] { "FinanceTransactionId", "CompanyId", "Date", "Amount" },
            ["TVDeals"] = new[] { "TvDealId", "CompanyId", "Reach", "ConstraintsJson", "RevenuePerPoint" },
            ["AudienceHistory"] = new[] { "AudienceHistoryId", "ShowId", "Week", "Audience" },
            ["YouthStructures"] = new[] { "YouthStructureId", "CompanyId" },
            ["YouthTrainees"] = new[] { "YouthTraineeId", "YouthStructureId", "WorkerId" },
            ["Calendars"] = new[] { "CalendarId", "CompanyId", "Date" },
            ["Venues"] = new[] { "VenueId", "Name" }
        };

    public DbValidationResult Valider(string cheminDb)
    {
        if (string.IsNullOrWhiteSpace(cheminDb))
        {
            return new DbValidationResult(false, new[] { "Chemin de base de données manquant." });
        }

        if (!File.Exists(cheminDb))
        {
            return new DbValidationResult(false, new[] { $"Base de données introuvable : {cheminDb}" });
        }

        var erreurs = new List<string>();
        using var connexion = new SqliteConnection($"Data Source={cheminDb}");
        connexion.Open();

        foreach (var (table, colonnes) in ColonnesObligatoires)
        {
            if (!TableExiste(connexion, table))
            {
                erreurs.Add($"Table obligatoire manquante : {table}.");
                continue;
            }

            var colonnesExistantes = ChargerColonnes(connexion, table);
            foreach (var colonne in colonnes)
            {
                if (!colonnesExistantes.Contains(colonne))
                {
                    erreurs.Add($"Colonne obligatoire manquante : {table}.{colonne}.");
                }
            }
        }

        if (TableExiste(connexion, "SchemaVersion"))
        {
            var version = ChargerVersionSchema(connexion);
            if (version is null)
            {
                erreurs.Add("Impossible de déterminer la version du schéma (SchemaVersion vide).");
            }
            else if (version < DbInitializer.SchemaVersionActuelle)
            {
                erreurs.Add($"Schéma obsolète (version {version}). Version requise : {DbInitializer.SchemaVersionActuelle}.");
            }
        }

        return erreurs.Count == 0
            ? DbValidationResult.Ok()
            : new DbValidationResult(false, erreurs);
    }

    private static bool TableExiste(SqliteConnection connexion, string table)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return command.ExecuteScalar() is not null;
    }

    private static HashSet<string> ChargerColonnes(SqliteConnection connexion, string table)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        var colonnes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            colonnes.Add(reader.GetString(1));
        }

        return colonnes;
    }

    private static int? ChargerVersionSchema(SqliteConnection connexion)
    {
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT MAX(Version) FROM SchemaVersion;";
        var valeur = command.ExecuteScalar();
        return valeur is null or DBNull ? null : Convert.ToInt32(valeur);
    }
}

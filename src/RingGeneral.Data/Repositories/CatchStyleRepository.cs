using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class CatchStyleRepository : RepositoryBase, ICatchStyleRepository
{
    public CatchStyleRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public async Task<IReadOnlyList<CatchStyle>> GetAllActiveStylesAsync()
    {
        return await Task.Run(() =>
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    CatchStyleId, Name, Description,
                    WrestlingPurity, EntertainmentFocus, HardcoreIntensity,
                    LuchaInfluence, StrongStyleInfluence,
                    FanExpectationMatchQuality, FanExpectationStorylines,
                    FanExpectationPromos, FanExpectationSpectacle,
                    MatchRatingMultiplier, PromoRatingMultiplier,
                    IconName, AccentColor, IsActive
                FROM CatchStyles
                WHERE IsActive = 1
                ORDER BY Name;
                """;

            using var reader = command.ExecuteReader();
            var styles = new List<CatchStyle>();

            while (reader.Read())
            {
                styles.Add(MapCatchStyle(reader));
            }

            return styles;
        });
    }

    public async Task<CatchStyle?> GetStyleByIdAsync(string styleId)
    {
        return await Task.Run(() =>
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    CatchStyleId, Name, Description,
                    WrestlingPurity, EntertainmentFocus, HardcoreIntensity,
                    LuchaInfluence, StrongStyleInfluence,
                    FanExpectationMatchQuality, FanExpectationStorylines,
                    FanExpectationPromos, FanExpectationSpectacle,
                    MatchRatingMultiplier, PromoRatingMultiplier,
                    IconName, AccentColor, IsActive
                FROM CatchStyles
                WHERE CatchStyleId = $styleId;
                """;
            command.Parameters.AddWithValue("$styleId", styleId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapCatchStyle(reader);
        });
    }

    public async Task<IReadOnlyList<CatchStyle>> GetCompatibleStylesAsync(string preferredProductType)
    {
        return await Task.Run(() =>
        {
            // Mapping Owner.PreferredProductType -> CatchStyles compatibles
            var compatibleStyleIds = preferredProductType switch
            {
                "Technical" => new[] { "STYLE_PURE_WRESTLING", "STYLE_STRONG_STYLE", "STYLE_HYBRID", "STYLE_INDIE" },
                "Entertainment" => new[] { "STYLE_SPORTS_ENTERTAINMENT", "STYLE_HYBRID", "STYLE_FAMILY_FRIENDLY", "STYLE_LUCHA_LIBRE" },
                "Hardcore" => new[] { "STYLE_HARDCORE", "STYLE_STRONG_STYLE", "STYLE_INDIE" },
                "Family-Friendly" => new[] { "STYLE_FAMILY_FRIENDLY", "STYLE_LUCHA_LIBRE", "STYLE_SPORTS_ENTERTAINMENT" },
                _ => new[] { "STYLE_HYBRID" }
            };

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            // Construction de la clause IN
            var placeholders = string.Join(", ", compatibleStyleIds.Select((_, i) => $"$style{i}"));
            command.CommandText = $"""
                SELECT
                    CatchStyleId, Name, Description,
                    WrestlingPurity, EntertainmentFocus, HardcoreIntensity,
                    LuchaInfluence, StrongStyleInfluence,
                    FanExpectationMatchQuality, FanExpectationStorylines,
                    FanExpectationPromos, FanExpectationSpectacle,
                    MatchRatingMultiplier, PromoRatingMultiplier,
                    IconName, AccentColor, IsActive
                FROM CatchStyles
                WHERE CatchStyleId IN ({placeholders})
                  AND IsActive = 1
                ORDER BY Name;
                """;

            for (int i = 0; i < compatibleStyleIds.Length; i++)
            {
                command.Parameters.AddWithValue($"$style{i}", compatibleStyleIds[i]);
            }

            using var reader = command.ExecuteReader();
            var styles = new List<CatchStyle>();

            while (reader.Read())
            {
                styles.Add(MapCatchStyle(reader));
            }

            return styles;
        });
    }

    public double CalculateStyleMatchBonus(CatchStyle style, int matchWorkrate, int matchEntertainment, int matchHardcore)
    {
        // Calcule l'alignement entre le match et le style de la compagnie
        // Retourne un multiplicateur (0.8 à 1.3)

        // Normalisation des attributs du match (0-100)
        matchWorkrate = Math.Clamp(matchWorkrate, 0, 100);
        matchEntertainment = Math.Clamp(matchEntertainment, 0, 100);
        matchHardcore = Math.Clamp(matchHardcore, 0, 100);

        // Calcul de l'adéquation (plus c'est proche du style, mieux c'est)
        double workrateAlignment = 1.0 - Math.Abs(style.WrestlingPurity - matchWorkrate) / 100.0;
        double entertainmentAlignment = 1.0 - Math.Abs(style.EntertainmentFocus - matchEntertainment) / 100.0;
        double hardcoreAlignment = 1.0 - Math.Abs(style.HardcoreIntensity - matchHardcore) / 100.0;

        // Moyenne pondérée
        double averageAlignment = (workrateAlignment + entertainmentAlignment + hardcoreAlignment) / 3.0;

        // Convertir en multiplicateur
        // averageAlignment = 1.0 (parfait) -> 1.3x
        // averageAlignment = 0.75 -> 1.15x
        // averageAlignment = 0.5 -> 1.0x
        // averageAlignment = 0.0 (totalement opposé) -> 0.8x

        double multiplier = 0.8 + (averageAlignment * 0.5);
        return Math.Round(multiplier, 2);
    }

    private static CatchStyle MapCatchStyle(SqliteDataReader reader)
    {
        return new CatchStyle(
            CatchStyleId: reader.GetString(0),
            Name: reader.GetString(1),
            Description: reader.IsDBNull(2) ? null : reader.GetString(2),
            WrestlingPurity: reader.GetInt32(3),
            EntertainmentFocus: reader.GetInt32(4),
            HardcoreIntensity: reader.GetInt32(5),
            LuchaInfluence: reader.GetInt32(6),
            StrongStyleInfluence: reader.GetInt32(7),
            FanExpectationMatchQuality: reader.GetInt32(8),
            FanExpectationStorylines: reader.GetInt32(9),
            FanExpectationPromos: reader.GetInt32(10),
            FanExpectationSpectacle: reader.GetInt32(11),
            MatchRatingMultiplier: reader.GetDouble(12),
            PromoRatingMultiplier: reader.GetDouble(13),
            IconName: reader.IsDBNull(14) ? null : reader.GetString(14),
            AccentColor: reader.IsDBNull(15) ? null : reader.GetString(15),
            IsActive: reader.GetInt32(16) == 1
        );
    }
}

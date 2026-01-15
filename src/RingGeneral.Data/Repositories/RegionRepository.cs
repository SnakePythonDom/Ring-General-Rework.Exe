using RingGeneral.Data.Database;
using RingGeneral.Core.Interfaces;
using System.Collections.Generic;

namespace RingGeneral.Data.Repositories;

public sealed class RegionRepository : RepositoryBase, IRegionRepository
{
    public RegionRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<RegionSelection> GetRegions()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.RegionId, r.Name, c.Name as CountryName, c.Continent, r.WrestlingImportance
            FROM Regions r
            INNER JOIN Countries c ON c.CountryId = r.CountryId
            ORDER BY c.Continent, c.Name, r.Name
            LIMIT 500;
            """;

        using var reader = command.ExecuteReader();
        var regions = new List<RegionSelection>();

        while (reader.Read())
        {
            regions.Add(new RegionSelection(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? "Unknown" : reader.GetString(3),
                reader.IsDBNull(4) ? 0 : reader.GetInt32(4)));
        }

        return regions;
    }
}

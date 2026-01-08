using RingGeneral.Data.Database;

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
            SELECT r.RegionId, r.Name, c.Name as CountryName
            FROM Regions r
            INNER JOIN Countries c ON c.CountryId = r.CountryId
            ORDER BY c.Name, r.Name
            LIMIT 500;
            """;

        using var reader = command.ExecuteReader();
        var regions = new List<RegionSelection>();

        while (reader.Read())
        {
            regions.Add(new RegionSelection(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2)));
        }

        return regions;
    }
}

public sealed record RegionSelection(string RegionId, string RegionName, string CountryName);

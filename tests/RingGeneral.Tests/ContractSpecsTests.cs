using RingGeneral.Specs.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class ContractSpecsTests
{
    [Fact]
    public void Specs_contrats_v1_se_charge()
    {
        var racine = TrouverRacineRepo();
        var reader = new SpecsReader();
        var specPath = Path.Combine(racine, "specs", "contracts", "contracts-v1.fr.json");
        var spec = reader.Charger<Dictionary<string, object>>(specPath);

        Assert.NotNull(spec);
        Assert.True(spec.ContainsKey("typesContrat"));
        Assert.True(spec.ContainsKey("clauses"));
    }

    private static string TrouverRacineRepo()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "specs")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException("Impossible de trouver le dossier specs.");
        }

        return directory.FullName;
    }
}

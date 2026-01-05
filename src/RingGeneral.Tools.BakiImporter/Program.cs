using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Import;
using RingGeneral.Core.Random;
using RingGeneral.Specs.Models.Import;
using RingGeneral.Specs.Services;
using RingGeneral.Tools.BakiImporter;

if (args.Length == 0)
{
    return AfficherAide();
}

var commande = args[0].ToLowerInvariant();
var options = args.Skip(1).ToArray();

try
{
    return commande switch
    {
        "attr-report" => ExecuterAttrReport(options),
        _ => AfficherAide()
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur : {ex.Message}");
    return 1;
}

static int ExecuterAttrReport(string[] options)
{
    var source = ExtraireOption(options, "--source");
    var output = ExtraireOption(options, "--out");
    var specPath = ExtraireOption(options, "--spec")
        ?? Path.Combine("specs", "import", "baki-attribute-mapping.fr.json");
    var seedValue = ExtraireOption(options, "--seed");

    if (string.IsNullOrWhiteSpace(source))
    {
        Console.Error.WriteLine("Chemin de base source manquant. Utilisez --source <baki.db>.");
        return 1;
    }

    if (string.IsNullOrWhiteSpace(output))
    {
        Console.Error.WriteLine("Chemin de sortie manquant. Utilisez --out <report.json>.");
        return 1;
    }

    if (!File.Exists(source))
    {
        Console.Error.WriteLine("La base source est introuvable.");
        return 1;
    }

    var specsReader = new SpecsReader();
    var spec = specsReader.Charger<BakiAttributeMappingSpec>(specPath);
    var seed = int.TryParse(seedValue, out var parsedSeed)
        ? parsedSeed
        : spec.RandomVariation?.Seed ?? 0;
    var random = new SeededRandomProvider(seed);
    var converter = new BakiAttributeConverter(spec, random);

    using var connection = new SqliteConnection($"Data Source={source}");
    connection.Open();

    var report = BakiAttributeReportGenerator.Generate(connection, spec, converter);
    var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(output, json);

    Console.WriteLine($"Rapport généré : {output}");
    return 0;
}

static string? ExtraireOption(string[] options, string nom)
{
    for (var i = 0; i < options.Length - 1; i++)
    {
        if (string.Equals(options[i], nom, StringComparison.OrdinalIgnoreCase))
        {
            return options[i + 1];
        }
    }

    return null;
}

static int AfficherAide()
{
    Console.WriteLine("RingGeneral.Tools.BakiImporter");
    Console.WriteLine("Usage :");
    Console.WriteLine("  baki attr-report --source <baki.db> --out <report.json> [--spec <path>] [--seed <int>]");
    return 1;
}

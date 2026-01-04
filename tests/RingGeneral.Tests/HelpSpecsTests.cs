using System.Text.Json;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class HelpSpecsTests
{
    [Fact]
    public void Specs_help_se_chargent()
    {
        var racine = TrouverRacineRepo();
        var reader = new SpecsReader();

        var tooltips = reader.Charger<HelpTooltipsSpec>(Path.Combine(racine, "specs", "help", "tooltips.fr.json"));
        var glossaire = reader.Charger<HelpGlossaireSpec>(Path.Combine(racine, "specs", "help", "glossaire.fr.json"));
        var systemes = reader.Charger<HelpSystemsSpec>(Path.Combine(racine, "specs", "help", "systems.fr.json"));
        var pages = reader.Charger<HelpPagesSpec>(Path.Combine(racine, "specs", "help", "pages.fr.json"));
        var tutoriel = reader.Charger<HelpTutorialSpec>(Path.Combine(racine, "specs", "help", "tutorial-first-season.fr.json"));

        Assert.NotEmpty(tooltips.Tooltips);
        Assert.NotEmpty(glossaire.Entrees);
        Assert.NotEmpty(systemes.Systemes);
        Assert.NotEmpty(pages.Pages);
        Assert.NotEmpty(tutoriel.Etapes);
    }

    [Fact]
    public void Tooltips_couvrent_les_attributs_principaux()
    {
        var racine = TrouverRacineRepo();
        var reader = new SpecsReader();
        var tooltips = reader.Charger<HelpTooltipsSpec>(Path.Combine(racine, "specs", "help", "tooltips.fr.json"));
        var ids = tooltips.Tooltips.Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var attendus = new[]
        {
            "attr.inring",
            "attr.entertainment",
            "attr.story",
            "attr.popularite",
            "attr.fatigue",
            "attr.momentum"
        };

        foreach (var id in attendus)
        {
            Assert.Contains(id, ids);
        }
    }

    [Fact]
    public void Navigation_contient_la_page_aide()
    {
        var racine = TrouverRacineRepo();
        var navigationPath = Path.Combine(racine, "specs", "navigation.fr.json");
        using var document = JsonDocument.Parse(File.ReadAllText(navigationPath));
        var routes = document.RootElement.GetProperty("sidebar").GetProperty("sections")
            .EnumerateArray()
            .Select(element => element.TryGetProperty("route", out var route) ? route.GetString() : null)
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .ToList();

        Assert.Contains("/aide", routes);
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

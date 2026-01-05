using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

namespace RingGeneral.UI.Services;

public sealed class UiPageSpecsProvider
{
    private readonly SpecsReader _reader = new();
    private readonly string? _specsRoot;

    public UiPageSpecsProvider()
    {
        _specsRoot = TrouverRacineSpecs();
    }

    public IReadOnlyList<UiPageSpec> ChargerPages()
    {
        if (_specsRoot is null)
        {
            return [];
        }

        var dossier = Path.Combine(_specsRoot, "ui", "pages");
        if (!Directory.Exists(dossier))
        {
            return [];
        }

        var pages = new List<UiPageSpec>();
        foreach (var chemin in Directory.EnumerateFiles(dossier, "*.fr.json").OrderBy(Path.GetFileName))
        {
            pages.Add(_reader.Charger<UiPageSpec>(chemin));
        }

        return pages;
    }

    private static string? TrouverRacineSpecs()
    {
        var candidats = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs")
        };

        foreach (var candidat in candidats)
        {
            if (Directory.Exists(candidat))
            {
                return candidat;
            }
        }

        return null;
    }
}

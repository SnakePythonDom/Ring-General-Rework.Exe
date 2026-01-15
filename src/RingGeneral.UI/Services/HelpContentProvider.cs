using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

namespace RingGeneral.UI.Services;

public sealed class HelpContentProvider
{
    private readonly SpecsReader _reader = new();
    private readonly string? _specsRoot;

    public HelpContentProvider()
    {
        _specsRoot = TrouverRacineSpecs();
    }

    public HelpTooltipsSpec ChargerTooltips()
    {
        return ChargerSpec<HelpTooltipsSpec>("help/tooltips.fr.json") ?? new HelpTooltipsSpec();
    }

    public HelpPagesSpec ChargerPages()
    {
        return ChargerSpec<HelpPagesSpec>("help/pages.fr.json") ?? new HelpPagesSpec();
    }

    public HelpGlossaireSpec ChargerGlossaire()
    {
        return ChargerSpec<HelpGlossaireSpec>("help/glossaire.fr.json") ?? new HelpGlossaireSpec();
    }

    public HelpSystemsSpec ChargerSystemes()
    {
        return ChargerSpec<HelpSystemsSpec>("help/systems.fr.json") ?? new HelpSystemsSpec();
    }

    public HelpTutorialSpec ChargerTutorial()
    {
        return ChargerSpec<HelpTutorialSpec>("help/tutorial-first-season.fr.json") ?? new HelpTutorialSpec();
    }

    private T? ChargerSpec<T>(string cheminRelatif)
    {
        var chemin = TrouverSpec(cheminRelatif);
        return chemin is null ? default : _reader.Charger<T>(chemin);
    }

    private string? TrouverSpec(string cheminRelatif)
    {
        if (_specsRoot is null)
        {
            return null;
        }

        var chemin = Path.Combine(_specsRoot, cheminRelatif.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(chemin) ? chemin : null;
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

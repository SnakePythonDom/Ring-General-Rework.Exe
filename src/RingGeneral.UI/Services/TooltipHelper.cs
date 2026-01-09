namespace RingGeneral.UI.Services;

public sealed class TooltipHelper
{
    private readonly IReadOnlyDictionary<string, string> _tooltips;

    public TooltipHelper(HelpContentProvider provider)
    {
        var spec = provider.ChargerTooltips();
        _tooltips = spec.Tooltips.ToDictionary(tooltip => tooltip.Id, tooltip => tooltip.Texte, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string> Tooltips => _tooltips;

    public string Obtenir(string id)
    {
        return _tooltips.TryGetValue(id, out var texte) ? texte : string.Empty;
    }
}

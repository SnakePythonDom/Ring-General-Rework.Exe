namespace RingGeneral.Specs.Services;

public sealed class SpecsValidator
{
    public IReadOnlyList<string> ValiderIdsUniques(IEnumerable<string> ids, string contexte)
    {
        var erreurs = new List<string>();
        var vus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                erreurs.Add($"Identifiant manquant dans {contexte}.");
                continue;
            }

            if (!vus.Add(id))
            {
                erreurs.Add($"Identifiant dupliqué '{id}' détecté dans {contexte}.");
            }
        }

        return erreurs;
    }
}

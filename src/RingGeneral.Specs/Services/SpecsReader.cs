using System.Text.Json;

namespace RingGeneral.Specs.Services;

public sealed class SpecsReader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public T Charger<T>(string chemin)
    {
        if (!File.Exists(chemin))
        {
            throw new FileNotFoundException($"Spécification introuvable: {chemin}");
        }

        var json = File.ReadAllText(chemin);
        var resultat = JsonSerializer.Deserialize<T>(json, _options);

        if (resultat is null)
        {
            throw new InvalidDataException($"Spécification invalide: {chemin}");
        }

        return resultat;
    }
}

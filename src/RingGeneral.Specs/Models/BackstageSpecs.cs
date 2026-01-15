using System.Text.Json.Serialization;

namespace RingGeneral.Specs.Models;

public sealed record BackstageSpec(
    [property: JsonPropertyName("meta")] BackstageMeta Meta,
    [property: JsonPropertyName("morale")] BackstageMoraleSpec Morale,
    [property: JsonPropertyName("discipline")] BackstageDisciplineSpec Discipline)
{
    public static BackstageSpec ParDefaut => new(
        new BackstageMeta("fr", "backstage-v1", 1),
        new BackstageMoraleSpec("0-100", 60, new[]
        {
            new BackstageMoraleThresholdSpec("bas", "Bas", 0, 39),
            new BackstageMoraleThresholdSpec("stable", "Stable", 40, 69),
            new BackstageMoraleThresholdSpec("haut", "Haut", 70, 100)
        }),
        new BackstageDisciplineSpec(new[]
        {
            new BackstageDisciplineActionSpec("avertissement", "Avertissement", 1, -2),
            new BackstageDisciplineActionSpec("amende", "Amende", 2, -5),
            new BackstageDisciplineActionSpec("suspension", "Suspension", 3, -8)
        }));
}

public sealed record BackstageMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele,
    [property: JsonPropertyName("version")] int Version);

public sealed record BackstageMoraleSpec(
    [property: JsonPropertyName("echelle")] string Echelle,
    [property: JsonPropertyName("valeurDefaut")] int ValeurDefaut,
    [property: JsonPropertyName("seuils")] IReadOnlyList<BackstageMoraleThresholdSpec> Seuils);

public sealed record BackstageMoraleThresholdSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle,
    [property: JsonPropertyName("min")] int Min,
    [property: JsonPropertyName("max")] int Max);

public sealed record BackstageDisciplineSpec(
    [property: JsonPropertyName("actions")] IReadOnlyList<BackstageDisciplineActionSpec> Actions);

public sealed record BackstageDisciplineActionSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle,
    [property: JsonPropertyName("gravite")] int Gravite,
    [property: JsonPropertyName("moraleDelta")] int MoraleDelta);

public sealed record IncidentLoreSpec(
    [property: JsonPropertyName("meta")] IncidentLoreMeta Meta,
    [property: JsonPropertyName("incidents")] IReadOnlyList<IncidentLoreEntrySpec> Incidents)
{
    public static IncidentLoreSpec ParDefaut => new(
        new IncidentLoreMeta("fr", "incidents-backstage"),
        new[]
        {
            new IncidentLoreEntrySpec("retard", "Retard", "{workers} arrivent en retard.", 0.2, 1, 2, 1, 3, -3, -1)
        });
}

public sealed record IncidentLoreMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele);

public sealed record IncidentLoreEntrySpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("titre")] string Titre,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("chance")] double Chance,
    [property: JsonPropertyName("participantsMin")] int ParticipantsMin,
    [property: JsonPropertyName("participantsMax")] int ParticipantsMax,
    [property: JsonPropertyName("graviteMin")] int GraviteMin,
    [property: JsonPropertyName("graviteMax")] int GraviteMax,
    [property: JsonPropertyName("moraleImpactMin")] int MoraleImpactMin,
    [property: JsonPropertyName("moraleImpactMax")] int MoraleImpactMax);

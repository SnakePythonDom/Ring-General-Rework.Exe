using System.Text.Json.Serialization;

namespace RingGeneral.Specs.Models;

public sealed record SegmentTypesSpec(
    [property: JsonPropertyName("meta")] SegmentTypesMeta Meta,
    [property: JsonPropertyName("types")] IReadOnlyList<SegmentTypeSpec> Types);

public sealed record SegmentTypesMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele);

public sealed record SegmentTypeSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle);

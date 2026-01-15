using System.Text.Json.Serialization;

namespace RingGeneral.Specs.Models;

public sealed record SegmentTypesSpec(
    [property: JsonPropertyName("meta")] SegmentTypesMeta Meta,
    [property: JsonPropertyName("types")] IReadOnlyList<SegmentTypeSpec> Types,
    [property: JsonPropertyName("consignes")] IReadOnlyDictionary<string, IReadOnlyList<string>>? Consignes = null);

public sealed record SegmentTypesMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele);

public sealed record SegmentTypeSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle,
    [property: JsonPropertyName("consignes")] IReadOnlyList<string>? Consignes = null,
    [property: JsonPropertyName("champsRequis")] IReadOnlyList<string>? ChampsRequis = null,
    [property: JsonPropertyName("validations")] IReadOnlyList<string>? Validations = null);

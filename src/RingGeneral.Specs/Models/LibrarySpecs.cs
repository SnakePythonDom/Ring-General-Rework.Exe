using System.Text.Json.Serialization;

namespace RingGeneral.Specs.Models;

public sealed record LibrarySegmentsSpec(
    [property: JsonPropertyName("meta")] LibrarySegmentsMeta Meta,
    [property: JsonPropertyName("templates")] IReadOnlyList<SegmentTemplateSpec> Templates);

public sealed record LibrarySegmentsMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele);

public sealed record SegmentTemplateSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("segments")] IReadOnlyList<SegmentTemplateSegmentSpec> Segments);

public sealed record SegmentTemplateSegmentSpec(
    [property: JsonPropertyName("typeSegment")] string TypeSegment,
    [property: JsonPropertyName("duree")] int Duree,
    [property: JsonPropertyName("mainEvent")] bool MainEvent,
    [property: JsonPropertyName("autoParticipants")] int? AutoParticipants,
    [property: JsonPropertyName("matchTypeId")] string? MatchTypeId);

public sealed record MatchTypesSpec(
    [property: JsonPropertyName("meta")] MatchTypesMeta Meta,
    [property: JsonPropertyName("types")] IReadOnlyList<MatchTypeSpec> Types);

public sealed record MatchTypesMeta(
    [property: JsonPropertyName("langue")] string Langue,
    [property: JsonPropertyName("modele")] string Modele);

public sealed record MatchTypeSpec(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("libelle")] string Libelle,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("participants")] int? Participants,
    [property: JsonPropertyName("dureeParDefaut")] int? DureeParDefaut);

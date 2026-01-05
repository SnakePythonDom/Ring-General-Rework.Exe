namespace RingGeneral.Core.Models;

public enum ShowType
{
    Tv,
    Ppv,
    House,
    Youth
}

public enum ShowStatus
{
    ABooker,
    Booke,
    Simule,
    Annule
}

public sealed record ShowSchedule(
    string ShowId,
    string CompanyId,
    string Nom,
    ShowType Type,
    DateOnly Date,
    int RuntimeMinutes,
    string? VenueId,
    string? Broadcast,
    decimal TicketPrice,
    ShowStatus Statut);

public sealed record ShowScheduleDraft(
    string CompanyId,
    string Nom,
    ShowType Type,
    DateOnly Date,
    int RuntimeMinutes,
    string? VenueId,
    string? Broadcast,
    decimal TicketPrice);

public sealed record ShowScheduleUpdate(
    string? Nom = null,
    DateOnly? Date = null,
    int? RuntimeMinutes = null,
    string? VenueId = null,
    string? Broadcast = null,
    decimal? TicketPrice = null,
    ShowStatus? Statut = null);

public sealed record ShowSettings(
    string ShowSettingsId,
    string CompanyId,
    ShowType Type,
    int RuntimeMinMinutes,
    int RuntimeMaxMinutes,
    decimal? TicketPriceMin,
    decimal? TicketPriceMax,
    bool DiffusionObligatoire);

public sealed record VenueDefinition(
    string VenueId,
    string Nom,
    string Region,
    string? Ville,
    int Capacite);

public sealed record CalendarEntry(
    string CalendarEntryId,
    string CompanyId,
    DateOnly Date,
    string Type,
    string? ReferenceId,
    string? Titre,
    string? Notes = null);

public sealed record ShowSchedulerResult(
    bool Reussite,
    IReadOnlyList<string> Erreurs,
    ShowSchedule? Show = null,
    CalendarEntry? CalendarEntry = null,
    IReadOnlyList<string>? Avertissements = null);

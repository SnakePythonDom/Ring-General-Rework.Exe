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

/// <summary>
/// Pattern de récurrence pour les shows récurrents
/// </summary>
public enum ShowRecurrencePattern
{
    /// <summary>
    /// Chaque semaine
    /// </summary>
    Weekly,
    
    /// <summary>
    /// Toutes les 2 semaines
    /// </summary>
    BiWeekly,
    
    /// <summary>
    /// Chaque mois
    /// </summary>
    Monthly,
    
    /// <summary>
    /// Pattern personnalisé
    /// </summary>
    Custom
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
    ShowStatus Statut,
    string? BrandId = null);

public sealed record ShowScheduleDraft(
    string CompanyId,
    string Nom,
    ShowType Type,
    DateOnly Date,
    int RuntimeMinutes,
    string? VenueId,
    string? Broadcast,
    decimal TicketPrice,
    string? BrandId = null);

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

/// <summary>
/// Template pour créer des shows récurrents
/// </summary>
public sealed record ShowTemplate(
    string TemplateId,
    string CompanyId,
    string Name,
    ShowType ShowType,
    ShowRecurrencePattern RecurrencePattern,
    DayOfWeek? DayOfWeek,
    int DefaultDuration,
    string? DefaultVenueId,
    string? DefaultBroadcast,
    bool IsActive);

/// <summary>
/// Contrôle de booking pour une child company
/// </summary>
public sealed record ChildCompanyBookingControl(
    string ControlId,
    string ChildCompanyId,
    Models.Booker.BookingControlLevel ControlLevel,
    bool OwnerCanOverride,
    bool AutoScheduleShows);

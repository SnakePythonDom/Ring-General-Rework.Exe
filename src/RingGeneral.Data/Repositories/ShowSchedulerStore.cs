using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation de IShowSchedulerStore utilisant ShowRepository et accès direct DB
/// </summary>
public sealed class ShowSchedulerStore : IShowSchedulerStore
{
    private readonly ShowRepository _showRepository;
    private readonly SqliteConnectionFactory _factory;

    public ShowSchedulerStore(ShowRepository showRepository, SqliteConnectionFactory factory)
    {
        _showRepository = showRepository ?? throw new ArgumentNullException(nameof(showRepository));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public ShowSchedule? ChargerShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowId, CompanyId, Name, ShowType, Date, DurationMinutes, VenueId, Broadcast, TicketPrice, Status, BrandId
            FROM Shows
            WHERE ShowId = $showId;
            """;
        command.Parameters.AddWithValue("$showId", showId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;
        
        var showTypeStr = reader.IsDBNull(3) ? "TV" : reader.GetString(3);
        var showType = Enum.TryParse<ShowType>(showTypeStr, true, out var type) ? type : ShowType.Tv;
        
        var statusStr = reader.IsDBNull(9) ? "ABOOKER" : reader.GetString(9);
        var status = Enum.TryParse<ShowStatus>(statusStr, true, out var stat) ? stat : ShowStatus.ABooker;
        
        var dateStr = reader.IsDBNull(4) ? null : reader.GetString(4);
        var showDate = dateStr != null && DateOnly.TryParse(dateStr, out var parsedDate) ? parsedDate : DateOnly.MinValue;
        
        return new ShowSchedule(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            showType,
            showDate,
            reader.GetInt32(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? 0m : (decimal)reader.GetDouble(8),
            status,
            reader.IsDBNull(10) ? null : reader.GetString(10));
    }

    public IReadOnlyList<ShowSchedule> ChargerShows(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowId, CompanyId, Name, ShowType, Date, DurationMinutes, VenueId, Broadcast, TicketPrice, Status, BrandId
            FROM Shows
            WHERE CompanyId = $companyId
            ORDER BY Date ASC;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var shows = new List<ShowSchedule>();
        while (reader.Read())
        {
            var showTypeStr = reader.IsDBNull(3) ? "TV" : reader.GetString(3);
            var showType = Enum.TryParse<ShowType>(showTypeStr, true, out var type) ? type : ShowType.Tv;
            
            var statusStr = reader.IsDBNull(9) ? "ABOOKER" : reader.GetString(9);
            var status = Enum.TryParse<ShowStatus>(statusStr, true, out var stat) ? stat : ShowStatus.ABooker;
            
            var dateStr = reader.IsDBNull(4) ? null : reader.GetString(4);
            var showDate = dateStr != null && DateOnly.TryParse(dateStr, out var parsedDate) ? parsedDate : DateOnly.MinValue;
            
            shows.Add(new ShowSchedule(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                showType,
                showDate,
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? 0m : (decimal)reader.GetDouble(8),
                status,
                reader.IsDBNull(10) ? null : reader.GetString(10)));
        }
        return shows;
    }

    public IReadOnlyList<ShowSchedule> ChargerShowsParDate(string companyId, DateOnly date)
    {
        return _showRepository.ChargerShowsParDate(companyId, date);
    }

    public IReadOnlyList<CalendarEntry> ChargerCalendarEntries(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT CalendarEntryId, CompanyId, Date, EntryType, ReferenceId, Title, Notes
            FROM CalendarEntries
            WHERE CompanyId = $companyId
            ORDER BY Date ASC;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var entries = new List<CalendarEntry>();
        while (reader.Read())
        {
            var dateStr = reader.GetString(2);
            var date = DateOnly.TryParse(dateStr, out var parsedDate) ? parsedDate : DateOnly.MinValue;
            
            entries.Add(new CalendarEntry(
                reader.GetString(0),
                reader.GetString(1),
                date,
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }
        return entries;
    }

    public IReadOnlyList<ShowSettings> ChargerShowSettings(string companyId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ShowSettingsId, CompanyId, ShowType, RuntimeMinMinutes, RuntimeMaxMinutes, 
                   TicketPriceMin, TicketPriceMax, BroadcastRequired
            FROM ShowSettings
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var settings = new List<ShowSettings>();
        while (reader.Read())
        {
            var showTypeStr = reader.GetString(2);
            var showType = Enum.TryParse<ShowType>(showTypeStr, true, out var type) ? type : ShowType.Tv;
            
            settings.Add(new ShowSettings(
                reader.GetString(0),
                reader.GetString(1),
                showType,
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : (decimal?)reader.GetDouble(5),
                reader.IsDBNull(6) ? null : (decimal?)reader.GetDouble(6),
                reader.GetInt32(7) == 1));
        }
        return settings;
    }

    public void AjouterShow(ShowSchedule show)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO Shows (ShowId, CompanyId, Name, ShowType, Date, DurationMinutes, VenueId, Broadcast, TicketPrice, Status, BrandId)
            VALUES ($showId, $companyId, $name, $showType, $date, $duration, $venueId, $broadcast, $ticketPrice, $status, $brandId);
            """;
        command.Parameters.AddWithValue("$showId", show.ShowId);
        command.Parameters.AddWithValue("$companyId", show.CompanyId);
        command.Parameters.AddWithValue("$name", show.Nom);
        command.Parameters.AddWithValue("$showType", show.Type.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$date", show.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$duration", show.RuntimeMinutes);
        command.Parameters.AddWithValue("$venueId", (object?)show.VenueId ?? DBNull.Value);
        command.Parameters.AddWithValue("$broadcast", (object?)show.Broadcast ?? DBNull.Value);
        command.Parameters.AddWithValue("$ticketPrice", show.TicketPrice);
        command.Parameters.AddWithValue("$status", show.Statut.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$brandId", (object?)show.BrandId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void MettreAJourShow(ShowSchedule show)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Shows
            SET Name = $name,
                ShowType = $showType,
                Date = $date,
                DurationMinutes = $duration,
                VenueId = $venueId,
                Broadcast = $broadcast,
                TicketPrice = $ticketPrice,
                Status = $status,
                BrandId = $brandId
            WHERE ShowId = $showId;
            """;
        command.Parameters.AddWithValue("$showId", show.ShowId);
        command.Parameters.AddWithValue("$name", show.Nom);
        command.Parameters.AddWithValue("$showType", show.Type.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$date", show.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$duration", show.RuntimeMinutes);
        command.Parameters.AddWithValue("$venueId", (object?)show.VenueId ?? DBNull.Value);
        command.Parameters.AddWithValue("$broadcast", (object?)show.Broadcast ?? DBNull.Value);
        command.Parameters.AddWithValue("$ticketPrice", show.TicketPrice);
        command.Parameters.AddWithValue("$status", show.Statut.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$brandId", (object?)show.BrandId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void SupprimerShow(string showId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        command.ExecuteNonQuery();
    }

    public void AjouterCalendarEntry(CalendarEntry entry)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO CalendarEntries (CalendarEntryId, CompanyId, Date, EntryType, ReferenceId, Title, Notes)
            VALUES ($entryId, $companyId, $date, $entryType, $referenceId, $title, $notes);
            """;
        command.Parameters.AddWithValue("$entryId", entry.CalendarEntryId);
        command.Parameters.AddWithValue("$companyId", entry.CompanyId);
        command.Parameters.AddWithValue("$date", entry.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$entryType", entry.Type);
        command.Parameters.AddWithValue("$referenceId", (object?)entry.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("$title", (object?)entry.Titre ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)entry.Notes ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void MettreAJourCalendarEntry(CalendarEntry entry)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE CalendarEntries
            SET CompanyId = $companyId,
                Date = $date,
                EntryType = $entryType,
                ReferenceId = $referenceId,
                Title = $title,
                Notes = $notes
            WHERE CalendarEntryId = $entryId;
            """;
        command.Parameters.AddWithValue("$entryId", entry.CalendarEntryId);
        command.Parameters.AddWithValue("$companyId", entry.CompanyId);
        command.Parameters.AddWithValue("$date", entry.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$entryType", entry.Type);
        command.Parameters.AddWithValue("$referenceId", (object?)entry.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("$title", (object?)entry.Titre ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)entry.Notes ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void SupprimerCalendarEntry(string entryId)
    {
        using var connexion = _factory.OuvrirConnexion();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM CalendarEntries WHERE CalendarEntryId = $entryId;";
        command.Parameters.AddWithValue("$entryId", entryId);
        command.ExecuteNonQuery();
    }
}

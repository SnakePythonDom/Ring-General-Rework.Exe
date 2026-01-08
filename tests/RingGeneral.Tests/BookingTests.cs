using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class BookingTests
{
    [Fact]
    public void CrudSegment_Persist_And_Reload()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral_{Guid.NewGuid():N}.db");
        try
        {
            new DbInitializer().CreateDatabaseIfMissing(dbPath);
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var repository = RepositoryFactory.CreateGameRepository(factory);

            var context = repository.ChargerShowContext("SHOW-001");
            Assert.NotNull(context);

            var settings = new Dictionary<string, string> { ["typeMatch"] = "Tag" };
            var segment = new SegmentDefinition(
                $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
                "match",
                new List<string> { "W-001", "W-002" },
                10,
                false,
                "S-001",
                "T-001",
                60,
                "W-001",
                "W-002",
                settings);

            repository.AjouterSegment(context!.Show.ShowId, segment, context.Segments.Count + 1);
            repository.MettreAJourSegment(segment with
            {
                DureeMinutes = 14,
                Settings = new Dictionary<string, string> { ["typeMatch"] = "Hardcore" }
            });

            var reloaded = repository.ChargerShowContext(context.Show.ShowId);
            Assert.NotNull(reloaded);
            var reloadedSegment = reloaded!.Segments.FirstOrDefault(s => s.SegmentId == segment.SegmentId);
            Assert.NotNull(reloadedSegment);
            Assert.Equal(14, reloadedSegment!.DureeMinutes);
            Assert.Equal("Hardcore", reloadedSegment.Settings!["typeMatch"]);

            repository.SupprimerSegment(segment.SegmentId);
            var afterDelete = repository.ChargerShowContext(context.Show.ShowId);
            Assert.DoesNotContain(afterDelete!.Segments, s => s.SegmentId == segment.SegmentId);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    [Fact]
    public void BookingWarnings_Appear_For_Missing_Main_Event_And_Participants()
    {
        var validator = new BookingValidator();
        var plan = new BookingPlan(
            "SHOW-001",
            new List<SegmentSimulationContext>
            {
                new("SEG-1", "promo", Array.Empty<string>(), 5, false)
            },
            120);

        var result = validator.ValiderBooking(plan);

        Assert.Contains(result.Issues, issue => issue.Code == "booking.main-event.missing");
        Assert.Contains(result.Issues, issue => issue.Code == "segment.participants.empty");
    }
}

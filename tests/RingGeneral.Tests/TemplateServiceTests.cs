using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class TemplateServiceTests
{
    [Fact]
    public void AppliquerTemplate_cree_les_segments_attendus()
    {
        var template = new SegmentTemplateDefinition(
            "template-test",
            "Template test",
            "Description",
            new List<SegmentTemplateSegmentDefinition>
            {
                new("match", 8, false, 2, "simple"),
                new("promo", 3, false, 1, null)
            });

        var workers = new List<WorkerSnapshot>
        {
            new("W-001", "Worker 1", 50, 50, 50, 50, 10, "AUCUNE", 40, "NONE"),
            new("W-002", "Worker 2", 50, 50, 50, 50, 10, "AUCUNE", 40, "NONE"),
            new("W-003", "Worker 3", 50, 50, 50, 50, 10, "AUCUNE", 40, "NONE")
        };

        var service = new TemplateService();
        var segments = service.AppliquerTemplate(template, workers);

        Assert.Equal(2, segments.Count);
        Assert.Equal(2, segments[0].Participants.Count);
        Assert.Equal(1, segments[1].Participants.Count);
        Assert.Equal("match", segments[0].TypeSegment);
        Assert.Equal("promo", segments[1].TypeSegment);
    }
}

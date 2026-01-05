using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class TemplateServiceTests
{
    [Fact]
    public void Appliquer_template_cree_un_segment_base()
    {
        var template = new SegmentTemplate(
            "TPL-001",
            "Match rapide",
            "match",
            10,
            false,
            65,
            "singles");
        var service = new TemplateService();

        var segment = service.AppliquerTemplate(template);

        Assert.StartsWith("SEG-", segment.SegmentId);
        Assert.Equal("match", segment.TypeSegment);
        Assert.Equal(10, segment.DureeMinutes);
        Assert.False(segment.EstMainEvent);
        Assert.Equal(65, segment.Intensite);
        Assert.Empty(segment.Participants);
    }
}

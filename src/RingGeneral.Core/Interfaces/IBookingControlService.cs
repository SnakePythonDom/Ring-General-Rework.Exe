using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Phase 1.2 - Service pour gérer les niveaux de contrôle du booking
/// </summary>
public interface IBookingControlService
{
    /// <summary>
    /// Génère un show selon le niveau de contrôle spécifié
    /// </summary>
    List<SegmentDefinition> GenerateShowWithControlLevel(
        BookingControlLevel controlLevel,
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null);
}

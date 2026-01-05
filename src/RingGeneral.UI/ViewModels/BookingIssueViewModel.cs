using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed record BookingIssueViewModel(
    string Code,
    string Message,
    ValidationSeverity Severity,
    string? SegmentId,
    string ActionLabel);

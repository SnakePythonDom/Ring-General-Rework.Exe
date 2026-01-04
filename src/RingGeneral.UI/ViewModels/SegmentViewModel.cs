namespace RingGeneral.UI.ViewModels;

public sealed class SegmentViewModel
{
    public SegmentViewModel(string segmentId, string typeSegment, string typeSegmentLibelle, int dureeMinutes, string participants, bool estMainEvent)
    {
        SegmentId = segmentId;
        TypeSegment = typeSegment;
        TypeSegmentLibelle = typeSegmentLibelle;
        DureeMinutes = dureeMinutes;
        Participants = participants;
        EstMainEvent = estMainEvent;
    }

    public string SegmentId { get; }
    public string TypeSegment { get; }
    public string TypeSegmentLibelle { get; }
    public int DureeMinutes { get; }
    public string Participants { get; }
    public bool EstMainEvent { get; }
}

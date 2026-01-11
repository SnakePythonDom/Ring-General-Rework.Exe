using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.Services.Messaging;

public class RequestWorkerSelectionEvent
{
    public SegmentViewModel Requester { get; }

    public RequestWorkerSelectionEvent(SegmentViewModel requester)
    {
        Requester = requester;
    }
}

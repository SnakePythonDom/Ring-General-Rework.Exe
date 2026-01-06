namespace RingGeneral.UI.Services.Messaging;

/// <summary>
/// Service de messagerie pour communication découplée entre ViewModels
/// Pattern Pub/Sub
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    /// Publie un événement
    /// </summary>
    void Publish<TEvent>(TEvent eventToPublish);

    /// <summary>
    /// S'abonne à un type d'événement
    /// </summary>
    /// <returns>IDisposable pour unsubscribe</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> action);

    /// <summary>
    /// Observable pour s'abonner avec ReactiveUI
    /// </summary>
    IObservable<TEvent> GetEvent<TEvent>();
}

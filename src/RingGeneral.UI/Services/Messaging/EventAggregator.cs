using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace RingGeneral.UI.Services.Messaging;

/// <summary>
/// Implémentation de l'EventAggregator avec ReactiveUI Subjects
/// </summary>
public sealed class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, object> _subjects = new();
    private readonly object _lock = new();

    public void Publish<TEvent>(TEvent eventToPublish)
    {
        if (eventToPublish is null)
        {
            return;
        }

        Subject<TEvent> subject;

        lock (_lock)
        {
            if (!_subjects.TryGetValue(typeof(TEvent), out var subjectObj))
            {
                return; // Aucun subscriber
            }

            subject = (Subject<TEvent>)subjectObj;
        }

        subject.OnNext(eventToPublish);
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> action)
    {
        var subject = GetSubject<TEvent>();
        return subject.Subscribe(action);
    }

    public IObservable<TEvent> GetEvent<TEvent>()
    {
        return GetSubject<TEvent>().AsObservable();
    }

    private Subject<TEvent> GetSubject<TEvent>()
    {
        lock (_lock)
        {
            if (_subjects.TryGetValue(typeof(TEvent), out var subjectObj))
            {
                return (Subject<TEvent>)subjectObj;
            }

            var subject = new Subject<TEvent>();
            _subjects[typeof(TEvent)] = subject;
            return subject;
        }
    }
}

using RingGeneral.Core.Interfaces;

namespace RingGeneral.Core.Services;

/// <summary>
/// Point d'accès global aux services de l'application.
/// Singleton thread-safe pour accès simplifié au ServiceContainer.
/// </summary>
public static class ApplicationServices
{
    private static ServiceContainer? _container;
    private static readonly object _lock = new();

    /// <summary>
    /// Initialise les services de l'application (à appeler au démarrage).
    /// </summary>
    public static void Initialize(ServiceContainer? container = null)
    {
        lock (_lock)
        {
            _container = container ?? ServiceContainer.CreateDefault();
        }
    }

    /// <summary>
    /// Obtient le conteneur de services.
    /// </summary>
    public static ServiceContainer Container
    {
        get
        {
            if (_container == null)
            {
                lock (_lock)
                {
                    _container ??= ServiceContainer.CreateDefault();
                }
            }
            return _container;
        }
    }

    /// <summary>
    /// Résout un service (raccourci vers Container.Resolve).
    /// </summary>
    public static TInterface Resolve<TInterface>() where TInterface : class
    {
        return Container.Resolve<TInterface>();
    }

    /// <summary>
    /// Obtient le logger (raccourci pour usage fréquent).
    /// </summary>
    public static ILoggingService Logger => Resolve<ILoggingService>();

    /// <summary>
    /// Reset le conteneur (pour tests uniquement).
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _container = null;
        }
    }
}

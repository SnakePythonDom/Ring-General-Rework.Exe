namespace RingGeneral.Core.Services;

/// <summary>
/// Conteneur de services simple pour l'injection de dépendances.
/// Permet d'enregistrer et de résoudre des services par interface.
/// </summary>
public sealed class ServiceContainer
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();

    /// <summary>
    /// Enregistre un singleton (une seule instance partagée).
    /// </summary>
    public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
    {
        _singletons[typeof(TInterface)] = instance;
    }

    /// <summary>
    /// Enregistre une factory (nouvelle instance à chaque résolution).
    /// </summary>
    public void RegisterTransient<TInterface>(Func<TInterface> factory) where TInterface : class
    {
        _factories[typeof(TInterface)] = () => factory();
    }

    /// <summary>
    /// Résout un service par son interface.
    /// </summary>
    public TInterface Resolve<TInterface>() where TInterface : class
    {
        var type = typeof(TInterface);

        // Chercher singleton d'abord
        if (_singletons.TryGetValue(type, out var singleton))
        {
            return (TInterface)singleton;
        }

        // Chercher factory
        if (_factories.TryGetValue(type, out var factory))
        {
            return (TInterface)factory();
        }

        throw new InvalidOperationException($"Service non enregistré: {type.Name}");
    }

    /// <summary>
    /// Vérifie si un service est enregistré.
    /// </summary>
    public bool IsRegistered<TInterface>()
    {
        var type = typeof(TInterface);
        return _singletons.ContainsKey(type) || _factories.ContainsKey(type);
    }

    /// <summary>
    /// Crée un conteneur avec les services par défaut.
    /// </summary>
    public static ServiceContainer CreateDefault()
    {
        var container = new ServiceContainer();

        // Logger par défaut (console)
        container.RegisterSingleton<ILoggingService>(new ConsoleLoggingService(LogLevel.Info));

        return container;
    }
}

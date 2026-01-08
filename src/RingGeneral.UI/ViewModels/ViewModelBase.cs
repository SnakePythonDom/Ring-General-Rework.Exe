using ReactiveUI;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels;

/// <summary>
/// Base class pour tous les ViewModels avec support logging intégré.
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// Logger accessible à tous les ViewModels dérivés.
    /// </summary>
    protected ILoggingService Logger { get; }

    protected ViewModelBase()
    {
        Logger = ApplicationServices.Logger;
    }

    protected ViewModelBase(ILoggingService logger)
    {
        Logger = logger;
    }
}

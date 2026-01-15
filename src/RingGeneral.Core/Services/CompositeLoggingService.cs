using RingGeneral.Core.Interfaces;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de logging composite qui délègue vers plusieurs loggers.
/// Permet de logger simultanément vers console, fichier, etc.
/// </summary>
public sealed class CompositeLoggingService : ILoggingService
{
    private readonly IReadOnlyList<ILoggingService> _loggers;

    public CompositeLoggingService(params ILoggingService[] loggers)
    {
        _loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
    }

    public CompositeLoggingService(IEnumerable<ILoggingService> loggers)
    {
        _loggers = loggers?.ToList() ?? throw new ArgumentNullException(nameof(loggers));
    }

    public void Debug(string message)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                logger.Debug(message);
            }
            catch
            {
                // Ignorer les erreurs d'un logger pour ne pas bloquer les autres
            }
        }
    }

    public void Info(string message)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                logger.Info(message);
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
    }

    public void Warning(string message)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                logger.Warning(message);
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
    }

    public void Error(string message, Exception? exception = null)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                logger.Error(message, exception);
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
    }

    public void Fatal(string message, Exception? exception = null)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                logger.Fatal(message, exception);
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
    }
}

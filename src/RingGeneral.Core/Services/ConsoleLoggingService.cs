using System.Diagnostics;

namespace RingGeneral.Core.Services;

/// <summary>
/// Implémentation du service de logging vers la console.
/// Format: [TIMESTAMP] [LEVEL] Message
/// </summary>
public sealed class ConsoleLoggingService : ILoggingService
{
    private readonly LogLevel _minLevel;
    private readonly bool _includeTimestamp;

    public ConsoleLoggingService(LogLevel minLevel = LogLevel.Info, bool includeTimestamp = true)
    {
        _minLevel = minLevel;
        _includeTimestamp = includeTimestamp;
    }

    public void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void Error(string message, Exception? exception = null)
    {
        var fullMessage = exception is null
            ? message
            : $"{message}\nException: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
        Log(LogLevel.Error, fullMessage);
    }

    public void Fatal(string message, Exception? exception = null)
    {
        var fullMessage = exception is null
            ? message
            : $"{message}\nException: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
        Log(LogLevel.Fatal, fullMessage);
    }

    private void Log(LogLevel level, string message)
    {
        if (level < _minLevel)
        {
            return;
        }

        var timestamp = _includeTimestamp ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " : "";
        var levelStr = $"[{level.ToString().ToUpperInvariant()}]";
        var formattedMessage = $"{timestamp}{levelStr} {message}";

        // Console output
        Console.WriteLine(formattedMessage);

        // Debug output (visible dans les outils de debug)
        System.Diagnostics.Debug.WriteLine(formattedMessage);
    }
}

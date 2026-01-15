using RingGeneral.Core.Interfaces;

namespace RingGeneral.Core.Services;

/// <summary>
/// Configuration pour le système de logging.
/// Peut être chargée depuis un fichier JSON ou créée programmatiquement.
/// </summary>
public sealed class LoggingConfiguration
{
    /// <summary>
    /// Niveau minimum de log (défaut: Info).
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Activer le logging vers la console (défaut: true).
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Activer le logging vers fichier (défaut: false).
    /// </summary>
    public bool EnableFileLogging { get; set; } = false;

    /// <summary>
    /// Chemin du fichier de log (si EnableFileLogging = true).
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Taille maximale du fichier de log en MB avant rotation (défaut: 10MB).
    /// </summary>
    public int MaxFileSizeMb { get; set; } = 10;

    /// <summary>
    /// Inclure les timestamps dans les logs (défaut: true).
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Créer une configuration par défaut.
    /// </summary>
    public static LoggingConfiguration Default()
    {
        return new LoggingConfiguration
        {
            MinimumLevel = LogLevel.Info,
            EnableConsoleLogging = true,
            EnableFileLogging = false,
            IncludeTimestamp = true
        };
    }

    /// <summary>
    /// Créer une configuration pour développement (plus de détails).
    /// </summary>
    public static LoggingConfiguration Development()
    {
        return new LoggingConfiguration
        {
            MinimumLevel = LogLevel.Debug,
            EnableConsoleLogging = true,
            EnableFileLogging = true,
            LogFilePath = "logs/ringgeneral_debug.log",
            MaxFileSizeMb = 5,
            IncludeTimestamp = true
        };
    }

    /// <summary>
    /// Créer une configuration pour production (logs critiques uniquement).
    /// </summary>
    public static LoggingConfiguration Production()
    {
        return new LoggingConfiguration
        {
            MinimumLevel = LogLevel.Warning,
            EnableConsoleLogging = false,
            EnableFileLogging = true,
            LogFilePath = "logs/ringgeneral.log",
            MaxFileSizeMb = 50,
            IncludeTimestamp = true
        };
    }

    /// <summary>
    /// Créer un ILoggingService basé sur cette configuration.
    /// </summary>
    public ILoggingService CreateLogger()
    {
        var loggers = new List<ILoggingService>();

        if (EnableConsoleLogging)
        {
            loggers.Add(new ConsoleLoggingService(MinimumLevel, IncludeTimestamp));
        }

        if (EnableFileLogging && !string.IsNullOrWhiteSpace(LogFilePath))
        {
            loggers.Add(new FileLoggingService(LogFilePath, MinimumLevel, IncludeTimestamp, MaxFileSizeMb));
        }

        return loggers.Count switch
        {
            0 => new ConsoleLoggingService(MinimumLevel, IncludeTimestamp), // Fallback
            1 => loggers[0],
            _ => new CompositeLoggingService(loggers)
        };
    }
}

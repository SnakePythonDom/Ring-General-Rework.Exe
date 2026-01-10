namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Service de logging structuré pour l'application Ring General.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Log un message de débogage (développement uniquement).
    /// </summary>
    void Debug(string message);

    /// <summary>
    /// Log un message d'information.
    /// </summary>
    void Info(string message);

    /// <summary>
    /// Log un avertissement.
    /// </summary>
    void Warning(string message);

    /// <summary>
    /// Log une erreur.
    /// </summary>
    void Error(string message, Exception? exception = null);

    /// <summary>
    /// Log une erreur fatale (crash imminent).
    /// </summary>
    void Fatal(string message, Exception? exception = null);
}

/// <summary>
/// Niveau de log.
/// </summary>
public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}
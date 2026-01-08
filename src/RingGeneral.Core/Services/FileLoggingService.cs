using System.Collections.Concurrent;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service de logging vers fichier avec buffer thread-safe.
/// Écrit les logs dans un fichier texte avec rotation automatique.
/// </summary>
public sealed class FileLoggingService : ILoggingService, IDisposable
{
    private readonly string _logFilePath;
    private readonly LogLevel _minLevel;
    private readonly bool _includeTimestamp;
    private readonly long _maxFileSizeBytes;
    private readonly BlockingCollection<string> _logQueue;
    private readonly Task _writerTask;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;

    /// <summary>
    /// Crée un nouveau FileLoggingService.
    /// </summary>
    /// <param name="logFilePath">Chemin du fichier de log</param>
    /// <param name="minLevel">Niveau minimum de log</param>
    /// <param name="includeTimestamp">Inclure le timestamp dans les logs</param>
    /// <param name="maxFileSizeMb">Taille maximale du fichier en MB avant rotation (défaut: 10MB)</param>
    public FileLoggingService(
        string logFilePath,
        LogLevel minLevel = LogLevel.Info,
        bool includeTimestamp = true,
        int maxFileSizeMb = 10)
    {
        _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
        _minLevel = minLevel;
        _includeTimestamp = includeTimestamp;
        _maxFileSizeBytes = maxFileSizeMb * 1024 * 1024;
        _logQueue = new BlockingCollection<string>(1000); // Buffer de 1000 messages
        _cancellationTokenSource = new CancellationTokenSource();

        // Créer le répertoire si nécessaire
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Démarrer le thread d'écriture asynchrone
        _writerTask = Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
    }

    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);

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
        if (level < _minLevel || _disposed)
        {
            return;
        }

        var timestamp = _includeTimestamp ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " : "";
        var levelStr = $"[{level.ToString().ToUpperInvariant()}]";
        var formattedMessage = $"{timestamp}{levelStr} {message}";

        // Ajouter au buffer pour écriture asynchrone
        if (!_logQueue.IsAddingCompleted)
        {
            try
            {
                _logQueue.Add(formattedMessage);
            }
            catch (InvalidOperationException)
            {
                // Queue fermée, ignorer
            }
        }
    }

    /// <summary>
    /// Thread de traitement des logs en arrière-plan.
    /// </summary>
    private void ProcessLogQueue(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var logMessage in _logQueue.GetConsumingEnumerable(cancellationToken))
            {
                try
                {
                    // Vérifier la rotation du fichier
                    CheckAndRotateLog();

                    // Écrire dans le fichier
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // En cas d'erreur, écrire dans la console en fallback
                    Console.Error.WriteLine($"[FileLoggingService] Erreur d'écriture: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal lors du Dispose
        }
    }

    /// <summary>
    /// Vérifie si le fichier doit être rotaté et effectue la rotation si nécessaire.
    /// </summary>
    private void CheckAndRotateLog()
    {
        if (!File.Exists(_logFilePath))
        {
            return;
        }

        var fileInfo = new FileInfo(_logFilePath);
        if (fileInfo.Length >= _maxFileSizeBytes)
        {
            // Rotation: renommer le fichier actuel avec timestamp
            var directory = Path.GetDirectoryName(_logFilePath) ?? ".";
            var fileName = Path.GetFileNameWithoutExtension(_logFilePath);
            var extension = Path.GetExtension(_logFilePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var rotatedFileName = $"{fileName}_{timestamp}{extension}";
            var rotatedFilePath = Path.Combine(directory, rotatedFileName);

            try
            {
                File.Move(_logFilePath, rotatedFilePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FileLoggingService] Erreur de rotation: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Flush tous les logs en attente (utile avant shutdown).
    /// </summary>
    public void Flush()
    {
        _logQueue.CompleteAdding();
        _writerTask.Wait(TimeSpan.FromSeconds(5)); // Attendre max 5 secondes
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Arrêter le thread d'écriture proprement
        _logQueue.CompleteAdding();
        _cancellationTokenSource.Cancel();

        try
        {
            _writerTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Ignore
        }

        _logQueue.Dispose();
        _cancellationTokenSource.Dispose();
    }
}

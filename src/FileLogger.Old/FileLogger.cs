using Microsoft.Extensions.Logging;

namespace FileLogger.Old;

public sealed class FileLogger(
    string category,
    Func<FileLoggerConfiguration> getCurrentConfig,
    ILogWriter logWriter) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= getCurrentConfig().MinLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        
        var timeStamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        var threadId = $"{Environment.CurrentManagedThreadId:##}";
        var level = $"{logLevel,-5}";
        var message = formatter(state, exception);

        if (exception != null)
        {
            message = message + Environment.NewLine + exception;
        }
        
        var logEntry = $"{timeStamp} [{threadId}] [{level}] [{category}] {message}";
        logWriter.EnqueueWrite(logEntry);
    }
}
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileLogger.Old;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly LogWriter _logWriter;
    private readonly IDisposable? _onChangeToken;
    private FileLoggerConfiguration _currentConfig;

    public FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config, IHostEnvironment hostEnvironment)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        _logWriter = new LogWriter(GetCurrentConfig, hostEnvironment);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, GetCurrentConfig, _logWriter));
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
        _logWriter.Dispose();
    }

    private FileLoggerConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }
}
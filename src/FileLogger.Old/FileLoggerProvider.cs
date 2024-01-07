using Microsoft.Extensions.Hosting;

namespace FileLogger.Old;

using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private FileLoggerConfiguration _currentConfig;

    private readonly ConcurrentDictionary<string, FileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly LogWriter _logWriter;

    public FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config, IHostEnvironment hostEnvironment)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        _logWriter = new LogWriter(GetCurrentConfig, hostEnvironment);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, GetCurrentConfig, _logWriter));

    private FileLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
        _logWriter.Dispose();
    }
}
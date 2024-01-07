using Microsoft.Extensions.Logging;

namespace FileLogger;

public class FileLoggerConfiguration
{
    public string? Filename { get; set; }
    public int MaxSize { get; set; }
    public int MaxFiles { get; set; }
    public LogLevel MinLevel { get; set; } = LogLevel.Trace;
}
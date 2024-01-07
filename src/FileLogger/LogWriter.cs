using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace FileLogger;

public interface ILogWriter
{
    void EnqueueWrite(string logEntry);
}

public record LogFileInfo(string Path, StreamWriter FileStream);

public class LogWriter : ILogWriter, IDisposable
{
    private readonly Func<FileLoggerConfiguration> _getCurrentConfig;
    private readonly IHostEnvironment _environment;

    private readonly Channel<string> _queue = Channel.CreateBounded<string>(
        new BoundedChannelOptions(6_000)
        {
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropNewest
        });

    private readonly Task _writeLoop;
    private readonly CancellationTokenSource _cts = new();

    public LogWriter(Func<FileLoggerConfiguration> getCurrentConfig, IHostEnvironment environment)
    {
        _getCurrentConfig = getCurrentConfig;
        _environment = environment;
        _writeLoop = Task.Run(() => WriteLoop(_cts.Token), _cts.Token);
    }

    private async Task WriteLoop(CancellationToken token)
    {
        LogFileInfo fileInfo = OpenLogFile(GetCurrentLogFilePath());
        try
        {
            await foreach (var logEntry in _queue.Reader.ReadAllAsync(token))
            {
                fileInfo = RollFile(fileInfo);

                await fileInfo.FileStream.WriteLineAsync(logEntry);
                await fileInfo.FileStream.FlushAsync(token);
            }
        }
        catch (OperationCanceledException e) when (token.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            
        }
    }

    private LogFileInfo RollFile(LogFileInfo fileInfo)
    {
        var config = _getCurrentConfig();
        var logFilePath = GetCurrentLogFilePath();

        if (fileInfo.Path.Equals(logFilePath, StringComparison.OrdinalIgnoreCase) == false)
        {
            // close old and open new file
        }

        if (fileInfo.FileStream.BaseStream.Length >= config.MaxSize)
        {
            fileInfo.FileStream.Dispose();
            File.Move(fileInfo.Path, GetNextArchiveName(fileInfo.Path));
            fileInfo = OpenLogFile(fileInfo.Path);
            // close old and open new file
        }

        var extension = Path.GetExtension(fileInfo.Path);
        var file = Path.GetFileNameWithoutExtension(fileInfo.Path);
        var dir = Path.GetDirectoryName(fileInfo.Path);
        var filesToDelete = Directory.EnumerateFiles(dir, $"{file}*{extension}")
            .Where(f => f.Equals(logFilePath, StringComparison.OrdinalIgnoreCase) == false)
            .OrderDescending()
            .Skip(config.MaxFiles)
            .ToList();

        foreach (var fileToDelete in filesToDelete)
        {
            File.Delete(fileToDelete);
        }

        // delete old files
        return fileInfo;
    }

    private string GetNextArchiveName(string path)
    {
        var extension = Path.GetExtension(path);
        var file = Path.GetFileNameWithoutExtension(path);
        var dir = Path.GetDirectoryName(path);
        var highestFileCount =
            Directory.EnumerateFiles(dir, $"{file}*{extension}")
                .Select(Path.GetFileNameWithoutExtension)
                .Select(f => f[file.Length..])
                .Select(f => int.TryParse(f, out var count) ? count : 0)
                .OrderDescending()
                .FirstOrDefault();

        return Path.Combine(dir, $"{file}{highestFileCount + 1}{extension}");
    }

    private static LogFileInfo OpenLogFile(string path)
    {
        var streamWriter = File.CreateText(path);
        return new LogFileInfo(path, streamWriter);
    }

    private string GetCurrentLogFilePath()
    {
        var logFilePath = Path.Combine(_environment.ContentRootPath, _getCurrentConfig().Filename);
        return logFilePath;
    }

    public void EnqueueWrite(string logEntry)
    {
        _queue.Writer.TryWrite(logEntry);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _writeLoop.GetAwaiter().GetResult();
        _cts.Dispose();
    }
}
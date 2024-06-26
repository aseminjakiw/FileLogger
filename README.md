# FileLogger

Microsoft.Extensions.Logging file logger for .NET / ASP.NET Core / .NET Worker.

## Why this one?

**log file appender friendly**. That means it does always write in the same (first) log file and on file roll over it
moves the content into a new file. So you can use tools like 
[Tailblazer](https://github.com/RolandPheasant/TailBlazer) or 
[Advanced Log Viewer](https://github.com/Scarfsail/AdvancedLogViewer)

## Getting started

Install nuget package https://www.nuget.org/packages/asemin.FileLogger/

```cmd
dotnet add package asemin.FileLogger
```

Either use application builder

```csharp
using asemin.FileLogger;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFile();
```

or use default builder
```csharp
using asemin.FileLogger;

var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

builder.ConfigureLogging(x => x.AddFile());
```

Tip: If you do not add any configuration, a default one will be used:

- Log file: `/logs/<appname>.app.log` and `appname` is the `IHostEnvironment.ApplicationName`
- max file size: 10 MiB
- max files: 10
- Default log level: Trace (Remember, the global log filters apply before this one)

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    },
    "File": {
      "Files": {
        "default": {
          "File": "logs/logs.log",
          "MaxSize": 104857600,
          "MaxFiles": 100,
          "LogLevel": {
            "Default": "None",
            "MyClass.Logger": "Debug"
          }
        },
        "other": {
          "File": "%ProgramData%/My app/otherLogFile.log",
          "MaxSize": 10485760,
          "MaxFiles": 10,
          "LogLevel": {
            "MyOtherClass": "Trace",
            "Microsoft": "Debug"
          }
        }
      }
    }
  }
}
```

In `Logging.File.Files` you can add as many files loggers as you want. The names (`default`, `other`) are only used
by .NET configuration. So you can make proper overrides with debug or production settings.

| Property   | Default             | Description                                                                                                                                                         |
|------------|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `File`     | `logs/logs.log`     | Name of the log file. Supports relative paths, absolut paths and environment parameters expansion (e.g. `%appdata%`).                                               |
| `MaxSize`  | `10485760` (10 MiB) | Max size of log file in bytes. If file exceeds this size a roll over will happen.                                                                                   |
| `MaxFiles` | `10`                | Max number of log files. Older files get deleted first.                                                                                                             |
| `LogLevel` | `Trace`             | Works the same way as Microsoft global LogLevel. Warning: this one applies *AFTER* global one. So you cannot set the level for a message lower than the global one. |

### C# code

```csharp
builder.Logging.AddFile(x =>
{
    x.Files = new Dictionary<string, LoggerConfig>
    {
        {
            "default",
            new LoggerConfig
            {
                File = @"C:\temp\my app\logs\logs.log",
                MaxSize = 100 * 1024 * 1024,
                MaxFiles = 25
            }
        }
    };
});
```

### F# code

```fsharp
builder.Logging.AddFile(fun config ->
    config.Files <-
        dict[("default", LoggerConfig(
            File = "mylog.log",
            MaxSize = 10 * 1024 * 1024,
            MaxFiles = 17))])
```

## TODOs

- ~~try out in serious applications~~ (confirmed)
- ~~add support for different filters for each log file~~
- ~~Test with Linux~~ (confirmed)
- customize format of logged messages (date, thread, level, etc.) (does someone actually uses that?)

# FileLogger

Microsoft.Extensions.Logging file logger for .NET / ASP.NET Core / .NET Worker.

## Why this one?

It is log file appender friendly. That means it does always write in the same (first) log file and on file roll over it
moves the content into a new file.

## Getting started

TODO: Add nuget package

Either use application builder

```csharp
using asemin.FileLogger;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFile();
```

or use default builder

TODO: Add default builder extension method

Tip: If you do not add any configuration, a default one will be used:
- Log file: `/logs/<appname>.app.log` and `appname` is the `IHostEnvironment.ApplicationName` 
- max file size: 10 MiB
- max files: 10

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
          "MaxFiles": 100
        },
        "other": {
          "File": "%ProgramData%/My app/otherLogFile.log",
          "MaxSize": 10485760,
          "MaxFiles": 10
        }
      }
    }
  }
}
```

In `Logging.File.Files` you can add as many files loggers as you want. The names (`default`, `other`) are only used
by .NET configuration. So you can make proper overrides with debug or production settings.

| Property   | Default             | Description                                                                                                           |
|------------|---------------------|-----------------------------------------------------------------------------------------------------------------------|
| `File`     | `logs/logs.log`     | Name of the log file. Supports relative paths, absolut paths and environment parameters expansion (e.g. `%appdata%`). |
| `MaxSize`  | `10485760` (10 MiB) | Max size of log file in bytes. If file exceeds this size a roll over will happen.                                     |
| `MaxFiles` | `10`                | Max number of log files. Older files get deleted first.                                                               |

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

- add automatic build pipeline
- add nuget package
- add Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder extension method
- try out in serious applications
- add support for different filters for each log file
- Test with Linux

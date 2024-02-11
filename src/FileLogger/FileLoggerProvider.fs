namespace FileLogger

open System
open System.Collections.Concurrent
open System.Runtime.Versioning
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options


[<UnsupportedOSPlatform("browser")>]
[<ProviderAlias("File")>]
type FileLoggerProvider
    (options: IOptionsMonitor<FileLoggerConfigurationDto>, timeProvider: TimeProvider, environment: IHostEnvironment) =
    let controller = LogController()
    let mapConfig = LoggerConfiguration.mapDto environment.ContentRootPath
    do options.CurrentValue |> mapConfig |> controller.UpdateConfig

    let changeSubscription =
        options.OnChange(fun configDtos -> configDtos |> mapConfig |> controller.UpdateConfig)

    let loggers = ConcurrentDictionary<string, FileLogger>()

    let mutable isDiposed = false

    interface ILoggerProvider with
        member this.CreateLogger(categoryName) =
            ObjectDisposedException.ThrowIf(isDiposed, this)
            loggers.GetOrAdd(categoryName, (fun name -> FileLogger(categoryName, timeProvider, controller.WriteLog)))

        member this.Dispose() =
            isDiposed <- true
            loggers.Clear()
            changeSubscription.Dispose()
            controller.Dispose()

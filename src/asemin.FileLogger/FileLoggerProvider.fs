namespace asemin.FileLogger

open System.Collections.Concurrent
open System.Runtime.Versioning
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options




[<UnsupportedOSPlatform("browser")>]
[<ProviderAlias("File")>]
type FileLoggerProvider
    (options: IOptionsMonitor<FileLoggerConfig>, timeProvider: ITimeProvider, environment: IHostEnvironment) =
    let controller = new LogController()

    let mapConfig =
        LoggerConfiguration.mapDto environment.ContentRootPath environment.ApplicationName

    do options.CurrentValue |> mapConfig |> controller.UpdateConfig

    let changeSubscription =
        options.OnChange(fun configDtos -> configDtos |> mapConfig |> controller.UpdateConfig)

    let loggers = ConcurrentDictionary<string, FileLogger>()

    let mutable isDisposed = false

    interface ILoggerProvider with
        member this.CreateLogger(categoryName) =
            RaiseIf.objectDisposed isDisposed this
            loggers.GetOrAdd(categoryName, (fun name -> FileLogger(categoryName, timeProvider, controller.WriteLog)))

        member this.Dispose() =
            isDisposed <- true
            loggers.Clear()
            changeSubscription.Dispose()
            controller.Dispose()

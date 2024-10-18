namespace asemin.FileLogger

open System.Collections.Concurrent
open System.Runtime.Versioning
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open asemin.FileLogger.ScopeProvider

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

    let mutable currentScopeProvider: IExternalScopeProvider =
        NullExternalScopeProvider.Instance

    interface ILoggerProvider with
        member this.CreateLogger(categoryName) =
            RaiseIf.objectDisposed isDisposed this

            loggers.GetOrAdd(
                categoryName,
                (fun name ->
                    FileLogger(categoryName, timeProvider, controller.WriteLog, (fun () -> currentScopeProvider)))
            )

        member this.Dispose() =
            isDisposed <- true
            loggers.Clear()
            changeSubscription.Dispose()
            controller.Dispose()

    interface ISupportExternalScope with
        member this.SetScopeProvider scopeProvider = currentScopeProvider <- scopeProvider

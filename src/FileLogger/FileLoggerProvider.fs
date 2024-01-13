namespace FileLogger

open System
open System.Collections.Concurrent
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options


type LogCollector(category: string, timeProvider: TimeProvider, writeLog) =
    interface ILogger with
        member this.BeginScope(state) = Unsafe.NullRef<IDisposable>()
        member this.IsEnabled(logLevel) = true

        member this.Log(logLevel, eventId, state, except, formatter) =
            let logEntry =
                { Time = timeProvider.GetLocalNow()
                  ThreadId = Environment.CurrentManagedThreadId
                  Level = logLevel
                  Category = category
                  Message = formatter.Invoke(state, except)
                  Exception = Option.ofObj except }

            writeLog logEntry


type FileLoggerProvider(options: IOptionsMonitor<FileLoggerConfigurationDto>, timeProvider: TimeProvider) =
    let mutable currentConfig = List.empty
    let loggers = ConcurrentDictionary<string, LogCollector>()

    let changeSubscription =
        options.OnChange(fun x -> currentConfig <- LoggerConfiguration.mapDto <| x)

    let getCurrentConfig () = currentConfig

    let writeLog str = ()

    interface ILoggerProvider with
        member this.CreateLogger(categoryName) =
            loggers.GetOrAdd(categoryName, (fun name -> LogCollector(categoryName, timeProvider, writeLog)))

        member this.Dispose() =
            loggers.Clear()
            changeSubscription.Dispose()

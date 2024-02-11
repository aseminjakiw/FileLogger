namespace FileLogger

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

type FileLogger(category: string, timeProvider: TimeProvider, writeLog) =
    interface ILogger with
        member this.BeginScope(state) = Unsafe.NullRef<IDisposable>()
        member this.IsEnabled(logLevel) = true //TODO: respect logLevel

        member this.Log(logLevel, eventId, state, except, formatter) =
            let logEntry =
                { Time = timeProvider.GetLocalNow()
                  ThreadId = Environment.CurrentManagedThreadId
                  Level = logLevel
                  Category = category
                  Message = formatter.Invoke(state, except)
                  Exception = Option.ofObj except }

            writeLog logEntry

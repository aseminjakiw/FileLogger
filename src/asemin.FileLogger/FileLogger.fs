namespace asemin.FileLogger

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

type ITimeProvider =
    abstract member GetLocalNow: unit -> DateTimeOffset
    
type SystemTimeProvider() =
    interface ITimeProvider with
        member this.GetLocalNow() = DateTimeOffset.Now

type FileLogger(category: string, timeProvider: ITimeProvider, writeLog) =
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

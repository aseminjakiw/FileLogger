namespace asemin.FileLogger

open System
open System.Runtime.CompilerServices
open System.Text
open Microsoft.Extensions.Logging
open asemin.FileLogger.ScopeProvider

type ITimeProvider =
    abstract member GetLocalNow: unit -> DateTimeOffset

type SystemTimeProvider() =
    interface ITimeProvider with
        member this.GetLocalNow() = DateTimeOffset.Now

type FileLogger
    (category: string, timeProvider: ITimeProvider, writeLog, getScopeProvider: unit -> IExternalScopeProvider) =
    interface ILogger with
        member this.BeginScope(state) = (getScopeProvider ()).Push state

        member this.IsEnabled(logLevel) = true //TODO: respect logLevel

        member this.Log(logLevel, eventId, state, except, formatter) =
            
            let x = fun (scope: obj) (state: StringBuilder) ->
                state.Append " => " |> ignore
                scope.ToString() |> state.Append |> ignore
                ()
            let stringBuilder = StringBuilder()
            (getScopeProvider ())
                .ForEachScope(x, stringBuilder)
            
                
            let logEntry =
                { Time = timeProvider.GetLocalNow()
                  ThreadId = Environment.CurrentManagedThreadId
                  Level = logLevel
                  Category = category
                  Message = formatter.Invoke(state, except)
                  Exception = Option.ofObj except
                  Scope = stringBuilder.ToString() }

            writeLog logEntry

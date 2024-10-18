namespace asemin.FileLogger.ScopeProvider

open System
open Microsoft.Extensions.Logging

type NullScope() =
    static member Instance = new NullScope()

    interface IDisposable with
        member this.Dispose() = ()

type NullExternalScopeProvider() =    
    static member Instance = NullExternalScopeProvider()
    interface IExternalScopeProvider with
        member this.ForEachScope(callback, state) = ()
        member this.Push(state) = NullScope.Instance

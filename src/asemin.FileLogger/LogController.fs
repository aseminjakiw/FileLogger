namespace asemin.FileLogger

open System

type WorkerState =
    { Worker: LogWorker
      Config: LoggerConfiguration }

type LogControllerState = { Workers: WorkerState list }

type LogControllerMessage =
    | WriteLog of LogEntry
    | UpdateConfig of LoggerConfiguration list
    | Stop of AsyncReplyChannel<unit>




type LogController() =
    let getLogLevel config (logEntry: LogEntry) =
        config.LogLevel
        |> Array.tryFind (fun x -> logEntry.Category.StartsWith x.Category)
        |> Option.map (_.Level)
        |> Option.defaultValue config.DefaultLogLevel

    let handle state message =
        match message with
        | UpdateConfig configs ->
            for worker in state.Workers do
                worker.Worker.Close()

            let workers =
                configs
                |> List.map (fun config ->
                    let worker = LogWorker()
                    worker.UpdateConfig config
                    { Worker = worker; Config = config })

            Some { Workers = workers }
        | WriteLog logEntry ->
            let log = LogEntry.toString logEntry

            for worker in state.Workers do
                let logLevel = getLogLevel worker.Config logEntry

                if logLevel <= logEntry.Level then
                    worker.Worker.WriteLog (log worker.Config.IncludeScope)

            Some state
        | Stop reply ->
            for worker in state.Workers do
                worker.Worker.Close()

            reply.Reply()
            None


    let agent = Agent.newStoppableAgent handle { Workers = [] }
    let mutable isDisposed = false

    member this.WriteLog log =
        if not isDisposed then
            log |> WriteLog |> agent.Post

    member this.UpdateConfig config =
        RaiseIf.objectDisposed isDisposed this
        config |> UpdateConfig |> agent.Post

    member this.Dispose() =
        isDisposed <- true
        agent.PostAndReply Stop
        agent.Dispose()

    interface IDisposable with
        member this.Dispose() = this.Dispose()

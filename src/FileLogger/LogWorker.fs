namespace FileLogger

open System
open System.IO

type Writer =
    { Stream: Stream; Writer: StreamWriter }

type LogWorkerState =
    { Writer: Writer option
      Config: LoggerConfiguration option }

type LogWorkerMessage =
    | WriteLog of string
    | UpdateConfig of LoggerConfiguration
    | Close of AsyncReplyChannel<unit>

type LogWorker() =
    let dispose writer =
        match writer with
        | Some(x: Writer) ->
            x.Writer.Flush()
            x.Writer.Dispose()
            x.Stream.Dispose()
        | None -> ()

    let rollFile state = state.Stream

    let handle state message =
        match message with
        | UpdateConfig config ->
            dispose state.Writer

            config.FileName |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore

            let stream =
                File.Open(config.FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite ||| FileShare.Delete)

            let writer = new StreamWriter(stream)
            writer.AutoFlush <- not (config.Buffered)

            Some
                { Writer = Some { Stream = stream; Writer = writer }
                  Config = Some config }
        | WriteLog log ->
            match state.Writer with
            | Some writer -> writer.Writer.WriteLine(log)
            | None -> ()

            Some state
        | Close reply ->
            dispose state.Writer
            reply.Reply()
            None

    let agent = Agent.newStoppableAgent handle { Writer = None; Config = None }
    let mutable isDisposed = false

    member this.Post message =
        ObjectDisposedException.ThrowIf(isDisposed, this)
        agent.Post message

    member this.WriteLog log =
        ObjectDisposedException.ThrowIf(isDisposed, this)
        log |> WriteLog |> this.Post

    member this.UpdateConfig config =
        ObjectDisposedException.ThrowIf(isDisposed, this)
        config |> UpdateConfig |> this.Post

    member this.Close() =
        isDisposed <- true
        agent.PostAndReply Close
        agent.Dispose()

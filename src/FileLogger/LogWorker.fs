namespace FileLogger

open System
open System.IO
open System.Text.RegularExpressions

type Writer =
    { Writer: StreamWriter
      Config: LoggerConfiguration }

type LogWorkerState =
    { Writer: Writer option
      Config: LoggerConfiguration option }

type LogWorkerMessage =
    | WriteLog of string
    | UpdateConfig of LoggerConfiguration
    | Close of AsyncReplyChannel<unit>

type LogWorker() =
    let dispose (writer: StreamWriter) =
        writer.Flush()
        writer.Dispose()

    let tryDispose writer =
        match writer with
        | Some(writer: Writer) -> dispose writer.Writer
        | None -> ()

    let getArchiveFiles config =
        let logName = Path.GetFileNameWithoutExtension config.FileName
        let logExtension = Path.GetExtension config.FileName
        let archiveSearchPattern = logName + ".*" + logExtension
        let logDir = Path.GetDirectoryName config.FileName
        Directory.EnumerateFiles(logDir, archiveSearchPattern)

    let getNextArchiveName config =
        let logName = Path.GetFileNameWithoutExtension config.FileName
        let logExtension = Path.GetExtension config.FileName
        let logDir = Path.GetDirectoryName config.FileName

        let tryGetArchiveNumber (file: string) =
            let fileName = Path.GetFileNameWithoutExtension file
            let res = Regex.Match(fileName, @".*\.([\d]+)$")
            if res.Success then Some res.Groups[1].Value else None

        let tryParse (input: string) =
            match Int32.TryParse input with
            | true, v -> Some v
            | false, _ -> None

        let maxArchiveNumber =
            getArchiveFiles config
            |> Seq.map tryGetArchiveNumber
            |> Seq.map (Option.bind tryParse)
            |> Seq.choose id
            |> Seq.append [ 0 ]
            |> Seq.max

        Path.Combine(logDir, $"{logName}.{maxArchiveNumber + 1}{logExtension}")

    let openLogFile config =
        config.FileName |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore

        let stream =
            File.Open(config.FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite ||| FileShare.Delete)

        let writer = new StreamWriter(stream, leaveOpen = false, AutoFlush = false)
        writer

    let deleteOldFiles config =
        getArchiveFiles config
        |> Seq.map (fun x -> (x, File.GetLastWriteTimeUtc x))
        |> Seq.sortByDescending snd
        |> Seq.map fst
        |> Seq.mapi (fun i file -> (i, file))
        |> Seq.skipWhile (fun (i, file) -> i < config.MaxFiles - 1)
        |> Seq.map snd
        |> Seq.iter File.Delete

    let archiveCurrentLog config =
        let archiveName = getNextArchiveName config
        File.Copy(config.FileName, archiveName)
        use x = File.Open(config.FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite)
        do x.Dispose()
        deleteOldFiles config




    let handle state message =
        match message with
        | UpdateConfig config ->
            Some
                { state with
                    LogWorkerState.Config = Some config }
        | WriteLog log ->
            try
                match state.Writer, state.Config with
                | _, None -> state
                | None, Some config ->
                    let streamWriter = openLogFile config
                    do streamWriter.WriteLine(log)

                    { state with
                        LogWorkerState.Writer =
                            Some
                                { Writer = streamWriter
                                  Config = config } }
                | Some writer, Some config ->
                    if writer.Config = config then
                        if writer.Writer.BaseStream.Position >= config.MaxSize then
                            do dispose writer.Writer
                            do archiveCurrentLog config
                            let streamWriter = openLogFile config
                            do streamWriter.WriteLine(log)

                            { state with
                                LogWorkerState.Writer =
                                    Some
                                        { Writer = streamWriter
                                          Config = config } }
                        else
                            do writer.Writer.WriteLine(log)
                            state
                    else
                        do dispose writer.Writer
                        let streamWriter = openLogFile config
                        do streamWriter.WriteLine(log)

                        { LogWorkerState.Writer =
                            Some
                                { Writer = streamWriter
                                  Config = config }
                          Config = Some config }
                |> Some
            with e ->
                Some state // ignore exception since we have no way of handling it
        | Close reply ->
            tryDispose state.Writer
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

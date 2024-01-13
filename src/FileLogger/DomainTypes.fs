namespace FileLogger

open System
open Microsoft.Extensions.Logging

open FileLogger

type LogEntry =
    { Time: DateTimeOffset
      ThreadId: int
      Level: LogLevel
      Category: string
      Message: string
      Exception: exn option }

module LogEntry =
    let toString log =
        let logBuilder =
            StringBuilder.createWithMinLength 50
            |> StringBuilder.appendFormat "yyyy-MM-dd HH:mm:ss.fff" log.Time
            |> StringBuilder.append " ["
            |> StringBuilder.appendFormat "##" log.ThreadId
            |> StringBuilder.append "] ["
            |> StringBuilder.appendFormat "%s-5" log.Level
            |> StringBuilder.append "] ["
            |> StringBuilder.append log.Category
            |> StringBuilder.append "] "
            |> StringBuilder.append log.Message

        match log.Exception with
        | None -> ()
        | Some exn ->
            logBuilder
            |> StringBuilder.append Environment.NewLine
            |> StringBuilder.append (exn.ToString())
            |> ignore

        logBuilder |> StringBuilder.getString
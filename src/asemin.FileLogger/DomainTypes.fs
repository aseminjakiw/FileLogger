namespace asemin.FileLogger

open System
open Microsoft.Extensions.Logging

type LogEntry =
    { Time: DateTimeOffset
      ThreadId: int
      Level: LogLevel
      Category: string
      Message: string
      Exception: exn option
      Scope: string }

module LogEntry =
    let levelToString logLevel =
        match logLevel with
        | LogLevel.Trace -> "TRACE"
        | LogLevel.Debug -> "DEBUG"
        | LogLevel.Information -> "INFO "
        | LogLevel.Warning -> "WARN "
        | LogLevel.Error -> "ERROR"
        | LogLevel.Critical -> "FATAL"
        | _ -> "N/A  "

    let toString log =
        let exceptionStr =
            match log.Exception with
            | None -> String.Empty
            | Some exn -> Environment.NewLine + exn.ToString()

        $"{log.Time:``yyyy-MM-dd HH:mm:ss.fff``} [{log.ThreadId:``00``}] [{log.Level |> levelToString}] [{log.Category}] {log.Message}{exceptionStr}"

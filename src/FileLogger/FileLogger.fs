module FileLogger.FileLogger

open System.IO


let writeToStream (stream : StreamWriter) log =
    let logLine = log |> LogEntry.toString
    stream.WriteLine logLine
    stream.Flush ()
    


let isOn (config: LoggerConfiguration) (log: LogEntry) = true

let logEntry (config: LoggerConfiguration) (log: LogEntry) = ()

let write (getCurrentConfig: unit -> LoggerConfiguration list) (log: LogEntry) =
    let configs = getCurrentConfig ()
    
    //TODO: write to file
    //TODO: respect config changes
    //TODO: to file rollover
    //TODO: write multiple files

    for config in configs do
        if isOn config log then
            logEntry config log

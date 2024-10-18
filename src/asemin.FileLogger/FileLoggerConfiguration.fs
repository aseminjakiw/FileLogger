namespace asemin.FileLogger

open System
open System.Collections.Generic
open System.IO
open Microsoft.Extensions.Logging

type FileFormatterOptions() =
    member val IncludeScopes = false with get, set

type LoggerConfig() =
    member val File = String.Empty with get, set
    member val MaxSize = Nullable() with get, set
    member val MaxFiles = Nullable() with get, set
    member val LogLevel: IDictionary<string, LogLevel> = Dictionary() with get, set


type FileLoggerConfig() =
    member val Files: IDictionary<string, LoggerConfig> = Dictionary() with get, set
    member val FormatterOptions = new FileFormatterOptions() with get, set


type LogFilter = { Category: string; Level: LogLevel }

type LoggerConfiguration =
    { FileName: string
      MaxSize: int
      MaxFiles: int
      LogLevel: LogFilter[]
      DefaultLogLevel: LogLevel
      IncludeScope: bool }

module LoggerConfiguration =
    let defaultLogSize = 10 * 1024 * 1024
    let defaultLogFiles = 10
    let defaultLogLevels = [||]
    let defaultLogLevel = LogLevel.Trace
    let defaultIncludeScope = false

    let defaultLogFileName appName =
        let appName =
            if String.IsNullOrWhiteSpace appName then
                "application"
            else
                appName

        Path.Combine("logs", appName + ".app.log")

    let getValues (dict: IDictionary<_, _>) = dict |> Seq.map (_.Value)

    let resolvePath baseDir (path: string) =
        let path = path |> Option.ofObj |> Option.defaultValue String.Empty
        let path = path.Replace('\\', '/')

        let path =
            if Path.DirectorySeparatorChar = '\\' then
                path.Replace('/', '\\')
            else
                path


        let path = Environment.ExpandEnvironmentVariables path

        if Path.IsPathRooted path then
            path
        else
            Path.Combine(baseDir, path)

    let defaultLogConfig baseDir appName =
        [ { FileName = resolvePath baseDir (defaultLogFileName appName)
            MaxSize = defaultLogSize
            MaxFiles = defaultLogFiles
            LogLevel = defaultLogLevels
            DefaultLogLevel = defaultLogLevel
            IncludeScope = defaultIncludeScope } ]

    let mapDto baseDir appName (config: FileLoggerConfig) =
        let configs =
            config.Files
            |> Option.ofObj
            |> Option.defaultValue (Dictionary())
            |> getValues
            |> Seq.map (fun dto ->
                { FileName = resolvePath baseDir dto.File
                  MaxSize = dto.MaxSize |> Option.ofNullable |> Option.defaultValue defaultLogSize
                  MaxFiles = dto.MaxFiles |> Option.ofNullable |> Option.defaultValue defaultLogFiles
                  LogLevel =
                    dto.LogLevel
                    |> Seq.filter (fun x -> not (x.Key.Equals("Default", StringComparison.OrdinalIgnoreCase)))
                    |> Seq.sortByDescending (_.Key)
                    |> Seq.map (fun x -> { Category = x.Key; Level = x.Value })
                    |> Seq.toArray
                  DefaultLogLevel =
                    dto.LogLevel
                    |> Seq.tryFind (_.Key.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    |> Option.map _.Value
                    |> Option.defaultValue defaultLogLevel
                  IncludeScope = config.FormatterOptions.IncludeScopes })
            |> Seq.toList

        if configs.Length = 0 then
            defaultLogConfig baseDir appName
        else
            configs

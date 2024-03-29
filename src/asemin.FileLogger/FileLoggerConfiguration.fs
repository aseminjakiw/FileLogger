﻿namespace asemin.FileLogger

open System
open System.Collections.Generic
open System.IO

type LoggerConfig() =
    member val File = String.Empty with get, set
    member val MaxSize = Nullable() with get, set
    member val MaxFiles = Nullable() with get, set

type FileLoggerConfig() =
    member val Files: IDictionary<string, LoggerConfig> = Dictionary<_, _>([]) with get, set


type LoggerConfiguration =
    { FileName: string
      MaxSize: int
      MaxFiles: int }

module LoggerConfiguration =
    let defaultLogSize = 10 * 1024 * 1024
    let defaultLogFiles = 10

    let defaultLogFileName appName =
        let appName =
            if String.IsNullOrWhiteSpace appName then
                "application"
            else
                appName

        Path.Combine("logs", appName + ".app.log")

    let getValues (dict: IDictionary<_, _>) = dict |> Seq.map (_.Value)

    let resolvePath baseDir (path:string) =        
        let path = path |> Option.ofObj |> Option.defaultValue String.Empty
        let path = path.Replace('\\', '/')
        let path = 
            if Path.DirectorySeparatorChar = '\\' then
                path.Replace('/','\\')
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
            MaxFiles = defaultLogFiles } ]

    let mapDto baseDir appName (dto: FileLoggerConfig) =
        let configs =
            dto.Files
            |> Option.ofObj
            |> Option.defaultValue (Dictionary())
            |> getValues
            |> Seq.map (fun dto ->
                { FileName = resolvePath baseDir dto.File
                  MaxSize = dto.MaxSize |> Option.ofNullable |> Option.defaultValue defaultLogSize
                  MaxFiles = dto.MaxFiles |> Option.ofNullable |> Option.defaultValue defaultLogFiles })
            |> Seq.toList

        if configs.Length = 0 then
            defaultLogConfig baseDir appName
        else
            configs

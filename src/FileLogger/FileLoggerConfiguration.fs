namespace FileLogger

open System
open System.Collections.Generic
open System.IO

[<CLIMutable>]
type LoggerConfigurationDto =
    { MaxSize: Nullable<int>
      MaxFiles: Nullable<int>
      Buffered: Nullable<bool> }


[<CLIMutable>]
type FileLoggerConfigurationDto =
    { Files: Dictionary<string, LoggerConfigurationDto> }


type LoggerConfiguration =
    { FileName: string
      MaxSize: int
      MaxFiles: int
      Buffered: bool }

module LoggerConfiguration =
    let defaultLogSize = 10 * 1024 * 1024
    let defaultLogFiles = 10

    let toMap (dict: Dictionary<_, _>) =
        dict |> Seq.map (fun x -> (x.Key, x.Value)) |> Map

    let resolvePath baseDir path =
        let path = Environment.ExpandEnvironmentVariables path

        if Path.IsPathRooted path then
            path
        else
            Path.Combine(baseDir, path)

    let mapDto baseDir (dto: FileLoggerConfigurationDto) =
        dto.Files
        |> Option.ofObj
        |> Option.defaultValue (Dictionary())
        |> toMap
        |> Map.map (fun logger config ->
            { FileName = resolvePath baseDir logger
              MaxSize = config.MaxSize |> Option.ofNullable |> Option.defaultValue defaultLogSize
              MaxFiles = config.MaxFiles |> Option.ofNullable |> Option.defaultValue defaultLogFiles
              Buffered = config.Buffered |> Option.ofNullable |> Option.defaultValue false })
        |> Map.values
        |> Seq.toList

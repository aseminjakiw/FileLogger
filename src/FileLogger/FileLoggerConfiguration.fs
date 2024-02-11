namespace FileLogger

open System
open System.Collections.Generic
open System.IO

[<CLIMutable>]
type LoggerConfigurationDto =
    { File: String
      MaxSize: Nullable<int>
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

    let getValues (dict: Dictionary<_, _>) = dict |> Seq.map (_.Value)

    let resolvePath baseDir path =
        let path = path |> Option.ofObj |> Option.defaultValue String.Empty
        let path = Environment.ExpandEnvironmentVariables path

        if Path.IsPathRooted path then
            path
        else
            Path.Combine(baseDir, path)

    let mapDto baseDir (dto: FileLoggerConfigurationDto) =
        let configs =
            dto.Files
            |> Option.ofObj
            |> Option.defaultValue (Dictionary())
            |> getValues
            |> Seq.map (fun dto ->
                { FileName = resolvePath baseDir dto.File
                  MaxSize = dto.MaxSize |> Option.ofNullable |> Option.defaultValue defaultLogSize
                  MaxFiles = dto.MaxFiles |> Option.ofNullable |> Option.defaultValue defaultLogFiles
                  Buffered = dto.Buffered |> Option.ofNullable |> Option.defaultValue false })
            |> Seq.toList

        if configs.Length = 0 then
            [ { FileName = resolvePath baseDir "logs.log"
                MaxSize = defaultLogSize
                MaxFiles = defaultLogFiles
                Buffered = false } ]
        else
            configs

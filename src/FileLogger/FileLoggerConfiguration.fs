namespace FileLogger

open System

[<CLIMutable>]
type LoggerConfigurationDto =
    { MaxSize: Nullable<int>
      MaxFiles: Nullable<int> }


[<CLIMutable>]
type FileLoggerConfigurationDto =
    { Loggers: Map<string, LoggerConfigurationDto> }


type LoggerConfiguration =
    { FileName: string
      MaxSize: int option
      MaxFiles: int option }

module LoggerConfiguration =
    let mapDto (dto: FileLoggerConfigurationDto) =
        dto.Loggers
        |> Map.map (fun logger config ->
            { FileName = logger
              MaxSize = config.MaxSize |> Option.ofNullable
              MaxFiles = config.MaxFiles |> Option.ofNullable })
        |> Map.values
        |> Seq.toList

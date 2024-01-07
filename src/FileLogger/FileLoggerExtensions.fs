namespace FileLogger

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging


[<Extension>]
type FileLoggerExtensions =
    [<Extension>]
    static member AddFile(builder: ILoggingBuilder) : ILoggingBuilder = failwith "not implemented"

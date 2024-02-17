namespace asemin.FileLogger

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Configuration


[<Extension>]
type FileLoggerExtensions =
    [<Extension>]
    static member AddFile(builder: ILoggingBuilder) =
        do builder.AddConfiguration()
        do builder.Services.TryAddSingleton<ITimeProvider,SystemTimeProvider>() 
        do builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>())

        do
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfig, FileLoggerProvider>(
                builder.Services
            )

        builder
    
    [<Extension>]
    static member AddFile(builder: ILoggingBuilder, configure: Action<FileLoggerConfig>) =
        do builder.AddFile() |> ignore
        do builder.Services.Configure configure |> ignore
        builder

﻿namespace asemin.FileLogger

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
        do builder.Services.TryAddSingleton<TimeProvider>(TimeProvider.System)
        do builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>())

        do
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfigurationDto, FileLoggerProvider>(
                builder.Services
            )

        builder

    static member AddFile(builder: ILoggingBuilder, configure: Action<FileLoggerConfigurationDto>) =
        do builder.AddFile() |> ignore
        do builder.Services.Configure configure |> ignore
        builder
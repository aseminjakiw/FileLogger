open System.Collections.Generic
open System.Diagnostics
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open asemin.FileLogger


type ExampleClass() =
    class
    end

let executeLogger (logger: ILogger<ExampleClass>) =
    logger.LogTrace "Hello from F#"
    logger.LogDebug "Hello from F#"
    logger.LogInformation "Hello from F#"
    logger.LogWarning "Hello from F#"
    logger.LogError "Hello from F#"
    logger.LogCritical "Hello from F#"

[<EntryPoint>]
let main args =
    task {
        let builder = Host.CreateApplicationBuilder(args)

        FileLoggerExtensions.AddFile(
            builder.Logging,
            (fun x ->
                let configs = dict["default", LoggerConfigurationDto(File = "mylog.log")]
                x.Files <- configs)
        )
        |> ignore

        let _ = builder.Logging.ClearProviders().AddFile()
        use host = builder.Build()
        do! host.StartAsync()

        let logger = host.Services.GetRequiredService<ILogger<ExampleClass>>()

        let stopwatch = Stopwatch()
        stopwatch.Start()

        System.Console.WriteLine "Start logging"

        for i in 1..1_000_000 do
            do executeLogger logger

        System.Console.WriteLine $"Finished logging, elapsed {stopwatch.Elapsed.ToString()}"


        do! host.StopAsync()
        do host.Dispose()

        System.Console.WriteLine $"Shutdown, elapsed {stopwatch.Elapsed.ToString()}"

        return 0
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

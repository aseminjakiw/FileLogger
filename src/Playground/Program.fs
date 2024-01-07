open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open FileLogger

type ExampleClass() =
    class
    end

let executeLogger (logger: ILogger<ExampleClass>) = logger.LogInformation "Hello from F#"

[<EntryPoint>]
let main args =
    task {
        let builder = Host.CreateApplicationBuilder(args)

        let _ = builder.Logging.AddFile()
        let host = builder.Build()
        do! host.StartAsync()

        let logger = host.Services.GetRequiredService<ILogger<ExampleClass>>()
        do executeLogger logger

        do! host.StopAsync()

        return 0
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

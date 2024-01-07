using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playground.Cs;

public class ExampleClass;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddFile();
        var host = builder.Build();
        await host.StartAsync();

        var logger = host.Services.GetRequiredService<ILogger<ExampleClass>>();
        ExecuteLogger(logger);

        await host.StopAsync();

        return 0;
    }

    private static void ExecuteLogger(ILogger<ExampleClass> logger)
    {
        logger.LogInformation("Hello from F#");
    }
}
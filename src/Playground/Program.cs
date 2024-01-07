using FileLogger;

namespace Playground;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.AddFile();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}
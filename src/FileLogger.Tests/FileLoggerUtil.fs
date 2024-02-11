module FileLogger.Tests.FileLoggerUtil

open System
open System.IO
open System.Text
open asemin.FileLogger
open FileLogger.Tests.Util
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open FileLogger
open Microsoft.Extensions.Time.Testing

/// Class for requesting logger with DI
type TestClass() =
    class
    end

type OtherTestClass() =
    class
    end


/// Contains temp directory for test and disposes it
type TestContext(configJson: string, testDir: TestDirectory) =
    let builder = Host.CreateApplicationBuilder()
    do builder.Environment.ContentRootPath <- testDir.Path |> FilePath.value
    let jsonStream = Encoding.UTF8.GetBytes(configJson) |> MemoryStream

    let _ = builder.Configuration.AddJsonStream(jsonStream)

    let _ = builder.Logging.ClearProviders()
    let _ = builder.Logging.SetMinimumLevel(LogLevel.Trace)

    let _ = builder.Logging.AddFile()
    let timeProvider = FakeTimeProvider()
    let _ = builder.Services.AddSingleton<TimeProvider>(timeProvider)
    let host = builder.Build()
    new(configJson) = new TestContext(configJson, new TestDirectory())

    member this.Logger = host.Services.GetRequiredService<ILogger<TestClass>>()
    member this.OtherLogger = host.Services.GetRequiredService<ILogger<OtherTestClass>>()
    member this.Directory = testDir.Path |> FilePath.value
    member this.FileNames = this.Directory |> getFileNames
    member this.Time = timeProvider
    member this.Host = host

    member this.SetTime isoTime =
        DateTimeOffset.Parse isoTime |> timeProvider.SetUtcNow


    interface IDisposable with
        member this.Dispose() =
            host.StopAsync() |> Async.AwaitTask |> Async.RunSynchronously
            testDir.Dispose()

/// short cut for short unit tests
let setup configJson = new TestContext(configJson)

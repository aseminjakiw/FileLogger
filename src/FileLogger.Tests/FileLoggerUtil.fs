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


/// Class for requesting logger with DI
type TestClass() =
    class
    end

type OtherTestClass() =
    class
    end

type TestTimeProvider() =
    member val CurrentTime = DateTimeOffset.MinValue with get, set

    interface ITimeProvider with
        member this.GetLocalNow() = this.CurrentTime

/// Contains temp directory for test and disposes it
type TestContext(configJson: string, testDir: TestDirectory) =
    let builder = Host.CreateApplicationBuilder()
    do builder.Environment.ContentRootPath <- testDir.Path |> FilePath.value
    let jsonStream = Encoding.UTF8.GetBytes(configJson) |> MemoryStream
    do builder.Configuration.AddJsonStream(jsonStream) |> ignore

    do builder.Logging.ClearProviders() |> ignore
    do builder.Logging.SetMinimumLevel(LogLevel.Trace) |> ignore
    do builder.Services.AddSingleton<ITimeProvider, TestTimeProvider>() |> ignore
    do builder.Logging.AddFile() |> ignore
    let host = builder.Build()
    new(configJson) = new TestContext(configJson, new TestDirectory())

    member this.Logger = host.Services.GetRequiredService<ILogger<TestClass>>()
    member this.OtherLogger = host.Services.GetRequiredService<ILogger<OtherTestClass>>()
    member this.Directory = testDir.Path |> FilePath.value
    member this.FileNames = this.Directory |> getFileNames

    member this.Time =
        host.Services.GetRequiredService<ITimeProvider>() :?> TestTimeProvider

    member this.Host = host

    member this.SetTime isoTime =
        this.Time.CurrentTime <- DateTimeOffset.Parse isoTime


    interface IDisposable with
        member this.Dispose() =
            host.StopAsync() |> Async.AwaitTask |> Async.RunSynchronously
            testDir.Dispose()

/// short cut for short unit tests
let setup configJson = new TestContext(configJson)

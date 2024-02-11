module FileLogger.Tests.LogTests

open System.IO
open Microsoft.Extensions.Logging
open Xunit
open FsUnit

open FileLogger.Tests.FileLoggerUtil

let config =
    """
    {
      "Logging": {
        "File": {
          "Files": {
            "logFile.log": {}
          }
        }
      }
    }
    """

let getLogs path = File.ReadAllLines path

let flushLogs (test:TestContext) = test.Host.Dispose()


[<Fact>]
let ``Write log message`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"
    
    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ])


[<Fact>]
let ``Write different log messages`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"
    test.Logger.LogInformation "test log3"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log1"
              "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2"
              "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log3" ])

[<Fact>]
let ``Write different times`` () : unit =
    use test = config |> setup

    test.SetTime "2022-05-06 21:43:23.456"
    test.Logger.LogInformation "test log1"

    test.SetTime "2023-12-31 21:43:23.456"
    test.Logger.LogInformation "test log2"

    test.SetTime "2021-01-01 05:43:23.456"
    test.Logger.LogInformation "test log3"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log1"
              "2023-12-31 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2"
              "2021-01-01 05:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log3" ])

[<Fact>]
let ``Write log level`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogTrace "test log"
    test.Logger.LogDebug "test log"
    test.Logger.LogInformation "test log"
    test.Logger.LogWarning "test log"
    test.Logger.LogError "test log"
    test.Logger.LogCritical "test log"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [TRACE] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [DEBUG] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [WARN ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ])


[<Fact>]
let ``Write different categories`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"
    test.OtherLogger.LogInformation "test log"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [INFO ] [OtherTestClass] test log" ])

[<Fact>]
let ``File roll over -> move content to archive file and still write in new file`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "logFile.log": {
                  "MaxSize": 5
                }
              }
            }
          }
        }
        """
        |> setup
    
    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ])


    $"{test.Directory}\logFile.1.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2" ])


[<Fact>]
let ``multiple loggers -> write in both files`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "logFile.log": {},
                "other.txt": {}
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    do flushLogs test
    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ])


    $"{test.Directory}\other.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ])

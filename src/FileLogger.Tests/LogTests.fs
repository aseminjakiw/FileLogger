module FileLogger.Tests.LogTests

open System.IO
open System.Text.RegularExpressions
open Microsoft.Extensions.Logging
open Xunit
open FsUnit

open Util
open FileLogger.Tests.FileLoggerUtil

let config =
    """
    {
      "Logging": {
        "File": {
          "Files": {
            "default": {
              "File": "logFile.log"
            }
          }
        }
      }
    }
    """

let normalizeThreadId (lines: string array) =
    lines |> Array.map (fun line -> Regex.Replace(line, @"\[..\]", "[00]"))

let getLogs path =
    path |> File.ReadAllLines |> normalizeThreadId

let flushLogs (test: TestContext) = test.Host.Dispose()

[<Fact>]
let ``Write log message`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]


[<Fact>]
let ``Write different log messages`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"
    test.Logger.LogInformation "test log3"

    do flushLogs test

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log1"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log3" ]

[<Fact>]
let ``Write different times`` () : unit =
    use test = config |> setup

    test.SetTime "2022-05-06 21:43:23.456"
    test.Logger.LogInformation "test log1"

    test.SetTime "2023-12-31 21:43:23.456"
    test.Logger.LogInformation "test log2"

    test.SetTime "2024-01-01 05:43:23.456"
    test.Logger.LogInformation "test log3"

    do flushLogs test

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log1"
          "2023-12-31 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2"
          "2024-01-01 05:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log3" ]

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

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [TRACE] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [DEBUG] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [WARN ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]

[<Fact>]
let ``Write different categories`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"
    test.OtherLogger.LogInformation "test log"

    do flushLogs test

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.OtherTestClass] test log" ]

[<Fact>]
let ``File roll over -> move content to archive file and still write in new file`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",                  
                  "MaxSize": 5
                }
              }
            }
          }
        }
        """
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"

    do flushLogs test

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2" ]


    $"{test.Directory}\logFile.1.log"
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log1" ]


[<Fact>]
let ``multiple loggers -> write in both files`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log"
                },
                "other": {
                  "File": "other.log"
                }
              }
            }
          }
        }
        """
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"

    do flushLogs test

    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]


    Path.Combine(test.Directory, "logFile.log")
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]

module FileLogger.Tests.FilterTests

open System.IO
open System.Text.RegularExpressions
open Microsoft.Extensions.Logging
open Xunit

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
let ``Respect Default LogLevel`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",
                  "LogLevel": {
                    "Default": "Error"
                  }
                }
              }
            }
          }
        }
        """
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogTrace "test log"
    test.Logger.LogDebug "test log"
    test.Logger.LogInformation "test log"
    test.Logger.LogWarning "test log"
    test.Logger.LogError "test log"
    test.Logger.LogCritical "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]


[<Fact>]
let ``No log levels, log all`` () : unit =
    use test =
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
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogTrace "test log"
    test.Logger.LogDebug "test log"
    test.Logger.LogInformation "test log"
    test.Logger.LogWarning "test log"
    test.Logger.LogError "test log"
    test.Logger.LogCritical "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [TRACE] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [DEBUG] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [WARN ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]

[<Fact>]
let ``Most specific log level wins `` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",
                  "LogLevel": {
                    "Default": "Error",
                    "FileLogger.Tests": "Warning",
                    "FileLogger.Tests.FileLoggerUtil": "Information",
                    "FileLogger.Tests.FileLoggerUtil.TestClass": "Debug"
                  }
                }
              }
            }
          }
        }
        """
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogTrace "test log"
    test.Logger.LogDebug "test log"
    test.Logger.LogInformation "test log"
    test.Logger.LogWarning "test log"
    test.Logger.LogError "test log"
    test.Logger.LogCritical "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [DEBUG] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [WARN ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]


[<Fact>]
let ``No matching log level, fallback to Default`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",
                  "LogLevel": {
                    "Default": "Information",
                    "Some.Stuff": "Error"
                  }
                }
              }
            }
          }
        }
        """
        |> setup

    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogTrace "test log"
    test.Logger.LogDebug "test log"
    test.Logger.LogInformation "test log"
    test.Logger.LogWarning "test log"
    test.Logger.LogError "test log"
    test.Logger.LogCritical "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [WARN ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [ERROR] [FileLogger.Tests.FileLoggerUtil.TestClass] test log"
          "2022-05-06 21:43:23.456 [00] [FATAL] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]
        
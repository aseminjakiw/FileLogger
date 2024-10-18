module FileLogger.Tests.LogTests

open System.Collections.Generic
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

let configWithScope =
    """
    {
      "Logging": {
        "File": {
          "Files": {
            "default": {
              "File": "logFile.log",
              "IncludeScopes": true
            }
          },
          "FormatterOptions": {
            "IncludeScopes": true
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

    combinePath2 test.Directory "logFile.log"
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

    combinePath2 test.Directory "logFile.log"
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
let ``Write different categories`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"
    test.OtherLogger.LogInformation "test log"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
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

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log2" ]


    combinePath2 test.Directory "logFile.1.log"
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

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]


    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] test log" ]

[<Fact>]
let ``Scope disabled`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    let scope = test.Logger.BeginScope("scope")
    test.Logger.LogInformation "inside"
    scope.Dispose()
    test.Logger.LogInformation "outside"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] inside"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] outside" ]

[<Fact>]
let ``Scope enabled`` () : unit =
    use test = configWithScope |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    let scope = test.Logger.BeginScope("scope")
    test.Logger.LogInformation "inside"
    scope.Dispose()
    test.Logger.LogInformation "outside"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] => scope | inside"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] outside" ]

[<Fact>]
let ``Scope multiple levels`` () : unit =
    use test = configWithScope |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    let scope = test.Logger.BeginScope("scope")
    test.Logger.LogInformation "level 1"

    let scope2 =
        test.Logger.BeginScope(KeyValuePair<string, int>("level", 2))

    test.Logger.LogInformation "level 2"
    scope2.Dispose()
    scope.Dispose()

    test.Logger.LogInformation "outside"

    do flushLogs test

    combinePath2 test.Directory "logFile.log"
    |> getLogs
    |> shouldBeEqual
        [ "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] => scope | level 1"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] => scope => [level, 2] | level 2"
          "2022-05-06 21:43:23.456 [00] [INFO ] [FileLogger.Tests.FileLoggerUtil.TestClass] outside" ]

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
        "File":{
          "default": {
            "File":"log.txt"
          }
        }
      }
    }
    """

let getLogs path = File.ReadAllLines path


[<Fact>]
let ``Write log message`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log" ])


[<Fact>]
let ``Write different log messages`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"
    test.Logger.LogInformation "test log3"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log1"
              "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log2"
              "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log3" ])

[<Fact>]
let ``Write different times`` () : unit =
    use test = config |> setup

    test.SetTime "2022-05-06 21:43:23.456"
    test.Logger.LogInformation "test log1"

    test.SetTime "2023-12-31 21:43:23.456"
    test.Logger.LogInformation "test log2"

    test.SetTime "2021-01-01 05:43:23.456"
    test.Logger.LogInformation "test log3"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log1"
              "2023-12-31 21:43:23.456 [01] [INFO ] [TestClass] test log2"
              "2021-01-01 05:43:23.456 [01] [INFO ] [TestClass] test log3" ])

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

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [TRACE] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [DEBUG] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [WARN ] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [ERROR] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [FATAL] [TestClass] test log" ])


[<Fact>]
let ``Write different categories`` () : unit =
    use test = config |> setup
    test.SetTime "2022-05-06 21:43:23.456"

    test.Logger.LogInformation "test log"
    test.OtherLogger.LogInformation "test log"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should
        be
        (equal
            [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log"
              "2022-05-06 21:43:23.456 [01] [INFO ] [OtherTestClass] test log" ])

[<Fact>]
let ``File roll over -> move content to archive file and still write in new file`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"logFile.log"
                "MaxSize": 5,
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log" ])


    $"{test.Directory}\logFile.1.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log2" ])


[<Fact>]
let ``multiple loggers -> write in both files`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"logFile.log"
              },
              "other": {
                "File":"other.log"
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    $"{test.Directory}\logFile.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log" ])


    $"{test.Directory}\other.log"
    |> getLogs
    |> should be (equal [ "2022-05-06 21:43:23.456 [01] [INFO ] [TestClass] test log" ])

module FileLogger.Tests.FileHandlingTests

open Microsoft.Extensions.Logging
open Xunit
open FsUnit

open FileLogger.Tests.FileLoggerUtil

// ###################################################################
// #   Tests for file handling and rolling
// ###################################################################
[<Fact>]
let ``no config -> create default log file "log.txt"`` () : unit =
    use test = "" |> setup

    test.Logger.LogInformation "Hans Dampf"

    test.FileNames |> should be (equal [ $"{test.Directory}\log.txt" ])


[<Fact>]
let ``File name in config -> use file name`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"logFile.log"
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    test.FileNames |> should be (equal [ $"{test.Directory}\logFile.log" ])

[<Fact>]
let ``multiple file loggers -> write in both`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"logFile.log"
              },              
              "other": {
                "File":"otherLogFile.log"
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    test.FileNames
    |> should be (equal [ $"{test.Directory}\logFile.log"; $"{test.Directory}\otherLogFile.log" ])

[<Fact>]
let ``relative path in file name -> append path to current dir`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"\logs\logFile.log"
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    test.FileNames |> should be (equal [ $"{test.Directory}\logs\logFile.log" ])

[<Fact>]
let ``absolute path in file name -> use absolute path`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"D:\Temp\logs\logFile.log"
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    test.FileNames |> should be (equal [ "D:\Temp\logs\logFile.log" ])

[<Fact>]
let ``log bigger than max size -> move old file and start new with configured file name`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"D:\Temp\logs\logFile.log"
                "MaxSize": 5
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"

    test.FileNames
    |> should be (equal [ "D:\Temp\logs\logFile.log"; "D:\Temp\logs\logFile.1.log" ])


[<Fact>]
let ``more log files than max files -> delete older ones`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"D:\Temp\logs\logFile.log"
                "MaxSize": 5,
                "MaxFiles: 3
              }
            }
          }
        }
        """
        |> setup

    for i in 1..110 do
        test.Logger.LogInformation "test log"

    test.FileNames
    |> should
        be
        (equal
            [ "D:\Temp\logs\logFile.log"
              "D:\Temp\logs\logFile.108.log"
              "D:\Temp\logs\logFile.109.log" ])


[<Fact>]
let ``log bigger than max size -> append correct number to archived file`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File":{
              "default": {
                "File":"D:\Temp\logs\logFile.log"
                "MaxSize": 5,
                "MaxFiles: 200
              }
            }
          }
        }
        """
        |> setup

    for i in 1..110 do
        test.Logger.LogInformation "test log"

    test.FileNames
    |> should
        be
        (equal
            [ "D:\Temp\logs\logFile.log"
              "D:\Temp\logs\logFile.1.log"
              "D:\Temp\logs\logFile.2.log"
              "D:\Temp\logs\logFile.3.log"
              "D:\Temp\logs\logFile.4.log"
              "D:\Temp\logs\logFile.5.log"
              "D:\Temp\logs\logFile.7.log"
              "D:\Temp\logs\logFile.8.log"
              "D:\Temp\logs\logFile.9.log"
              "D:\Temp\logs\logFile.10.log"
              "D:\Temp\logs\logFile.11.log"
              "D:\Temp\logs\logFile.12.log"
              "D:\Temp\logs\logFile.13.log"
              "D:\Temp\logs\logFile.14.log"
              "D:\Temp\logs\logFile.15.log"
              "D:\Temp\logs\logFile.17.log"
              "D:\Temp\logs\logFile.18.log"
              "D:\Temp\logs\logFile.19.log"
              "D:\Temp\logs\logFile.20.log"
              "D:\Temp\logs\logFile.21.log"
              "D:\Temp\logs\logFile.22.log"
              "D:\Temp\logs\logFile.23.log"
              "D:\Temp\logs\logFile.24.log"
              "D:\Temp\logs\logFile.25.log"
              "D:\Temp\logs\logFile.27.log"
              "D:\Temp\logs\logFile.28.log"
              "D:\Temp\logs\logFile.29.log"
              "D:\Temp\logs\logFile.30.log"
              "D:\Temp\logs\logFile.31.log"
              "D:\Temp\logs\logFile.32.log"
              "D:\Temp\logs\logFile.33.log"
              "D:\Temp\logs\logFile.34.log"
              "D:\Temp\logs\logFile.35.log"
              "D:\Temp\logs\logFile.37.log"
              "D:\Temp\logs\logFile.38.log"
              "D:\Temp\logs\logFile.39.log"
              "D:\Temp\logs\logFile.40.log"
              "D:\Temp\logs\logFile.41.log"
              "D:\Temp\logs\logFile.42.log"
              "D:\Temp\logs\logFile.43.log"
              "D:\Temp\logs\logFile.44.log"
              "D:\Temp\logs\logFile.45.log"
              "D:\Temp\logs\logFile.47.log"
              "D:\Temp\logs\logFile.48.log"
              "D:\Temp\logs\logFile.49.log"
              "D:\Temp\logs\logFile.50.log"
              "D:\Temp\logs\logFile.51.log"
              "D:\Temp\logs\logFile.52.log"
              "D:\Temp\logs\logFile.53.log"
              "D:\Temp\logs\logFile.54.log"
              "D:\Temp\logs\logFile.55.log"
              "D:\Temp\logs\logFile.57.log"
              "D:\Temp\logs\logFile.58.log"
              "D:\Temp\logs\logFile.59.log"
              "D:\Temp\logs\logFile.60.log"
              "D:\Temp\logs\logFile.61.log"
              "D:\Temp\logs\logFile.62.log"
              "D:\Temp\logs\logFile.63.log"
              "D:\Temp\logs\logFile.64.log"
              "D:\Temp\logs\logFile.65.log"
              "D:\Temp\logs\logFile.67.log"
              "D:\Temp\logs\logFile.68.log"
              "D:\Temp\logs\logFile.69.log"
              "D:\Temp\logs\logFile.70.log"
              "D:\Temp\logs\logFile.71.log"
              "D:\Temp\logs\logFile.72.log"
              "D:\Temp\logs\logFile.73.log"
              "D:\Temp\logs\logFile.74.log"
              "D:\Temp\logs\logFile.75.log"
              "D:\Temp\logs\logFile.77.log"
              "D:\Temp\logs\logFile.78.log"
              "D:\Temp\logs\logFile.79.log"
              "D:\Temp\logs\logFile.80.log"
              "D:\Temp\logs\logFile.81.log"
              "D:\Temp\logs\logFile.82.log"
              "D:\Temp\logs\logFile.83.log"
              "D:\Temp\logs\logFile.84.log"
              "D:\Temp\logs\logFile.85.log"
              "D:\Temp\logs\logFile.87.log"
              "D:\Temp\logs\logFile.88.log"
              "D:\Temp\logs\logFile.89.log"
              "D:\Temp\logs\logFile.90.log"
              "D:\Temp\logs\logFile.91.log"
              "D:\Temp\logs\logFile.92.log"
              "D:\Temp\logs\logFile.93.log"
              "D:\Temp\logs\logFile.94.log"
              "D:\Temp\logs\logFile.95.log"
              "D:\Temp\logs\logFile.97.log"
              "D:\Temp\logs\logFile.98.log"
              "D:\Temp\logs\logFile.99.log"
              "D:\Temp\logs\logFile.100.log"
              "D:\Temp\logs\logFile.101.log"
              "D:\Temp\logs\logFile.102.log"
              "D:\Temp\logs\logFile.103.log"
              "D:\Temp\logs\logFile.104.log"
              "D:\Temp\logs\logFile.105.log"
              "D:\Temp\logs\logFile.107.log"
              "D:\Temp\logs\logFile.108.log"
              "D:\Temp\logs\logFile.109.log" ])

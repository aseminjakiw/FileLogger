module FileLogger.Tests.FileHandlingTests

open FileLogger
open FileLogger.Tests.Util
open Microsoft.Extensions.Logging
open Xunit
open FsUnit

open FileLogger.Tests.FileLoggerUtil

// ###################################################################
// #   Tests for file handling and rolling
// ###################################################################
let flushLogs (test: TestContext) = test.Host.Dispose()

[<Fact>]
let ``no config -> create default log file "logs.log"`` () : unit =
    use test = "{}" |> setup

    test.Logger.LogInformation "Hans Dampf"

    do flushLogs test
    test.FileNames |> should be (equal [ $"{test.Directory}\logs\logs.log" ])


[<Fact>]
let ``File name in config -> use file name`` () : unit =
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

    test.Logger.LogInformation "test log"

    do flushLogs test
    test.FileNames |> should be (equal [ $"{test.Directory}\logFile.log" ])

[<Fact>]
let ``multiple file loggers -> write in both`` () : unit =
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
                  "File": "otherLogFile.log"
                }
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    do flushLogs test

    test.FileNames
    |> should be (equal [ $"{test.Directory}\logFile.log"; $"{test.Directory}\otherLogFile.log" ])

[<Fact>]
let ``relative path (backward slash) in file name -> append path to current dir`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logs\\logFile.log"
                }
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    do flushLogs test
    test.FileNames |> should be (equal [ $"{test.Directory}\logs\logFile.log" ])

[<Fact>]
let ``relative path (forward slash) in file name -> append path to current dir`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logs/logFile.log"
                }
              }
            }
          }
        }
        """
        |> setup

    test.Logger.LogInformation "test log"

    do flushLogs test
    test.FileNames |> should be (equal [ $"{test.Directory}\logs\logFile.log" ])

[<Fact>]
let ``absolute path in file name -> use absolute path`` () : unit =
    use testDir = new TestDirectory()
    let toForwardSlashes (str: string) = str.Replace('\\', '/')
    let jsonCompatibleDir = testDir.Path |> FilePath.value |> toForwardSlashes

    let config =
        $$"""
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "{{jsonCompatibleDir}}/logFile.log"
                }
              }
            }
          }
        }
        """

    use test = new TestContext(config, testDir)
    test.Logger.LogInformation "test log"
    do flushLogs test

    test.FileNames
    |> should be (equal [ $"{testDir.Path |> FilePath.value}\logFile.log" ])

[<Fact>]
let ``log bigger than max size -> move old file and start new with configured file name`` () : unit =
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

    test.Logger.LogInformation "test log1"
    test.Logger.LogInformation "test log2"

    do flushLogs test

    test.FileNames
    |> should be (equivalent [ $"{test.Directory}\logFile.log"; $"{test.Directory}\logFile.1.log" ])


[<Fact>]
let ``more log files than max files -> delete older ones`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",
                  "MaxSize": 5,
                  "MaxFiles": 3
                }
              }
            }
          }
        }
        """
        |> setup

    for i in 1..110 do
        test.Logger.LogInformation "test log"

    do flushLogs test

    test.FileNames
    |> should
        be
        (equivalent
            [ $"{test.Directory}\logFile.log"
              $"{test.Directory}\logFile.108.log"
              $"{test.Directory}\logFile.109.log" ])


[<Fact>]
let ``log bigger than max size -> append correct number to archived file`` () : unit =
    use test =
        """
        {
          "Logging": {
            "File": {
              "Files": {
                "default": {
                  "File": "logFile.log",
                  "MaxSize": 5,
                  "MaxFiles": 200
                }
              }
            }
          }
        }
        """
        |> setup

    for i in 1..110 do
        test.Logger.LogInformation "test log"

    do flushLogs test

    let expectedFiles =
        [ $"{test.Directory}\logFile.log"
          $"{test.Directory}\logFile.1.log"
          $"{test.Directory}\logFile.2.log"
          $"{test.Directory}\logFile.3.log"
          $"{test.Directory}\logFile.4.log"
          $"{test.Directory}\logFile.5.log"
          $"{test.Directory}\logFile.6.log"
          $"{test.Directory}\logFile.7.log"
          $"{test.Directory}\logFile.8.log"
          $"{test.Directory}\logFile.9.log"
          $"{test.Directory}\logFile.10.log"
          $"{test.Directory}\logFile.11.log"
          $"{test.Directory}\logFile.12.log"
          $"{test.Directory}\logFile.13.log"
          $"{test.Directory}\logFile.14.log"
          $"{test.Directory}\logFile.15.log"
          $"{test.Directory}\logFile.16.log"
          $"{test.Directory}\logFile.17.log"
          $"{test.Directory}\logFile.18.log"
          $"{test.Directory}\logFile.19.log"
          $"{test.Directory}\logFile.20.log"
          $"{test.Directory}\logFile.21.log"
          $"{test.Directory}\logFile.22.log"
          $"{test.Directory}\logFile.23.log"
          $"{test.Directory}\logFile.24.log"
          $"{test.Directory}\logFile.25.log"
          $"{test.Directory}\logFile.26.log"
          $"{test.Directory}\logFile.27.log"
          $"{test.Directory}\logFile.28.log"
          $"{test.Directory}\logFile.29.log"
          $"{test.Directory}\logFile.30.log"
          $"{test.Directory}\logFile.31.log"
          $"{test.Directory}\logFile.32.log"
          $"{test.Directory}\logFile.33.log"
          $"{test.Directory}\logFile.34.log"
          $"{test.Directory}\logFile.35.log"
          $"{test.Directory}\logFile.36.log"
          $"{test.Directory}\logFile.37.log"
          $"{test.Directory}\logFile.38.log"
          $"{test.Directory}\logFile.39.log"
          $"{test.Directory}\logFile.40.log"
          $"{test.Directory}\logFile.41.log"
          $"{test.Directory}\logFile.42.log"
          $"{test.Directory}\logFile.43.log"
          $"{test.Directory}\logFile.44.log"
          $"{test.Directory}\logFile.45.log"
          $"{test.Directory}\logFile.46.log"
          $"{test.Directory}\logFile.47.log"
          $"{test.Directory}\logFile.48.log"
          $"{test.Directory}\logFile.49.log"
          $"{test.Directory}\logFile.50.log"
          $"{test.Directory}\logFile.51.log"
          $"{test.Directory}\logFile.52.log"
          $"{test.Directory}\logFile.53.log"
          $"{test.Directory}\logFile.54.log"
          $"{test.Directory}\logFile.55.log"
          $"{test.Directory}\logFile.56.log"
          $"{test.Directory}\logFile.57.log"
          $"{test.Directory}\logFile.58.log"
          $"{test.Directory}\logFile.59.log"
          $"{test.Directory}\logFile.60.log"
          $"{test.Directory}\logFile.61.log"
          $"{test.Directory}\logFile.62.log"
          $"{test.Directory}\logFile.63.log"
          $"{test.Directory}\logFile.64.log"
          $"{test.Directory}\logFile.65.log"
          $"{test.Directory}\logFile.66.log"
          $"{test.Directory}\logFile.67.log"
          $"{test.Directory}\logFile.68.log"
          $"{test.Directory}\logFile.69.log"
          $"{test.Directory}\logFile.70.log"
          $"{test.Directory}\logFile.71.log"
          $"{test.Directory}\logFile.72.log"
          $"{test.Directory}\logFile.73.log"
          $"{test.Directory}\logFile.74.log"
          $"{test.Directory}\logFile.75.log"
          $"{test.Directory}\logFile.76.log"
          $"{test.Directory}\logFile.77.log"
          $"{test.Directory}\logFile.78.log"
          $"{test.Directory}\logFile.79.log"
          $"{test.Directory}\logFile.80.log"
          $"{test.Directory}\logFile.81.log"
          $"{test.Directory}\logFile.82.log"
          $"{test.Directory}\logFile.83.log"
          $"{test.Directory}\logFile.84.log"
          $"{test.Directory}\logFile.85.log"
          $"{test.Directory}\logFile.86.log"
          $"{test.Directory}\logFile.87.log"
          $"{test.Directory}\logFile.88.log"
          $"{test.Directory}\logFile.89.log"
          $"{test.Directory}\logFile.90.log"
          $"{test.Directory}\logFile.91.log"
          $"{test.Directory}\logFile.92.log"
          $"{test.Directory}\logFile.93.log"
          $"{test.Directory}\logFile.94.log"
          $"{test.Directory}\logFile.95.log"
          $"{test.Directory}\logFile.96.log"
          $"{test.Directory}\logFile.97.log"
          $"{test.Directory}\logFile.98.log"
          $"{test.Directory}\logFile.99.log"
          $"{test.Directory}\logFile.100.log"
          $"{test.Directory}\logFile.101.log"
          $"{test.Directory}\logFile.102.log"
          $"{test.Directory}\logFile.103.log"
          $"{test.Directory}\logFile.104.log"
          $"{test.Directory}\logFile.105.log"
          $"{test.Directory}\logFile.106.log"
          $"{test.Directory}\logFile.107.log"
          $"{test.Directory}\logFile.108.log"
          $"{test.Directory}\logFile.109.log" ]

    test.FileNames |> should be (equivalent expectedFiles)

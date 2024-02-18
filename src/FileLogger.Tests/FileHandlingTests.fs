module FileLogger.Tests.FileHandlingTests

open System.IO
open asemin.FileLogger
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

    test.FileNames
    |> shouldBeEqual [ combinePath3 test.Directory "logs" "ReSharperTestRunner.app.log" ]


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
    test.FileNames |> shouldBeEqual [ combinePath2 test.Directory "logFile.log" ]


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
    |> shouldBeEqual
        [ combinePath2 test.Directory "logFile.log"
          combinePath2 test.Directory "otherLogFile.log" ]

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

    test.FileNames
    |> shouldBeEqual [ combinePath3 test.Directory "logs" "logFile.log" ]

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

    test.FileNames
    |> shouldBeEqual [ combinePath3 test.Directory "logs" "logFile.log" ]

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
    |> shouldBeEqual [ combinePath2 (testDir.Path |> FilePath.value) "logFile.log" ]

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
    |> shouldBeEqual
        [ combinePath2 test.Directory "logFile.log"
          combinePath2 test.Directory "logFile.1.log" ]


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
    |> shouldBeEqual
        [ combinePath2 test.Directory "logFile.log"
          combinePath2 test.Directory "logFile.108.log"
          combinePath2 test.Directory "logFile.109.log" ]


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


    test.FileNames
    |> shouldBeEqual
        [ combinePath2 test.Directory "logFile.log"
          combinePath2 test.Directory "logFile.1.log"
          combinePath2 test.Directory "logFile.2.log"
          combinePath2 test.Directory "logFile.3.log"
          combinePath2 test.Directory "logFile.4.log"
          combinePath2 test.Directory "logFile.5.log"
          combinePath2 test.Directory "logFile.6.log"
          combinePath2 test.Directory "logFile.7.log"
          combinePath2 test.Directory "logFile.8.log"
          combinePath2 test.Directory "logFile.9.log"
          combinePath2 test.Directory "logFile.10.log"
          combinePath2 test.Directory "logFile.11.log"
          combinePath2 test.Directory "logFile.12.log"
          combinePath2 test.Directory "logFile.13.log"
          combinePath2 test.Directory "logFile.14.log"
          combinePath2 test.Directory "logFile.15.log"
          combinePath2 test.Directory "logFile.16.log"
          combinePath2 test.Directory "logFile.17.log"
          combinePath2 test.Directory "logFile.18.log"
          combinePath2 test.Directory "logFile.19.log"
          combinePath2 test.Directory "logFile.20.log"
          combinePath2 test.Directory "logFile.21.log"
          combinePath2 test.Directory "logFile.22.log"
          combinePath2 test.Directory "logFile.23.log"
          combinePath2 test.Directory "logFile.24.log"
          combinePath2 test.Directory "logFile.25.log"
          combinePath2 test.Directory "logFile.26.log"
          combinePath2 test.Directory "logFile.27.log"
          combinePath2 test.Directory "logFile.28.log"
          combinePath2 test.Directory "logFile.29.log"
          combinePath2 test.Directory "logFile.30.log"
          combinePath2 test.Directory "logFile.31.log"
          combinePath2 test.Directory "logFile.32.log"
          combinePath2 test.Directory "logFile.33.log"
          combinePath2 test.Directory "logFile.34.log"
          combinePath2 test.Directory "logFile.35.log"
          combinePath2 test.Directory "logFile.36.log"
          combinePath2 test.Directory "logFile.37.log"
          combinePath2 test.Directory "logFile.38.log"
          combinePath2 test.Directory "logFile.39.log"
          combinePath2 test.Directory "logFile.40.log"
          combinePath2 test.Directory "logFile.41.log"
          combinePath2 test.Directory "logFile.42.log"
          combinePath2 test.Directory "logFile.43.log"
          combinePath2 test.Directory "logFile.44.log"
          combinePath2 test.Directory "logFile.45.log"
          combinePath2 test.Directory "logFile.46.log"
          combinePath2 test.Directory "logFile.47.log"
          combinePath2 test.Directory "logFile.48.log"
          combinePath2 test.Directory "logFile.49.log"
          combinePath2 test.Directory "logFile.50.log"
          combinePath2 test.Directory "logFile.51.log"
          combinePath2 test.Directory "logFile.52.log"
          combinePath2 test.Directory "logFile.53.log"
          combinePath2 test.Directory "logFile.54.log"
          combinePath2 test.Directory "logFile.55.log"
          combinePath2 test.Directory "logFile.56.log"
          combinePath2 test.Directory "logFile.57.log"
          combinePath2 test.Directory "logFile.58.log"
          combinePath2 test.Directory "logFile.59.log"
          combinePath2 test.Directory "logFile.60.log"
          combinePath2 test.Directory "logFile.61.log"
          combinePath2 test.Directory "logFile.62.log"
          combinePath2 test.Directory "logFile.63.log"
          combinePath2 test.Directory "logFile.64.log"
          combinePath2 test.Directory "logFile.65.log"
          combinePath2 test.Directory "logFile.66.log"
          combinePath2 test.Directory "logFile.67.log"
          combinePath2 test.Directory "logFile.68.log"
          combinePath2 test.Directory "logFile.69.log"
          combinePath2 test.Directory "logFile.70.log"
          combinePath2 test.Directory "logFile.71.log"
          combinePath2 test.Directory "logFile.72.log"
          combinePath2 test.Directory "logFile.73.log"
          combinePath2 test.Directory "logFile.74.log"
          combinePath2 test.Directory "logFile.75.log"
          combinePath2 test.Directory "logFile.76.log"
          combinePath2 test.Directory "logFile.77.log"
          combinePath2 test.Directory "logFile.78.log"
          combinePath2 test.Directory "logFile.79.log"
          combinePath2 test.Directory "logFile.80.log"
          combinePath2 test.Directory "logFile.81.log"
          combinePath2 test.Directory "logFile.82.log"
          combinePath2 test.Directory "logFile.83.log"
          combinePath2 test.Directory "logFile.84.log"
          combinePath2 test.Directory "logFile.85.log"
          combinePath2 test.Directory "logFile.86.log"
          combinePath2 test.Directory "logFile.87.log"
          combinePath2 test.Directory "logFile.88.log"
          combinePath2 test.Directory "logFile.89.log"
          combinePath2 test.Directory "logFile.90.log"
          combinePath2 test.Directory "logFile.91.log"
          combinePath2 test.Directory "logFile.92.log"
          combinePath2 test.Directory "logFile.93.log"
          combinePath2 test.Directory "logFile.94.log"
          combinePath2 test.Directory "logFile.95.log"
          combinePath2 test.Directory "logFile.96.log"
          combinePath2 test.Directory "logFile.97.log"
          combinePath2 test.Directory "logFile.98.log"
          combinePath2 test.Directory "logFile.99.log"
          combinePath2 test.Directory "logFile.100.log"
          combinePath2 test.Directory "logFile.101.log"
          combinePath2 test.Directory "logFile.102.log"
          combinePath2 test.Directory "logFile.103.log"
          combinePath2 test.Directory "logFile.104.log"
          combinePath2 test.Directory "logFile.105.log"
          combinePath2 test.Directory "logFile.106.log"
          combinePath2 test.Directory "logFile.107.log"
          combinePath2 test.Directory "logFile.108.log"
          combinePath2 test.Directory "logFile.109.log" ]

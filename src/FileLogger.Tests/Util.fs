module FileLogger.Tests.Util

open System
open System.IO
open Xunit

open FileLogger


// ###################################################################
// #   helper for creating temp directory and deleting it
// ###################################################################
type TestDirectory() =
    let randomDirectoryName =
        Path.GetRandomFileName() |> Path.GetFileNameWithoutExtension |> FilePath

    let pathToTemp = Path.GetTempPath() |> FilePath

    let tempDirectory = randomDirectoryName |> FilePath.combine pathToTemp

    do Directory.CreateDirectory(tempDirectory |> FilePath.value) |> ignore



    interface IDisposable with
        member this.Dispose() =
            if FilePath.directoryExists tempDirectory then
                Directory.Delete(tempDirectory |> FilePath.value, true)

    member this.Path = tempDirectory
    member this.Dispose() = (this :> IDisposable).Dispose()


let getFileNames path =
    Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories) |> Seq.toList


// ###################################################################
// #   helpers for xUnit parametrized tests
// ###################################################################
module TestHelper =
    let toTheoryData1 data =
        let theory = TheoryData<'a>()
        data |> List.iter theory.Add
        theory

    let toTheoryData2 data =
        let theory = TheoryData<'a, 'b>()
        data |> List.iter theory.Add
        theory

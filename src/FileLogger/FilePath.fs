namespace FileLogger.FilePath

open System
open System.IO


// ###################################################################
// #   FilePath handling
// ###################################################################

type FilePath = FilePath of string

module FilePath =
    let value path =
        match path with
        | FilePath x -> x

    let combine path1 path2 =
        let combinedPath =
            match (path1, path2) with
            | FilePath a, FilePath b -> Path.Combine(a, b)

        FilePath combinedPath

    let enumerateFilesRecursive path =
        Directory.EnumerateFiles(path |> value, "*", SearchOption.AllDirectories)
        |> Seq.map FilePath

    let enumerateDirectoriesRecursive path =
        Directory.EnumerateDirectories(path |> value, "*", SearchOption.AllDirectories)
        |> Seq.map FilePath

    let deleteFile path = path |> value |> File.Delete

    let createDirectory path =
        path |> value |> Directory.CreateDirectory |> ignore

    let relativeTo relativeTo filePath =
        Path.GetRelativePath(relativeTo |> value, filePath |> value) |> FilePath

    let deleteDirectory filePath = filePath |> value |> Directory.Delete

    let directoryExists filePath = filePath |> value |> Directory.Exists

    let directorySeperator = Path.DirectorySeparatorChar

    let getDirectoryName filePath =
        filePath |> value |> Path.GetDirectoryName |> FilePath

    let openFile (fileMode: FileMode) filePath = File.Open(filePath |> value, fileMode)

    let openFileAdvanced fileMode fileAccess fileShare filePath =
        File.Open(filePath |> value, fileMode, fileAccess, fileShare)

    let getCreationTime filePath =
        let dateTime = filePath |> value |> File.GetCreationTimeUtc
        DateTimeOffset(dateTime, TimeSpan.Zero)

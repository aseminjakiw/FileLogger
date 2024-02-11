namespace FileLogger

open System
open System.IO
open System.Text

// ###################################################################
// #   F# wrapper for System.Text.StringBuilder
// ###################################################################
module StringBuilder =
    let create () = StringBuilder()
    let createWithMinLength (length: int) = StringBuilder(length)
    let append (str: string) (stringBuilder: StringBuilder) = stringBuilder.Append(str)
    let appendFormat str (object: obj) (stringBuilder: StringBuilder) = stringBuilder.AppendFormat(str, object)
    let getString (stringBuilder: StringBuilder) = stringBuilder.ToString()


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


// ###################################################################
// #   Agent Utils
// ###################################################################
/// Utility functions for working with MailboxProcessor / Agents
module Agent =
    let newStoppableAgent handler initialState =
        let mailBoxWorker (inbox: MailboxProcessor<_>) =
            let rec loop oldState =
                async {
                    let! message = inbox.Receive()
                    let newState = handler oldState message

                    match newState with
                    | Some state -> return! loop state
                    | None -> return ()
                }

            loop initialState

        MailboxProcessor.Start(mailBoxWorker)

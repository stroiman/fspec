#I @"./packages/FAKE/tools"
#r @"./packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git

type Version = { Major:int; Minor:int; Build:int }

[<AutoOpen>]
module Helpers =
    let parseVersion v =
        let regex = new System.Text.RegularExpressions.Regex("^(\d+).(\d+).(\d+)$");
        let m = regex.Match(v)
        if not (m.Success) then failwith "Invalid version file"
        let getValue (x:int) = m.Groups.Item(x).Value |> System.Int32.Parse
        { Major = getValue 1;
          Minor = getValue 2;
          Build = getValue 3 }

    let versionToString version = 
        sprintf "%d.%d.%d" version.Major version.Minor version.Build

    let versionToCommitMsg version =
        version
        |> versionToString
        |> sprintf "v-%s"
          
let getVersion () = 
    ReadFileAsString "version.txt"
    |> parseVersion

let getCommitMsg () =
    getVersion ()
    |> versionToCommitMsg

let writeVersion version =
    version
    |> versionToString
    |> WriteStringToFile false "version.txt"

Target "IncBuildNo" <| fun _ ->
    let version = getVersion()
    { version with Build = version.Build + 1 }
    |> writeVersion

Target "Commit" <| fun _ ->
    StageAll "."
    let commitMsg = getCommitMsg ()
    Commit "." commitMsg
    sprintf "tag %s" commitMsg
    |> runSimpleGitCommand "." 
    |> trace

// Default target
Target "Default" (fun _ ->
    trace "Hello World from FAKE"
    )

// start build
RunTargetOrDefault "Default"

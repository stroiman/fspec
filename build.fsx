#I @"./packages/FAKE/tools"
#r @"./packages/FAKE/tools/FakeLib.dll"
open System
open Fake
open Fake.Git
open Fake.AssemblyInfoFile

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

Target "Build" <| fun _ ->
    let version = getVersion () |> versionToString
    CreateFSharpAssemblyInfo "./core/AssemblyInfo.fs" [
        Attribute.Title "FSpec"
        Attribute.Version version
        Attribute.FileVersion version
    ]

    let result = ExecProcess (fun info ->
        info.FileName <- "rake"
        info.WorkingDirectory <- ".") (TimeSpan.FromMinutes 5.0)
    if result <> 0 then failwithf "MyProc.exe returned with a non-zero exit code"

Target "CreatePackage" <| fun _ ->
    let version = getVersion() |> versionToString
    ensureDirectory "NuGet"
    NuGet (fun p -> 
        {p with
            Version = version
            WorkingDir = "."
        })
        "fspec.nuspec"
    
Target "IncBuildNo" <| fun _ ->
    let version = getVersion()
    { version with Build = version.Build + 1 }
    |> writeVersion

Target "IncMinorVersion" <| fun _ ->
    let version = getVersion()
    { version with Build = 0; Minor = version.Minor + 1 }
    |> writeVersion

Target "Commit" <| fun _ ->
    StageAll "."
    let commitMsg = getCommitMsg ()
    Commit "." commitMsg
    sprintf "tag %s" commitMsg
    |> runSimpleGitCommand "." 
    |> trace

// Default target
Target "Default" <| fun _ -> ()
Target "CreateBuild" (fun _ -> ())
Target "CreateMinor" (fun _ -> ())

// Dependencies
"IncBuildNo"
    ==> "Build"
    ==> "CreatePackage"
    ==> "Commit"
    ==> "CreateBuild"
    
"IncMinorVersion"
    ==> "Build"
    ==> "CreatePackage"
    ==> "Commit"
    ==> "CreateMinor"
    
"Build" ==> "Default"

// start build
RunTargetOrDefault "Default"

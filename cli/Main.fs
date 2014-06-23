module Main
open System.Reflection
open FSpec
open FSpec.TestDiscovery

[<EntryPoint>]
let main args =
    let options = { TreeReporterOptions.Default with PrintSuccess = false }
    let reporter = TreeReporter.create options
    args
    |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
    |> Seq.mapMany getSpecsFromAssembly
    |> runSpecsWithReporter reporter
    |> toExitCode

module Main
open System.Reflection
open FSpec.Core.TestDiscovery

[<EntryPoint>]
let main args =
    args
    |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
    |> Seq.mapMany getSpecsFromAssembly
    |> runSpecs
    |> toExitCode

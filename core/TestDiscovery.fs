module FSpec.TestDiscovery
open Microsoft.FSharp.Reflection
open System.Reflection
open FSpec.Dsl

module Seq =
    let mapMany f x = seq { for y in x do yield! f y }

let getSpecsFromAssembly (assembly : Assembly) =
    let toExampleGroup (value : obj) =
        let exampleGroupFromOp = function
            | AddExampleGroupOperation g -> Some g
            | _ -> None

        match value with
        | :? Operation as o ->
            exampleGroupFromOp o 
            |> Option.bind (fun x -> Some [x])
        | :? ExampleGroup.T as g -> Some [g]
        | :? List<ExampleGroup.T> as g -> Some g
        | :? List<Operation> as l -> Some (l |> List.choose exampleGroupFromOp)
        | _ -> None
        
    let specs =
        assembly.ExportedTypes
        |> Seq.where (fun x -> FSharpType.IsModule x)
        |> Seq.map (fun x -> x.GetProperty("specs"))
        |> Seq.where (fun x -> x <> null)
        |> Seq.map (fun x -> x.GetValue(null)) 
        |> Seq.choose toExampleGroup 
        |> Seq.mapMany (fun x -> x)
        |> List.ofSeq
    specs

let runSpecsWithRunnerAndReporter runner reporter specs =
    specs
    |> runner reporter
    |> reporter.Success

let runSpecsWithReporter reporter specs =
    let runner = Runner.run
    runSpecsWithRunnerAndReporter runner reporter specs

let runSpecs specs = 
    let reporter = TreeReporter.create TreeReporterOptions.Default
    runSpecsWithReporter reporter specs

let toExitCode result =
    match result with
    | true -> 0
    | false -> 1

let runSingleAssemblyWithConfig config assembly = 
    let runner = Runner.fromConfig config
    let reporter = TreeReporter.create TreeReporterOptions.Default
    assembly 
    |> getSpecsFromAssembly 
    |> runSpecsWithRunnerAndReporter runner reporter
    |> toExitCode

let runSingleAssembly assembly = 
    let config = Configuration.defaultConfig
    runSingleAssemblyWithConfig config assembly

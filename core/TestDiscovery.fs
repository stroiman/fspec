module FSpec.Core.TestDiscovery
open Microsoft.FSharp.Reflection
open System.Reflection
open FSpec.Core.Dsl

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

let runSpecs specs =
    let emptyReport = TreeReporter.Zero
    let reporter = TreeReporter.createReporter
    let report = 
        specs 
        |> Seq.fold (fun rep grp -> Runner.doRun grp reporter rep) emptyReport
        |> reporter.EndTestRun
    report |> reporter.Success

let toExitCode result =
    match result with
    | true -> 0
    | false -> 1

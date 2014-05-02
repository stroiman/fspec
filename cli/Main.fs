module Main
open FSpec.Core
open Dsl
open DomainTypes
open Microsoft.FSharp.Reflection
open System
open System.Reflection

module Seq =
    let mapMany x y =
        seq { for item in y do
                yield! x item
        }

let getSpecsFromAssembly (assembly : Assembly) =
    let toExampleGroup (value : obj) =
        match value with
        | :? ExampleGroup.T as g -> 
            Some g
        | _ -> None
        
    let specs =
        assembly.ExportedTypes
        |> Seq.where (fun x -> FSharpType.IsModule x)
        |> Seq.map (fun x -> x.GetProperty("specs"))
        |> Seq.where (fun x -> x <> null)
        |> Seq.map (fun x -> x.GetValue(null)) 
        |> Seq.choose toExampleGroup 
        |> List.ofSeq
    c.examples::specs

let runSpecs specs =
    let report = TestReport()
    specs |> List.iter (fun grp -> ExampleGroup.run grp report)
    report.failedTests() |> List.iter (fun x -> printfn "%s" x)
    printfn "%s" (report.summary())

[<EntryPoint>]
let main args =
    let specs = 
        args
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.mapMany getSpecsFromAssembly
        |> Seq.toList
    runSpecs specs
    0

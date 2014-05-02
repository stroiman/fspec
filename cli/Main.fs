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
            Some [g]
        | :? List<ExampleGroup.T> as g -> Some g
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
    c.examples::specs

let runSpecs specs =
    let report = specs |> List.fold (fun rep grp -> Runner.run grp rep) (Report.create())
    report.failed 
    |> List.rev
    |> List.iter (fun x -> printfn "%s" x)
    printfn "%s" (report |> Report.summary)
    report |> Report.success

[<EntryPoint>]
let main args =
    let specs = 
        args
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.mapMany getSpecsFromAssembly
        |> Seq.toList
    match runSpecs specs with
    | true -> 0
    | false -> 1

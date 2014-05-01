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

[<EntryPoint>]
let main args =
    let specs = args
                |> Seq.map (fun x -> Assembly.LoadFrom(x))
                |> Seq.mapMany (fun x -> x.ExportedTypes)
                |> Seq.where (fun x -> FSharpType.IsModule x)
                |> Seq.map (fun x -> x.GetProperty("specs"))
                |> Seq.where (fun x -> x <> null)
    let toExampleGroup (value : obj) =
        match value with
        | :? ExampleGroup.T as g -> 
            Some g
        | _ -> None
    let specs' = specs |> Seq.map (fun x -> x.GetValue(null)) |> Seq.choose toExampleGroup |> List.ofSeq

    let report = TestReport()
    let exampleGroups = c.examples::specs'
    exampleGroups |> List.iter (fun grp -> ExampleGroup.run grp report)
    report.failedTests() |> List.iter (fun x -> printfn "%s" x)
    printfn "%s" (report.summary())
    0

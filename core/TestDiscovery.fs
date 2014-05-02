module FSpec.Core.TestDiscovery
open FSpec.Core.Dsl
open Microsoft.FSharp.Reflection
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
    let report = specs |> Seq.fold (fun rep grp -> Runner.run grp rep) (Report.create())
    report.failed 
    |> List.rev
    |> List.iter (fun x -> printfn "%s" x)
    printfn "%s" (report |> Report.summary)
    report 

let toExitCode result =
    match result |> Report.success with
    | true -> 0
    | false -> 1

module Main
open FSpec
open Expectations
open Microsoft.FSharp.Reflection
open System
open System.Reflection

[<EntryPoint>]
let main args =
    let assembly = Assembly.Load("fspec.selftests")
    let specs = assembly.ExportedTypes
                |> Seq.where (fun x -> FSharpType.IsModule x)
                |> Seq.map (fun x -> x.GetProperty("specs"))
                |> Seq.where (fun x -> x <> null)
    specs |> Seq.iter (fun x -> x.GetValue(null) |> ignore)

    let report = TestReport()
    c.run(report)
    report.failedTests() |> List.iter (fun x -> printfn "%s" x)
    printfn "%s" (report.summary())
    0

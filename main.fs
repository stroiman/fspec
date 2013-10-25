module Main
open FSpec
open Expectations
open SelfTests

let report = TestReport()
c.run(report)
report.failedTests() |> List.iter (fun x -> printfn "%s" x)
printfn "%s" (report.summary())

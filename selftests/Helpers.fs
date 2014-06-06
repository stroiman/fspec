module FSpec.SelfTests.Helpers
open FSpec.Core
open ExampleGroup

let stringBuilderPrinter builder =
    fun color msg ->
        Printf.bprintf builder "%s" msg

let tryExecute test =
    try
        test ()
        None
    with
        | AssertionError(info) -> Some(info)

let getErrorMsg test : string =
    match tryExecute test with
    | None -> failwith "Expected test failure"
    | Some(x) -> x.Message
    
module TestReporter =
    type ReportType =
        | BeginGroup of string
        | EndGroup
        | Example of string * TestResultType

    type T = { CallList: ReportType list }
    
    let appendToReport n r = { r with CallList = n::r.CallList }

    let instance = {
        BeginGroup = fun grp -> grp.Name |> BeginGroup |> appendToReport
        ReportExample = fun ex res -> (ex.Name, res) |> Example |> appendToReport
        EndTestRun = fun r -> r
        EndGroup = EndGroup |> appendToReport
        Success = fun _ -> true
        BeginTestRun = fun _ -> { CallList = [] } 
    }

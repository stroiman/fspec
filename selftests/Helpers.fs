module FSpec.SelfTests.Helpers
open FSpec.Core

let stringBuilderPrinter builder =
    fun color msg ->
        Printf.bprintf builder "%s" msg

let tryExecute test =
    try
        test ()
        None
    with
        | AssertionError(info) -> Some(info)

let shouldPass test =
    match tryExecute test with
    | None -> ()
    | Some(x) -> failwithf "Test failed with %A" x

let getErrorMsg test : string =
    match tryExecute test with
    | None -> failwith "Expected test failure"
    | Some(x) -> x.Message
    
let shouldFail test = getErrorMsg test |> ignore

module TestReporter =
    type ReportType =
        | BeginGroup of string
        | EndGroup
        | Example of string * TestResultType

    type T = { CallStack: ReportType list }
    
    let appendToReport n r = { r with CallStack = n::r.CallStack }

    let instance = {
        BeginGroup = fun grp -> grp |> ExampleGroup.name |> BeginGroup |> appendToReport
        ReportExample = fun ex res -> (ex |> Example.name, res) |> Example |> appendToReport
        EndTestRun = fun r -> r
        EndGroup = EndGroup |> appendToReport
        Success = fun _ -> true
        Zero = { CallStack = [] } 
    }

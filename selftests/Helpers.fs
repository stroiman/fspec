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

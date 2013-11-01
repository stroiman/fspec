module FSpec.SelfTests.MatcherV2Specs
open FSpec.Core
open Dsl
open MatchersV2

type A = { X: string }
type B = { Y: string }

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
    
let shouldFail test =
    match tryExecute test with
    | None -> failwith "Expected test failure"
    | Some(x) -> ()

let specs =
    describe "TypeOf matcher" <| fun() ->
        it "succeeds when object is of expected type" <| fun () -> 
            let actual = { X = "dummy" }
            (fun () -> actual |> shouldBeTypeOf<A>)
                |> shouldPass

        it "fails when object is of wrong type" <| fun () ->
            let actual = { X = "dummy" }
            (fun () -> actual |> shouldBeTypeOf<B>)
                |> shouldFail

        it "fails when object is null" <| fun () ->
            (fun () -> null |> shouldBeTypeOf<B>)
                |> shouldFail

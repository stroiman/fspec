module FSpec.SelfTests.MatcherV2Specs
open FSpec.Core
open Dsl
open MatchersV2

type A() =
    let mutable dummy = 0
type A'() =
    inherit A()

type B() =
    let mutable dummy = 0

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
            let actual = A()
            (fun () -> actual |> should beOfType<A>)
                |> shouldPass
        
        it "succeeds when actual is subclass of expected type" <| fun () ->
            (fun () -> A'() |> should beOfType<A>)
                |> shouldPass

        it "fails when actual is superclass of expected type" <| fun () ->
            (fun () -> A() |> should beOfType<A'>)
                |> shouldFail

        it "fails when object is of wrong type" <| fun () ->
            let actual = A()
            (fun () -> actual |> should beOfType<B>)
                |> shouldFail

        it "fails when object is null" <| fun () ->
            (fun () -> null |> should beOfType<B>)
                |> shouldFail

    describe "Not typeof" <| fun () ->
        it "succeeds when object is of different type" <| fun () ->
            (fun () -> A() |> shouldNot beOfType<B>)
                |> shouldPass

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

let getErrorMsg test =
    match tryExecute test with
    | None -> failwith "Expected test failure"
    | Some(x) -> x.Message
    
let shouldFail test =
    match tryExecute test with
    | None -> failwith "Expected test failure"
    | Some(x) -> ()

let specs =
    describe "Equal matcher" <| fun () ->
        describe "when used normally" <| fun () ->
            it "succeeds when objects are equal" <| fun () ->
                let test () = 5 |> should equal 5
                test |> shouldPass

            it "fails when objects are not equal" <| fun () ->
                let test () = 5 |> should equal 6
                test |> shouldFail

            it "fails with the right error message" <| fun () ->
                let test () = 5 |> should equal 6
                test |> getErrorMsg |> should equal "expected 5 to equal 6"

        describe "when used negated" <| fun () ->
            it "succeeds when objects are not equal" <| fun () ->
                let test () = 5 |> shouldNot equal 6
                test |> shouldPass

            it "fails when objects are equal" <| fun () ->
                let test () = 5 |> shouldNot equal 5
                test |> shouldFail

            it "fails with the right error message" <| fun () ->
                let test () = 5 |> shouldNot equal 5
                test |> getErrorMsg |> should equal "expected 5 to not equal 5"
        
    describe "TypeOf matcher" <| fun() ->
        it "succeeds when object is of expected type" <| fun () -> 
            let test () = A() |> should beOfType<A>
            test |> shouldPass
        
        it "succeeds when actual is subclass of expected type" <| fun () ->
            let test () = A'() |> should beOfType<A>
            test |> shouldPass

        it "fails when actual is superclass of expected type" <| fun () ->
            let test () = A() |> should beOfType<A'>
            test |> shouldFail

        it "fails when object is of wrong type" <| fun () ->
            let test () = A() |> should beOfType<B>
            test |> shouldFail

        it "fails when object is null" <| fun () ->
            let test () = null |> should beOfType<B>
            test |> shouldFail

        it "fails with the right error message" pending

    describe "Not typeof" <| fun () ->
        it "succeeds when object is of different type" <| fun () ->
            let test () = A() |> shouldNot beOfType<B>
            test |> shouldPass

        it "fails with the right error message" pending

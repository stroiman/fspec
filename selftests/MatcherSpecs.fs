module FSpec.SelfTests.MatcherSpecs
open FSpec.Core
open Dsl
open Matchers

let specs =
    describe "equals matcher" <| fun _ ->
        it "passes when objects equal" <| fun _ ->
            (5).should equal 5
        it "fails when the objects are not equal" <| fun _ ->
            (fun () -> (5).should equal 6)
                |> should throw ()

    describe "greaterThan matcher" <| fun _ ->
        it "passes when actual is greater than expected" <| fun _ ->
            5 |> should be.greaterThan 4
        it "fails when actual is less than expected" <| fun _ ->
            (fun () -> 5 |> should be.greaterThan 6)
                |> should throw ()
        
        it "fails when actual is equal to expected" <| fun _ ->
            (fun () -> 5 |> should be.greaterThan 5)
                |> should throw ()

    describe "matchRegex matcher" <| fun _ ->
        it "passes when the input matches the pattern" <| fun _ ->
            "Some strange expression" |> should matchRegex "strange"

        it "fails when the input does not match the pattern" <| fun _ ->
            (fun () -> "some value" |> should matchRegex "invalidPattern")
                |> should throw ()
            
    describe "throw matcher" <| fun _ ->
        let thrown = ref false
        let test (x : unit -> unit) =
            try
                x |> should throw ()
            with
                | _ -> thrown := true

        it "passed when an exception is thrown" <| fun _ ->
            (fun () -> failwith "error") |> test
            !thrown |> should equal false
        it "fails when no exception is thrown" <| fun _ ->
            (fun () -> ()) |> test
            !thrown |> should equal true

module FSpec.SelfTests.MatcherSpecs
open FSpec.Core

let specs =
    describe "Assertion helpers" <| fun() ->
        describe "equals" <| fun() ->
            it "passes when objects equal" <| fun() ->
                (5).should equal 5
            it "fails when the objects are not equal" <| fun() ->
                (fun () -> (5).should equal 6)
                    |> should throw ()

        describe "greaterThan" <| fun() ->
            it "passes when actual is greater than expected" <| fun() ->
                5 |> should be.greaterThan 4
            it "fails when actual is less than expected" <| fun() ->
                (fun () -> 5 |> should be.greaterThan 6)
                    |> should throw ()
            
            it "fails when actual is equal to expected" <| fun() ->
                (fun () -> 5 |> should be.greaterThan 5)
                    |> should throw ()

        describe "matches" <| fun() ->
            it "passes when the input matches the pattern" <| fun() ->
                "Some strange expression" |> should matchRegex "strange"

            it "fails when the input does not match the pattern" <| fun() ->
                (fun () -> "some value" |> should matchRegex "invalidPattern")
                    |> should throw ()
                
        describe "throw matcher" <| fun() ->
            let thrown = ref false
            let test (x : unit -> unit) =
                try
                    x |> should throw ()
                with
                    | _ -> thrown := true

            it "passed when an exception is thrown" <| fun () ->
                (fun () -> failwith "error") |> test
                !thrown |> should equal false
            it "fails when no exception is thrown" <| fun() ->
                (fun () -> ()) |> test
                !thrown |> should equal true

module FSpec.SelfTests.MatcherV3Specs
open FSpec.Core
open Dsl
open MatchersV3
open Helpers

let specs = [
    describe "equalTo matcher" [
        describe "should be.EqualTo" [
            it "succeeds when equal" <| fun _ ->
                let test () = 5 |> should (be.equalTo 5)
                test |> shouldPass

            it "fails when not equal" <| fun _ ->
                let test () = 5 |> should (be.equalTo 6)
                test |> getErrorMsg 
                |> should (be.equalTo "Expected 5 to be equal to 6")
        ]

        describe "shouldNot be.equalTo" [
            it "succeeds when not equal" <| fun _ ->
                5 |> shouldNot (be.equalTo 6)

            it "fails when equal" <| fun _ ->
                let test () = 5 |> shouldNot (be.equalTo 5)
                test |> getErrorMsg
                |> should (be.equalTo "Expected 5 to not be equal to 5")
        ]
    ]
    
    describe "Collection matchers" [
        describe "should have.length" [
            it "succeeds when length is expected" <| fun _ ->
                let test () = [1;2;3] |> should (have.length (be.equalTo 3))
                test |> shouldPass
        ]

        describe "should have.atLeastOneElement" [
            it "succeeds when collection has one element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (be.equalTo 3))
                test |> shouldPass

            it "succeeds when collection have multiple matching elements" <| fun _ ->
                let test () = [1;2;2;3] |> should (have.atLeastOneElement (be.equalTo 2))
                test |> shouldPass

            it "fails when collection has no matching element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (be.equalTo 4))
                let expected =  "Expected [1; 2; 3] to contain at least one element to be equal to 4"
                test |> getErrorMsg |> should (be.equalTo expected)
        ]

        describe "shouldNot have.atLeastOneElement" [
            it "fails when collection has matching element" <| fun _ ->
                let test () = [1;2;3] |> shouldNot (have.atLeastOneElement (be.equalTo 2))
                test |> getErrorMsg
                |> should (be.equalTo "Expected [1; 2; 3] to contain no elements to be equal to 2")
        ]
    ]
]

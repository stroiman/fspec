module FSpec.SelfTests.MatcherV3Specs
open FSpec.Core
open Dsl
open MatchersV3
open Helpers

let specs = [
    describe "System.Object extesions" [
        describe ".Should" [
            it "works correctly on objects of type 'obj'" (fun _ ->
                (1).Should (equal 1)
            )
        ]

        describe ".ShouldNot" [
            it "works correctly on objects of type 'obj'" (fun _ ->
                let test = fun _ -> (1).ShouldNot (equal 1)
                test |> shouldFail
            )
        ]
    ]

    describe "equal matcher" [
        describe "should be.EqualTo" [
            it "succeeds when equal" <| fun _ ->
                let test () = 5 |> should (equal 5)
                test |> shouldPass

            it "fails when not equal" <| fun _ ->
                let test () = 5 |> should (equal 6)
                test |> getErrorMsg 
                |> should (be.string.matching "5 was expected to equal 6")
        ]

        describe "shouldNot equal" [
            it "succeeds when not equal" <| fun _ ->
                5 |> shouldNot (equal 6)

            it "fails when equal" <| fun _ ->
                let test () = 5 |> shouldNot (equal 5)
                test |> getErrorMsg
                |> should (be.string.matching "5 was expected to not equal 5")
        ]
    ]

    describe "Regex matcher" [
        it "succeeds when regular expression is a match" <| fun _ ->
            let test () = "abcd" |> should (be.string.matching "bcd")
            test |> shouldPass
    ]

    describe "True/False matchers" [
        describe "should be.True" [
            it "succeeds for true values" <| fun _ ->
                let test () = true |> should be.True
                test |> shouldPass

            it "fail for false values" <| fun _ ->
                let test () = false |> should be.True
                test |> shouldFail
        ]
        describe "should be.False" [
            it "fail for true values" <| fun _ ->
                let test () = true |> should be.False
                test |> shouldFail

            it "succeed for false values" <| fun _ ->
                let test () = false |> should be.False
                test |> shouldPass
        ]
    ]
    
    describe "Collection matchers" [
        describe "should have.length" [
            it "succeeds when length is expected" <| fun _ ->
                let test () = [1;2;3] |> should (have.length (equal 3))
                test |> shouldPass
        ]

        describe "should have.exactly" [
            it "succeeds when correct no of elements match" <| fun _ ->
                let test () = [1;2;2;3] |> should (have.exactly 2 (equal 2))
                test |> shouldPass
        ]

        describe "should have.atLeastOneElement" [
            it "succeeds when collection has one element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (equal 3))
                test |> shouldPass

            it "succeeds when collection have multiple matching elements" <| fun _ ->
                let test () = [1;2;2;3] |> should (have.atLeastOneElement (equal 2))
                test |> shouldPass

            it "fails when collection has no matching element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (equal 4))
                let expected =  "[1; 2; 3] was expected to contain at least one element to equal 4"
                test |> getErrorMsg |> should (be.string.containing expected)
        ]

        describe "shouldNot have.atLeastOneElement" [
            it "fails when collection has matching element" <| fun _ ->
                let test () = [1;2;3] |> shouldNot (have.atLeastOneElement (equal 2))
                test |> getErrorMsg
                |> should (be.string.containing "[1; 2; 3] was expected to contain no elements to equal 2")
        ]
    ]

    describe "exception matchers" [
        describe "failWithMsg" [
            it "succeeds when exception thrown with specified message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "custom msg")
                test |> shouldPass

            it "fails when no exception is thrown" <| fun _ ->
                let f () = ()
                let test () = f |> should (throwException.withMessageContaining "dummy")
                test |> shouldFail

            it "fails when exception thrown with different message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test |> shouldFail

            it "displays the actual exception message in the assertion failure message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test |> getErrorMsg
                |> should (be.string.containing "custom msg")

            it "should display 'was expected to throw exception'" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test |> getErrorMsg
                |> should (be.string.containing "was expected to throw exception")
        ]
    ]
]

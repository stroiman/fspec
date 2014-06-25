module FSpec.SelfTests.MatcherSpecs
open FSpec
open Dsl
open Matchers
open CustomMatchers

let specs = [
    describe "System.Object extesions" [
        describe ".Should" [
            it "works correctly on objects of type 'obj'" (fun _ ->
                (1).Should (equal 1)
            )
        ]

        describe ".ShouldNot" [
            it "works correctly on objects of type 'obj'" (fun _ ->
                let test = fun () -> (1).ShouldNot (equal 1)
                test.Should fail
            )
        ]
    ]

    describe "fail/succeed matcher" [
        let shouldPass test =
            test ()

        let shouldFail test = 
            try
                test ()
                failwith "Error expected"
            with
                | _ -> ()

        yield describe "fail matcher" [
            it "should pass when function throws exception" <| fun _ ->
                let failingFunc () = failwith "dummy"; ()
                let test () = failingFunc |> should fail
                test |> shouldPass

            it "should fail when function does not throw" <| fun _ ->
                let passingFunc () = ()
                let test () = passingFunc |> should fail
                test |> shouldFail
        ]

        yield describe "succeed matcher" [
            it "should fail when function throws exception" <| fun _ ->
                let failingFunc () = failwith "dummy"; ()
                let test () = failingFunc |> should succeed
                test |> shouldFail

            it "should pass when function does not throw" <| fun _ ->
                let passingFunc () = ()
                let test () = passingFunc |> should succeed
                test |> shouldPass
        ]
    ]

    describe "equal matcher" [
        describe "should be.EqualTo" [
            it "succeeds when equal" <| fun _ ->
                let test () = 5 |> should (equal 5)
                test.Should succeed

            it "fails when not equal" <| fun _ ->
                let test () = 5 |> should (equal 6)
                test.Should (failWithAssertionError "5 was expected to equal 6")
        ]

        describe "shouldNot equal" [
            it "succeeds when not equal" <| fun _ ->
                5 |> shouldNot (equal 6)

            it "fails when equal" <| fun _ ->
                let test () = 5 |> shouldNot (equal 5)
                test.Should (failWithAssertionError "5 was expected to not equal 5")
        ]
    ]

    describe "greaterThan matcher" [
        it "succeeds when value is greater than expected" <| fun _ ->
            let test () = 5 |> should (be.greaterThan 4)
            test.Should succeed

        it "fails when value is equal to expected" <| fun _ ->
            let test () = 5 |> should (be.greaterThan 5)
            test.Should fail
    ]

    describe "Regex matcher" [
        it "succeeds when regular expression is a match" <| fun _ ->
            let test () = "abcd" |> should (be.string.matching "bcd")
            test.Should succeed
    ]

    describe "True/False matchers" [
        describe "should be.True" [
            it "succeeds for true values" <| fun _ ->
                let test () = true |> should be.True
                test.Should succeed

            it "fail for false values" <| fun _ ->
                let test () = false |> should be.True
                test.Should fail
        ]
        describe "should be.False" [
            it "fail for true values" <| fun _ ->
                let test () = true |> should be.False
                test.Should fail

            it "succeed for false values" <| fun _ ->
                let test () = false |> should be.False
                test.Should succeed
        ]
    ]
    
    describe "Collection matchers" [
        describe "should have.length" [
            it "succeeds when length is expected" <| fun _ ->
                let test () = [1;2;3] |> should (have.length (equal 3))
                test.Should succeed
        ]

        describe "should have.exactly" [
            it "succeeds when correct no of elements match" <| fun _ ->
                let test () = [1;2;2;3] |> should (have.exactly 2 (equal 2))
                test.Should succeed
        ]

        describe "should have.atLeastOneElement" [
            it "succeeds when collection has one element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (equal 3))
                test.Should succeed

            it "succeeds when collection have multiple matching elements" <| fun _ ->
                let test () = [1;2;2;3] |> should (have.atLeastOneElement (equal 2))
                test.Should succeed

            it "fails when collection has no matching element" <| fun _ ->
                let test () = [1;2;3] |> should (have.atLeastOneElement (equal 4))
                let expected =  "[1; 2; 3] was expected to contain at least one element to equal 4"
                test.Should (failWithAssertionError expected)
        ]

        describe "shouldNot have.atLeastOneElement" [
            it "fails when collection has matching element" <| fun _ ->
                let test () = [1;2;3] |> shouldNot (have.atLeastOneElement (equal 2))
                test.Should (failWithAssertionError "[1; 2; 3] was expected to contain no elements to equal 2")
        ]
    ]

    describe "exception matchers" [
        describe "failWithMsg" [
            it "succeeds when exception thrown with specified message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "custom msg")
                test.Should succeed

            it "fails when no exception is thrown" <| fun _ ->
                let f () = ()
                let test () = f |> should (throwException.withMessageContaining "dummy")
                test.Should fail

            it "fails when exception thrown with different message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test.Should fail

            it "displays the actual exception message in the assertion failure message" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test.Should (failWithAssertionError "custom msg")

            it "should display 'was expected to throw exception'" <| fun _ ->
                let f () = raise (new System.Exception("custom msg"))
                let test () = f |> should (throwException.withMessageContaining "wrong msg")
                test.Should (failWithAssertionError "was expected to throw exception")
        ]
    ]
]

module FSpec.SelfTests.MatcherSpecs
open FSpec
open Dsl
open Matchers
open CustomMatchers

type MatchOf<'T,'U> =
    static member createMatcherTest (ctx:TestContext) (f: Matcher<'T,'U> -> 'T -> unit) =
        let actual = ctx.MetaData.Get<'T> "actual"
        let matcher : Matcher<'T,'U> = ctx?matcher
        fun () -> actual |> f matcher
 
    static member ShouldPass =
        examples [
            it "succeeds when used with 'should'" (fun ctx ->
                let test = MatchOf<'T,'U>.createMatcherTest ctx should
                test.Should succeed)
            
            it "fails when used with 'shouldNot'" (fun ctx ->
                let test = MatchOf<'T,'U>.createMatcherTest ctx shouldNot
                test.Should fail)
        ]

    static member ShouldFail =
        examples [
            it "succeeds when used with 'shouldNot'" (fun ctx ->
                let test = MatchOf<'T,'U>.createMatcherTest ctx shouldNot
                test.Should succeed)

            it "fails when used with 'should'" (fun ctx ->
                let test = MatchOf<'T,'U>.createMatcherTest ctx should
                test.Should fail)
        ]

    static member ShouldFailWith message =
        it (sprintf "fails with message '%s' when used with 'should'" message) (fun ctx ->
            let test = MatchOf<'T,'U>.createMatcherTest ctx should
            test.Should (failWithAssertionError message))

type MatchOf<'T> = MatchOf<'T,obj>

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

    describe "matchers" [
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

        ("matcher", be.equalTo 42) **>
        describe "be.equalTo matcher" [
            ("actual", 43) **>
            context "when values are not equal" [
                MatchOf<int>.ShouldFail
            ]

            ("actual", 42) **>
            context "when values are equal" [
                MatchOf<int>.ShouldPass
            ]
        ]

        ("matcher", be.lessThan 42) **>
        describe "be.lessThan matcher" [
            ("actual", 41) **>
            context "when actual is less than expected" [
                MatchOf<int>.ShouldPass
            ]
            
            ("actual", 42) **>
            context "when actual is equal to expected" [
                MatchOf<int>.ShouldFail
            ]

            ("actual", 43) **>
            context "when actual is equal to expected" [
                MatchOf<int>.ShouldFail
            ]
        ]

        ("matcher", be.lessThan 50 &&& be.greaterThan 40) **>
        describe "be.lessThan 50 &&& be.greaterThan 40" [
            ("actual", 40) **>
            context "when actual is 40" [
                MatchOf<int,int>.ShouldFail
            ]
            ("actual", 45) **>
            context "when actual is 45" [
                MatchOf<int,int>.ShouldPass
            ]
            ("actual", 50) **>
            context "when actual is 50" [
                MatchOf<int,int>.ShouldFail
            ]
        ]

        ("matcher", be.greaterThan 42) **>
        describe "be.greaterThan matcher" [
            ("actual", 41) **>
            context "when actual is less than expected" [
                MatchOf<int>.ShouldFail
            ]
            
            ("actual", 42) **>
            context "when actual is equal to expected" [
                MatchOf<int>.ShouldFail
            ]

            ("actual", 43) **>
            context "when actual is equal to expected" [
                MatchOf<int>.ShouldPass
            ]
        ]

        ("matcher", be.string.matching "^ab*c$") **>
        describe "be.string.matching" [
            ("actual", "abbbc") **>
            context "when actual is a string matching the pattern" [
                MatchOf<string>.ShouldPass
            ]
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

        ("matcher", be.ofType<int>() ) **>
        describe "be.ofType<int> matcher" [
            ("actual", 42) **>
            context "when value is an int" [
                MatchOf<obj,int>.ShouldPass
            ]

            ("actual", "foo") **>
            context "when value is a string" [
                MatchOf<obj,int>.ShouldFail

                MatchOf<obj,int>.ShouldFailWith "be of type Int32 but was System.String"
            ]
        ]

        ("matcher", (be.ofType<int>()) >>> (be.equalTo 42)) **>
        describe "Matcher combinators" [
            ("actual", 42) **>
            context "when value matches both matchers" [
                MatchOf<obj>.ShouldPass
            ]

            ("actual", 43) **>
            context "when value matches first, but fails last matcher" [
                MatchOf<obj>.ShouldFail
            ]

            ("actual", "foo") **>
            context "when value fails first matcher" [
                MatchOf<obj>.ShouldFail
            ]
        ]
    ]
]

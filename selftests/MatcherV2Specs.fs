module FSpec.SelfTests.MatcherV2Specs
open FSpec.Core
open DslV2
open MatchersV2

type A() = class end
type B() = class end
type A'() = inherit A()

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
    
let shouldFail test = getErrorMsg test |> ignore

let specs = [
    describe "Equal matcher" [
        context "when used with 'should'" [
            it "succeeds when objects are equal" <| fun _ ->
                let test () = 5 |> should equal 5
                test |> shouldPass

            it "fails when objects are not equal" <| fun _ ->
                let test () = 5 |> should equal 6
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let test () = 5 |> should equal 6
                test |> getErrorMsg |> should equal "expected 5 to equal 6"
        ]

        context "when used with 'should not'" [
            it "succeeds when objects are not equal" <| fun _ ->
                let test () = 5 |> shouldNot equal 6
                test |> shouldPass

            it "fails when objects are equal" <| fun _ ->
                let test () = 5 |> shouldNot equal 5
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let test () = 5 |> shouldNot equal 5
                test |> getErrorMsg |> should equal "expected 5 to not equal 5"
        ]
    ]

    describe "Regex matcher" [
        context "when used with 'should'" [
            it "succeeds when the value match the pattern" <| fun _ ->
                let test () = "blah blah" |> should matchRegex "^blah blah$"
                test |> shouldPass

            it "fails when the value does not match the pattern" <| fun _ ->
                let test () = "blah blah" |> should matchRegex "^blah$"
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let test () = "blah blah" |> should matchRegex "^blah$"
                test |> getErrorMsg |> should equal "expected \"blah blah\" to match regex pattern \"^blah$\""
        ]
        
        context "when used with 'should not'" [
            it "succeeds when the value does not match the pattern" <| fun _ ->
                let test () = "blah blah" |> shouldNot matchRegex "^blah$"
                test |> shouldPass

            it "fails when the value does match the pattern" <| fun _ ->
                let test () = "blah blah" |> shouldNot matchRegex "^blah blah$"
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let test () = "blah blah" |> shouldNot matchRegex "^blah blah$"
                test |> getErrorMsg |> should equal "expected \"blah blah\" to not match regex pattern \"^blah blah$\""
        ]
    ]

    describe "TypeOf matcher" [
        context "when used with 'should'" [
            it "succeeds when object is of expected type" <| fun _ -> 
                let test () = A() |> should beOfType<A>
                test |> shouldPass
            
            it "succeeds when actual is subclass of expected type" <| fun _ ->
                let test () = A'() |> should beOfType<A>
                test |> shouldPass

            it "fails when actual is superclass of expected type" <| fun _ ->
                let test () = A() |> should beOfType<A'>
                test |> shouldFail

            it "fails when object is of wrong type" <| fun _ ->
                let test () = A() |> should beOfType<B>
                test |> shouldFail

            it "fails when object is null" <| fun _ ->
                let test () = null |> should beOfType<B>
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let test () = A() |> should beOfType<B>
                test |> getErrorMsg |> should matchRegex "^expected .*A to be of type .*B$"
        ]

        context "when used with 'should not'" [
            it "succeeds when object is of different type" <| fun _ ->
                let test () = A() |> shouldNot beOfType<B>
                test |> shouldPass

            it "fails with the right error message" <| fun _ ->
                let test () = A() |> shouldNot beOfType<A>
                test |> getErrorMsg |> should matchRegex "^expected .*A to not be of type .*A$"
        ]
    ]

    describe "Fail matcher" [
        context "when used with 'should'" [
            it "succeeds when function fails" <| fun _ ->
                let test () = failwith "dummy"
                test |> should fail

            it "fails when the function doesn't fail" <| fun _ ->
                let nonFailingFunction () = ()
                let test () = nonFailingFunction |> should fail
                test |> shouldFail

            it "fails with the right error message" <| fun _ ->
                let nonFailingFunction () = ()
                let test () = nonFailingFunction |> should fail
                test |> getErrorMsg |> should equal "expected exception to be thrown, but none was thrown"
        ]

        context "when used with 'should not'" [
            it "fails with the right error message" <| fun _ ->
                let failingFunction () = failwith "dummy"
                let test () = failingFunction |> shouldNot fail
                test |> getErrorMsg |> should equal "exception was thrown when none was expected"
        ]
    ]
]

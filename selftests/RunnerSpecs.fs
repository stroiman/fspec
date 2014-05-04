module FSpec.SelfTests.RunnerSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper
open ExampleHelper

let callList = ref []
let addToCallList x = callList := x::!callList
let actualCallList () = !callList |> List.rev
let clearCallList _ = callList := []

let recordFunctionCall name = fun _ -> addToCallList name

let specs =
    describe "Test runner" <| fun _ ->
        before <| clearCallList

        describe "test execution order" <| fun _ ->
            it "tests execute in the order they appear" (fun _ ->
                anExampleGroup
                |> withExampleCode (recordFunctionCall "test 1")
                |> withExampleCode (recordFunctionCall "test 2")
                |> run |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]
            )

            it "child contexts execute in the order they appear" (fun _ ->
                anExampleGroup
                |> withChildGroup (
                    anExampleGroup
                    |> withExampleCode (recordFunctionCall "test 1"))
                |> withChildGroup (
                    anExampleGroup
                    |> withExampleCode (recordFunctionCall "test 2"))
                |> run |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]
            )

        describe "General setup/test/teardown handling" <| fun _ ->
            it "runs the sequence before, spec, teardown" <| fun _ ->
                anExampleGroup
                |> withSetupCode (recordFunctionCall "setup")
                |> withTearDownCode (recordFunctionCall "tearDown")
                |> withExampleCode (recordFunctionCall "test")
                |> run |> ignore
                actualCallList() |> should equal ["setup"; "test"; "tearDown"]

            it "runs outer setup before inner setup" <| fun _ ->
                anExampleGroup
                |> withSetupCode (recordFunctionCall "outer setup")
                |> withChildGroup (
                    anExampleGroup
                    |> withSetupCode (recordFunctionCall "inner setup")
                    |> withExampleCode (recordFunctionCall "test"))
                |> run |> ignore
                let expected = [ "outer setup"; "inner setup"; "test" ]
                actualCallList() |> should equal expected

            it "runs inner tear down before outer tear down" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (recordFunctionCall "outer tear down")
                |> withChildGroup (
                    anExampleGroup
                    |> withTearDownCode (recordFunctionCall "inner tear down")
                    |> withExampleCode (recordFunctionCall "test"))
                |> run |> ignore
                let expected = ["test"; "inner tear down"; "outer tear down"]
                actualCallList() |> should equal expected

            it "runs setup/teardown once for each test" <| fun _ ->
                anExampleGroup
                |> withSetupCode (recordFunctionCall "setup")
                |> withTearDownCode (recordFunctionCall "tear down")
                |> withExampleCode (recordFunctionCall "test 1")
                |> withExampleCode (recordFunctionCall "test 2")
                |> run |> ignore
                let expected = [
                    "setup"; "test 1"; "tear down";
                    "setup"; "test 2"; "tear down"]
                actualCallList() |> should equal expected
            
        describe "setup" <| fun _ ->
            it "is only run in same context, or nested context" <| fun _ ->
                anExampleGroup
                |> withSetupCode (recordFunctionCall "outer setup")
                |> withExampleCode (recordFunctionCall "outer test")
                |> withChildGroup (
                    anExampleGroup
                    |> withSetupCode (recordFunctionCall "inner setup")
                    |> withExampleCode (recordFunctionCall "inner test 1")
                    |> withExampleCode (recordFunctionCall "inner test 2"))
                |> run |> ignore
                actualCallList() |> should equal
                    ["outer setup"; "outer test";
                     "outer setup"; "inner setup"; "inner test 1";
                     "outer setup"; "inner setup"; "inner test 2"]

        describe "tear down" <| fun _ ->
            it "runs if test fail" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (fun _ -> addToCallList "tearDown")
                |> withExamples [ aFailingExample ]
                |> run |> ignore
                actualCallList() |> should equal ["tearDown"]

            it "runs teardown in the right context" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (fun _ -> addToCallList "outer tearDown")
                |> withExampleCode (fun _ -> addToCallList "outer test")
                |> withChildGroup (
                    anExampleGroup
                        |> withTearDownCode (recordFunctionCall "inner tearDown")
                        |> withExampleCode (recordFunctionCall "inner test"))
                |> run |> ignore
                actualCallList() |> should equal 
                    ["outer test"; "outer tearDown";
                     "inner test"; "inner tearDown"; "outer tearDown"]


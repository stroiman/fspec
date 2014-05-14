module FSpec.SelfTests.RunnerSpecs
open FSpec.Core
open Dsl
open Matchers
open ExampleHelper

let callList = ref []
let addToCallList x = callList := x::!callList
let actualCallList () = !callList |> List.rev
let clearCallList _ = callList := []

let recordFunctionCall name = fun _ -> addToCallList name

let specs =
    describe "Test runner" [
        before <| clearCallList

        context "test execution order" [
            it "tests execute in the order they appear" <| fun _ ->
                anExampleGroup
                |> withExampleCode (recordFunctionCall "test 1")
                |> withExampleCode (recordFunctionCall "test 2")
                |> run |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]

            it "child contexts execute in the order they appear" (fun _ ->
                anExampleGroup
                |> withNestedGroup (
                    withExampleCode (recordFunctionCall "test 1"))
                |> withNestedGroup (
                    withExampleCode (recordFunctionCall "test 2"))
                |> run |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]
            )
        ]

        context "General setup/test/teardown handling" [
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
                |> withNestedGroup (
                    withSetupCode (recordFunctionCall "inner setup")
                    >> withExampleCode (recordFunctionCall "test"))
                |> run |> ignore
                let expected = [ "outer setup"; "inner setup"; "test" ]
                actualCallList() |> should equal expected

            it "runs inner tear down before outer tear down" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (recordFunctionCall "outer tear down")
                |> withNestedGroup (
                    withTearDownCode (recordFunctionCall "inner tear down")
                    >> withExampleCode (recordFunctionCall "test"))
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
        ]
            
        context "setup" [
            it "is only run in same context, or nested context" <| fun _ ->
                anExampleGroup
                |> withSetupCode (recordFunctionCall "outer setup")
                |> withExampleCode (recordFunctionCall "outer test")
                |> withNestedGroup (
                    withSetupCode (recordFunctionCall "inner setup")
                    >> withExampleCode (recordFunctionCall "inner test 1")
                    >> withExampleCode (recordFunctionCall "inner test 2"))
                |> run |> ignore
                actualCallList() |> should equal
                    ["outer setup"; "outer test";
                     "outer setup"; "inner setup"; "inner test 1";
                     "outer setup"; "inner setup"; "inner test 2"]
        ]

        context "tear down" [
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
                |> withNestedGroup (
                    withTearDownCode (recordFunctionCall "inner tearDown")
                    >> withExampleCode (recordFunctionCall "inner test"))
                |> run |> ignore
                actualCallList() |> should equal 
                    ["outer test"; "outer tearDown";
                     "inner test"; "inner tearDown"; "outer tearDown"]
        ]

        describe "context cleanup" [
            context "context contains an IDisposable instance" [
                it "is disposed after test run" <| fun _ ->
                    let disposed = ref false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = disposed := true }
                    anExampleGroup
                    |> withExampleCode (fun c -> c?dummy <- disposable)
                    |> run |> ignore
                    !disposed |> should equal true

                it "is disposed if test fails" <| fun ctx ->
                    let disposed = ref false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = disposed := true }
                    anExampleGroup
                    |> withSetupCode (fun c -> c?dummy <- disposable)
                    |> withExampleCode (fun _ -> failwith "dummy")
                    |> run |> ignore
                    !disposed |> should equal true

                it "is disposed if teardown fails" <| fun ctx ->
                    let disposed = ref false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = disposed := true }
                    anExampleGroup
                    |> withSetupCode (fun c -> c?dummy <- disposable)
                    |> withTearDownCode (fun _ -> failwith "dummy")
                    |> withAnExample
                    |> run |> ignore
                    !disposed |> should equal true

                it "is not disposed in teardown code" <| fun ctx ->
                    let disposed = ref false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = disposed := true }
                    anExampleGroup
                    |> withSetupCode (fun c -> c?dummy <- disposable)
                    |> withTearDownCode (fun _ -> ctx?disposedDuringTearDown <- !disposed)
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposedDuringTearDown |> should equal false
            ]
        ]
    ]

module FSpec.SelfTests.RunnerSpecs
open FSpec.Core
open Dsl
open Matchers
open ExampleHelper

let callList = ref []
let actualCallList () = !callList |> List.rev
let clearCallList _ = callList := []

let record name = 
    fun _ -> callList := name::!callList

let shouldRecord expected grp =
    grp |> run |> ignore
    actualCallList() |> should equal expected

let specs =
    describe "Test runner" [
        before <| clearCallList

        context "test execution order" [
            it "tests execute in the order they appear" <| fun _ ->
                anExampleGroup
                |> withExampleCode (record "test 1")
                |> withExampleCode (record "test 2")
                |> shouldRecord ["test 1"; "test 2"]

            it "child contexts execute in the order they appear" (fun _ ->
                anExampleGroup
                |> withNestedGroup (
                    withExampleCode (record "test 1"))
                |> withNestedGroup (
                    withExampleCode (record "test 2"))
                |> shouldRecord ["test 1"; "test 2"]
            )
        ]

        context "General setup/test/teardown handling" [
            it "runs the sequence before, spec, teardown" <| fun _ ->
                anExampleGroup
                |> withSetupCode (record "setup")
                |> withTearDownCode (record "tearDown")
                |> withExampleCode (record "test")
                |> shouldRecord ["setup"; "test"; "tearDown"]

            it "runs outer setup before inner setup" <| fun _ ->
                anExampleGroup
                |> withSetupCode (record "outer setup")
                |> withNestedGroup (
                    withSetupCode (record "inner setup")
                    >> withExampleCode (record "test"))
                |> shouldRecord [ "outer setup"; "inner setup"; "test" ]

            it "runs inner tear down before outer tear down" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (record "outer tear down")
                |> withNestedGroup (
                    withTearDownCode (record "inner tear down")
                    >> withExampleCode (record "test"))
                |> shouldRecord ["test"; "inner tear down"; "outer tear down"]

            it "runs setup/teardown once for each test" <| fun _ ->
                anExampleGroup
                |> withSetupCode (record "setup")
                |> withTearDownCode (record "tear down")
                |> withExampleCode (record "test 1")
                |> withExampleCode (record "test 2")
                |> shouldRecord
                    [ "setup"; "test 1"; "tear down";
                      "setup"; "test 2"; "tear down"]
        ]
            
        context "setup" [
            it "is only run in same context, or nested context" <| fun _ ->
                anExampleGroup
                |> withSetupCode (record "outer setup")
                |> withExampleCode (record "outer test")
                |> withNestedGroup (
                    withSetupCode (record "inner setup")
                    >> withExampleCode (record "inner test 1")
                    >> withExampleCode (record "inner test 2"))
                |> shouldRecord
                    ["outer setup"; "outer test";
                     "outer setup"; "inner setup"; "inner test 1";
                     "outer setup"; "inner setup"; "inner test 2"]

            it "is executed in the order they appear" <| fun _ ->
                anExampleGroup
                |> withSetupCode (record "setup 1")
                |> withSetupCode (record "setup 2")
                |> withAnExample
                |> shouldRecord ["setup 1";"setup 2"]
        ]

        context "tear down" [
            it "runs if test fail" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (record "tearDown")
                |> withExamples [ aFailingExample ]
                |> shouldRecord ["tearDown"]

            it "runs teardown in the right context" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (record "outer tearDown")
                |> withExampleCode (record "outer test")
                |> withNestedGroup (
                    withTearDownCode (record "inner tearDown")
                    >> withExampleCode (record "inner test"))
                |> shouldRecord
                    ["outer test"; "outer tearDown";
                     "inner test"; "inner tearDown"; "outer tearDown"]

            it "runs in the order it appears" <| fun _ ->
                anExampleGroup
                |> withTearDownCode (record "teardown 1")
                |> withTearDownCode (record "teardown 2")
                |> withAnExample
                |> shouldRecord ["teardown 1";"teardown 2"]
        ]

        describe "context cleanup" [
            context "setup code initializes an IDisposable" [
                subject <| fun ctx ->
                    ctx?disposed <- false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = ctx?disposed <- true }

                    anExampleGroup
                    |> withSetupCode (fun c -> c?dummy <- disposable)

                it "is disposed after test run" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposed |> should equal true

                it "is disposed if test fails" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withExampleCode (fun _ -> failwith "dummy")
                    |> run |> ignore
                    ctx?disposed |> should equal true

                it "is disposed if teardown fails" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withTearDownCode (fun _ -> failwith "dummy")
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposed |> should equal true

                it "is not disposed in teardown code" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withTearDownCode (fun _ -> 
                        let disposed : bool = ctx?disposed
                        ctx?disposedDuringTearDown <- disposed)
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposedDuringTearDown |> should equal false
            ]
        ]
    ]

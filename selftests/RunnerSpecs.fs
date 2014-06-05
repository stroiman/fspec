module FSpec.SelfTests.RunnerSpecs
open FSpec.Core
open Dsl
open MatchersV3
open ExampleHelper
open ExampleGroup

let callList = ref []
let actualCallList () = !callList |> List.rev
let clearCallList _ = callList := []

let record name = 
    fun _ -> callList := name::!callList

let shouldRecord expected grp =
    grp |> run |> ignore
    actualCallList() |> should (equal expected)

let specs =
    describe "Test runner" [
        before <| clearCallList
        describe "execution order" [

            describe "of examples" [
                it "executes examples in the order they appear" <| fun _ ->
                    anExampleGroup
                    |> withExampleCode (record "test 1")
                    |> withExampleCode (record "test 2")
                    |> shouldRecord ["test 1"; "test 2"]

                it "executes child groups in the order they appear" (fun _ ->
                    anExampleGroup
                    |> withNestedGroup (
                        withExampleCode (record "test 1"))
                    |> withNestedGroup (
                        withExampleCode (record "test 2"))
                    |> shouldRecord ["test 1"; "test 2"]
                )
            ]

            describe "of setup and teardown" [
                it "runs setup/teardown once for each test" <| fun _ ->
                    anExampleGroup
                    |> withSetupCode (record "setup")
                    |> withTearDownCode (record "tear down")
                    |> withExampleCode (record "test 1")
                    |> withExampleCode (record "test 2")
                    |> shouldRecord
                        [ "setup"; "test 1"; "tear down";
                          "setup"; "test 2"; "tear down"]
                    
                describe "setup" [
                    it "is executed before the example" <| fun _ ->
                        anExampleGroup
                        |> withSetupCode (record "setup")
                        |> withExampleCode (record "test")
                        |> shouldRecord ["setup"; "test"]

                    it "is executed for example in child group" <| fun _ ->
                        anExampleGroup
                        |> withSetupCode (record "outer setup")
                        |> withNestedGroup (
                            withSetupCode (record "inner setup")
                            >> withExampleCode (record "test"))
                        |> shouldRecord [ "outer setup"; "inner setup"; "test" ]

                    it "is not executed for subling group examples" <| fun _ ->
                        anExampleGroup
                        |> withNestedGroup (
                            withSetupCode (record "setup"))
                        |> withNestedGroup (
                            withExampleCode (record "sibling test"))
                        |> shouldRecord ["sibling test"]

                    it "is executed in the order they appear" <| fun _ ->
                        anExampleGroup
                        |> withSetupCode (record "setup 1")
                        |> withSetupCode (record "setup 2")
                        |> withAnExample
                        |> shouldRecord ["setup 1";"setup 2"]
                ]

                describe "tear down" [
                    it "is executed after the example" <| fun _ ->
                        anExampleGroup
                        |> withTearDownCode (record "tearDown")
                        |> withExampleCode (record "test")
                        |> shouldRecord ["test"; "tearDown"]

                    it "is executed if example fails" <| fun _ ->
                        anExampleGroup
                        |> withTearDownCode (record "tearDown")
                        |> withExamples [ aFailingExample ]
                        |> shouldRecord ["tearDown"]

                    it "is executed for example in child group" <| fun _ ->
                        anExampleGroup
                        |> withTearDownCode (record "outer tear down")
                        |> withNestedGroup (
                            withTearDownCode (record "inner tear down")
                            >> withExampleCode (record "test"))
                        |> shouldRecord ["test"; "inner tear down"; "outer tear down"]

                    it "is not executed for sibling group examples" <| fun _ ->
                        anExampleGroup
                        |> withNestedGroup (
                            withTearDownCode (record "tearDown"))
                        |> withNestedGroup (
                            withExampleCode (record "sibling test"))
                        |> shouldRecord ["sibling test"]
                    
                    it "runs in the order it appears" <| fun _ ->
                        anExampleGroup
                        |> withTearDownCode (record "teardown 1")
                        |> withTearDownCode (record "teardown 2")
                        |> withAnExample
                        |> shouldRecord ["teardown 1";"teardown 2"]
                ]
            ]
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
                    ctx?disposed |> should be.True

                it "is disposed if test fails" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withExampleCode (fun _ -> failwith "dummy")
                    |> run |> ignore
                    ctx?disposed |> should be.True

                it "is disposed if teardown fails" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withTearDownCode (fun _ -> failwith "dummy")
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposed |> should be.True

                it "is not disposed in teardown code" <| fun ctx ->
                    ctx |> TestContext.getSubject
                    |> withTearDownCode (fun _ -> 
                        let disposed : bool = ctx?disposed
                        ctx?disposedDuringTearDown <- disposed)
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposedDuringTearDown |> should be.False
            ]
        ]
        
        describe "example filtering" [
            context "with default configuration" [
                it "runs examples with the 'focus' metadata" pending
                it "runs example groups with the 'focus' metadata" pending
            ]

            context "with a custom function" [
                let itRunsTheSelectedExample = 
                    context "get executed examples" [
                        subject <| fun ctx ->
                            let runner =
                                { Configuration.defaultConfig with
                                    Include = ctx.MetaData?filter_func }
                                |> Runner.fromConfig
                            [ctx |> TestContext.getSubject]
                            |> runner Helpers.TestReporter.instance
                            |> ignore
                            actualCallList()

                        ("filter_func" ++ (fun (x:Example.T) -> x.Name = "ex1")) ==>
                        context "when example 1 is included" [
                            it "does not run example 2" <| fun ctx ->
                                ctx.Subject.Should (equal ["ex1"])
                        ]

                        ("filter_func" ++ (fun (x:Example.T) -> x.Name = "ex2")) ==>
                        context "when example 2 is included" [
                            it "does not run example 1" <| fun ctx ->
                                ctx.Subject.Should (equal ["ex2"])
                        ]

                        ("filter_func" ++ (fun (x:Example.T) -> false)) ==>
                        context "when no examples are included" [
                            it "runs all examples" <| fun ctx ->
                                ctx.Subject.Should (equal ["ex1";"ex2"])
                        ]
                    ]
                        
                yield context "an example group with two exampels" [
                    subject <| fun ctx ->
                        ctx?example1 <- Example.create "ex1" (record "ex1")
                        ctx?example2 <- Example.create "ex2" (record "ex2")
                        anExampleGroup
                        |> withExamples [ctx?example1; ctx?example2]
                    
                    itRunsTheSelectedExample
                ]

                yield context "an example group with child groups" [
                    subject <| fun ctx ->
                        ctx?example1 <- Example.create "ex1" (record "ex1")
                        ctx?example2 <- Example.create "ex2" (record "ex2")
                        anExampleGroup
                        |> withExamples [ctx?example1]
                        |> withNestedGroup
                            (withExamples [ctx?example2])

                    itRunsTheSelectedExample
                ]
            ]
        ]
    ]

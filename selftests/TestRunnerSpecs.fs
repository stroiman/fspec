module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open MatchersV3
open Dsl
open ExampleHelper
open Helpers
open TestReporter

let beExampleWithResult f =
    function
    | Example(_,r) -> f r
    | _ -> false
    |> createSimpleMatcher

let itIsFailure = function | Failure _ -> true | _ -> false
let itIsError = function | Error _ -> true | _ -> false

let itRaisesException = fun _ -> raise (new System.NotImplementedException())
let beExample = beExampleWithResult (fun _ -> true)
let bePending = beExampleWithResult (fun r -> r = Pending)
let beSuccess = beExampleWithResult (fun r -> r = Success)
let runExamples grp = grp |> run |> ignore

let doRun grp =
    let reporter = TestReporter.instance
    let data = Runner.doRun grp reporter (reporter.BeginTestRun())
    data.CallList |> List.rev
    
let itReportsExactlyOneExample f = 
    MultipleOperations [
        it "should report exactly one example" <| fun c ->
            c.Subject.Should (have.exactly 1 (beExample))

        it "should report correct result" <| fun c ->
            c.Subject.Should (have.exactly 1 (beExampleWithResult f))
    ]

let specs =
    describe "Test runner" [
        describe "progress reporting" [
            describe "Report example result" [
                subject <| fun c ->
                    anExampleGroup
                    |> withExamples [ c.metadata?example ]
                    |> doRun

                ("example" ++ aPassingExample) ==>
                context "example group contains one passing example" [
                    itReportsExactlyOneExample (fun r -> r = Success)
                ]

                ("example" ++ aPendingExample) ==>
                context "example group contains one pending example" [
                    itReportsExactlyOneExample (fun r -> r = Pending)
                ]

                ("example" ++ aFailingExample) ==>
                context "example group contains one failing example" [
                    itReportsExactlyOneExample (itIsFailure)
                ]

                ("example" ++ anExceptionThrowingExample) ==>
                context "example throw exception" [
                    itReportsExactlyOneExample (itIsError)
                ]
            ]
            
            describe "Report test structure" [
                subject <| fun _ ->
                    anExampleGroupNamed "parent"
                    |> withNestedGroupNamed "child1" (
                        withAnExampleNamed "test1")
                    |> withNestedGroupNamed "child2" (
                        withAnExampleNamed "test2")
                    |> doRun

                it "Reports groups" <| fun c ->
                    let expected = [
                        BeginGroup "parent"
                        BeginGroup "child1"
                        Example ("test1", Success)
                        EndGroup
                        BeginGroup "child2"
                        Example ("test2", Success)
                        EndGroup
                        EndGroup ]
                    c.Subject.Should (be.equalTo expected)
            ]
        ]

        describe "Error handling" [
            context "setup code raises exception" [
                subject <| fun c ->
                    anExampleGroup
                    |> withSetupCode (itRaisesException)
                    |> withAnExample
                    |> doRun

                itReportsExactlyOneExample (itIsError)
            ]
            
            context "tear down code raises exception" [
                subject <| fun c ->
                    anExampleGroup
                    |> withTearDownCode (itRaisesException)
                    |> withAnExample
                    |> doRun

                itReportsExactlyOneExample (itIsError)
            ]
        ]
        
        describe "metadata handling" [
            context "test contains metadata" [
                before <| fun ctx ->
                    ctx?testFunc <- (fun (c:TestContext) -> ctx?answer <- c.metadata.Get<int> "answer")

                it "passes the metadata to the test" <| fun ctx ->
                    anExampleGroup
                    |> withExamples [
                        anExampleWithCode ctx?testFunc
                        |> withExampleMetaData ("answer", 42) ]
                    |> runExamples
                    ctx?answer |> should (be.equalTo 42)

                it "passes the metadata to the setup" <| fun c ->
                    anExampleGroup
                    |> withSetupCode c?testFunc
                    |> withAnExampleWithMetaData ("answer", 42)
                    |> runExamples
                    c?answer |> should (be.equalTo 42)
            ]

            context "example group contains metadata" [
                before <| fun ctx ->
                    let withExampleMetaData =
                        match ctx.metadata?addExampleMetaData with
                        | false -> id
                        | true -> withExampleMetaData ("source", "example")
                    let withChildGroupMetaData =
                        match ctx.metadata?addChildGroupMetaData with
                        | false -> id
                        | true -> withMetaData ("source", "child group")

                    anExampleGroup
                    |> withMetaData ("source", "parent group")
                    |> withNestedGroup (
                        withChildGroupMetaData
                        >> withExamples [
                            anExampleWithCode (fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source))
                            |> withExampleMetaData
                        ])
                    |> runExamples

                ("addChildGroupMetaData" ++ false |||
                 "addExampleMetaData" ++ false) ==>
                context "test contains no metadata" [
                    it "uses metadata from setup" <| fun ctx ->
                        ctx.Get "source" |> should (be.equalTo "parent group")
                ]   

                ("addChildGroupMetaData" ++ false |||
                 "addExampleMetaData" ++ true) ==>
                context "test overrides same metadata" [
                    it "uses the metadata specified in test" <| fun ctx ->
                        ctx?source.Should (be.equalTo "example")
                ]

                ("addChildGroupMetaData" ++ true |||
                 "addExampleMetaData" ++ false) ==>
                context "nested example group overrides metadata" [
                    it "uses the metadata from the child group" <| fun ctx ->
                        ctx?source.Should (be.equalTo "child group")
                ]
            ]
        ]
    ]

module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open MatchersV3
open Dsl
open ExampleHelper
open TestContextOperations
open Helpers
open TestReporter

let beExampleWithResult f =
    function
    | Example(_,r) -> f r
    | _ -> false
    |> createSimpleMatcher

let itIsFailure = function
    | Failure(_) -> true
    | _ -> false

let itIsError = function
    | Error(_) -> true
    | _ -> false

let itRaisesException = fun _ -> raise (new System.NotImplementedException())
let beExample = beExampleWithResult (fun _ -> true)
let bePending = beExampleWithResult (fun r -> r = Pending)
let beSuccess = beExampleWithResult (fun r -> r = Success)

let doRun grp =
    let reporter = TestReporter.instance
    let data = Runner.doRun grp reporter (reporter.BeginTestRun())
    data.CallStack |> List.rev
    
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
                    c |> getSubject
                    |> should (be.equalTo expected)
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
        
        context "metadata handling" [
            context "test contains metadata" [
                it "passes the metadata to the test" <| fun c ->
                    anExampleGroup
                    |> withExamples [
                        anExampleWithCode (fun ctx ->
                            let tmp : int = ctx.metadata?answer
                            c?answer <- tmp)
                        |> withExampleMetaData ("answer", 42) ]
                    |> run |> ignore
                    c?answer |> should (be.equalTo 42)

                it "passes the metadata to the setup" <| fun _ ->
                    let actual = ref 0
                    anExampleGroup
                    |> withSetupCode (fun ctx -> actual := ctx.metadata?answer)
                    |> withAnExampleWithMetaData ("answer", 42)
                    |> run |> ignore
                    !actual |> should (be.equalTo 42)
            ]

            context "example group contains metadata" [
                subject <| fun _ ->
                    anExampleGroup
                    |> withMetaData ("source", "example group")

                context "test contains no metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> withExampleCode (fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source))
                                
                    it "uses metadata from setup" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should (be.equalTo "example group")
                ]   
                context "test overrides same metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> withExamples [
                            anExampleWithCode <| fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source)
                            |> withExampleMetaData("source", "example")]

                    it "uses the metadata specified in test" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should (be.equalTo "example")
                ]
                context "nested example group overrides metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> ExampleGroup.addChildGroup(
                            anExampleGroup
                            |> withMetaData ("source", "child context")
                            |> withExampleCode (fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source)))

                    it "uses the metadata from the child group" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should (be.equalTo "child context")
                ]
            ]
        ]
    ]

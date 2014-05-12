module FSpec.SelfTests.SuiteBuilderSpecs
open FSpec.Core
open Dsl
open MatchersV3
open ExampleHelper
open TestContextOperations

let getFailed (report : Report.T) = report.failed |> List.reduce (+)

type ReportType =
    | BeginGroup of string
    | EndGroup
    | Example of string * TestResultType

type reporterData = { CallStack: ReportType list }
let appendToReport n r = { r with CallStack = n::r.CallStack }

let reporter = {
        BeginGroup = fun grp -> grp |> ExampleGroup.name |> BeginGroup |> appendToReport
        ReportExample = fun ex res -> (ex |> Example.name, res) |> Example |> appendToReport
        EndTestRun = fun r -> r
        EndGroup = EndGroup |> appendToReport
        Success = fun _ -> true
        Zero = { CallStack = [] } 
    }

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
    let data = Runner.doRun grp reporter (reporter.Zero)
    data.CallStack |> List.rev
    
let itReportsExactlyOneExample f = 
    MultipleOperations [
        it "should report exactly one example" <| fun c ->
            c |> getSubject |> should (have.exactly 1 (beExample))

        it "should report correct result" <| fun c ->
            c |> getSubject |> should (have.exactly 1 (beExampleWithResult f))
    ]

let specs = [
    describe "TestRunner" [
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

            ("example" ++ (anExample (itRaisesException))) ==>
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
    ]
]

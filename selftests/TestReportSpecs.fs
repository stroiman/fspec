module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open Runner
open Helpers
open System.Text
open TestContextOperations

let anExampleNamed name = Example.create name (fun _ -> ())
let anExampleGroupNamed name = ExampleGroup.create name
let anExample = anExampleNamed "dummy"
let aFailureWithMessage message = Failure {Message=message}
let aFailure = aFailureWithMessage "dummy"

let getSubject<'T> (ctx : TestContext) =
    ctx |> getSubject<Reporter<'T>>

type TestContext with
    member ctx.Builder : StringBuilder = ctx?builder
    member ctx.ClearOutput = ctx.Builder.Clear
    member ctx.Lines =
        let chars = [|"\r";"\n"|]
        ctx.Builder.ToString().Split(chars, System.StringSplitOptions.RemoveEmptyEntries);
       
    member ctx.Report f =
        let r = getSubject<TreeReporter.T> ctx
        let report = r.BeginTestRun() |> (f r)
        ctx.Builder.Clear() |> ignore
        report |> r.EndTestRun |> ignore
    member ctx.ShouldNotHaveLineMatching pattern =
        ctx.Lines |> shouldNot have.element (toBe matchRegex pattern)
    
    member ctx.ShouldHaveLineMatching pattern =
        ctx.Lines |> should have.element (toBe matchRegex pattern)

let setupReport f = before (fun c -> c.Report f)

let itBehavesLikeATestReporter<'T> () =
    let getSubject = getSubject<'T>

    context "reporter" [
        context "With success reported" [
            it "Is a success" <| fun c ->
                let r = getSubject c
                r.BeginTestRun()
                |> r.ReportExample anExample Success
                |> r.Success |> should equal true
        ]
            
        context "With pendings reported" [
            it "Is not a failure" <| fun c ->
                let r = getSubject c
                r.BeginTestRun()
                |> r.ReportExample anExample Pending
                |> r.Success |> should equal true
        ]

        context "With errors reported" [
            it "Is a failure" <| fun c ->
                let r = getSubject c
                r.BeginTestRun()
                |> r.ReportExample anExample (Error(System.Exception()))
                |> r.Success |> should equal false
        ]
            
        context "With failures reported" [
            it "Is a failure" <| fun c ->
                let r = getSubject c
                r.BeginTestRun()
                |> r.ReportExample anExample (aFailure)
                |> r.Success |> should equal false
        ]
    ]
    
let specs =
    describe "TestReport" [
        describe "ListHelper" [
            describe "diff" [
                it "reports extra elements in x" <| fun _ ->
                    Helper.diff [1;2;3] []
                    |> should equal ([1;2;3],[])
                it "reports one extra element in x" <| fun _ ->
                    Helper.diff [1;2;3] [2;3]
                    |> should equal ([1],[])

                it "reports all diffs, when partial match" <| fun _ ->
                    Helper.diff ["a";"b";"x";"y"] ["1";"2";"3";"x";"y"]
                    |> should equal (["a";"b"],["1";"2";"3"])
                    
                it "reports all diffs, when no match" <| fun _ ->
                    Helper.diff ["a";"b"] ["1";"2";"3";]
                    |> should equal (["a";"b"],["1";"2";"3"])

                it "returns empty lists, when both input empty" <| fun _ ->
                    Helper.diff [] []
                    |> should equal ([],[])
            ]
        ]

        context "Tree reporter" [
            subject <| fun ctx -> 
                let builder = StringBuilder()
                ctx?builder <- builder
                TreeReporter.createReporterWithPrinter (stringBuilderPrinter builder)

            itBehavesLikeATestReporter<TreeReporter.T>()

            context "With no errors reported" [
                setupReport (fun r -> r.ReportExample anExample Success)

                it "writes one line" (fun c ->
                    c.Lines.Length |> should equal 1
                )

                it "prints '1 success'" (fun c ->
                    c.ShouldHaveLineMatching "1 success")

                it "prints '0 failed'" (fun c ->
                    c?builder.ToString() |> should matchRegex "0 failed"
                )
            ]

            context "With pending tests reported" [
                setupReport (fun r -> 
                    r.ReportExample (anExampleNamed "Example") Pending)

                it "prints '1 pending'" <| fun c ->
                    c.ShouldHaveLineMatching "1 pending"
            ]

            context "With failure 'Failure msg' reported" [
                setupReport (fun r ->
                    r.BeginGroup (anExampleGroupNamed "Group")
                    >> r.ReportExample (anExampleNamed "Test1") (aFailureWithMessage "Failure msg")
                    >> r.EndGroup)

                it "writes more than one line" (fun c ->
                    c.Lines.Length |> should be.greaterThan 1)

                it "Does print errors" (fun c ->
                    c?builder.ToString() |> should matchRegex "1 failed" )

                it "Prints the example name" (fun c ->
                    c.ShouldHaveLineMatching "Test1")

                it "Prints the example group name" (fun c ->
                    c.ShouldHaveLineMatching "Group")

                it "Prints 'Failure msg'" (fun c ->
                    c.ShouldHaveLineMatching "Failure msg")
            ]

            context "With two errors reported" [
                setupReport (fun r ->
                    r.ReportExample (anExampleNamed "x1") aFailure
                    >> r.ReportExample (anExampleNamed "x2") aFailure)

                it "Does print errors" <| fun c ->
                    c?builder.ToString() |> should matchRegex "2 failed"

                it "Prints example 1 failed" <| fun c ->
                    c.ShouldHaveLineMatching "x1"

                it "Prints example 2 failed" <| fun c ->
                    c.ShouldHaveLineMatching "x2"
            ]

            context "with two examples, one fails" [
                setupReport (fun r ->
                    r.ReportExample (anExampleNamed "Test1") Success
                    >> r.ReportExample (anExampleNamed "Test2") aFailure)

                it "does not print 'Test1'" <| fun c ->
                    c.ShouldNotHaveLineMatching "Test1"

                it "prints 'Test2'" <| fun c ->
                    c.ShouldHaveLineMatching "Test2"
            ]

            context "One example group with two failing tests" [
                setupReport (fun r ->
                    r.BeginGroup (anExampleGroupNamed "group")
                    >> r.ReportExample anExample aFailure
                    >> r.ReportExample anExample aFailure)
                
                it "displays the group name only once" <| fun c ->
                    c.Lines |> should have.exactly 1 (toBe matchRegex "group")
            ]
        ]
    ]

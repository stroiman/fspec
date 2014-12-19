module FSpec.SelfTests.TestReportSpecs
open System
open System.Text
open FSpec
open Dsl
open Matchers
open Runner
open ExampleHelper
open Helpers
open CustomMatchers

let aFailureWithMessage message = Failure {Message=message}
let aFailure = aFailureWithMessage "dummy"

let getSubject<'T when 'T :> IReporter> (ctx:TestContext) = ctx.GetSubject<'T> () :> IReporter

type TestContext with
    member ctx.Builder = ctx.GetOrDefault "builder" (fun _ -> StringBuilder())
    member ctx.ClearOutput = ctx.Builder.Clear
    member ctx.Lines =
        let chars = [|"\r";"\n"|]
        ctx.Builder.ToString().Split(chars, StringSplitOptions.RemoveEmptyEntries);

let setupReport (f : IReporter -> IReporter) = before <| fun ctx ->
    let r = getSubject<TreeReporter.Reporter> ctx
    r.BeginTestRun() |> f |> (fun r -> r.EndTestRun ()) |> ignore

let anExampleNamed name = { Name = name; MetaData = TestDataMap.Zero }
let anExample = anExampleNamed "Dummy"
let anExampleGroupNamed = anExampleNamed


let specs =
    describe "TestReport" [
        describe "ListHelper" [
            describe "diff" [
                it "reports extra elements in x" <| fun _ ->
                    Helper.diff [1;2;3] []
                    |> should (equal ([1;2;3],[]))
                it "reports one extra element in x" <| fun _ ->
                    Helper.diff [1;2;3] [2;3]
                    |> should (equal ([1],[]))

                it "reports all diffs, when partial match" <| fun _ ->
                    Helper.diff ["a";"b";"x";"y"] ["1";"2";"3";"x";"y"]
                    |> should (equal (["a";"b"],["1";"2";"3"]))
                    
                it "reports all diffs, when no match" <| fun _ ->
                    Helper.diff ["a";"b"] ["1";"2";"3";]
                    |> should (equal (["a";"b"],["1";"2";"3"]))

                it "returns empty lists, when both input empty" <| fun _ ->
                    Helper.diff [] []
                    |> should (equal ([],[]))
            ]
        ]

        context "Tree reporter" [
            subject <| fun ctx -> 
                let printer = stringBuilderPrinter ctx.Builder
                let options = 
                    { TreeReporterOptions.Default with 
                          Printer = printer
                          PrintSuccess = false}
                TreeReporter.Reporter options

            context "With no errors reported" [
                setupReport (fun r -> r.ReportExample anExample Success)

                it "writes one line" <| fun c ->
                    c.Lines.Length.Should (equal 1)

                it "prints '1 success'" <| fun c ->
                    c.Lines.Should (haveLineMatching "1 success")

                it "prints '0 failed'" <| fun c ->
                    c?builder.ToString().Should 
                        (be.string.containing "0 failed")
            ]

            context "With pending tests reported" [
                setupReport (fun r -> 
                    r.ReportExample (anExampleNamed "Example") Pending)

                it "prints '1 pending'" <| fun c ->
                    c.Lines.Should (haveLineMatching "1 pending")
            ]

            context "With failure 'Failure msg' reported" [
                setupReport (fun r ->
                    r.BeginGroup (anExampleGroupNamed "Group") 
                    |> fun r -> r.ReportExample (anExampleNamed "Test1") (aFailureWithMessage "Failure msg")
                    |> fun r -> r.EndGroup ())

                it "writes more than one line" (fun c ->
                    c.Lines.Length |> should (be.greaterThan 1))

                it "Does print errors" (fun c ->
                    c?builder.ToString() |> should (be.string.matching "1 failed" ))

                it "Prints the example name" (fun c ->
                    c.Lines.Should (haveLineMatching "Test1"))

                it "Prints the example group name" (fun c ->
                    c.Lines.Should (haveLineMatching "Group"))

                it "Prints 'Failure msg'" (fun c ->
                    c.Lines.Should (haveLineMatching "Failure msg"))
            ]

            context "With two errors reported" [
                setupReport (fun r ->
                    r.ReportExample (anExampleNamed "x1") aFailure
                    |> fun r -> r.ReportExample (anExampleNamed "x2") aFailure)

                it "Does print errors" <| fun c ->
                    c?builder.ToString() |> should (be.string.matching "2 failed")

                it "Prints example 1 failed" <| fun c ->
                    c.Lines.Should (haveLineMatching "x1")

                it "Prints example 2 failed" <| fun c ->
                    c.Lines.Should (haveLineMatching "x2")
            ]

            context "with two examples, one fails" [
                setupReport (fun r ->
                    r.ReportExample (anExampleNamed "Test1") Success
                    |> fun r -> r.ReportExample (anExampleNamed "Test2") aFailure)

                it "does not print 'Test1'" <| fun c ->
                    c.Lines.ShouldNot (haveLineMatching "Test1")

                it "prints 'Test2'" <| fun c ->
                    c.Lines.Should (haveLineMatching "Test2")
            ]

            context "One example group with two failing tests" [
                setupReport (fun r ->
                    r.BeginGroup (anExampleGroupNamed "group")
                    |> fun r -> r.ReportExample anExample aFailure
                    |> fun r -> r.ReportExample anExample aFailure)
                
                it "displays the group name only once" <| fun c ->
                    c.Lines.Should (have.exactly 1 (be.string.matching "group"))
            ]
        ]
    ]

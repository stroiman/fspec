module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open Runner
open Helpers
open System.Text

let anExample = Example.create "dummy" (fun _ -> ())
let aFailure = Failure({Message="Dummy"})

let getSubject<'T> (ctx : TestContext) =
    ctx.Subject<Reporter<'T>> ()

type TestContext with
    member ctx.Builder : StringBuilder = ctx?builder
    member ctx.ClearOutput = ctx.Builder.Clear
    member ctx.Lines =
        let chars = [|"\r";"\n"|]
        ctx.Builder.ToString().Split(chars, System.StringSplitOptions.RemoveEmptyEntries);

let itBehavesLikeATestReporter<'T> () =
    let getSubject = getSubject<'T>

    context "reporter" [
        context "With success reported" [
            it "Is a success" <| fun c ->
                let r = getSubject c
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample Success
                |> r.Success |> should equal true
        ]
            
        context "With pendings reported" [
            it "Is not a failure" <| fun c ->
                let r = getSubject c
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample Pending
                |> r.Success |> should equal true
        ]

        context "With errors reported" [
            it "Is a failure" <| fun c ->
                let r = getSubject c
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample (Error(System.Exception()))
                |> r.Success |> should equal false
        ]
            
        context "With failures reported" [
            it "Is a failure" <| fun c ->
                let r = getSubject c
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample (Failure(AssertionErrorInfo.create))
                |> r.Success |> should equal false
        ]
    ]
    
let specs =
    describe "TestReport" [

        context "Classic reporter" [
            subject <| fun _ -> ClassicReporter().createReporter()
            
            itBehavesLikeATestReporter<Report.T>()
        ]

        context "Tree reporter" [
            subject <| fun ctx -> 
                let builder = StringBuilder()
                ctx?builder <- builder
                TreeReporter.createReporterWithPrinter (stringBuilderPrinter builder)

            itBehavesLikeATestReporter<TreeReporter.T>()

            context "With no errors reported" [
                before (fun c ->
                    let r = getSubject<TreeReporter.T> c
                    let b = c.Get<StringBuilder> "builder"
                    let report = 
                        r.Zero
                        |> r.BeginExample anExample
                        |> r.EndExample Success
                    b.Clear() |> ignore
                    report |> r.EndTestRun |> ignore
                )

                it "writes one line" (fun c ->
                    c.Lines.Length |> should equal 1
                )

                it "Does not print errors" (fun c ->
                    c?builder.ToString() |> should matchRegex "0 failed"
                )
            ]

            context "With errors reported" [
                before (fun c ->
                    let r = getSubject<TreeReporter.T> c
                    let b = c.Get<StringBuilder> "builder"
                    let report = 
                        r.Zero
                        |> r.BeginExample anExample
                        |> r.EndExample aFailure
                    b.Clear() |> ignore
                    report |> r.EndTestRun |> ignore
                )

                it "writes more than one line" (fun c ->
                    c.Lines.Length |> should be.greaterThan 1)

                it "Does print errors" (fun c ->
                    c?builder.ToString() |> should matchRegex "1 failed"
                )
            ]

            context "With two errors reported" [
                before (fun c ->
                    let r = getSubject<TreeReporter.T> c
                    let b = c.Get<StringBuilder> "builder"
                    let report = 
                        r.Zero
                        |> r.BeginExample anExample
                        |> r.EndExample aFailure
                        |> r.BeginExample anExample
                        |> r.EndExample aFailure
                    b.Clear() |> ignore
                    report |> r.EndTestRun |> ignore
                )
                it "Does print errors" (fun c ->
                    c?builder.ToString() |> should matchRegex "2 failed"
                )
            ]
        ]
    ]

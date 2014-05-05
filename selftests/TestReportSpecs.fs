module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open Runner

let anExample = Example.create "dummy" (fun _ -> ())

let itBehavesLikeATestReporter<'T> () =
    let getSubject (ctx : TestContext.T) =
        ctx.subject<Reporter<'T>> ()

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
            subject <| fun _ -> TreeReporter.createReporter

            itBehavesLikeATestReporter<TreeReporter.T>()
        ]
    ]
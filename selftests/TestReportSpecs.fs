module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open Runner

type TestContext.T with 
    member self.Reporter () = self.subject<Reporter<Report.T>> ()
   
let anExample = Example.create "dummy" (fun _ -> ())

let itBehavesLikeATestReporter<'T> () =
    let getSubject (ctx : TestContext.T) =
        ctx.subject<Reporter<Report.T>> ()

    context "reporter" [
        context "With success reported" [
            it "Is a success" <| fun c ->
                let r = c.Reporter ()
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample Success
                |> r.Success |> should equal true
        ]

        context "With errors reported" [
            it "Is a failure" <| fun c ->
                let r = c.Reporter ()
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample (Error(System.Exception()))
                |> r.Success |> should equal false
        ]
            
        context "With failures reported" [
            it "Is a failure" <| fun c ->
                let r = c.Reporter ()
                r.Zero
                |> r.BeginExample anExample
                |> r.EndExample (Failure(AssertionErrorInfo.create))
                |> r.Success |> should equal false
        ]
    ]
    
let specs =
    describe "TestReport" [
        subject <| fun _ -> ClassicReporter().createReporter()
        
        itBehavesLikeATestReporter<Report.T>()
    ]
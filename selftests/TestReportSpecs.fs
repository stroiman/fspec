module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open DslV2
open Matchers

let specs =
    describe "TestReport" [
        context "With success reported" [
            it "Is a success" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Success)
                |> Report.success |> should equal true
        ]

        context "With errors reported" [
            it "Is a failure" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Error(System.Exception()))
                |> Report.success |> should equal false
        ]
            
        context "With failures reported" [
            it "Is a failure" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Failure(AssertionErrorInfo.create))
                |> Report.success |> should equal false
        ]
    ]
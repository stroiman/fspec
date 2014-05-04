module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers

let specs =
    describe "TestReport" <| fun _ ->
        describe "With success reported" <| fun _ ->
            it "Is a success" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Success)
                |> Report.success |> should equal true

        describe "With errors reported" <| fun _ ->
            it "Is a failure" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Error(System.Exception()))
                |> Report.success |> should equal false

        describe "With failures reported" <| fun _ ->
            it "Is a failure" <| fun _ ->
                Report.create ()
                |> Report.reportTestName "dummy" (Failure(AssertionErrorInfo.create))
                |> Report.success |> should equal false
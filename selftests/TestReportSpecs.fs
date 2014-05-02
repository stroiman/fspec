module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    describe "TestReport" <| fun _ ->
        describe "With success reported" <| fun _ ->
            it "Is a success" <| fun _ ->
                let r = TestReport()
                let r' = r.reportTestName "dummy" (Success)
                r'.success() |> should equal true

        describe "With errors reported" <| fun _ ->
            it "Is a failure" <| fun _ ->
                let r = TestReport()
                let r' = r.reportTestName "dummy" (Error(System.Exception()))
                r'.success() |> should equal false

        describe "With failures reported" <| fun _ ->
            it "Is a failure" <| fun _ ->
                let r = TestReport()
                let r' = r.reportTestName "dummy" (Failure(AssertionErrorInfo.create))
                r'.success() |> should equal false

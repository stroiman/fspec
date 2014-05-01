module FSpec.SelfTests.TestReportSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    describe "TestReport" <| fun() ->
        describe "With success reported" <| fun () ->
            it "Is a success" <| fun _ ->
                let r = TestReport()
                r.reportTestName "dummy" (Success)
                r.success() |> should equal true

        describe "With errors reported" <| fun() ->
            it "Is a failure" <| fun _ ->
                let r = TestReport()
                r.reportTestName "dummy" (Error(System.Exception()))
                r.success() |> should equal false

        describe "With failures reported" <| fun() ->
            it "Is a failure" <| fun _ ->
                let r = TestReport()
                r.reportTestName "dummy" (Failure(AssertionErrorInfo.create))
                r.success() |> should equal false

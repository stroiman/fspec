module FSpec.SelfTests.SuiteBuilderSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    let helper = DslHelper()
    let run () = helper.run()
    let _describe = helper.describe
    let _it = helper.it
    let _before = helper.before 
    let _after = helper.after

    describe "TestCollection" <| fun() ->
        it "handles lazy initialization" <| fun () ->
            let c = TestCollection()
            let initCount = ref 0
            c.describe "Ctx" <| fun () ->
                let value = c.init <| fun () ->
                    initCount := !initCount + 1
                    "dummy"

                c.it "uses value" <| fun () ->
                    let x = value()
                    ()

                c.it "uses value twice" <| fun () ->
                    let x = value()
                    let y = value()
                    ()

            c.run()
            !initCount |> should equal 2

    describe "TestCollection" <| fun() ->
        describe "Run" <| fun () ->
            it "reports test failures" <| fun () ->
                _it "Is a failure" fail

                let report = run()
                report.summary() |> should equal "1 run, 1 failed"

            it "reports test success" <| fun() ->
                _it "Is a success" pass

                let report = run()
                report.summary() |> should equal  "1 run, 0 failed"

            it "reports pending tests" <| fun() ->
                _it "Is pending" pending
                let report = run()
                report.summary() |> should equal "1 run, 0 failed, 1 pending"

            it "runs the tests in the right order" <| fun() ->
                let order = ref []
                _describe("context") <| fun() ->
                    _it("has test 1") <| fun() ->
                        order := 1::!order
                    _it("has test 2") <| fun() ->
                        order := 2::!order

                run() |> ignore
                !order |> should equal [2;1]

            it "runs the contexts in the right order" <| fun() ->
                let order = ref []
                _describe("context") <| fun() ->
                    _it("has test 1") <| fun() ->
                        order := 1::!order
                _describe("other context") <| fun() ->
                    _it("has test 2") <| fun() ->
                        order := 2::!order
                run() |> ignore
                !order |> should equal [2;1]

        describe "Running status" <| fun () ->
            it "Is reported while running" <| fun () ->
                _describe "Some context" <| fun () ->
                    _it "has some behavior" pass
                let report = run()
                report.testOutput() |> should equal ["Some context has some behavior - passed"]

            it "Reports multiple test results" <| fun () ->
                _describe "Some context" <| fun() ->
                    _it "has some behavior" pass
                    _it "has some other behavior" pass

                let report = run()
                let actual = report.testOutput()
                let expected = ["Some context has some behavior - passed";
                                "Some context has some other behavior - passed"]
                actual.should equal expected

            it "Reports nested contexts correctly" <| fun () ->
                _describe "Some context" <| fun() ->
                    _describe "in some special state" <| fun() ->
                        _it "has some special behavior" pass

                let report = run()
                let actual = report.testOutput() |> List.head
                actual |> should matchRegex "Some context in some special state has some special behavior"

        describe "Failed tests" <| fun() ->
            it "Writes the output to the test report" <| fun() ->
                _it "Is a failing test" <| fun() ->
                    (5).should equal 6
                let result = run()
                let actual = result.failedTests() |> List.reduce (+)
                actual |> should matchRegex "expected 5 to equal 6"

            it "write the right output for comparison tests" <| fun() ->
                _it "Is a failing test" <| fun() ->
                    5 |> should be.greaterThan 6
                let result = run()
                let actual = result.failedTests() |> List.reduce (+)
                actual |> should matchRegex "expected 5 to be greater than 6"

            it "Is empty when no tests fail" <| fun() ->
                _it "Is a passing test" pass
                let result = run()
                let actual = result.failedTests() |> List.length
                actual.should equal 0

        describe "Tests with errors" <| fun() ->
            it "writes the exception name" <| fun() ->
                _it "Is a failing test" <| fun() ->
                    raise (new System.NotImplementedException())
                    ()
                let result = run()
                let actual = result.failedTests() |> List.reduce (+)
                actual |> should matchRegex "NotImplementedException"

    describe "TestReport" <| fun() ->
        describe "With success reported" <| fun () ->
            it "Is a success" <| fun () ->
                let r = TestReport()
                r.reportTestName "dummy" (Success)
                r.success() |> should equal true

        describe "With errors reported" <| fun() ->
            it "Is a failure" <| fun() ->
                let r = TestReport()
                r.reportTestName "dummy" (Error(System.Exception()))
                r.success() |> should equal false

        describe "With failures reported" <| fun() ->
            it "Is a failure" <| fun () ->
                let r = TestReport()
                r.reportTestName "dummy" (Failure(AssertionErrorInfo.create))
                r.success() |> should equal false

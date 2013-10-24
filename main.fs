module Main
open FSpec
open Expectations

let assertTrue value =
  if not value then
    failwithf "Value was false"

let assertFalse value =
  if value then
    failwithf "Value was true"

let assertMatches actual pattern =
    let regex = System.Text.RegularExpressions.Regex pattern
    if not (regex.IsMatch actual) then
        let msg = sprintf "String was not a match. Pattern %s - actual %s" pattern actual
        printfn "%s" msg
        failwithf "%s" msg

let pass () = ()
let fail () = failwithf "Test failure"

let c = TestCollection()
let describe = c.describe
let it = c.it
let before = c.before
let init = c.init

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
    let col = init (fun () -> TestCollection())
    let res = init (fun () -> TestReport())
    let run () =
        col().run(res())
        res()

    describe "Setup" <| fun () ->
        it "runs before the test is run" <| fun () ->
            let wasSetupWhenTestWasRun = ref false
            let wasSetup = ref false
            col().before <| fun() ->
                wasSetup := true
            col().it "dummy" <| fun() ->
                wasSetupWhenTestWasRun := !wasSetup
            run() |> ignore
            assertTrue !wasSetupWhenTestWasRun  

        it "is only run for in same context, or nested context" <| fun () ->
            let outerSetupRunCount = ref 0
            let innerSetupRunCount = ref 0
            col().describe "Ctx" <| fun () ->
                col().before <| fun () ->
                    outerSetupRunCount := !outerSetupRunCount + 1
                col().it "Outer test" pass
                col().describe "Inner ctx" <| fun () ->
                    col().before <| fun () ->
                        innerSetupRunCount := !innerSetupRunCount + 1
                    col().it "Inner test" pass
                    col().it "Inner test2" pass
            run() |> ignore
            !innerSetupRunCount |> should equal 2
            !outerSetupRunCount |> should equal 3

    describe "Run" <| fun () ->
        it "reports test failures" <| fun () ->
            col().it "Is a failure" fail

            let report = run()
            report.summary() |> should equal "1 run, 1 failed"

        it "reports test success" <| fun() ->
            col().it "Is a success" pass

            let report = run()
            report.summary() |> should equal  "1 run, 0 failed"

        it "runs the tests in the right order" <| fun() ->
            let no = ref 0
            let testNo () =
                no := !no + 1
                !no
            let test1No = ref 0
            let test2No = ref 0
            col().describe("context") <| fun() ->
                col().it("has test 1") <| fun() ->
                    test1No := testNo()
                col().it("has test 2") <| fun() ->
                    test2No := testNo()

            run() |> ignore
            !test1No |> should equal 1
            !test2No |> should equal 2

        it "runs the contexts in the right order" <| fun() ->
            let no = ref 0
            let testNo () =
                no := !no + 1
                !no
            let test1No = ref 0
            let test2No = ref 0
            col().describe("context") <| fun() ->
                col().it("has test 1") <| fun() ->
                    test1No := testNo()
            col().describe("other context") <| fun() ->
                col().it("has test 2") <| fun() ->
                    test2No := testNo()

            run() |> ignore
            !test1No |> should equal 1
            !test2No |> should equal 2

    describe "Running status" <| fun () ->
        it "Is reported while running" <| fun () ->
            col().describe "Some context" <| fun () ->
                col().it "has some behavior" pass
            let report = run()
            report.testOutput() |> should equal ["Some context has some behavior - passed"]

        it "Reports multiple test results" <| fun () ->
            col().describe "Some context" <| fun() ->
                col().it "has some behavior" pass
                col().it "has some other behavior" pass

            let report = run()
            let actual = report.testOutput()
            let expected = ["Some context has some behavior - passed";
                            "Some context has some other behavior - passed"]
            actual.should equal expected

    describe "Failed tests" <| fun() ->
        it "Writes the output to the test report" <| fun() ->
            col().it "Is a failing test" <| fun() ->
                (5).should equal 6
            let result = run()
            let actual = result.failedTests() |> List.reduce (+)
            assertMatches actual "expected 5 to equal 6"

        it "Is empty when no tests fail" <| fun() ->
            col().it "Is a passing test" pass
            let result = run()
            let actual = result.failedTests() |> List.length
            actual.should equal 0

describe "TestReport" <| fun() ->
    describe "With no failures reported" <| fun () ->
        it "Is a success" <| fun () ->
            let r = TestReport()
            assertTrue (r.success())

    describe "With failures reported" <| fun() ->
        it "Is a failure" <| fun () ->
            let r = TestReport()
            r.reportFailure()
            assertFalse(r.success())

describe "Assertion helpers" <| fun() ->
    describe "equals" <| fun() ->
        it "passes when objects equal" <| fun() ->
            (5).should equal 5
        it "fails when the objects are not equal" <| fun() ->
            try
                (5).should equal 6
                failwithf "No exception throws"
            with
                | AssertionError(_) -> ()
    describe "greaterThan" <| fun() ->
        5 |> should be.greaterThan 4

let report = TestReport()
c.run(report)
report.failedTests() |> List.iter (fun x -> printfn "%s" x)
printfn "%s" (report.summary())

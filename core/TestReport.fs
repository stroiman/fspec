namespace FSpec.Core

type AssertionErrorInfo = { 
    Message: string
}

type TestResultType =
    | Success
    | Error of System.Exception
    | Failure of AssertionErrorInfo

module Report =
    type T = {
        noOfTestRuns: int;
        noOfFails: int;
        output: string list;
        failed: string list;
    }

    let create () = {
        noOfTestRuns = 0;
        noOfFails = 0;
        output = [];
        failed = [];
    }

    let reportRun report = { report with noOfTestRuns = report.noOfTestRuns + 1 }
    let reportFail report = { report with noOfFails = report.noOfFails + 1 }
    let addOutput report output = { report with output = output::report.output }
    let addFail report fail = { report with failed = fail::report.failed }

type TestReport() =
    let mutable report = Report.create()

    member self.reportTestRun () =
        report <- Report.reportRun report

    member self.reportFailure () =
        report <- Report.reportFail report

    member self.summary() = 
        sprintf "%d run, %d failed" report.noOfTestRuns report.noOfFails

    member self.success() =
        report.noOfFails = 0

    member self.reportTestName name result =
        let name2 = match result with
                    | Success -> sprintf "%s - passed" name
                    | Error(ex) -> sprintf "%s - failed - %s" name (ex.ToString())
                    | Failure(errorInfo) -> 
                        sprintf "%s - failed - %s" name errorInfo.Message
        match result with
            | Success -> ()
            | _ -> report <- Report.addFail report name2
        report <- Report.addOutput report name2

    member self.testOutput() =
        report.output |> List.rev

    member self.failedTests() = 
        report.failed |> List.rev

namespace FSpec.Core

type AssertionErrorInfo = { 
    Message: string
} with
    static member create = { Message = "" }


type TestResultType =
    | Success
    | Error of System.Exception
    | Failure of AssertionErrorInfo

module Report =
    type T = {
        noOfTestRuns: int;
        output: string list;
        failed: string list;
    }

    let create () = {
        noOfTestRuns = 0;
        output = [];
        failed = [];
    }

    let reportRun report = { report with noOfTestRuns = report.noOfTestRuns + 1 }
    let addOutput report output = { report with output = output::report.output }
    let addFail report fail = { report with failed = fail::report.failed }
    let success report = report.failed = []
    let reportTestName report name result =
        let name' = match result with
                    | Success -> sprintf "%s - passed" name
                    | Error(ex) -> sprintf "%s - failed - %s" name (ex.ToString())
                    | Failure(errorInfo) -> 
                        sprintf "%s - failed - %s" name errorInfo.Message
        let report' = match result with
                        | Success -> report
                        | _ -> addFail report name'
        addOutput report' name'

type TestReport() =
    let mutable report = Report.create()

    member self.reportTestRun () =
        report <- Report.reportRun report

    member self.summary() = 
        let noOfFails = report.failed |> List.length
        sprintf "%d run, %d failed" report.noOfTestRuns noOfFails

    member self.success() =
        Report.success report

    member self.reportTestName name result =
        report <- Report.reportTestName report name result

    member self.testOutput() =
        report.output |> List.rev

    member self.failedTests() = 
        report.failed |> List.rev

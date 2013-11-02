namespace FSpec.Core

type AssertionErrorInfo = { 
    Message: string
} with
    static member create = { Message = "" }

exception AssertionError of AssertionErrorInfo
exception PendingError

type TestResultType =
    | Success
    | Pending
    | Error of System.Exception
    | Failure of AssertionErrorInfo

module Report =
    type T = {
        output: string list;
        failed: string list;
        pending: string list;
    }

    let create () = {
        output = [];
        failed = [];
        pending = [];
    }

    let addOutput report output = { report with output = output::report.output }
    let addFail report fail = { report with failed = fail::report.failed }
    let addPending report pending = { report with pending = pending::report.pending }

    let success report = report.failed = []
    let reportTestName report name result =
        let name' = match result with
                    | Success -> sprintf "%s - passed" name
                    | Pending -> sprintf "%s - pending" name
                    | Error(ex) -> sprintf "%s - failed - %s" name (ex.ToString())
                    | Failure(errorInfo) -> 
                        sprintf "%s - failed - %s" name errorInfo.Message
        let report' = match result with
                        | Success -> report
                        | Pending -> addPending report name'
                        | _ -> addFail report name'
        addOutput report' name'

type TestReport() =
    let mutable report = Report.create()

    member self.summary() = 
        let noOfFails = report.failed |> List.length
        let noOfRuns = report.output |> List.length
        let noOfPendings = report.pending |> List.length
        if (noOfPendings > 0) then
            sprintf "%d run, %d failed, %d pending" noOfRuns noOfFails noOfPendings
        else
            sprintf "%d run, %d failed" noOfRuns noOfFails

    member self.success() =
        Report.success report

    member self.reportTestName name result =
        report <- Report.reportTestName report name result

    member self.testOutput() =
        report.output |> List.rev

    member self.failedTests() = 
        report.failed |> List.rev

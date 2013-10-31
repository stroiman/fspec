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
        noOfFails: int
    }

    let create () = {
        noOfTestRuns = 0;
        noOfFails = 0
    }

    let reportRun report = { report with noOfTestRuns = report.noOfTestRuns + 1 }
    let reportFail report = { report with noOfFails = report.noOfFails + 1 }

type TestReport() =
    let mutable report = Report.create()
    let mutable output = []
    let mutable failed = []

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
            | _ -> failed <- name2::failed
        output <- name2::output

    member self.testOutput() =
        output |> List.rev

    member self.failedTests() = 
        failed |> List.rev

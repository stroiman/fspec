namespace FSpec.Core

type AssertionErrorInfo = { 
    Message: string
}

type TestResultType =
    | Success
    | Error of System.Exception
    | Failure of AssertionErrorInfo

type TestReport() =
    let mutable noOfTestsRun = 0
    let mutable noOfFails = 0
    let mutable output = []
    let mutable failed = []

    member self.reportTestRun () =
        noOfTestsRun <- noOfTestsRun + 1

    member self.reportFailure () =
        noOfFails <- noOfFails + 1

    member self.summary() = 
        sprintf "%d run, %d failed" noOfTestsRun noOfFails

    member self.success() =
        noOfFails = 0

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

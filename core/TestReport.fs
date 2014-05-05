namespace FSpec.Core

type Reporter<'T> = {
    BeginGroup : ExampleGroup.T -> 'T -> 'T
    BeginExample: Example.T -> 'T -> 'T
    EndExample: TestResultType -> 'T -> 'T
    EndGroup: 'T -> 'T;
    Success: 'T -> bool;
    Zero: 'T }

module TreeReporter =
    type T = {
        Success: bool;
        Indentation: string list }
    let Zero = { Success = true; Indentation = [] }
    let printIndentation report =
        report.Indentation |> List.rev |> List.iter (printf "%s")
    let beginGroup exampleGroup report =
        printIndentation report
        printfn "%s" (exampleGroup |> ExampleGroup.name)
        { report with Indentation = "  " :: report.Indentation }
    let popIndentation report = { report with Indentation = report.Indentation.Tail }
    let endGroup = popIndentation
    let beginExample example report =
        printIndentation report
        printf "- %s" (example |> Example.name)
        report
    let endExample result report =
        printfn " - %A" result
        report
    let success report = report.Success;
    let createReporter = {
        BeginGroup  = beginGroup;
        EndGroup = endGroup;
        BeginExample = beginExample;
        EndExample = endExample;
        Success = success;
        Zero = Zero }
    
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

    let failed report = report.failed
    let output report = report.output
    let success report = report.failed = []
    let reportTestName name result report =
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

    let summary report =
        let noOfFails = report.failed |> List.length
        let noOfRuns = report.output |> List.length
        let noOfPendings = report.pending |> List.length
        if (noOfPendings > 0) then
            sprintf "%d run, %d failed, %d pending" noOfRuns noOfFails noOfPendings
        else
            sprintf "%d run, %d failed" noOfRuns noOfFails

type ClassicReporter() = 
    // Not that classic - just mimcs the way the system worked during
    // the first iterations. This class serves mostly to keep unit
    // tests running until they have been rewritten to support the
    // separation of runner and reporter
    let mutable (names : string list) = []
    let beginGroup group (report : Report.T) =
        names <- (group |> ExampleGroup.name) :: names
        report
    let beginExample example (report : Report.T) =
        names <- (example |> Example.name) :: names
        report
    let endExample result (report : Report.T) =
        let rec printNameStack(stack) : string =
            match stack with
            | []    -> ""
            | head::[] -> head
            | head::tail ->sprintf "%s %s" (printNameStack(tail)) head
        let (name : string) = printNameStack names
        names <- names.Tail
        report |> Report.reportTestName name result
    let endGroup (report : Report.T) = 
        names <- names.Tail
        report
    member self.createReporter () = {
        BeginGroup = beginGroup;
        BeginExample = beginExample;
        EndGroup = endGroup;
        EndExample = endExample;
        Success = Report.success;
        Zero = Report.create () }


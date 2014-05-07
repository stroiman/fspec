namespace FSpec.Core
open System

/// Represenations of the colors used to print to the console
type Color =
    | Red | Yellow | Green | Default

type Reporter<'T> = {
    BeginGroup : ExampleGroup.T -> 'T -> 'T
    BeginExample: Example.T -> 'T -> 'T
    EndExample: TestResultType -> 'T -> 'T
    EndTestRun: 'T -> 'T
    EndGroup: 'T -> 'T
    Success: 'T -> bool
    Zero: 'T }
module TreeReporter =
    type T = {
        Success: bool;
        Indentation: string list }
    let Zero = { Success = true; Indentation = [] }
    
    let printIndentation report =
        report.Indentation |> List.rev |> List.iter (printf "%s")

    let beginGroup printer exampleGroup report =
        printIndentation report
        sprintf "%s\n" (exampleGroup |> ExampleGroup.name) |> printer Color.Default
        { report with Indentation = "  " :: report.Indentation }

    let endGroup report = { report with Indentation = report.Indentation.Tail }
    
    let beginExample printer example report =
        printIndentation report
        sprintf "- %s" (example |> Example.name) |> printer Default
        report

    let printSummary printer report =
        match report.Success with
        | true -> ()
        | false -> "\n Test run not successful\n" |> printer Red
        report
    let endExample printer result report =
        sprintf " - " |> printer Default
        let success = 
            match result with
            | Success -> 
                sprintf "%s" "Success" |> printer Green
                report.Success
            | Pending -> 
                sprintf "%s" "Pending" |> printer Yellow
                report.Success
            | Failure e -> 
                sprintf "%A" e.Message |> printer Red
                false
            | Error(_) -> 
                sprintf "%A" result |> printer Red
                false
        "\n" |> printer Default
        { report with Success = success }
    let success report = report.Success;

    let consolePrinter color (msg:string) =
        let old = System.Console.ForegroundColor 
        let consoleColor = 
            match color with
            | Red -> System.Console.ForegroundColor <- ConsoleColor.Red
            | Yellow -> System.Console.ForegroundColor <- ConsoleColor.Yellow
            | Green -> System.Console.ForegroundColor <- ConsoleColor.Green
            | Default -> ()
        try 
            System.Console.Write msg
        finally
            System.Console.ForegroundColor <- old

    let createReporterWithPrinter printer = {
        BeginGroup  = beginGroup printer;
        EndGroup = endGroup;
        BeginExample = beginExample printer;
        EndExample = endExample printer;
        Success = success;
        EndTestRun = printSummary printer;
        Zero = Zero }
    let createReporter = createReporterWithPrinter consolePrinter
    
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
        EndTestRun = id
        Success = Report.success;
        Zero = Report.create () }


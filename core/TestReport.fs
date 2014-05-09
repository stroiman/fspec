namespace FSpec.Core
open System

/// Represenations of the colors used to print to the console
type Color =
    | Red | Yellow | Green | Default

type Reporter<'T> = {
    BeginGroup : ExampleGroup.T -> 'T -> 'T
    ReportExample: Example.T -> TestResultType -> 'T -> 'T
    EndTestRun: 'T -> 'T
    EndGroup: 'T -> 'T
    Success: 'T -> bool
    Zero: 'T }

module TreeReporter =
    type ExecutedExample = {
        Example: Example.T
        Result : TestResultType
        ContainingGroups: ExampleGroup.T List }

    type T = {
        FailedTests: ExecutedExample list
        Groups: ExampleGroup.T list
        Indentation: string list }
    let Zero = { 
        Groups = []
        FailedTests = []
        Indentation = [] }
    
    let printIndentation printer report =
        report.Indentation |> List.rev |> List.iter (printer Default)

    let exampleName x = x.Example |> Example.name
    let result ex = ex.Result

    let printFailedExample printer executedExample =
        let rec print indentation executedExample = 
            match executedExample.ContainingGroups with
            | head::tail ->
                head |> ExampleGroup.name |> (sprintf "%s%s\n" indentation) |> printer Default
                print (indentation + "  ") { executedExample with ContainingGroups = executedExample.ContainingGroups.Tail }
            | [] -> 
                sprintf "%s- %s - " indentation (executedExample |> exampleName) |> printer Default
                match result executedExample with
                | Failure _ | Error _ -> 
                    "FAILED\n" |> printer Red
                    sprintf "%A\n" executedExample.Result |> printer Default
                | Pending -> "PENDING\n" |> printer Yellow
                | _ -> ()
        match result executedExample with
        | Success -> ()
        | _ -> print "" { executedExample with ContainingGroups = executedExample.ContainingGroups |> List.rev }

    let beginGroup printer exampleGroup report =
        printIndentation printer report
        sprintf "%s\n" (exampleGroup |> ExampleGroup.name) |> printer Color.Default
        { report with 
            Indentation = "  " :: report.Indentation
            Groups = exampleGroup :: report.Groups }

    let endGroup report = 
        { report with 
            Indentation = report.Indentation.Tail
            Groups = report.Groups.Tail }
    
    let getSummary report =
        let folder (success,pending,fail) = function
            | Failure _ | Error _ -> (success,pending,fail+1)
            | Pending -> (success,pending+1,fail)
            | Success -> (success+1,pending,fail)
        report.FailedTests |> 
        List.map result |> 
        List.fold folder (0,0,0)

    let printSummary printer report =
        let (success,pending,failed) =  getSummary report
        match (failed,pending) with
        | (0,0) -> ()
        | (0,_) -> "There are pending examples: \n" |> printer Yellow
        | _ -> "There are filed examples: \n" |> printer Red
        report.FailedTests |> List.iter (printFailedExample printer)
        sprintf "%d success, %d pending, %d failed\n" success pending failed |> printer Default
        report

    let reportExample printer example result report =
        printIndentation printer report
        sprintf "- %s - " (example |> Example.name) |> printer Default
        let executedExample = 
            { Example = example; ContainingGroups = report.Groups; Result = result }
        match result with
        | Success -> sprintf "%s" "Success" |> printer Green
        | Pending -> sprintf "%s" "Pending" |> printer Yellow
        | Failure e -> sprintf "%A" e.Message |> printer Red
        | Error(_) -> sprintf "%A" result |> printer Red
        "\n" |> printer Default
        { report with FailedTests = executedExample :: report.FailedTests }
    let success report = 
        let (_,_,failed) =  getSummary report
        failed = 0

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
        ReportExample = reportExample printer;
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

    let reportExample example result (report : Report.T) =
        names <- (example |> Example.name) :: names
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
        EndGroup = endGroup;
        ReportExample = reportExample;
        EndTestRun = id
        Success = Report.success;
        Zero = Report.create () }


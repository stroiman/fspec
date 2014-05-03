namespace FSpec.Core

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

module Runner =
    let rec performSetup exampleGroups ctx =
        match exampleGroups with
            | [] -> ()
            | head::tail ->
                performSetup tail ctx
                head |> ExampleGroup.setups |> List.iter (fun y -> y ctx)
    
    let rec performTearDown exampleGroups ctx =
        match exampleGroups with
            | [] -> ()
            | head::tail ->
                head |> ExampleGroup.tearDowns |> List.iter (fun y -> y ctx)
                performTearDown tail ctx
    
    let run exampleGroup report =
        let rec run exampleGroups report =
            let exampleGroup = exampleGroups |> List.head
            let metaData = exampleGroups |> List.map ExampleGroup.getMetaData |> List.fold (fun state x -> x |> MetaData.merge state) MetaData.Zero
            let rec printNameStack(stack) : string =
                match stack with
                | []    -> ""
                | head::[] -> head
                | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

            let execExample (example:Example.T) =
                let metaDataStack = example.MetaData :: (exampleGroups |> List.map ExampleGroup.getMetaData)
                let metaData = metaDataStack |> List.fold MetaData.merge MetaData.Zero
                try
                    let context = metaData |> TestContext.create
                    try
                        performSetup exampleGroups context
                        example.Test context
                    finally
                        performTearDown exampleGroups context
                    Success
                with
                | PendingError -> Pending
                | AssertionError(e) -> Failure e
                | ex -> Error ex

            let runExample (example:Example.T) report =
                let nameStack = example.Name :: (exampleGroups |> List.map ExampleGroup.name |> List.filter (fun x -> x <> null))
                let name = printNameStack(nameStack)
                let testResult = execExample example
                Report.reportTestName name testResult report

            let report' = exampleGroup |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) report
            exampleGroup |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::exampleGroups) rep) report'
        run [exampleGroup] report
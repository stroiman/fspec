module FSpec.Core.Runner

let rec performSetup groupStack ctx =
    match groupStack with
        | [] -> ()
        | head::tail ->
            performSetup tail ctx
            head |> ExampleGroup.setups |> List.iter (fun y -> y ctx)

let rec performTearDown groupStack ctx =
    match groupStack with
        | [] -> ()
        | head::tail ->
            head |> ExampleGroup.tearDowns |> List.iter (fun y -> y ctx)
            performTearDown tail ctx

let doRun exampleGroup reporter report =
    let rec run groupStack report =
        let metaData = groupStack |> List.map ExampleGroup.getMetaData |> List.fold (fun state x -> x |> TestDataMap.merge state) TestDataMap.Zero

        let execExample (example:Example.T) =
            let metaDataStack = example.MetaData :: (groupStack |> List.map ExampleGroup.getMetaData)
            let metaData = metaDataStack |> List.fold TestDataMap.merge TestDataMap.Zero
            try
                let context = metaData |> TestContext.create
                try
                    try
                        performSetup groupStack context
                        example.Test context
                    finally
                        performTearDown groupStack context
                finally
                    TestContext.cleanup context
                Success
            with
            | PendingError -> Pending
            | AssertionError(e) -> Failure e
            | ex -> Error ex

        let runExample (example:Example.T) report =
            let testResult = execExample example
            reporter.ReportExample example testResult report

        let grp = groupStack |> List.head
        report 
        |> reporter.BeginGroup grp
        |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) grp
        |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::groupStack) rep) grp
        |> reporter.EndGroup 

    run [exampleGroup] report

/// Runs a collection of top level group specs, using a specific
/// reporter to report progress. The generated report is returned
/// to the caller.
let run reporter topLevelGroups =
    let fold r = Seq.fold (fun r g -> doRun g reporter r) r topLevelGroups
    reporter.BeginTestRun ()
    |> fold
    |> reporter.EndTestRun
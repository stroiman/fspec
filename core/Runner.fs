module FSpec.Core.Runner

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

let doRun exampleGroup reporter report =
    let cleanupContext (ctx : TestContext) =
        let tryDispose (x:obj) = 
            match x with
            | :? System.IDisposable as d -> d.Dispose ()
            | _ -> ()
        ctx.Data.Data |> Map.iter (fun _ x -> tryDispose x)

    let rec run exampleGroups report =
        let exampleGroup = exampleGroups |> List.head
        let report = reporter.BeginGroup exampleGroup report
        let metaData = exampleGroups |> List.map ExampleGroup.getMetaData |> List.fold (fun state x -> x |> TestDataMap.merge state) TestDataMap.Zero

        let execExample (example:Example.T) =
            let metaDataStack = example.MetaData :: (exampleGroups |> List.map ExampleGroup.getMetaData)
            let metaData = metaDataStack |> List.fold TestDataMap.merge TestDataMap.Zero
            try
                let context = metaData |> TestContext.create
                try
                    try
                        performSetup exampleGroups context
                        example.Test context
                    finally
                        performTearDown exampleGroups context
                finally
                    context |> cleanupContext
                Success
            with
            | PendingError -> Pending
            | AssertionError(e) -> Failure e
            | ex -> Error ex

        let runExample (example:Example.T) report =
            let testResult = execExample example
            reporter.ReportExample example testResult report

        let report'' = exampleGroup |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) report
        let report''' = exampleGroup |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::exampleGroups) rep) report''
        reporter.EndGroup report'''
    run [exampleGroup] report

let run exampleGroup report =
    let classicReporter = ClassicReporter()
    let reporter = classicReporter.createReporter ()
    doRun exampleGroup reporter report

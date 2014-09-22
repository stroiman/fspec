namespace FSpec
open ExampleGroup

module Configuration =
    type T = {
        Include : TestDataMap.T -> bool
        Exclude : TestDataMap.T -> bool
    }
    let defaultConfig = 
        {
            Include = TestDataMap.containsKey "focus"
            Exclude = TestDataMap.containsKey "slow"
        }
    let includeAll =
        {
            Include = fun x -> false
            Exclude = fun x -> false
        }

module Runner =
    open Configuration
            
    let runMany ctx = List.rev >> List.iter (fun x -> x ctx)
    let rec performSetup groupStack ctx =
        match groupStack with
            | [] -> ()
            | x::xs ->
                performSetup xs ctx
                x.Setups |> runMany ctx

    let rec performTearDown groupStack ctx =
        match groupStack with
            | [] -> ()
            | x::xs ->
                x.TearDowns |> runMany ctx
                performTearDown xs ctx

    let doRun exampleGroup reporter report =
        let rec run groupStack report =
            let execExample example =
                let exMetaData = example |> Example.getMetaData
                let metaDataStack = exMetaData :: (groupStack |> List.map (fun x -> x.MetaData))
                let metaData = metaDataStack |> List.fold TestDataMap.merge TestDataMap.Zero
                try
                    use context = metaData |> TestContext.create
                    try
                        performSetup groupStack context
                        example |> Example.run context
                    finally
                        performTearDown groupStack context
                    Success
                with
                | PendingError -> Pending
                | AssertionError(e) -> Failure e
                | ex -> Error ex

            let runExample example =
                execExample example 
                |> reporter.ReportExample example 

            let grp = groupStack |> List.head
            report 
            |> reporter.BeginGroup grp
            |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) grp
            |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::groupStack) rep) grp
            |> reporter.EndGroup 

        run [exampleGroup] report

    let fromConfig cfg =
        let filterExamples f groups =
            let filteredGroups = groups |> ExampleGroup.filterGroups f
            match filteredGroups with
            | [] -> groups
            | x -> x

        fun reporter topLevelGroups ->
            let filteredGroups = 
                topLevelGroups 
                |> List.ofSeq
                |> filterExamples cfg.Include
                |> ExampleGroup.filterGroups (cfg.Exclude >> not)
            let fold r = List.fold (fun r g -> doRun g reporter r) r filteredGroups
            reporter.BeginTestRun ()
            |> fold
            |> reporter.EndTestRun
            
    /// Runs a collection of top level group specs, using a specific
    /// reporter to report progress. The generated report is returned
    /// to the caller.
    let run reporter topLevelGroups =
        let runner = defaultConfig |> fromConfig
        topLevelGroups |> runner reporter

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

module RunnerHelper =
    let createWrapper<'T> (reporter : Reporter<'T>) =
        let rec create (state:'T) =
            let beginGroup desc = reporter.BeginGroup desc state |> create
            let reportExample desc result = reporter.ReportExample desc result state |> create
            let endTestRun () = reporter.EndTestRun state :> obj
            let endGroup () = reporter.EndGroup state |> create
            let beginTestRun () = reporter.BeginTestRun () |> create
            { new IReporter with
                member __.BeginGroup x = beginGroup x
                member __.ReportExample x r = reportExample x r
                member __.EndTestRun () = endTestRun ()
                member __.EndGroup () = endGroup ()
            }
        reporter.BeginTestRun () |> create

module Runner =
    open Configuration
    open RunnerHelper

    type SingleExample = {
        Example : Example.T
        ContainingGroups : ExampleGroup.T list }
            
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

    let runSingleExample x =
        let example = x.Example
        let groupStack = x.ContainingGroups
        let metaDataStack = example.MetaData :: (groupStack |> List.map (fun x -> x.MetaData))
        let metaData = metaDataStack |> List.fold TestDataMap.merge TestDataMap.Zero
        use context = metaData |> TestContextImpl.create
        try
            performSetup groupStack context
            example |> Example.run context
        finally
            performTearDown groupStack context

    let execExample x =
        try
            runSingleExample x
            Success
        with
        | PendingError -> Pending
        | AssertionError(e) -> Failure e
        | ex -> Error ex

    let doRun exampleGroup =
        let rec run groupStack (reporter:IReporter) =
            let runExample (example:Example.T) (reporter:IReporter) =
                { Example = example;
                  ContainingGroups = groupStack }
                |> execExample
                |> reporter.ReportExample (example |> Example.getDescriptor)

            let grp = groupStack |> List.head
            reporter.BeginGroup (grp |> ExampleGroup.getDescriptor)
            |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) grp
            |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::groupStack) rep) grp
            |> fun x -> x.EndGroup()
        run [exampleGroup]

    let filterGroupsFromConfig cfg topLevelGroups =
        let filterExamples f groups =
            let filteredGroups = groups |> ExampleGroup.filterGroups f
            match filteredGroups with
            | [] -> groups
            | x -> x
        topLevelGroups 
        |> List.ofSeq
        |> filterExamples cfg.Include
        |> ExampleGroup.filterGroups (cfg.Exclude >> not)

    let fromConfigWrapped cfg =
        fun (reporter : IReporter) topLevelGroups ->
            let filteredGroups = 
                topLevelGroups
                |> filterGroupsFromConfig cfg
            let fold r = List.fold (fun r g -> doRun g r) r filteredGroups
            let r = reporter |> fold
            r.EndTestRun()

    let fromConfig<'T> cfg =
        fun (reporter : Reporter<'T>) topLevelGroups ->
            let f = fromConfigWrapped cfg
            let wrapper = createWrapper reporter
            let result = f wrapper topLevelGroups 
            result :?> 'T
            
    /// Runs a collection of top level group specs, using a specific
    /// reporter to report progress. The generated report is returned
    /// to the caller.
    let runWithWrapper reporter topLevelGroups =
        let runner = defaultConfig |> fromConfigWrapped
        topLevelGroups |> runner reporter

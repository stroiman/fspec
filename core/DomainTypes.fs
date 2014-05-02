module FSpec.Core.DomainTypes

module MetaData =
    type T = { Data: Map<string,obj> }
    let create data = 
        let downCastData = data |> List.map (fun (x,y) -> (x, y :> obj))
        { Data = Map<string,obj> downCastData }
    let Zero = create []
    let get<'T> name metaData = metaData.Data.Item name :?> 'T
        
    let merge a b =
        let newMap =
            a.Data
            |> Map.fold (fun state key value -> state |> Map.add key value) b.Data
        { Data= newMap }
    type T with
        member self.get<'T> name = get<'T> name self
        member self.add name value = { self with Data = self.Data |> Map.add name (value :> obj) }
        member self.Count with get() = self.Data.Count

module TestContext =
    type T = { 
        MetaData: MetaData.T;
        mutable Subect: obj;
        mutable Data: MetaData.T }
        with
            member self.metadata<'T> name = self.MetaData.get<'T> name
            member ctx.add name value = ctx.Data <- ctx.Data.add name value
            member ctx.get<'T> name = ctx.Data.get<'T> name
            member ctx.setSubject s = ctx.Subect <- s :> obj
            member ctx.subject<'T> () = ctx.Subect :?> 'T

    let create metaData = { MetaData = metaData; Data = MetaData.Zero; Subect = null }

module Example =
    type T = {
        Name: string; 
        Test: TestContext.T -> unit;
        MetaData: MetaData.T
    }
    let create name test = { Name = name; Test = test; MetaData = MetaData.Zero }
    let addMetaData metaData example = { example with MetaData = metaData }

module ExampleGroup =
    type TestFunc = TestContext.T -> unit
    type T = {
        Name: string
        Examples: Example.T list;
        Setups: TestFunc list;
        TearDowns: TestFunc list;
        ChildGroups : T list;
        }

    let create name = { 
        Name = name;
        Examples = [];
        Setups = [];
        TearDowns = [];
        ChildGroups = [];
    }
    let name grp = grp.Name
    let setups grp = grp.Setups
    let tearDowns grp = grp.TearDowns
    let addExample test ctx = { ctx with Examples = test::ctx.Examples }
    let addSetup setup ctx = { ctx with Setups = setup::ctx.Setups }
    let addTearDown tearDown ctx = { ctx with TearDowns = tearDown::ctx.TearDowns }
    let addChildContext child ctx = { ctx with ChildGroups = child::ctx.ChildGroups }
    let childGroups grp = grp.ChildGroups |> List.rev
    let examples grp = grp.Examples 
    let foldExamples folder state grp = grp.Examples |> List.rev |> List.fold folder state
    let foldChildGroups folder state grp = grp.ChildGroups |> List.rev |> List.fold folder state

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
            let rec printNameStack(stack) : string =
                match stack with
                | []    -> ""
                | head::[] -> head
                | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

            let runExample (example:Example.T) report =
                let nameStack = example.Name :: (exampleGroups |> List.map ExampleGroup.name |> List.filter (fun x -> x <> null))
                let name = printNameStack(nameStack)
                let testResult =
                    try
                        let context = example.MetaData |> TestContext.create
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
                Report.reportTestName name testResult report

            let report' = exampleGroup |> ExampleGroup.foldExamples (fun rep ex -> runExample ex rep) report
            exampleGroup |> ExampleGroup.foldChildGroups (fun rep grp -> run (grp::exampleGroups) rep) report'
        run [exampleGroup] report

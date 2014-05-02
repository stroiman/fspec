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
    let addExample test ctx = { ctx with Examples = test::ctx.Examples }
    let addSetup setup ctx = { ctx with Setups = setup::ctx.Setups }
    let addTearDown tearDown ctx = { ctx with TearDowns = tearDown::ctx.TearDowns }
    let addChildContext child ctx = { ctx with ChildGroups = child::ctx.ChildGroups }
    let childGroups grp = grp.ChildGroups
    let examples grp = grp.Examples

    let rec performSetup exampleGroups ctx =
        match exampleGroups with
            | [] -> ()
            | head::tail ->
                performSetup tail ctx
                head.Setups |> List.iter (fun y -> y ctx)
    
    let rec performTearDown exampleGroups ctx =
        match exampleGroups with
            | [] -> ()
            | head::tail ->
                head.TearDowns |> List.iter (fun y -> y ctx)
                performTearDown tail ctx
    
    let run exampleGroup results =
        let rec run exampleGroups (results : TestReport) =
            let exampleGroup = exampleGroups |> List.head
            let rec printNameStack(stack) : string =
                match stack with
                | []    -> ""
                | head::[] -> head
                | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

            exampleGroup.Examples |> List.rev |> List.iter (fun example -> 
                let nameStack = example.Name :: (exampleGroups |> List.map (fun x -> x.Name) |> List.filter (fun x -> x <> null))
                let name = printNameStack(nameStack)
                try
                    let context = example.MetaData |> TestContext.create
                    try
                        performSetup exampleGroups context
                        example.Test context
                    finally
                        performTearDown exampleGroups context
                    results.reportTestName name Success
                with
                | PendingError -> results.reportTestName name Pending
                | AssertionError(e) ->
                    results.reportTestName name (Failure(e))
                | ex -> 
                    results.reportTestName name (Error(ex))
            )

            exampleGroup.ChildGroups |> List.rev |> List.iter (fun x ->
                run (x::exampleGroups) results
            )
        run [exampleGroup] results

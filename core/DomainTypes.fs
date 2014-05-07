namespace FSpec.Core

type AssertionErrorInfo = { 
    Message: string
} with
    static member create = { Message = "" }

exception AssertionError of AssertionErrorInfo
exception PendingError

type TestResultType =
    | Success
    | Pending
    | Error of System.Exception
    | Failure of AssertionErrorInfo

module MetaData =
    type T = { Data: Map<string,obj> }
    let create data = 
        let downCastData = data |> List.map (fun (x,y) -> (x, y :> obj))
        { Data = Map<string,obj> downCastData }
    let Zero = create []
    let tryGet<'T> name metaData =
        metaData.Data.TryFind name 
        |> Option.bind (fun x -> Some (x :?> 'T))
    let get<'T> name metaData = metaData.Data.Item name :?> 'T
        
    let merge a b =
        let newMap =
            a.Data
            |> Map.fold (fun state key value -> state |> Map.add key value) b.Data
        { Data= newMap }
    type T with
        member self.Get<'T> name = get<'T> name self
        member self.Add name value = { self with Data = self.Data |> Map.add name (value :> obj) }
        member self.Count with get() = self.Data.Count
        static member (?) (self,name) = get name self
        static member (|||) (a,b) = merge a b

    let (++) a b = [(a,b)] |> create

type TestContext =
    { 
        MetaData: MetaData.T;
        mutable Subject: obj;
        mutable Data: MetaData.T }

module TestContextOperations =
    let getSubject<'T> (context : TestContext) = context.Subject :?> 'T

type TestContext
    with
        member self.metadata = self.MetaData
        member ctx.Set name value = ctx.Data <- ctx.Data.Add name value
        member ctx.Get<'T> name = ctx.Data.Get<'T> name
        member ctx.TryGet<'T> name = ctx.Data |> MetaData.tryGet<'T> name
        member ctx.SetSubject s = ctx.Subject <- s :> obj
        static member (?) (self:TestContext,name) = self.Get name 
        static member (?<-) (self:TestContext,name,value) = self.Set name value 
        static member create metaData = { MetaData = metaData; Data = MetaData.Zero; Subject = null }

module Example =
    type T = {
        Name: string; 
        Test: TestContext -> unit;
        MetaData: MetaData.T
    }
    let create name test = { Name = name; Test = test; MetaData = MetaData.Zero }
    let addMetaData metaData example = { example with Name = example.Name; MetaData = metaData }

module ExampleGroup =
    type TestFunc = TestContext -> unit
    type T = {
        Name: string
        Examples: Example.T list;
        Setups: TestFunc list;
        TearDowns: TestFunc list;
        ChildGroups : T list;
        MetaData : MetaData.T
        }

    let create name = { 
        Name = name;
        Examples = [];
        Setups = [];
        TearDowns = [];
        ChildGroups = [];
        MetaData = MetaData.Zero
    }
    let name grp = grp.Name
    let setups grp = grp.Setups
    let tearDowns grp = grp.TearDowns
    let addExample test grp = { grp with Examples = test::grp.Examples }
    let addSetup setup grp = { grp with Setups = setup::grp.Setups }
    let addTearDown tearDown grp = { grp with TearDowns = tearDown::grp.TearDowns }
    let addChildGroup child grp = { grp with ChildGroups = child::grp.ChildGroups }
    let addMetaData data grp = { grp with Name = grp.Name; MetaData = data }
    let getMetaData grp = grp.MetaData
    let childGroups grp = grp.ChildGroups |> List.rev
    let examples grp = grp.Examples 
    let foldExamples folder state grp = grp.Examples |> List.rev |> List.fold folder state
    let foldChildGroups folder state grp = grp.ChildGroups |> List.rev |> List.fold folder state


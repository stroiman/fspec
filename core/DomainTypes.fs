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
        static member (?) (self,name) = get name self
        static member (|||) (a,b) = merge a b

    let (++) a b = [(a,b)] |> create

module TestContext =
    type T = { 
        MetaData: MetaData.T;
        mutable Subect: obj;
        mutable Data: MetaData.T }

    let get<'T> name context = context.Data.get<'T> name
    let set<'T> name value context = context.Data <- context.Data.add name value
    type T with
        member self.metadata = self.MetaData
        member ctx.set name value = ctx.Data <- ctx.Data.add name value
        member ctx.get<'T> name = ctx.Data.get<'T> name
        member ctx.setSubject s = ctx.Subect <- s :> obj
        member ctx.subject<'T> () = ctx.Subect :?> 'T
        static member (?) (self,name) = get name self
        static member (?<-) (self,name,value) = set name value self

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
    let addExample test ctx = { ctx with Examples = test::ctx.Examples }
    let addSetup setup ctx = { ctx with Setups = setup::ctx.Setups }
    let addTearDown tearDown ctx = { ctx with TearDowns = tearDown::ctx.TearDowns }
    let addChildGroup child ctx = { ctx with ChildGroups = child::ctx.ChildGroups }
    let addMetaData data ctx = { ctx with MetaData = data }
    let getMetaData grp = grp.MetaData
    let childGroups grp = grp.ChildGroups |> List.rev
    let examples grp = grp.Examples 
    let foldExamples folder state grp = grp.Examples |> List.rev |> List.fold folder state
    let foldChildGroups folder state grp = grp.ChildGroups |> List.rev |> List.fold folder state


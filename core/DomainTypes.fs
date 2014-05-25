namespace FSpec.Core

/// Contains error information when an assertion failed
type AssertionErrorInfo = { Message: string } 
    with static member create = { Message = "" }

/// Thrown when an assertion failed
exception AssertionError of AssertionErrorInfo

/// Thrown when an example has yet to be implemented
exception PendingError

/// The different outcomes for an example
type TestResultType =
    | Success
    | Pending
    | Error of System.Exception
    | Failure of AssertionErrorInfo

/// Represents a collection of test specific data. Used for both
/// context data and example metadata
module TestDataMap =
    type T = { Data: Map<string,obj> }

    /// Creates a new collection of data from a list of key-value pairs.
    /// The values must be of the same data type, otherwise the compiler
    /// will complain. To mix different types of values, merge two 
    /// TestDataMap instances with the different data
    let create data = 
        let downCastData = data |> List.map (fun (x,y) -> (x, y :> obj))
        { Data = Map<string,obj> downCastData }

    /// Gets an empty TestDataMap
    let Zero = create []
    
    /// Attempts to get an element from the data map with a specific
    /// key. If the element is found, it is cast to 'T and returned,
    /// otherwise None is returned.
    let tryGet<'T> name metaData =
        match metaData.Data.TryFind name with
        | None -> None
        | Some x ->
            match x with
            | :? 'T as y -> Some y
            | _ -> failwithf "error getting data with key %A. Expected data of type %s but the actual type was %s"
                                name
                                typeof<'T>.Name
                                (x.GetType().Name)

    /// Retrieves a piece of data with the specified key. 
    /// Throws an exception of the data is not found.
    let get<'T> name metaData = 
        match tryGet<'T> name metaData with
        | Some x -> x
        | None -> failwithf "Test data with key \"%s\" not found" name

    /// Merges two TestDataMap instances. In case the same key
    /// exists in both data maps, the value from 'a' will win.
    let merge a b = { Data = a.Data |> Map.fold (fun s k v -> s |> Map.add k v) b.Data }

    /// Creates a TestDataMap with a single element
    let (++) k v = [(k,v)] |> create

    type T with
        member self.Get<'T> name = get<'T> name self
        member self.Add name value = { self with Data = self.Data |> Map.add name (value :> obj) }
        member self.Merge other = merge other self
        member self.Count with get() = self.Data.Count
        static member (?) (self,name) = get name self

        /// Synonym for merge
        static member (|||) (a,b) = merge a b

type TestContext =
    { 
        MetaData: TestDataMap.T
        mutable Disposables: System.IDisposable list
        mutable Subject: obj
        mutable Data: TestDataMap.T }
    with
        static member getSubject<'T> context = context.Subject :?> 'T
        static member registerDisposable (x:obj) ctx =
            match x with
            | :? System.IDisposable as d -> ctx.Disposables <- d::ctx.Disposables
            | _ -> ()
            
        static member cleanup ctx =
            ctx.Disposables |> List.iter (fun x -> x.Dispose())

module TestContextOperations =
    let getSubject<'T> (context : TestContext) = context.Subject :?> 'T

type TestContext
    with
        member self.metadata = self.MetaData
        member ctx.Set name value =
            ctx.Data <- ctx.Data.Add name value
            ctx |> TestContext.registerDisposable value

        member ctx.Get<'T> name = ctx.Data.Get<'T> name
        member ctx.TryGet<'T> name = ctx.Data |> TestDataMap.tryGet<'T> name
        member self.GetOrDefault<'T> name (initializer : TestContext -> 'T) =
            match self.TryGet<'T> name with
            | Some x -> x
            | None -> 
                let result = initializer self
                self.Set name result
                result
        
        member ctx.GetSubject<'T> () = TestContext.getSubject<'T> ctx
        member ctx.SetSubject s = 
            ctx.Subject <- s :> obj
            ctx |> TestContext.registerDisposable s

        static member (?) (self:TestContext,name) = self.Get name 
        static member (?<-) (self:TestContext,name,value) = self.Set name value 
        static member create metaData = { 
            MetaData = metaData
            Data = TestDataMap.Zero
            Disposables = []
            Subject = null }

module Example =
    type T = {
        Name: string; 
        Test: TestContext -> unit;
        MetaData: TestDataMap.T
    }
    let create name test = { Name = name; Test = test; MetaData = TestDataMap.Zero }
    let name example = example.Name
    let addMetaData data ex = { ex with Name = ex.Name; MetaData = ex.MetaData.Merge data }
    let run context example = example.Test context

module ExampleGroup =
    type TestFunc = TestContext -> unit
    type T = {
        Name: string
        Examples: Example.T list;
        Setups: TestFunc list;
        TearDowns: TestFunc list;
        ChildGroups : T list;
        MetaData : TestDataMap.T
        }

    let create name = { 
        Name = name;
        Examples = [];
        Setups = [];
        TearDowns = [];
        ChildGroups = [];
        MetaData = TestDataMap.Zero
    }
    let name grp = grp.Name
    let setups grp = grp.Setups
    let tearDowns grp = grp.TearDowns
    let addExample test grp = { grp with Examples = test::grp.Examples }
    let addSetup setup grp = { grp with Setups = setup::grp.Setups }
    let addTearDown tearDown grp = { grp with TearDowns = tearDown::grp.TearDowns }
    let addChildGroup child grp = { grp with ChildGroups = child::grp.ChildGroups }
    let addMetaData data grp = { grp with Name = grp.Name; MetaData = grp.MetaData.Merge data }
    let getMetaData grp = grp.MetaData
    let childGroups grp = grp.ChildGroups |> List.rev
    let examples grp = grp.Examples 
    let foldExamples folder grp state = grp.Examples |> List.rev |> List.fold folder state
    let foldChildGroups folder grp state = grp.ChildGroups |> List.rev |> List.fold folder state

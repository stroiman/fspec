namespace FSpec

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
    let containsKey key data = data.Data.ContainsKey key

    /// Creates a TestDataMap with a single element
    let (++) k v = [(k,v)] |> create

    type T with
        member self.Get<'T> name = get<'T> name self
        member self.Add name value = { self with Data = self.Data |> Map.add name (value :> obj) }
        member self.Merge other = merge other self
        member self.Count with get() = self.Data.Count
        member self.ContainsKey name = self.Data.ContainsKey name
        static member (?) (self,name) = get name self

        /// Synonym for merge
        static member (|||) (a,b) = merge a b

[<AbstractClass>]
type TestContext() =
    abstract member TryGet<'T> : string -> 'T option
    abstract Subject : obj with get
    abstract member SetSubject<'T> : (TestContext -> 'T) -> unit
    abstract member GetOrDefault<'T> : string -> (TestContext -> 'T) -> 'T
    abstract member Get<'T> : string -> 'T
    abstract member Set<'T> : string -> 'T -> unit
    abstract member GetSubject<'T> : unit -> 'T
    abstract member MetaData : TestDataMap.T with get

    static member (?) (self:TestContext,name) = self.Get name 
    static member (?<-) (self:TestContext,name,value) = self.Set name value 

type TestFunc = TestContext -> unit

module Example =
    [<ReferenceEqualityAttribute>]
    type T = {
        Name: string; 
        Test: TestFunc;
        MetaData: TestDataMap.T
    }
    let create name test = { Name = name; Test = test; MetaData = TestDataMap.Zero }
    let addMetaData data ex = { ex with Name = ex.Name; MetaData = ex.MetaData.Merge data }
    let run context example = example.Test context
    let hasMetaData name ex = ex.MetaData.ContainsKey name
    let getMetaData ex = ex.MetaData

module ExampleGroup =
    [<ReferenceEqualityAttribute>]
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
    let addExample test grp = { grp with Examples = test::grp.Examples }
    let addSetup setup grp = { grp with Setups = setup::grp.Setups }
    let addTearDown tearDown grp = { grp with TearDowns = tearDown::grp.TearDowns }
    let addChildGroup child grp = { grp with ChildGroups = child::grp.ChildGroups }
    let addMetaData data grp = { grp with Name = grp.Name; MetaData = grp.MetaData.Merge data }
    let foldExamples folder grp state = grp.Examples |> List.rev |> List.fold folder state
    let foldChildGroups folder grp state = grp.ChildGroups |> List.rev |> List.fold folder state
    let empty grp = match (grp.ChildGroups, grp.Examples) with
                    | ([],[]) -> true
                    | _ -> false

    let filterGroups (f:TestDataMap.T->bool) grps =
        let rec filterGroups metaData grps =
            let filterGroup grp =
                let metaData = grp.MetaData |> TestDataMap.merge metaData
                let filteredExamples = 
                    grp.Examples 
                    |> List.filter (fun x -> x.MetaData |> TestDataMap.merge metaData |> f)
                let filteredGroups = grp.ChildGroups |> filterGroups metaData
                { grp with 
                    Examples = filteredExamples; 
                    ChildGroups = filteredGroups 
                }

            let notEmpty = empty >> not
            grps |> List.map filterGroup |> List.filter notEmpty
        grps |> filterGroups TestDataMap.Zero

module FSpec.Core.Dsl
open Matchers

let pending = fun _ -> raise PendingError

module MetaData =
    type T = Map<string,obj>
    let create () = Map<string,obj> []

module TestContext =
    type T = { MetaData: MetaData.T }
    let create metaData = { MetaData = metaData }

module Example =
    type T = {
        Name: string; 
        Test: TestContext.T -> unit
    }
    let create name test = { Name = name; Test = test }

module ExampleGroup =
    type TestFunc = unit -> unit
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

    let rec perform_setup contexts =
        match contexts with
            | [] -> ()
            | head::tail ->
                perform_setup tail
                head.Setups |> List.iter (fun y -> y())
    
    let rec perform_teardown contexts =
        match contexts with
            | [] -> ()
            | head::tail ->
                head.TearDowns |> List.iter (fun y -> y())
                perform_teardown tail
    
    let rec run exampleGroups (results : TestReport) =
        let exampleGroup = exampleGroups |> List.head
        let rec printNameStack(stack) : string =
            match stack with
            | []    -> ""
            | head::[] -> head
            | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

        exampleGroup.Examples |> List.rev |> List.iter (fun x -> 
            let nameStack = x.Name :: (exampleGroups |> List.map (fun x -> x.Name) |> List.filter (fun x -> x <> null))
            let name = printNameStack(nameStack)
            try
                try
                    let context = MetaData.create () |> TestContext.create
                    perform_setup exampleGroups
                    x.Test context
                finally
                    perform_teardown exampleGroups
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

type TestCollection() =
    let mutable exampleGroup = ExampleGroup.create null
    let mutateGroup f = exampleGroup <- f exampleGroup

    member self.init (f: unit -> 'a) : (unit -> 'a) =
        let value = ref None
        self.before <| fun() ->
              value := None
        let r () =
            match !value with
            | None -> let result = f()
                      value := Some(result)
                      result
            | Some(x) -> x
        r

    member self.describe (name: string) (f: unit -> unit) = 
        let oldContext = exampleGroup
        exampleGroup <- ExampleGroup.create name
        f()
        exampleGroup <- ExampleGroup.addChildContext exampleGroup oldContext

    member self.before f = ExampleGroup.addSetup f |> mutateGroup
    member self.after f = ExampleGroup.addTearDown f |> mutateGroup
    member self.it name f = Example.create name f |> ExampleGroup.addExample |> mutateGroup
    member self.run(results) = ExampleGroup.run [exampleGroup] results
    member self.run() = self.run(TestReport())

let c = TestCollection()
let describe = c.describe
let it = c.it
let before = c.before
let context = describe
let init = c.init

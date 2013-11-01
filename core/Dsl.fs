module FSpec.Core.Dsl
open Matchers

module TestContext =
    type testFunc = unit -> unit
    type Test = {Name: string; Test: unit -> unit}
    type T = {
        Name: string
        Tests: Test list;
        Setups: testFunc list;
        TearDowns: testFunc list;
        ParentContext : T option;
        ChildContexts : T list;
        }

    let create name parent = { 
        Name = name;
        ParentContext = parent;
        Tests = [];
        Setups = [];
        TearDowns = [];
        ChildContexts = [];
    }
    let addTest ctx test = { ctx with Tests = test::ctx.Tests }
    let addSetup ctx setup = { ctx with Setups = setup::ctx.Setups }
    let addTearDown ctx tearDown = { ctx with TearDowns = tearDown::ctx.TearDowns }
    let addChildContext ctx child = { ctx with ChildContexts = child::ctx.ChildContexts }

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
    
    let rec name_stack context =
        match context.ParentContext with
        | None    -> []
        | Some(x) -> (context.Name)::(name_stack x)

    let rec run (contexts : T list) (results : TestReport) =
        let context = contexts |> List.head
        let rec printNameStack(stack) : string =
            match stack with
            | []    -> ""
            | head::[] -> head
            | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

        context.Tests |> List.rev |> List.iter (fun x -> 
            perform_setup contexts
            let nameStack = x.Name :: (contexts |> List.map (fun x -> x.Name) |> List.filter (fun x -> x <> null))
            let name = printNameStack(nameStack)
            try
                x.Test()
                results.reportTestName name Success
            with
            | AssertionError(e) ->
                results.reportTestName name (Failure(e))
            | ex -> 
                results.reportTestName name (Error(ex))
            perform_teardown contexts
        )

        context.ChildContexts |> List.rev |> List.iter (fun x ->
            run (x::contexts) results
        )

type TestCollection() =
    let mutable context = TestContext.create null None

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
        let oldContext = context
        context <- TestContext.create name (Some(context))
        f()
        let newContext = context
        context <- oldContext
        context <- TestContext.addChildContext context newContext

    member self.before (f: unit -> unit) =
        context <- TestContext.addSetup context f

    member self.after (f: unit -> unit) =
        context <- TestContext.addTearDown context f

    member self.it (name: string) (f: unit -> unit) = 
        context <- TestContext.addTest context { Name = name; Test = f}

    member self.run(results: TestReport) =
        TestContext.run [context] results

    member self.run() = 
        self.run(TestReport())

let c = TestCollection()
let describe = c.describe
let it = c.it
let before = c.before
let init = c.init

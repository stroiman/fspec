[<AutoOpen>]
module FSpec.Core.SuiteBuilders
open Expectations

type TestResultType =
    | Success
    | Error
    | Failure of AssertionErrorInfo

type TestReport() =
    let mutable noOfTestsRun = 0
    let mutable noOfFails = 0
    let mutable output = []
    let mutable failed = []

    member self.reportTestRun () =
        noOfTestsRun <- noOfTestsRun + 1

    member self.reportFailure () =
        noOfFails <- noOfFails + 1

    member self.summary() = 
        sprintf "%d run, %d failed" noOfTestsRun noOfFails

    member self.success() =
        noOfFails = 0

    member self.reportTestName name result =
        let name2 = match result with
                    | Success -> sprintf "%s - passed" name
                    | Error -> sprintf "%s - failed" name
                    | Failure(errorInfo) -> 
                        sprintf "%s - failed - %s" name errorInfo.Message
        match result with
            | Success -> ()
            | _ -> failed <- name2::failed
        output <- name2::output

    member self.testOutput() =
        output |> List.rev

    member self.failedTests() = 
        failed |> List.rev

type Test = {Name: string; test: unit -> unit}

type TestCollection(parent, name) =
    let mutable tests = []
    let mutable setups = []
    let mutable tearDowns = []
    let mutable contexts = []
    let mutable current = None
    new () = TestCollection(None, null)

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
        match current with 
        | None -> let innerCollection = TestCollection(Some(self), name)
                  current <- Some(innerCollection)
                  f()
                  current <- None
                  contexts <- innerCollection::contexts
        | Some(v) -> v.describe name f

    member self.before (f: unit -> unit) =
        match current with
        | None    -> setups <- f::setups
        | Some(v) -> v.before f

    member self.after (f: unit -> unit) =
        match current with
        | None    -> tearDowns <- f::tearDowns
        | Some(v) -> v.after f

    member self.it (name: string) (f: unit -> unit) = 
        match current with
        | None    -> tests <- {Name = name; test = f}::tests
        | Some(v) -> v.it name f

    member self.perform_setup() =
        match parent with
        | None    -> ()
        | Some(x) -> x.perform_setup()
        setups |> List.iter (fun y -> y())

    member self.performTearDown() =
        tearDowns |> List.iter (fun y -> y())
        match parent with
        | None    -> ()
        | Some(x) -> x.performTearDown()

    member self.nameStack () =
        match parent with
        | None    -> []
        | Some(x) -> name::x.nameStack()

    member self.run(results : TestReport) =
        let rec printNameStack(stack) : string =
            match stack with
            | []    -> ""
            | head::[] -> head
            | head::tail ->sprintf "%s %s" (printNameStack(tail)) head

        tests |> List.rev |> List.iter (fun x -> 
            self.perform_setup()
            results.reportTestRun()
            let nameStack = x.Name :: self.nameStack()
            let name = printNameStack(nameStack)
            try
                x.test()
                results.reportTestName name Success
            with
            | AssertionError(e) ->
                results.reportFailure()
                results.reportTestName name (Failure(e))
            | ex -> 
                results.reportFailure()
                results.reportTestName name Error
            self.performTearDown()
        )

        contexts |> List.rev |> List.iter (fun x ->
            x.run(results)
        )

    member self.run() = 
        self.run(TestReport())

let c = TestCollection()
let describe = c.describe
let it = c.it
let before = c.before
let init = c.init

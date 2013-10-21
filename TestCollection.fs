module FSpec

type TestResult() =
  let mutable noOfTestsRun = 0
  let mutable noOfFails = 0

  member self.reportTestRun () =
    noOfTestsRun <- noOfTestsRun + 1

  member self.reportFailure () =
    noOfFails <- noOfFails + 1

  member self.summary() = 
    sprintf "%d run, %d failed" noOfTestsRun noOfFails

  member self.success() =
    noOfFails = 0

type TestCollection(parent) =
  let mutable tests = []
  let mutable setups = []
  let mutable contexts = []
  let mutable current = None
  new () = TestCollection(None)

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
    | None -> let innerCollection = TestCollection(Some(self))
              current <- Some(innerCollection)
              f()
              current <- None
              contexts <- innerCollection::contexts
    | Some(v) -> v.describe name f

  member self.before (f: unit -> unit) =
    match current with
    | None    -> setups <- f::setups
    | Some(v) -> v.before f

  member self.it (name: string) (f: unit -> unit) = 
    match current with
    | None    -> tests <- f::tests
    | Some(v) -> v.it name f

  member self.perform_setup() =
    match parent with
    | None    -> ()
    | Some(x) -> x.perform_setup()
    setups |> List.iter (fun y -> y())

  member self.run(results : TestResult) =
    tests |> List.iter (fun x -> 
      self.perform_setup()
      results.reportTestRun()
      try
        x()
      with
      | x -> results.reportFailure()
    )

    contexts |> List.iter (fun x ->
      x.run(results)
    )

  member self.run() = 
    self.run(TestResult())

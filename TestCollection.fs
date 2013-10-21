module FSpec

type TestCollection(parent) =
  let mutable tests = []
  let mutable setups = []
  let mutable contexts = []
  let mutable current = None
  new () = TestCollection(None)

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

  member self.NoOfTests() = 
    let childTests = contexts |> List.map (fun x -> x.NoOfTests()) |> List.sum
    let myTests = tests |> List.length
    childTests + myTests

  member self.run() = 
    tests |> List.iter (fun x -> 
      self.perform_setup()
      x()
    )

    contexts |> List.iter (fun x ->
      x.run()
    )

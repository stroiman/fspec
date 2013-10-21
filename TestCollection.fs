module FSpec

type TestCollection() =
  let mutable tests = []
  let mutable setups = []

  member self.describe (name: string) (f: unit -> unit) = 
    f()

  member self.before (f: unit -> unit) =
    setups <- f:: setups

  member self.it (name: string) (f: unit -> unit) = 
    tests <- f::tests

  member self.NoOfTests() = tests |> List.length

  member self.run() = 
    tests |> List.iter (fun x -> 
      setups |> List.iter (fun y -> y())
      x()
    )

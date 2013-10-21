module FSpec

type TestCollection() =
  let mutable tests = []

  member self.describe (name: string) (f: unit -> unit) = 
    f()

  member self.it (name: string) (f: unit -> unit) = 
    tests <- f::tests

  member self.NoOfTests() = tests |> List.length

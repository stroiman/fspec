module FSpec.MbUnitWrapper
open MbUnit.Framework
open System.Collections.Generic

type TestFactory () =
  [<DynamicTestFactory>]
  member __.FSpecWrapper () : IEnumerable<Test> =
    let test = TestCase("Test", (fun _ -> ()))
    [
        test :> Test
    ] |> List.toSeq

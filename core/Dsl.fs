module FSpec.Core.Dsl
open DomainTypes

let pending = fun _ -> raise PendingError

type TestCollection() =
    let mutable exampleGroup = ExampleGroup.create null
    let mutateGroup f = exampleGroup <- f exampleGroup

    member self.init (f: unit -> 'a) : (unit -> 'a) =
        let value = ref None
        self.before <| fun _ ->
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

    member self.examples = exampleGroup
    member self.before f = ExampleGroup.addSetup f |> mutateGroup
    member self.after f = ExampleGroup.addTearDown f |> mutateGroup
    member self.it name f = Example.create name f |> ExampleGroup.addExample |> mutateGroup
    member self.it_ metadata name f = Example.create name f |> Example.addMetaData (MetaData.create metadata) |> ExampleGroup.addExample |> mutateGroup
    member self.run(results) = Runner.run exampleGroup results

let c = TestCollection()
let describe = c.describe
let it = c.it
let it_ = c.it_
let before = c.before
let context = describe
let init = c.init

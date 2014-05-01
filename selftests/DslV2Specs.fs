module FSpec.SelfTests.DslV2Specs
open FSpec.Core.DomainTypes

let specs =
    ExampleGroup.create "Group"
    |> ExampleGroup.addExample (Example.create "Example" (fun _ ->
        ()))
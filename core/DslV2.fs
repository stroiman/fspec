module FSpec.Core.DslV2
open FSpec.Core.DomainTypes

type Operation =
    | AddExampleOperation of Example.T
    | AddExampleGroupOperation of ExampleGroup.T

let it name func = AddExampleOperation <| Example.create name func
    
let applyOperation grp op =
    match op with
    | AddExampleOperation example -> grp |> ExampleGroup.addExample example
    | AddExampleGroupOperation childGrp -> grp |> ExampleGroup.addChildContext childGrp

let describe name operations =
    let grp = ExampleGroup.create name
    operations |> List.fold applyOperation grp
    
let context name operations =
    let grp = describe name operations
    grp |> AddExampleGroupOperation
    
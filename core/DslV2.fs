module FSpec.Core.DslV2

type Operation =
    | AddExampleOperation of Example.T
    | AddExampleGroupOperation of ExampleGroup.T
    | AddSetupOperation of ExampleGroup.TestFunc

let it name func = AddExampleOperation <| Example.create name func

let applyOperation grp op =
    match op with
    | AddExampleOperation example -> grp |> ExampleGroup.addExample example
    | AddExampleGroupOperation childGrp -> grp |> ExampleGroup.addChildContext childGrp
    | AddSetupOperation f -> grp |> ExampleGroup.addSetup f

let describe name operations =
    let grp = ExampleGroup.create name
    operations |> List.fold applyOperation grp
    
let context name operations =
    let grp = describe name operations
    grp |> AddExampleGroupOperation
    
let before f = AddSetupOperation f

let subject f = before (fun ctx -> ctx.setSubject (f ctx))
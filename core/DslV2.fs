module FSpec.Core.DslV2

let pending = fun _ -> raise PendingError
type Operation =
    | AddExampleOperation of Example.T
    | AddExampleGroupOperation of ExampleGroup.T
    | AddSetupOperation of ExampleGroup.TestFunc
    with
        static member applyMetaData metaData op =
            match op with
            | AddExampleOperation e ->
                e |> Example.addMetaData metaData |> AddExampleOperation
            | AddExampleGroupOperation g ->
                g |> ExampleGroup.addMetaData metaData |> AddExampleGroupOperation
            | _ -> failwith "not supported"
        static member (==>) (md, op) = Operation.applyMetaData md op

let it name func = AddExampleOperation <| Example.create name func

let applyOperation grp op =
    match op with
    | AddExampleOperation example -> grp |> ExampleGroup.addExample example
    | AddExampleGroupOperation childGrp -> grp |> ExampleGroup.addChildGroup childGrp
    | AddSetupOperation f -> grp |> ExampleGroup.addSetup f

let describe name operations =
    let grp = ExampleGroup.create name
    operations |> List.fold applyOperation grp
    
let context name operations =
    let grp = describe name operations
    grp |> AddExampleGroupOperation
    
let before f = AddSetupOperation f

let subject f = before (fun ctx -> ctx.setSubject (f ctx))

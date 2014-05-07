module FSpec.Core.Dsl

let pending = fun _ -> raise PendingError
type Operation =
    | AddExampleOperation of Example.T
    | AddExampleGroupOperation of ExampleGroup.T
    | AddSetupOperation of ExampleGroup.TestFunc
    | MultipleOperations of Operation list
    static member ApplyMetaData metaData op =
        match op with
        | AddExampleOperation e ->
            e |> Example.addMetaData metaData |> AddExampleOperation
        | AddExampleGroupOperation g ->
            g |> ExampleGroup.addMetaData metaData |> AddExampleGroupOperation
        | _ -> failwith "not supported"
    static member (==>) (md, op) = Operation.ApplyMetaData md op

let applyGroup s f = function
    | AddExampleGroupOperation grp -> s grp
    | _ -> f ()

let it name func = AddExampleOperation <| Example.create name func

let rec applyOperation grp op =
    match op with
    | AddExampleOperation example -> grp |> ExampleGroup.addExample example
    | AddExampleGroupOperation childGrp -> grp |> ExampleGroup.addChildGroup childGrp
    | AddSetupOperation f -> grp |> ExampleGroup.addSetup f
    | MultipleOperations o -> o |> List.fold applyOperation grp

let describe name operations =
    let grp = ExampleGroup.create name
    operations |> List.fold applyOperation grp
    |> AddExampleGroupOperation
    
let context = describe
    
let before f = AddSetupOperation f

let subject f = before (fun ctx -> ctx.SetSubject (f ctx))

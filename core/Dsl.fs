module FSpec.Core.Dsl
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

let pending = fun _ -> raise PendingError

type Operation =
    | AddExampleOperation of Example.T
    | AddExampleGroupOperation of ExampleGroup.T
    | AddSetupOperation of ExampleGroup.TestFunc
    | AddTearDownOperation of ExampleGroup.TestFunc
    | MultipleOperations of Operation list
    | AddMetaDataOperation of string*obj
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

let createExampleFromExpression (expr : Expr<MatchersV3.Matcher<_>>) =
    let name =
        match expr with
        | PropertyGet (x,y,z) -> 
            sprintf "should %s %s" 
                y.DeclaringType.Name
                (y.Name.ToLower())
        | _ -> failwith "Unrecognized pattern"
    Example.create name (fun _ -> ())

let itShould expr =
    AddExampleOperation <| createExampleFromExpression expr

let describe name operations =
    let rec applyOperation (grp,md) op =
        match op with
        | AddExampleOperation example -> 
            let example = example |> Example.addMetaData (TestDataMap.create md)
            let grp = grp |> ExampleGroup.addExample example
            (grp,[])
        | AddExampleGroupOperation childGrp -> 
            let cg = childGrp |> ExampleGroup.addMetaData (TestDataMap.create md)
            let grp = grp |> ExampleGroup.addChildGroup cg
            (grp,[])
        | AddSetupOperation f -> 
            let grp = grp |> ExampleGroup.addSetup f
            (grp,md)
        | AddTearDownOperation f -> 
            let grp = grp |> ExampleGroup.addTearDown f
            (grp,md)
        | MultipleOperations o -> 
            o |> List.fold applyOperation (grp,md)
        | AddMetaDataOperation (k,v) -> (grp, (k,v)::md)

    let grp = ExampleGroup.create name
    operations |> List.fold applyOperation (grp,[])
    |> fun (grp,_) -> grp
    |> AddExampleGroupOperation
    
let context = describe
let before f = AddSetupOperation f
let after f = AddTearDownOperation f
let subject f = before (fun ctx -> ctx.SetSubject (f ctx))
let examples x = MultipleOperations x

let (++) = TestDataMap.(++)
let (<<-) a b = AddMetaDataOperation(a,b)

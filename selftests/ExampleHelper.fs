module FSpec.SelfTests.ExampleHelper
open FSpec.Core
open Matchers
open Dsl

let pass = fun _ -> ()
let fail = fun _ -> failwithf "Test failure"

let anExampleGroupNamed = ExampleGroup.create
let anExampleGroup = anExampleGroupNamed "dummy"

let withExamples examples exampleGroup =
    let folder grp ex = ExampleGroup.addExample ex grp
    examples |> List.fold folder exampleGroup

let withSetupCode = ExampleGroup.addSetup
let withTearDownCode = ExampleGroup.addTearDown

let applyNestedContext f grp = grp |> f |> ExampleGroup.addChildGroup
let withNestedGroupNamed name f = anExampleGroupNamed name |> applyNestedContext f
let withNestedGroup f = anExampleGroup |> applyNestedContext f
let anExampleNamed name = Example.create name pass
let anExample = Example.create "dummy"
let aPassingExample = anExample pass
let aFailingExample = anExample fail
let aPendingExample = anExample pending

let createAnExampleWithMetaData metaData f =
    let metaData' = MetaData.create [metaData]
    anExample f |> Example.addMetaData metaData'

let run exampleGroup = 
    Runner.run exampleGroup (Report.create())

let runSingleExample example =
    anExampleGroup |> withExamples [example] |> run

let withAnExampleWithMetaData metaData =
    createAnExampleWithMetaData metaData (fun _ -> ())
    |> ExampleGroup.addExample
let withExampleCode f = Example.create "dummy" f |> ExampleGroup.addExample
let withAnExample = aPassingExample |> ExampleGroup.addExample
let withAnExampleNamed name = anExampleNamed name |> ExampleGroup.addExample

let shouldPass group =
    let report' = Runner.run group (Report.create())
    report' |> Report.success |> should equal true

let withMetaData data = MetaData.create [data] |> ExampleGroup.addMetaData

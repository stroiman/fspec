module FSpec.SelfTests.ExampleHelper
open FSpec.Core
open Matchers

let anExampleGroup = ExampleGroup.create "dummy"
let withExamples examples exampleGroup =
    let folder grp ex = ExampleGroup.addExample ex grp
    examples |> List.fold folder exampleGroup

let anExample = Example.create "dummy"

let createAnExampleWithMetaData metaData f =
    let metaData' = MetaData.create [metaData]
    anExample f |> Example.addMetaData metaData'

let runSingleExample example =
    let group = anExampleGroup |> withExamples [example]
    Runner.run group (Report.create())

let withSetupCode f = ExampleGroup.addSetup f
let withAnExampleWithMetaData metaData =
    createAnExampleWithMetaData metaData (fun _ -> ())
    |> ExampleGroup.addExample

let run exampleGroup = 
    Runner.run exampleGroup (Report.create())
    |> ignore

let shouldPass group =
    let report' = Runner.run group (Report.create())
    report' |> Report.success |> should equal true

let withMetaData data = MetaData.create [data] |> ExampleGroup.addMetaData

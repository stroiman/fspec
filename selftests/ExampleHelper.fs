module FSpec.SelfTests.ExampleHelper
open FSpec
open Dsl
open Matchers
open ExampleGroup

// Example building helpers
let pass = fun _ -> ()
let fail = fun _ -> raise (AssertionError { Message = "failed" })

let anExampleNamed name = Example.create name pass
let anExampleWithCode = Example.create "dummy"
let aPassingExample = anExampleWithCode pass
let aFailingExample = anExampleWithCode fail
let aPendingExample = anExampleWithCode pending
let anExample = aPassingExample
let anExceptionThrowingExample = anExampleWithCode (fun _ -> raise (new System.Exception()))

let withExampleMetaData md = TestDataMap.create [md] |> Example.addMetaData
let anExampleWithMetaData data = aPassingExample |> withExampleMetaData data

let aSlowExample = anExampleWithMetaData("slow", true)
let aFocusedExample = anExampleWithMetaData("focus", true)

// Example group building helpers
let anExampleGroupNamed = ExampleGroup.create
let anExampleGroup = anExampleGroupNamed "dummy"

let withMetaData data = TestDataMap.create [data] |> ExampleGroup.addMetaData
let withSetupCode = ExampleGroup.addSetup
let withTearDownCode = ExampleGroup.addTearDown

let applyNestedContext f grp = grp |> f |> ExampleGroup.addChildGroup
let withNestedGroupNamed name f = anExampleGroupNamed name |> applyNestedContext f
let withNestedGroup f = anExampleGroup |> applyNestedContext f

let withExamples examples exampleGroup =
    let folder grp ex = ExampleGroup.addExample ex grp
    examples |> List.fold folder exampleGroup

let withAnExampleWithMetaData metaData =
    anExample
    |> withExampleMetaData metaData
    |> ExampleGroup.addExample

let withExampleCode f = anExampleWithCode f |> ExampleGroup.addExample
let withAnExampleNamed name = anExampleNamed name |> ExampleGroup.addExample
let withAnExample = anExample |> ExampleGroup.addExample

// Run helper
let run exampleGroup = 
    let reporter = Helpers.TestReporter.Report()
    Runner.runWithWrapper reporter [exampleGroup] :?> Helpers.TestReporter.T

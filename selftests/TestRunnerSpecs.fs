module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open Matchers
open DslV2

let anExampleGroup = ExampleGroup.create "dummy"
let withExamples examples exampleGroup =
    let folder grp ex = ExampleGroup.addExample ex grp
    examples |> List.fold folder exampleGroup

let createAnExampleWithMetaData metaData f =
    let metaData' = MetaData.create [metaData]
    Example.create "dummy" f |> Example.addMetaData metaData'

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

let specs =
    describe "Test runner" [
        context "metadata handling" [
            context "test contains metadata" [
                it "passes the metadata to the test" <| fun _ ->
                    createAnExampleWithMetaData ("answer", 42) <| fun ctx ->
                        ctx.metadata "answer" |> should equal 42
                    |> runSingleExample
                    |> Report.success |> should equal true

                it "passes the metadata to the setup" <| fun _ ->
                    let actual = ref 0
                    anExampleGroup
                    |> withSetupCode (fun ctx -> actual := ctx.metadata "answer")
                    |> withAnExampleWithMetaData ("answer", 42)
                    |> run
                    !actual |> should equal 42
            ]
        ]
    ]
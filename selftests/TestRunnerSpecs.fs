module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open Matchers
open DslV2

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

            context "example group contains metadata" [
                subject <| fun _ ->
                    anExampleGroup
                    |> withMetaData ("source", "example group")

                context "test contains no metadata" [
                    subject <| fun ctx ->
                        ctx.subject ()
                        |> ExampleGroup.addExample (
                            anExample <| fun testCtx ->
                                ctx.add "source" (testCtx.metadata "source"))
                                
                    it "uses metadata from setup" <| fun ctx ->
                        ctx.subject () |> run
                        ctx.get "source" |> should equal "example group"
                ]   
                context "test overrides same metadata" [
                    subject <| fun ctx ->
                        ctx.subject ()
                        |> ExampleGroup.addExample (
                            createAnExampleWithMetaData ("source", "example") <| fun testCtx ->
                                ctx.add "source" (testCtx.metadata "source"))

                    it "uses the metadata specified in test" <| fun ctx ->
                        ctx.subject () |> run
                        ctx.get "source" |> should equal "example"
                ]
                context "nested example group overrides metadata" [
                    subject <| fun ctx ->
                        ctx.subject ()
                        |> ExampleGroup.addChildContext (
                            anExampleGroup
                            |> withMetaData ("source", "child context")
                            |> ExampleGroup.addExample (
                                anExample <| fun testCtx ->
                                    ctx.add "source" (testCtx.metadata "source")))

                    it "uses the metadata from the child group" <| fun ctx ->
                        ctx.subject () |> run
                        ctx.get "source" |> should equal "child context"
                ]
            ]
        ]
    ]
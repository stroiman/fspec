module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open Matchers
open Dsl
open ExampleHelper
open TestContextOperations

let specs =
    describe "Test runner" [
        context "metadata handling" [
            context "test contains metadata" [
                it "passes the metadata to the test" <| fun _ ->
                    createAnExampleWithMetaData ("answer", 42) <| fun ctx ->
                        ctx.metadata?answer |> should equal 42
                    |> runSingleExample
                    |> Report.success |> should equal true

                it "passes the metadata to the setup" <| fun _ ->
                    let actual = ref 0
                    anExampleGroup
                    |> withSetupCode (fun ctx -> actual := ctx.metadata?answer)
                    |> withAnExampleWithMetaData ("answer", 42)
                    |> run |> ignore
                    !actual |> should equal 42
            ]

            context "example group contains metadata" [
                subject <| fun _ ->
                    anExampleGroup
                    |> withMetaData ("source", "example group")

                context "test contains no metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> ExampleGroup.addExample (
                            anExample <| fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source))
                                
                    it "uses metadata from setup" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should equal "example group"
                ]   
                context "test overrides same metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> ExampleGroup.addExample (
                            createAnExampleWithMetaData ("source", "example") <| fun testCtx ->
                                ctx.Set "source" (testCtx.metadata?source))

                    it "uses the metadata specified in test" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should equal "example"
                ]
                context "nested example group overrides metadata" [
                    subject <| fun ctx ->
                        ctx |> getSubject
                        |> ExampleGroup.addChildGroup(
                            anExampleGroup
                            |> withMetaData ("source", "child context")
                            |> ExampleGroup.addExample (
                                anExample <| fun testCtx ->
                                    ctx.Set "source" (testCtx.metadata?source)))

                    it "uses the metadata from the child group" <| fun ctx ->
                        ctx |> getSubject |> run |> ignore
                        ctx.Get "source" |> should equal "child context"
                ]
            ]
        ]
    ]

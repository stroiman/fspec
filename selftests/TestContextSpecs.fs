module FSpec.SelfTests.TestContextSpecs
open FSpec.Core
open DslV2
open MatchersV2
open DomainTypes

let specs =
    describe "TestContext" [
        it "can receive data" <| fun _ ->
            let ctx = TestContext.create MetaData.Zero
            ctx.add "answer" 42
            ctx.get "answer" |> should equal 42
    ]

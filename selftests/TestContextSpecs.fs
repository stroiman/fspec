module FSpec.SelfTests.TestContextSpecs
open FSpec.Core
open Dsl
open Matchers

let specs =
    describe "TestContext" [
        it "can receive data" <| fun _ ->
            let ctx = TestContext.create MetaData.Zero
            ctx.add "answer" 42
            ctx.get "answer" |> should equal 42
    ]

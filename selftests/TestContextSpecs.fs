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
        
        it "can set and retrieve data using operator ?" <| fun _ ->
            let ctx = TestContext.create MetaData.Zero
            ctx?answer <- 42
            ctx?answer |> should equal 42
    ]

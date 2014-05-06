module FSpec.SelfTests.TestContextSpecs
open FSpec.Core
open Dsl
open Matchers

let itCanLookupTheData =
    MultipleOperations [
        it "can be retrieved using 'get'" <| fun c ->
            c.get "answer" |> should equal 42
        
        it "can be retrieved using dynamic operator" <| fun c ->
            c?answer |> should equal 42
    ]

let specs =
    describe "TestContext" [
        context "data initialized with dynamic operator" [
            before (fun c -> c?answer <- 42)
            itCanLookupTheData
        ]

        context "data initialized with 'set'" [
            before (fun c -> c.set "answer" 42)
            itCanLookupTheData
        ]
    ]

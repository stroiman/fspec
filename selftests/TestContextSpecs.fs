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

        describe "tryGet" [
            context "data initialized in the context" [
                before (fun c -> c?data <- 42)
                
                it "retrieves the expected data" (fun c ->
                    match c.tryGet "data" with
                    | Some x -> x |> should equal 42
                    | None -> failwith "Data not found"
                )
            ]

            context "data not initialized in the context" [
                it "returns none" (fun c ->
                    match c.tryGet "data" with
                    | None -> ()
                    | _ -> failwith "Data should not be found"
                )
            ]
        ]
    ]

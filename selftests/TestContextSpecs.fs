module FSpec.SelfTests.TestContextSpecs
open FSpec.Core
open Dsl
open Matchers
open TestContextOperations

let itCanLookupTheData =
    MultipleOperations [
        it "can be retrieved using 'get'" <| fun c ->
            c.Get "answer" |> should equal 42
        
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
            before (fun c -> c.Set "answer" 42)
            itCanLookupTheData
        ]

        describe "tryGet" [
            context "data initialized in the context" [
                before (fun c -> c?data <- 42)
                
                it "retrieves the expected data" (fun c ->
                    match c.TryGet "data" with
                    | Some x -> x |> should equal 42
                    | None -> failwith "Data not found"
                )
            ]

            context "data not initialized in the context" [
                it "returns none" (fun c ->
                    match c.TryGet "data" with
                    | None -> ()
                    | _ -> failwith "Data should not be found"
                )
            ]
        ]

        describe "subject" [
            context "subject is a function" [
                subject <| fun _ ->
                    (fun () -> ())
                it "is evaluated when a matcher expects a function" <| fun ctx ->
                    ctx |> getSubject |> shouldNot fail
            ]
        ]
    ]

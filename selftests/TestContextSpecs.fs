module FSpec.SelfTests.TestContextSpecs
open FSpec.Core
open Dsl
open Matchers
open TestContextOperations


type DisposeSpy () =
    member val Disposed = false with get, set
    interface System.IDisposable with
        member self.Dispose () = self.Disposed <- true
let createContext = TestDataMap.Zero |> TestContext.create
        
let specs =
    describe "TestContext" [
        describe "set and get data" [
            let itCanLookupTheData =
                examples [
                    it "can be retrieved using 'get'" 
                        (fun ctx -> ctx.Get "answer" |> should equal 42)
                    
                    it "can be retrieved using dynamic operator" 
                        (fun ctx -> ctx?answer |> should equal 42)
                ]
                
            yield context "data initialized with dynamic operator" [
                before (fun ctx -> ctx?answer <- 42)
                itCanLookupTheData
            ]

            yield context "data initialized with 'set' function" [
                before (fun ctx -> ctx.Set "answer" 42)
                itCanLookupTheData
            ]
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

        describe "cleanup" [
            it "calls dispose on objects" <| fun _ ->
                let x = new DisposeSpy()
                let ctx = createContext
                ctx?x <- x
                ctx |> TestContext.cleanup
                x.Disposed |> should equal true

            it "disposes instances, that are no longer present" <| fun _ ->
                let x = new DisposeSpy()
                let y = new DisposeSpy()
                let ctx = createContext
                ctx?x <- x
                ctx?x <- y
                ctx |> TestContext.cleanup
                x.Disposed |> should equal true

            it "calls dispose on Subject" (fun _ ->
                let x = new DisposeSpy()
                let ctx = createContext
                ctx.SetSubject x
                ctx |> TestContext.cleanup
                x.Disposed |> should equal true

            )
        ]
    ]

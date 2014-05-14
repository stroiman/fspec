module FSpec.SelfTests.MetaDataSpecs
open FSpec.Core
open Dsl
open Matchers
open TestContextOperations

type TestContext with
    member self.data with get () = self |> getSubject<TestDataMap.T>

let specs =
    describe "MetaData" [
        context "Dynamic operator" [
            it "can retrieve data" (fun _ ->
                let md = TestDataMap.create [("data",1)]
                md?data |> should equal 1
            )
        ]
        context "Merge" [
            context "When two metadata sets have different objects" [
                subject <| fun _ ->
                    let a = TestDataMap.create [("a",1)]
                    let b = TestDataMap.create [("b",2)]
                    a |> TestDataMap.merge b
                    
                it "contains two elements" <| fun ctx ->
                    let result = ctx.data
                    result.Count |> should equal 2

                it "returns a metadata with both objects" <| fun ctx ->
                    ctx.data.Get "a" |> should equal 1
                    ctx.data.Get "b" |> should equal 2
            ]

            context "when two metadata sets have same objects" [
                subject <| fun _ ->
                    let a = TestDataMap.create [("a",1)]
                    let b = TestDataMap.create [("a",2)]
                    a |> TestDataMap.merge b
                    
                it "should have one element" <| fun ctx ->
                    ctx.data.Count |> should equal 1

                it "returns a metadata with the 'last' given value" <| fun ctx ->
                    ctx.data.Get "a" |> should equal 2
            ]
        ]
    ]

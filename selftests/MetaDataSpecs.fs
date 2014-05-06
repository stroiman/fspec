module FSpec.SelfTests.MetaDataSpecs
open FSpec.Core
open Dsl
open Matchers

type TestContext.T with
    member self.data with get () = self.Subject<MetaData.T> ()

let specs =
    describe "MetaData" [
        context "Dynamic operator" [
            it "can retrieve data" (fun _ ->
                let md = MetaData.create [("data",1)]
                md?data |> should equal 1
            )
        ]
        context "Merge" [
            context "When two metadata sets have different objects" [
                subject <| fun _ ->
                    let a = MetaData.create [("a",1)]
                    let b = MetaData.create [("b",2)]
                    a |> MetaData.merge b
                    
                it "contains two elements" <| fun ctx ->
                    let result = ctx.Subject<MetaData.T>()
                    result.Count |> should equal 2

                it "returns a metadata with both objects" <| fun ctx ->
                    ctx.data.Get "a" |> should equal 1
                    ctx.data.Get "b" |> should equal 2
            ]

            context "when two metadata sets have same objects" [
                subject <| fun _ ->
                    let a = MetaData.create [("a",1)]
                    let b = MetaData.create [("a",2)]
                    a |> MetaData.merge b
                    
                it "should have one element" <| fun ctx ->
                    ctx.Subject<MetaData.T>().Count |> should equal 1

                it "returns a metadata with the 'last' given value" <| fun ctx ->
                    ctx.Subject<MetaData.T>().Get "a" |> should equal 2
            ]
        ]
    ]

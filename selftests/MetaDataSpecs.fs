module FSpec.SelfTests.MetaDataSpecs
open FSpec.Core
open DslV2
open DomainTypes
open Matchers
open DslHelper

type TestContext.T with
    member self.data with get () = self.subject<MetaData.T> ()

let specs =
    describe "MetaData" [
        context "Merge" [
            context "When two metadata sets have different objects" [
                subject <| fun _ ->
                    let a = MetaData.create [("a",1)]
                    let b = MetaData.create [("b",2)]
                    a |> MetaData.merge b
                    
                it "contains two elements" <| fun ctx ->
                    let result = ctx.subject<MetaData.T>()
                    result.Count |> should equal 2

                it "returns a metadata with both objects" <| fun ctx ->
                    ctx.data.get "a" |> should equal 1
                    ctx.data.get "b" |> should equal 2
            ]

            context "when two metadata sets have same objects" [
                subject <| fun _ ->
                    let a = MetaData.create [("a",1)]
                    let b = MetaData.create [("a",2)]
                    a |> MetaData.merge b
                    
                it "should have one element" <| fun ctx ->
                    ctx.subject<MetaData.T>().Count |> should equal 1

                it "returns a metadata with the 'last' given value" <| fun ctx ->
                    ctx.subject<MetaData.T>().get "a" |> should equal 2
            ]
        ]
    ]
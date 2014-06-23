module FSpec.SelfTests.MetaDataSpecs
open FSpec
open Dsl
open Matchers

type TestContext with
    member self.data with get() = self.GetSubject<TestDataMap.T> ()
let createMap = TestDataMap.create
let mergeMap = TestDataMap.merge

let specs =
    describe "MetaData" [
        describe "dynamic get operator" [
            it "can retrieve data" <| fun _ ->
                let md = createMap [("data",1)]
                md?data.Should (equal 1)
        ]

        describe "merge" [
            context "When two metadata sets have different objects" [
                subject <| fun _ ->
                    let a = createMap [("a",1)]
                    let b = createMap [("b",2)]
                    a |> TestDataMap.merge b
                    
                it "contains two elements" <| fun ctx ->
                    let result = ctx.data
                    result.Count.Should (equal 2)

                it "returns a metadata with both objects" <| fun ctx ->
                    ctx.data.Get "a" |> should (equal 1)
                    ctx.data.Get "b" |> should (equal 2)
            ]

            context "when two metadata sets have same objects" [
                subject <| fun _ ->
                    let a = createMap [("a",1)]
                    let b = createMap [("a",2)]
                    a |> mergeMap b
                    
                it "should have one element" <| fun ctx ->
                    ctx.data.Count.Should (equal 1)

                it "returns a metadata with the 'last' given value" <| fun ctx ->
                    ctx.data.Get "a" |> should (equal 2)
            ]
        ]
    ]

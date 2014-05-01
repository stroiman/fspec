module FSpec.SelfTests.MetaDataSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    describe "TestBuilder" <| fun _ ->
        describe "TestMetaData" <| fun _ ->
            it "is initialized from test" <| fun _ ->
                let sut = TestCollection()
                sut.it_ [("answer", 42)] "dummy" <| fun _ -> ()
                sut.examples.Examples.Head.MetaData.get "answer" |> should equal 42
                
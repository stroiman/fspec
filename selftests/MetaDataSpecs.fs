module FSpec.SelfTests.MetaDataSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    describe "MetaData" <| fun _ ->
        it_ [("answer", 42)] "can be retrieved from context" <| fun ctx ->
            ctx.metadata "answer" |> should equal 42
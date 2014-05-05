module FSpec.SelfTests.DslV2Specs
open FSpec.Core
open Dsl
open Matchers
open MetaData

let pass = fun _ -> ()

let specs =
    describe "Example building DSL" [
        context "an example group initialized with one example" [
            before <| fun ctx ->
                describe "Group" [
                    it "Test" pass
                ] |> ctx.setSubject

            it "should have no child groups" <| fun ctx ->
                ctx.subject ()
                |> ExampleGroup.childGroups
                |> List.length |> should equal 0

            it "should have one example named 'Test'" <| fun ctx ->
                match ctx.subject () |> ExampleGroup.examples with
                | [ex] -> ex.Name |> should equal "Test"
                | _ -> failwith "Example count mismatch"
        ]

        it "builds dsl with nested example group" <| fun _ ->
            let group =
                describe "Group" [
                    context "Context" [
                        it "Test" pass
                ]]
            match group.ChildGroups with
            | [grp] -> 
                match grp.Examples with
                | [ex] -> ex.Name |> should equal "Test"
                | _ -> failwith "Bad examples"
            | _ -> failwith "Bad groups"

        it "builds examplegroup with setup" <| fun _ ->
            let group =
                describe "Group" [
                    before <| fun _ -> ()
                    before <| fun _ -> ()
                    it "Test" pass
                ]
            group.Setups.Length |> should equal 2

        it "builds example group with metadata" <| fun _ ->
            let group =
                describe "grp" [
                    ("answer" ++ 42) ==>
                    context "child" []]
            group.ChildGroups.Head.MetaData.get "answer" |> should equal 42

        it "builds example with metadata" <| fun _ ->
            let group =
                describe "grp" [
                    ("answer" ++ 42 |||
                     "question" ++ "universe" |||
                     "More" ++ Some "blah") ==>
                    it "Test" pass
                ]
            group.Examples.Head.MetaData.get "answer" |> should equal 42
            group.Examples.Head.MetaData.get "question" |> should equal "universe"
    ]
module FSpec.SelfTests.DslV2Specs
open FSpec.Core
open Dsl
open Matchers
open MetaData
open TestContextOperations

let pass = fun _ -> ()
let extractGroup = applyGroup id (fun _ -> failwith "error")

let specs =
    describe "Example building DSL" [
        context "an example group initialized with one example" [
            subject <| fun _ ->
                describe "Group" [
                    it "Test" pass
                ] |> extractGroup

            it "should have no child groups" <| fun ctx ->
                ctx |> getSubject
                |> ExampleGroup.childGroups
                |> List.length |> should equal 0

            it "should have one example named 'Test'" <| fun ctx ->
                match ctx |> getSubject |> ExampleGroup.examples with
                | [ex] -> ex.Name |> should equal "Test"
                | _ -> failwith "Example count mismatch"
        ]

        it "builds dsl with nested example group" <| fun _ ->
            let group =
                describe "Group" [
                    describe "Context" [
                        it "Test" pass
                ]] |> extractGroup
            match group.ChildGroups with
            | [grp] -> 
                match grp.Examples with
                | [ex] -> ex.Name |> should equal "Test"
                | _ -> failwith "Bad examples"
            | _ -> failwith "Bad groups"

        it "builds dsl with nested context" <| fun _ ->
            let group =
                describe "Group" [
                    context "Context" [
                        it "Test" pass
                ]] |> extractGroup
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
                ] |> extractGroup
            group.Setups.Length |> should equal 2

        it "builds example group with metadata" <| fun _ ->
            let group =
                describe "grp" [
                    ("answer" ++ 42) ==>
                    context "child" []] |> extractGroup
            group.ChildGroups.Head.MetaData.Get "answer" |> should equal 42

        it "builds example with metadata" <| fun _ ->
            let group =
                describe "grp" [
                    ("answer" ++ 42 |||
                     "question" ++ "universe" |||
                     "More" ++ Some "blah") ==>
                    it "Test" pass
                ] |> extractGroup
            group.Examples.Head.MetaData.Get "answer" |> should equal 42
            group.Examples.Head.MetaData.Get "question" |> should equal "universe"
    ]

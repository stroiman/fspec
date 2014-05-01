module FSpec.SelfTests.DslV2Specs
open FSpec.Core.DomainTypes
open FSpec.Core.DslV2
open FSpec.Core.MatchersV2

let pass = fun _ -> ()

let specs =
    describe "Example building DSL" [
        it "builds example groups" <| fun _ ->
            let group = 
                describe "Group" [
                    it "Test" pass
                ]
            group.Name |> should equal "Group"
            group.Examples.Length |> should equal 1
            let [example] = group.Examples
            example.Name |> should equal "Test"

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
    ]
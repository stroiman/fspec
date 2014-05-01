module FSpec.SelfTests.DslV2Specs
open FSpec.Core.DomainTypes
open FSpec.Core.DslV2
open FSpec.Core.MatchersV2

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
    ]
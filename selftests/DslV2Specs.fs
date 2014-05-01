module FSpec.SelfTests.DslV2Specs
open FSpec.Core.DomainTypes
open FSpec.Core.DslV2
open FSpec.Core.MatchersV2

let specs =
    describe "Example building DSL" [
        it "builds example groups" <| fun _ ->
            let group = 
                describe "Group" [
                    it "Test" <| fun _ -> ()
                ]
            group.Name |> should equal "Group"
            group.Examples.Length |> should equal 1
            let [example] = group.Examples
            example.Name |> should equal "Test"
    ]
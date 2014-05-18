module FSpec.SelfTests.DomainTypesSpecs
open FSpec.Core
open Dsl
open MatchersV3
open ExampleHelper

let specs =
    [
        describe "ExampleGroup" [
            context "with existing metadata" [
                subject (fun _ -> 
                    anExampleGroup 
                    |> withMetaData ("a", 42))
                    
                describe "addMetaData" [
                    it "does not clear existing metadata" <| fun c ->
                        let grp = 
                            c |> TestContext.getSubject
                            |> ExampleGroup.addMetaData ("b" ++ 43)
                        grp.MetaData?a |> should (be.equalTo 42)

                    it "'wins' if name is the same as existing metadata" <| fun c ->
                        let grp = 
                            c |> TestContext.getSubject
                            |> ExampleGroup.addMetaData ("a" ++ 43)
                        grp.MetaData?a |> should (be.equalTo 43)
                ]
            ]
        ]

        describe "Example" [
            context "with existing metadata" [
                subject (fun _ -> anExampleWithMetaData ("a", 42))
                    
                describe "addMetaData" [
                    it "does not clear existing metadata" <| fun c ->
                        let grp = 
                            c |> TestContext.getSubject
                            |> Example.addMetaData ("b" ++ 43)
                        grp.MetaData?a |> should (be.equalTo 42)

                    it "'wins' if name is the same as existing metadata" <| fun c ->
                        let ex = 
                            c |> TestContext.getSubject
                            |> Example.addMetaData ("a" ++ 43)
                        ex.MetaData?a |> should (be.equalTo 43)
                ]
            ]
        ]
    ]

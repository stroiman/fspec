module FSpec.SelfTests.DomainTypesSpecs
open FSpec
open Dsl
open Matchers
open ExampleHelper
open CustomMatchers

let change f = fun (c:TestContext) -> c.Subject.Apply f

let specs =
    [
        describe "An example group" [
            subject (fun _ -> anExampleGroup)

            context "with existing metadata" [
                subject <| change (withMetaData ("a", 42))
                    
                describe "addMetaData" [
                    context "with different key" [
                        subject <| change (ExampleGroup.addMetaData ("b" ++ 43))

                        it "does not clear existing metadata" <| fun c ->
                            c.Subject.Should (haveMetaData "a" 42)
                    ]

                    context "with existing key" [
                        subject <| change (ExampleGroup.addMetaData ("a" ++ 43))

                        it "overwrites the existing value" <| fun c ->
                            c.Subject.Should (haveMetaData "a" 43)
                    ]
                ]
            ]
        ]

        describe "An example" [
            subject (fun _ -> aPassingExample)
            
            context "with existing metadata" [
                subject <| change (withExampleMetaData ("a", 42))
                    
                describe "addMetaData" [
                    it "does not clear existing metadata" <| fun c ->
                        let grp = 
                            c.Subject.Apply (Example.addMetaData ("b" ++ 43))
                        grp.MetaData?a.Should (equal 42)

                    it "'wins' if name is the same as existing metadata" <| fun c ->
                        let ex = 
                            c.Subject.Apply
                             (Example.addMetaData ("a" ++ 43))
                        ex.MetaData?a.Should (equal 43)
                ]
            ]
        ]
    ]

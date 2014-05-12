﻿module FSpec.SelfTests.DslV2Specs
open FSpec.Core
open Dsl
open MatchersV3
open MetaData
open TestContextOperations

let pass = fun _ -> ()
let extractGroup = applyGroup id (fun _ -> failwith "error")
let setGroup x =
    subject (fun _ -> x |> extractGroup)

let createMatcher<'T> (f:'T->bool)=
    { new Matcher<'T> () with
        member __.ApplyActual g actual = f actual |> g
        member __.FailureMsgForShould = "FAIL"
        member __.FailureMsgForShouldNot = "FAIL" }

let haveChildGroups expected =
    createMatcher (fun actual ->
        actual |> ExampleGroup.childGroups |> Seq.length = expected)

let haveNoOfExampleExamples expected =
    createMatcher (fun actual ->
        actual
        |> ExampleGroup.examples
        |> Seq.length = expected)

let haveExampleName expected =
    (fun actual -> actual |> Example.name = expected)
    |> createMatcher

let haveGroupName expected =
    (fun a -> a |> ExampleGroup.name = expected)
    |> createMatcher

let itBehavesLikeAGroupWithChildGroup name =
    MultipleOperations [
        it "should have exactly one child group" <| fun c ->
            c |> getSubject
            |> should (haveChildGroups 1)
        
        it "should have a group with the right name" <| fun c ->
            c |> getSubject
            |> ExampleGroup.childGroups
            |> should (have.atLeastOneElement (haveGroupName name))
    ]

let specs =
    describe "Example building DSL" [
        context "an example group initialized with one example" [
            setGroup <|
                describe "Group" [
                    it "Test" pass
                ]

            it "should have no child groups" <| fun ctx ->
                ctx |> getSubject
                |> should (haveChildGroups 0)

            it "should have exactly one example" <| fun ctx ->
                ctx |> getSubject
                |> should (haveNoOfExampleExamples 1)

            it "should have one example named 'Test'" <| fun ctx ->
                ctx |> getSubject
                |> ExampleGroup.examples
                |> should (have.atLeastOneElement (haveExampleName "Test"))
        ]

        context "a 'Describe' statement inside a 'Describe' statement" [
            setGroup <|
                describe "Group" [
                    describe "ChildGroup" [
                        it "Test" pass
                    ]
                ]

            itBehavesLikeAGroupWithChildGroup "ChildGroup"
        ]

        context "a 'context' statement inside a 'describe' statement" [
            setGroup <|
                describe "Group" [
                    context "child context" [
                        it "test" pass
                    ]
                ]

            itBehavesLikeAGroupWithChildGroup "child context"
        ]

        context "example group initialized with two 'before' and one 'after" [
            setGroup <|
                describe "group" [
                    before <| fun _ -> ()
                    before <| fun _ -> ()
                    after <| fun _ -> ()
                ]

            it "contains two setups" <| fun c ->
                c |> getSubject
                |> ExampleGroup.setups
                |> should (have.length (be.equalTo 2))

            it "contains one teardown" <| fun c ->
                c |> getSubject
                |> ExampleGroup.tearDowns
                |> should (have.length (be.equalTo 1))
        ]

        describe "metadata initialization" [
            context "example group has meta data applied" [
                setGroup <|
                    (("answer" ++ 42) ==>
                     describe "group" [])

                it "should store the meta data on the example group" <| fun c ->
                    let grp = c |> getSubject<ExampleGroup.T>
                    grp.MetaData?answer |> should (be.equalTo 42)
            ]

            context "example has meta data applied" [
                setGroup <|
                    describe "group" [
                        ("answer" ++ 42) ==>
                        it "has metadata" pass
                    ]

                it "should store the meta data on the example" <| fun c ->
                    let example =
                        c |> getSubject
                        |> ExampleGroup.examples
                        |> List.head
                    example.MetaData?answer |> should (be.equalTo 42)
            ]
        ]
    ]

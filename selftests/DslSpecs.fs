module FSpec.SelfTests.DslV2Specs
open FSpec.Core
open Dsl
open MatchersV3
open TestDataMap
open TestContextOperations
open ExampleGroup

let pass = fun _ -> ()

let setGroup x =
    subject <| fun _ -> 
        match x with
        | AddExampleGroupOperation g -> g
        | _ -> failwith "error"

let createMatcher = createSimpleMatcher
let getSetups grp = grp.Setups
let getTearDowns grp = grp.TearDowns
let getExamples grp = grp.Examples
let getChildGroups grp = grp.ChildGroups

let haveChildGroups expected =
    createMatcher (fun actual ->
        actual.ChildGroups |> Seq.length = expected)

let haveNoOfExampleExamples expected =
    createMatcher (fun actual ->
        actual
        |> getExamples
        |> Seq.length = expected)

let haveExampleName expected =
    (fun (actual:Example.T) -> actual.Name = expected)
    |> createMatcher

let haveGroupName expected =
    (fun a -> a.Name = expected)
    |> createMatcher

let itBehavesLikeAGroupWithChildGroup name =
    MultipleOperations [
        it "should have exactly one child group" <| fun c ->
            c |> getSubject
            |> should (haveChildGroups 1)
        
        it "should have a group with the right name" <| fun c ->
            c |> getSubject
            |> getChildGroups
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
                |> getExamples
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
                |> getSetups
                |> should (have.length (be.equalTo 2))

            it "contains one teardown" <| fun c ->
                c |> getSubject
                |> getTearDowns
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

            context "child group has meta data applied" [
                setGroup <|
                    describe "grp" [
                        ("answer" <<- 42)
                        context "child" []
                    ]

                it "should store the meta data on the child group" <| fun c ->
                    let child =
                        c |> getSubject 
                        |> getChildGroups
                        |> List.head
                    child.MetaData?answer |> should (be.equalTo 42)
            ]

            context "example has meta data applied" [
                let itStoresMeaDataOnTheExample = 
                    it "should store the meta data on the example" <| fun c ->
                        let example =
                            c |> getSubject
                            |> getExamples
                            |> List.head
                        example.MetaData?answer |> should (be.equalTo 42)

                yield context "using ++ and ==> operators" [
                    setGroup <|
                        describe "group" [
                            ("answer" ++ 42) ==>
                            it "has metadata" pass
                        ]
                    itStoresMeaDataOnTheExample
                ]

                yield context "using <<- operator" [
                    setGroup <|
                        describe "group" [
                            ("answer" <<- 42)
                            it "has metadata" pass
                        ]
                    itStoresMeaDataOnTheExample
                ]
            ]
        ]

        describe "'itShould' example expressions" [
            setGroup <|
                describe "group" [
                    itShould (be.False)
                ]

            it "should have one example" <| fun ctx ->
                ctx |> getSubject
                |> should (haveNoOfExampleExamples 1)
        ]

        describe "'itShouldNot' example expressions" [
            setGroup <|
                describe "group" [
                    itShouldNot (be.False)
                ]

            it "should have one example" <| fun ctx ->
                ctx |> getSubject
                |> should (haveNoOfExampleExamples 1)
        ]
    ]

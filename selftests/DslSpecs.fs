module FSpec.SelfTests.DslV2Specs
open FSpec
open Dsl
open Matchers
open TestDataMap
open ExampleGroup
open CustomMatchers

let pass = fun _ -> ()

let setGroup x =
    subject <| fun _ -> 
        match x with
        | AddExampleGroupOperation g -> g
        | _ -> failwith "error"

let getSetups grp = grp.Setups
let getTearDowns grp = grp.TearDowns
let getExamples grp = grp.Examples
let getChildGroups grp = grp.ChildGroups
let getMetaData grp = grp.MetaData

let itBehavesLikeAGroupWithChildGroup name =
    behavior [
        it "should have exactly one child group" <| fun c ->
            c.Subject.Should (haveChildGroups 1)
        
        it "should have a group with the right name" <| fun c ->
            c.Subject.Apply getChildGroups
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
                ctx.Subject.Should (haveChildGroups 0)

            it "should have exactly one example" <| fun ctx ->
                ctx.Subject.Should (haveNoOfExamples 1)

            it "should have one example named 'Test'" <| fun ctx ->
                ctx.Subject.Apply getExamples
                |> should (have.atLeastOneElement (haveExampleNamed "Test"))
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
                c.Subject.Apply getSetups
                |> should (have.length (be.equalTo 2))

            it "contains one teardown" <| fun c ->
                c.Subject.Apply getTearDowns
                |> should (have.length (be.equalTo 1))
        ]

        describe "metadata initialization" [
            context "example group has meta data applied" [
                setGroup <|
                    (("answer" ++ 42) ==>
                     describe "group" [])

                it "should store the meta data on the example group" <| fun c ->
                    let grp = c.GetSubject<ExampleGroup.T> ()
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
                        c.Subject.Apply getChildGroups
                        |> List.head
                    child.MetaData?answer |> should (be.equalTo 42)
            ]

            context "child group has 'focus' keyword" [
                setGroup <|
                    describe "grp" [
                        focus
                        context "child" []
                    ]

                it "should store a 'focus' metadata on the group" (fun c ->
                    let child =
                        c.Subject.Apply getChildGroups
                        |> List.head
                    child.MetaData?focus |> should (be.True))

            ]

            context "example has meta data applied" [
                let itStoresMeaDataOnTheExample = 
                    it "should store the meta data on the example" <| fun c ->
                        let example =
                            c.Subject.Apply getExamples
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
                ctx.Subject.Should (haveNoOfExamples 1)
        ]

        describe "'itShouldNot' example expressions" [
            setGroup <|
                describe "group" [
                    itShouldNot (be.False)
                ]

            it "should have one example" <| fun ctx ->
                ctx.Subject.Should (haveNoOfExamples 1)
        ]
    ]

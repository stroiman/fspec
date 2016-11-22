module FSpec.SelfTests.CustomMatchers
open FSpec
open Example
open ExampleGroup
open Matchers

let haveLineMatching pattern = have.element (be.string.matching pattern)

let haveChildGroups expected =
    createSimpleMatcher (fun actual ->
        actual.ChildGroups |> Seq.length = expected)

let haveNoOfExamples expected =
    createSimpleMatcher (fun actual ->
        actual
        |> (fun x -> x.Examples)
        |> Seq.length = expected)

let haveExampleNamed expected =
    (fun (actual:Example.T) -> actual.Name = expected)
    |> createSimpleMatcher

let haveGroupName expected =
    (fun a -> a.Name = expected)
    |> createSimpleMatcher

let failWithAssertionError expected =
    let matcher = be.string.containing expected
    let f a = 
        try
            a ()
            MatchFail "No exception thrown"
        with
            | AssertionError(info) -> 
                info.Message |> matcher.Run
    createMatcher f
        (sprintf "fail assertion with message %A" expected)

let haveMetaData k v =
    let f a =
        let x =
            a.MetaData |> TestDataMap.tryGet k
        match x with
        | Some y -> y = v
        | None -> false
    createSimpleMatcher f

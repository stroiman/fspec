module FSpec.SelfTests.CustomMatchers
open FSpec.Core
open Example
open ExampleGroup
open MatchersV3

let haveLineMatching pattern = have.element (be.string.matching pattern)

let createMatcher = createSimpleMatcher
let haveChildGroups expected =
    createMatcher (fun actual ->
        actual.ChildGroups |> Seq.length = expected)

let haveNoOfExamples expected =
    createMatcher (fun actual ->
        actual
        |> (fun x -> x.Examples)
        |> Seq.length = expected)

let haveExampleNamed expected =
    (fun (actual:Example.T) -> actual.Name = expected)
    |> createMatcher

let haveGroupName expected =
    (fun a -> a.Name = expected)
    |> createMatcher

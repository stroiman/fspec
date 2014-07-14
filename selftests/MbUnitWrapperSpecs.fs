module FSpec.SelfTests.MbUnitWrapperSpecs
open FSpec.Dsl
open FSpec.Matchers
open ExampleHelper
open MbUnit.Framework
open FSpec.ExampleGroup

let rec createSuiteFromExampleGroup g =
  let s = new TestSuite (g.Name)
  g.ChildGroups |> List.iter (fun x ->
    s.Children.Add(createSuiteFromExampleGroup x))
  g.Examples |> List.iter (fun x -> 
    s.Children.Add(TestCase (x.Name, fun _ -> ())))
  s

module SuiteHelpers =
  module beSuite =
    let withName m =
      createCompoundMatcher m (fun (x:TestSuite) -> x.Name) "have name"
open SuiteHelpers

let specs =
  describe "MbUnit wrapper" [
    describe "createSuiteFromExampleGroup()" [
      context "with an example group" [
        before <| fun ctx -> ctx?group <- anExampleGroupNamed "Group"
        subject (fun c -> c?group |> createSuiteFromExampleGroup)

        context "with an example" [
          before (fun c -> 
            let example = anExampleNamed "Example"
            c?group <- c?group |> withExamples [ example ])

          it "Creates a test suite named 'Group'" <| fun ctx ->
            ctx.Subject.Should (beSuite.withName (equal "Group"))

          it "Creates a test suite with an example" <| fun ctx ->
            let suite = ctx.GetSubject<TestSuite> ()
            let testCase : TestCase = suite.Children |> Seq.exactlyOne :?> TestCase
            testCase.Name.Should (equal "Example")
        ]

        context "with a child group" [
          before (fun c ->
            c?group <- c?group |> withNestedGroupNamed "Child" id)

          it "Creates a suite with nested suite" <| fun ctx ->
            let suite = ctx.GetSubject<TestSuite> ()
            let childSuite = suite.Children |> Seq.exactlyOne :?> TestSuite
            childSuite.Should (beSuite.withName (equal "Child"))
        ]
      ]
    ]
  ]
module FSpec.MbUnitWrapper
open MbUnit.Framework
open FSpec
open FSpec.Runner
open FSpec.ExampleGroup
open System.Collections.Generic

type FSpecTestCase (example : Runner.SingleExample) =
  inherit TestCase(example.Example.Name, fun _ -> Runner.runSingleExample example)

  member __.WrappedExample = example

let createSuiteFromExampleGroup g =
  let rec createWithParentGroups g p =
    let containingGroups = g::p
    let s = new TestSuite (g.Name)
    g.ChildGroups |> List.iter (fun x ->
      s.Children.Add(createWithParentGroups x containingGroups))
    g.Examples |> List.iter (fun x -> 
      let x = {
        Example = x
        ContainingGroups = containingGroups }
      s.Children.Add(FSpecTestCase x))
    s :> Test
  createWithParentGroups g []

let createSuiteFromExampleGroups gs =
  gs |> List.map createSuiteFromExampleGroup

let createSuitesFromExampleGroups gs =
  let cfg = Configuration.defaultConfig
  gs
  |> ExampleGroup.filterGroups (cfg.Exclude >> not)
  |> List.map createSuiteFromExampleGroup

[<AbstractClass>]
type MbUnitWrapperBase () =
  [<DynamicTestFactory>]
  member this.ActualTestFactory () : IEnumerable<Test> =

    this.GetType().Assembly
    |> TestDiscovery.getSpecsFromAssembly
    |> createSuitesFromExampleGroups
    |> List.toSeq

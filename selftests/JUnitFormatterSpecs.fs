module JUnitFormatterSpecs
open XmlHelpers
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open FSpec.Formatters

module Helpers =
  let desc name = { Name = name; MetaData = TestDataMap.Zero }
  let beginGroup n (reporter : IReporter) = reporter.BeginGroup (desc n)
  let reportExample n (reporter : IReporter) = reporter.ReportExample (desc n) Success
  let endGroup (reporter : IReporter) = reporter.EndGroup()
  let endTestRun (reporter : IReporter) = reporter.EndTestRun () |> ignore

  let withRun name f examples =
    context name [
      yield subject (fun _ -> 
        use stream = new System.IO.MemoryStream()
        let formatter = JUnitFormatter(stream) :> IReporter
        formatter |> f |> endTestRun
        System.Text.Encoding.UTF8.GetString (stream.ToArray()) )

      yield itShould beValidJUnitXml
      yield! examples
    ]

open Helpers

let specs =
  describe "JUnitFormatter" [
    describe "xml output" [
      withRun 
        "with one group and one example"
        (beginGroup "Group" >> reportExample "Example" >> endGroup) [

        it "create a single test element" (fun c ->
          c.Subject.Should beJUnitXmlWithOneTestCase)

        it "assigns the 'name' attribute" (fun c ->
          c.Subject.Should (beJUnitXmlWithOneTestCase >>> withAttribute "name" >>> equal "Example")
        )

        it "assigns the 'classname' attribute" (fun c ->
          c.Subject.Should (beJUnitXmlWithOneTestCase >>> withAttribute "classname" >>> equal "Group")
        )
      ]

      withRun
        "with two sibling groups"
        (beginGroup "Group1" >> beginGroup "GroupA" >> reportExample "Example1" >> endGroup >> 
         beginGroup "GroupB" >> reportExample "Example2" >> endGroup >> endGroup) [
          itShould (beJUnitXmlWithOneTestSuite >>> select (fun x -> x.Elements(xname "testcase") |> Seq.head) >>> withAttribute "classname" >>> equal "Group1.GroupA")

          itShould (beJUnitXmlWithOneTestSuite >>> select (fun x -> x.Elements(xname "testcase") |> Seq.last) >>> withAttribute "classname" >>> equal "Group1.GroupB")
        ]

      withRun
        "with two nested groups"
        (beginGroup "Group1" >> beginGroup "Group2" >> reportExample "Example1" >> endGroup >> endGroup) [
          itShould (beJUnitXmlWithOneTestCase >>> withAttribute "classname" >>> equal "Group1.Group2")
        ]

      withRun
        "with one group and two examples" 
        (beginGroup "Group" >> reportExample "Example1" >> reportExample "Example2" >> endGroup) [

        itShould (beJUnitXmlWithOneTestSuite >>> withNoOfElementsNamed "testcase" >>> equal 2)

        it "creates two test elements" (fun c ->
          c.Subject.Should (beJUnitXmlWithOneTestSuite >>> withNoOfElementsNamed "testcase" >>> equal 2)
        )

        itShould (beJUnitXmlWithOneTestSuite >>> withAttribute "tests" >>> equal "2")
      ]
      
      withRun 
        "with two groups with one example each" 
        (beginGroup "Group1" >> reportExample "Example1" >> endGroup >>
         beginGroup "Group2" >> reportExample "Example2" >> endGroup) [

        it "creates two test elements" (fun c ->
          c.Subject.Should (beJUnitXmlWithOneTestSuite >>> withNoOfElementsNamed "testcase" >>> equal 2)
        )
      ]
    ]
  ]

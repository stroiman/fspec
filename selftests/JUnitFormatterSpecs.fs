module JUnitFormatterSpecs
open System.Xml.Linq
open XmlHelpers
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open FSpec.Formatters

let desc name = { Name = name; MetaData = TestDataMap.Zero }

let withOneTestSuite : Matcher<XElement> =
  let f (actual : XElement) =
    match actual.Elements() |> List.ofSeq with
    | [e] -> 
      match e.Name.LocalName with
      | "testsuite" -> MatchSuccess e
      | _ -> MatchFail actual
    | _ -> MatchFail actual
  createMatcher f "with one test suite"

let withAttribute name : Matcher<XElement> =
  let f (actual : XElement) =
    match actual.Attribute(xname name) with
    | null -> MatchFail (sprintf "Attribute '%s' was not found" name)
    | x -> MatchSuccess x.Value
  createMatcher f (sprintf "with attribute '%s'" name)

let specs =
  describe "JUnitFormatter" [
    it "creates valid xml" (fun _ ->
      let result = JUnitFormatter.run ()
      result.Should beValidJUnitXml
    )

    ("input", ExampleGroupReport (desc "group", [ExampleReport (desc "example", Success)]) )**>
    context "Test contains one group with one test" [
      subject (fun ctx ->
        JUnitFormatter.createJUnitReport ctx?input
      )

      it "creates a 'testsuite' element with one 'testcase' element" (fun ctx ->
        let report = ExampleGroupReport (desc "group", [ExampleReport (desc "example", Success)])
        let junitReport = JUnitFormatter.createJUnitReport report
        let withNameAttribute = withAttribute "name"
        ctx.Subject.Should (beXml |>> withRootElement "testsuites" |>> withOneTestSuite |>> withNameAttribute |>> equal "group")
      )

      itShould beValidJUnitXml
    ]
  ]

module JUnitFormatterSpecs
open System.Xml.Linq
open XmlHelpers
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open FSpec.Formatters

let desc name = { Name = name; MetaData = TestDataMap.Zero }

let withSingleElement name : Matcher<XElement> =
  let f (actual : XElement) =
    match actual.Elements(xname name) |> List.ofSeq with
    | [e] -> MatchSuccess e
    | [] -> MatchFail "No element present"
    | _ -> MatchFail "More than one element present"
  createMatcher f (sprintf "with single child element named '%s'" name)

let withOneTestSuite : Matcher<XElement> =
  let f (actual : XElement) =
    match actual.Elements() |> List.ofSeq with
    | [e] -> 
      match e.Name.LocalName with
      | "testsuite" -> MatchSuccess e
      | _ -> MatchFail actual
    | _ -> MatchFail actual
  createMatcher f "with one test suite"

let withOneTestCase : Matcher<XElement> = withSingleElement "testcase"

let withAttribute name : Matcher<XElement> =
  let f (actual : XElement) =
    match actual.Attribute(xname name) with
    | null -> MatchFail (sprintf "Attribute '%s' was not found" name)
    | x -> MatchSuccess x.Value
  createMatcher f (sprintf "with attribute '%s'" name)

let withNameAttribute = withAttribute "name"
let beJunitWithOneTestSuite =
  beXml |>> withRootElement "testsuites" |>> withOneTestSuite
let beJUnitXmlWithOneTestCase = beJunitWithOneTestSuite |>> withOneTestCase

let withErrorElement : Matcher<XElement> = withSingleElement "error"
let withFailureElement : Matcher<XElement> = withSingleElement "failure"
let withSkippedElement : Matcher<XElement> = withSingleElement "skipped"

let specs =
  +describe "JUnitFormatter" [
    context "Test contains one group with one test" [
      before (fun ctx ->
        let result = ctx.GetOrDefault "test_result" (fun _ -> Success)
        ctx?input <- 
          ExampleGroupReport (desc "group", [ExampleReport (desc "example", result)]) 
      )
      
      subject (fun ctx -> JUnitFormatter.createJUnitReport ctx?input )

      it "creates a 'testsuite' element with one 'testcase' element" (fun ctx ->
        ctx.Subject.Should (beJunitWithOneTestSuite |>> withNameAttribute |>> equal "group")
      )

      it "creates a 'testcase'" (fun ctx ->
        ctx.Subject.Should (beJunitWithOneTestSuite |>> withOneTestCase |>> withNameAttribute |>> equal "example")
      )

      ("test_result", Success) **>
      context "when test results in a success" [
        itShouldNot (beJUnitXmlWithOneTestCase |>> withErrorElement)
        itShouldNot (beJUnitXmlWithOneTestCase |>> withFailureElement)
        itShouldNot (beJUnitXmlWithOneTestCase |>> withSkippedElement)

        itShould beValidJUnitXml
      ]

      ("test_result", Failure({ AssertionErrorInfo.create with Message = "message"})) **>
      context "when test fails" [
        itShould (beJUnitXmlWithOneTestCase |>> withFailureElement)

        itShouldNot (beJUnitXmlWithOneTestCase |>> withErrorElement)
        itShouldNot (beJUnitXmlWithOneTestCase |>> withSkippedElement)

        itShould beValidJUnitXml
      ]

      ("test_result", Pending) **>
      context "when test is pending" [
        itShould (beJUnitXmlWithOneTestCase |>> withSkippedElement)

        itShouldNot (beJUnitXmlWithOneTestCase |>> withFailureElement)
        itShouldNot (beJUnitXmlWithOneTestCase |>> withErrorElement)

        itShould beValidJUnitXml
      ]
      
      ("test_result", Error (new System.Exception())) **>
      context "when test is an error" [
        itShould (beJUnitXmlWithOneTestCase |>> withErrorElement)

        itShouldNot (beJUnitXmlWithOneTestCase |>> withFailureElement)
        itShouldNot (beJUnitXmlWithOneTestCase |>> withSkippedElement)

        itShould beValidJUnitXml
      ]
      itShould beValidJUnitXml
    ]
  ]
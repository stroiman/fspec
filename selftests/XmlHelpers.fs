module XmlHelpers
open FSpec.Matchers
open System.Xml
open System.Xml.Linq
open System.Xml.Schema
open System.Reflection

let assembly = Assembly.GetExecutingAssembly()
let resourceName = "JUnit.xsd"
let openSchemaStream () = assembly.GetManifestResourceStream(resourceName)
let xname = XName.Get

let validateJUnitXml xml =
  let messages = ref []
  let eventHandler (sender:obj) (e:ValidationEventArgs) =
    let invalidate () =
      messages := e.Message :: !messages
      printfn "XML error: %s" e.Message
    match e.Severity with
    | XmlSeverityType.Error -> invalidate()
    | XmlSeverityType.Warning -> invalidate()
    | _ -> ()
  let document = new XmlDocument()
  let schemaStream = openSchemaStream()
  let reader = XmlReader.Create(schemaStream)
  document.LoadXml xml
  document.Schemas.Add("", reader) |> ignore
  document.Validate(new ValidationEventHandler(eventHandler))
  !messages

let beValidJUnitXml =
  let f actual =
    let issues = validateJUnitXml actual
    match issues with
    | [] -> MatchSuccess ""
    | _ -> MatchFail issues
  createMatcher f "be valid JUnit xml"

let beXml =
  let f (actual : string) =
    try
      let doc = XDocument.Parse actual
      MatchSuccess doc
    with
      | _ -> MatchFail actual
  createMatcher f "be a well-formed xml document"

let withRootElement name =
  let f (actual : XDocument) =
    let root = actual.Root
    if root.Name = xname name then
      MatchSuccess actual.Root
    else
      MatchFail root
  createMatcher f (sprintf "with root element: '%s'" name)

let withOneElement name =
  let f (actual : XElement) =
    let c = actual.Elements (xname name) |> List.ofSeq
    match c with
    | [x] -> MatchSuccess x
    | [] -> MatchFail (sprintf "no elements with name '%s'" name)
    | _ -> MatchFail (sprintf "more than elements with name '%s'" name)
  createMatcher f (sprintf "with one element named: '%s'" name)

let withNoOfElementsNamed name =
  let f (actual : XElement) = actual.Elements (xname name) |> Seq.length |> MatchSuccess
  createMatcher f (sprintf "with no of elements named: '%s'" name)

let beJUnitXmlWithOneTestSuite =
  beXml >>> withRootElement "testsuites" >>> withOneElement "testsuite"

let beJUnitXmlWithOneTestCase =
  beJUnitXmlWithOneTestSuite >>> withOneElement "testcase"

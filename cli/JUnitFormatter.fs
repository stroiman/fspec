module FSpec.Formatters
open System.Xml.Linq

type Report = 
  | ExampleReport of ExampleDescriptor * TestResultType
  | ExampleGroupReport of ExampleDescriptor * Report list

let xname = XName.Get

module JUnitFormatter =
  let rec createElement report =
    match report with
    | ExampleGroupReport (desc, xs) ->
      let attribute = XAttribute(xname "name", desc.Name)
      let attribute2 = XAttribute(xname "tests", "")
      let children = xs |> List.map createElement
      XElement(xname "testsuite", "", attribute, attribute2, children)
    | ExampleReport (desc,result) -> 
      let children =
        match result with
        | Failure _ -> [XElement(xname "failure", "")]
        | Pending -> [XElement(xname "skipped", "")]
        | Error _ -> [XElement(xname "error", "")]
        | _ -> []
      XElement(xname "testcase", XAttribute(xname "name", desc.Name), children)

  let createJUnitReport report =
    let suite = createElement report
    let suites = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| suites |])
    doc.ToString ()

module FSpec.Formatters
open System.Xml.Linq

type Report = 
  | ExampleReport of ExampleDescriptor * TestResultType
  | ExampleGroupReport of ExampleDescriptor * Report list

let xname = XName.Get

module JUnitFormatter =
  let statsAttributes noOfTests = [XAttribute(xname "tests", sprintf "%d" noOfTests)]

  let rec createElement report =
    match report with
    | ExampleGroupReport (desc, xs) ->
      let (children,state) = xs |> List.map createElement |> List.unzip
      let sum = state |> List.sum

      let testSuiteElement = 
        XElement(xname "testsuite", "", 
          XAttribute(xname "name", desc.Name), 
          XAttribute(xname "tests", sum.ToString()), 
          children)
      (testSuiteElement, sum)

    | ExampleReport (desc,result) -> 
      let attributes =
        match result with
        | Failure _ -> [XElement(xname "failure", "")]
        | Pending -> [XElement(xname "skipped", "")]
        | Error _ -> [XElement(xname "error", "")]
        | _ -> []
      (XElement(xname "testcase", XAttribute(xname "name", desc.Name), attributes), 1)

  let createJUnitReport report =
    let (suite, _) = createElement report
    let suites = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| suites |])
    doc.ToString ()

module FSpec.Formatters
open System.Xml.Linq

type Report = 
  | ExampleReport of ExampleDescriptor * TestResultType
  | ExampleGroupReport of ExampleDescriptor * Report list

let xname = XName.Get

module JUnitFormatter =
  let run () = 
    let suite = XElement(xname "testsuite", "")
    let elms = XElement(xname "testsuites", XElement(xname "testsuite", "")) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    doc.ToString ()

  let createJUnitReport report =
    let attribute = XAttribute(xname "name", "group")
    let attribute2 = XAttribute(xname "tests", "group")
    let suite = XElement(xname "testsuite", "", attribute, attribute2)
    let suites = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| suites |])
    doc.ToString ()

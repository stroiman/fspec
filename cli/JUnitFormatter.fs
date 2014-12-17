module FSpec.Formatters
open System.Xml.Linq

let xname = XName.Get

module JUnitFormatter =
  let run () = 
    let name = XAttribute(xname "name", "name")
    let tests = XAttribute(xname "tests", "0")
    let suite = XElement(xname "testsuite", name, tests)
    let elms = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    doc.ToString ()
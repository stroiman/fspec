module FSpec.Formatters
open System.Xml.Linq

let xname = XName.Get

module JUnitFormatter =
  let run () = 
    let suite = XElement(xname "testsuite", "")
    let elms = XElement(xname "testsuites", XElement(xname "testsuite", "")) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    doc.ToString ()
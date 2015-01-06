module FSpec.Formatters
open System.Xml.Linq

let xname = XName.Get

type JUnitFormatter (stream:System.IO.Stream) =
  let write (doc:XDocument) =
    let settings = System.Xml.XmlWriterSettings()
    settings.Encoding <- System.Text.UTF8Encoding(false)
    use writer = System.Xml.XmlWriter.Create(stream, settings)
    doc.WriteTo(writer)

  member __.Run () = 
    let name = XAttribute(xname "name", "name")
    let tests = XAttribute(xname "tests", "0")
    let suite = XElement(xname "testsuite", name, tests)
    let elms = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    write doc

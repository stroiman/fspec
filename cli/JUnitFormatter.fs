module FSpec.Formatters
open System.Xml.Linq

let xname = XName.Get

type JUnitFormatter (stream:System.IO.Stream) as self =
  let mutable tests = []
  let write (doc:XDocument) =
    let settings = System.Xml.XmlWriterSettings()
    settings.Encoding <- System.Text.UTF8Encoding(false)
    use writer = System.Xml.XmlWriter.Create(stream, settings)
    doc.WriteTo(writer)
  let x = self :> IReporter

  member __.Run () = 
    let tests = tests
    let noOfTests = tests |> List.length
    let name = XAttribute(xname "name", "FSpec suite")
    let suite = XElement(xname "testsuite", name, XAttribute(xname "tests", noOfTests.ToString()), tests)
    let elms = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    write doc

  interface IReporter with
    member __.BeginGroup _ = x
    member __.EndGroup () = x
    member __.ReportExample _ _ = 
        tests <- XElement(xname "testcase", XAttribute(xname "name", "name")) :: tests
        x
    member self.EndTestRun () = 
        self.Run ()
        null :> obj

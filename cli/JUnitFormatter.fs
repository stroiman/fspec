module FSpec.Formatters
open System.Xml.Linq

let xname = XName.Get

type JUnitFormatter (stream:System.IO.Stream) as self =
  let mutable tests = []
  let mutable groups = []
  let write (doc:XDocument) =
    let settings = System.Xml.XmlWriterSettings()
    settings.Encoding <- System.Text.UTF8Encoding(false)
    use writer = System.Xml.XmlWriter.Create(stream, settings)
    doc.WriteTo(writer)
    writer.Close()
    stream.Close()
  let x = self :> IReporter

  let stripChars (str:string) = str.Replace(" ", "_").Replace(".","_").Replace("(","_").Replace(")","_")
  let getClassName () =
    let names = groups |> List.rev |> List.map (fun x -> x.Name |> stripChars) |> List.toArray 
    System.String.Join(".", names)

  member __.Run () = 
    let tests = tests |> List.rev
    let noOfTests = tests |> List.length
    let name = XAttribute(xname "name", "FSpec suite")
    let suite = XElement(xname "testsuite", name, XAttribute(xname "tests", noOfTests.ToString()), tests)
    let elms = XElement(xname "testsuites", suite) :> obj
    let doc = XDocument( XDeclaration("1.0", "UTF-8", "yes"), [| elms |])
    write doc

  interface IReporter with
    member __.BeginGroup desc = 
        groups <- desc::groups
        x
    member __.EndGroup () = 
        match groups with
        | [] -> failwith "EndGroup called too many times"
        | x::xs -> groups <- xs
        x
    member __.ReportExample desc _ = 
        let nameAttribute = XAttribute(xname "name", desc.Name |> stripChars)
        let classnameAttribute = XAttribute(xname "classname", getClassName())
        tests <- XElement(xname "testcase", nameAttribute, classnameAttribute) :: tests
        x
    member self.EndTestRun () = 
        self.Run ()
        null :> obj

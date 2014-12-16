module XmlHelpers
open System.Xml
open System.Xml.Schema
open System.Reflection

let assembly = Assembly.GetExecutingAssembly()
let resourceName = "JUnit.xsd"
let openSchemaStream () = assembly.GetManifestResourceStream(resourceName)

let isValidJUnitXml xml =
  let valid = ref true
  let eventHandler (sender:obj) (e:ValidationEventArgs) =
    let invalidate () =
      valid := false
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
  !valid


module JUnitFormatterSpecs
open XmlHelpers
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open FSpec.Formatters

let run () =
    use stream = new System.IO.MemoryStream()
    let formatter = JUnitFormatter(stream)
    formatter.Run()
    System.Text.Encoding.UTF8.GetString (stream.ToArray())

let specs =
  describe "JUnitFormatter" [
    it "creates valid xml" (fun _ ->
      let result = run ()
      result.Should beValidJUnitXml
    )
  ]

module JUnitFormatterSpecs
open XmlHelpers
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open FSpec.Formatters

let specs =
  describe "JUnitFormatter" [
    it "creates valid xml" (fun _ ->
      let result = JUnitFormatter.run ()
      result.Should beValidJUnitXml
    )
  ]

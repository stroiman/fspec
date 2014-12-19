module FSpec.SelfTests.IntegrationTests
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open Main
open ExampleHelper

let specs =
  describe "Main" [
    describe "exit code" [
      subject (fun ctx ->
        let grp = anExampleGroup |> withExamples [ ctx?example ]
        runExampleGroupsAndGetExitCode [grp] 
      )

      ("example", aPendingExample) **>
      context "when suite contains a single pending example" [
        itShould (equal 0)
      ]

      ("example", anExceptionThrowingExample) **>
      context "when suite contains a single example reporting an error" [
        itShouldNot (equal 0)
      ]

      ("example", aFailingExample) **>
      context "when suite contains a single failing example" [
        itShouldNot (equal 0)
      ]

      ("example", aPassingExample) **>
      context "when suite contains a single passing example" [
        itShould (equal 0)
      ]
    ]
  ]
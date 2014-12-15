module FSpec.SelfTest.sCommandLineParserSpecs
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open Main

let specs =
    describe "CommandLineParser" [
        context "Called with two assembly files" [
            it "loads the two assembly files" (fun _ ->
                let input = [| "assembly1.dll"; "assembly2.dll" |]
                let parsedData = parseArguments input
                let expected = List.ofArray input
                parsedData.AssemblyFiles |> should (equal expected)
            )
        ]
    ]
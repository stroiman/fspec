module FSpec.SelfTest.sCommandLineParserSpecs
open FSpec
open FSpec.Dsl
open FSpec.Matchers
open Main

module Helpers =
    let join separator (s:string list) = System.String.Join(" ", s |> List.toArray)

    let withArguments args specs =
        ("input", args) **> 
        context (sprintf "when called with '%s'" (args |> join " ")) specs

    let parse (matcher : Matcher<ParsedArguments>) =
        let f =
            function
            | Success x -> matcher.ApplyActual id x
            | Fail x -> MatchResult.MatchFail x
        createMatcher f (sprintf "be success %s" matcher.FailureMsgForShould)

    let withAssemblies m = createCompoundMatcher m (fun x -> x.AssemblyFiles) (sprintf "with AssemblyFiles %s" m.FailureMsgForShould)

    let printMessage (m:Matcher<string>) =
        let f = function
                | Success x -> MatchResult.MatchFail x
                | Fail x -> m.ApplyActual id x
        createMatcher f (sprintf "print message %s" m.FailureMsgForShould)
open Helpers

let mainSpecs =
    describe "Main" [
        context "Called with invalid args" [
            it "Returns non-zero exit code" (fun _ ->
                let input = [|"--invalid-argument-not-supported"|]
                let result = main input
                result |> shouldNot (equal 0)
            )
        ]
    ]

let specs =
    describe "CommandLineParser" [
        subject (fun ctx -> 
            ctx?input
            |> List.toArray
            |> parseArguments)

        withArguments ["assembly1.dll"; "assembly2.dll"] [
            itShould (parse (withAssemblies (equal ["assembly1.dll"; "assembly2.dll"])))
        ]

        withArguments ["--invalid-arguments-not-supported"] [
            itShould (printMessage (be.string.containing "FSpec"))
        ]
    ]
module FSpec.SelfTests.SuiteBuilderSpecs
open FSpec.Core
open Dsl
open Matchers
open ExampleHelper

let getFailed (report : Report.T) = report.failed |> List.reduce (+)
let specs =
    describe "Reporting" [
        context "summary" [
            it "reports test success" <| fun _ ->
                aPassingExample
                |> runSingleExample
                |> Report.summary |> should equal  "1 run, 0 failed"

            it "reports test failures" <| fun _ ->
                aFailingExample
                |> runSingleExample
                |> Report.summary |> should equal "1 run, 1 failed"

            it "reports pending tests" <| fun _ ->
                aPendingExample
                |> runSingleExample 
                |> Report.summary |> should equal "1 run, 0 failed, 1 pending"
        ]

        context "Running status" [
            it "Is reported while running" <| fun _ ->
                anExampleGroupNamed "Some context"
                |> withAnExampleNamed "has some behavior"
                |> run |> Report.output
                |> should equal ["Some context has some behavior - passed"]

            it "Reports multiple test results" <| fun _ ->
                anExampleGroupNamed "Some context"
                |> withExamples [
                    anExampleNamed "has some behavior"
                    anExampleNamed "has some other behavior" 
                ]
                |> run |> Report.output |> List.rev
                |> should equal ["Some context has some behavior - passed"
                                 "Some context has some other behavior - passed"]
 

            it "Reports nested contexts correctly" <| fun _ ->
                anExampleGroupNamed "Some context"
                |> withNestedGroupNamed "in some special state" (
                    withAnExampleNamed "has some special behavior")
                |> run
                |> Report.output |> List.reduce (+)
                |> should matchRegex "Some context in some special state has some special behavior"
        ]

        context "Failed tests" [
            it "handles test failures in setup code" (fun _ ->
                anExampleGroup
                |> withSetupCode (fun _ -> failwith "error")
                |> withAnExample
                |> run
                |> Report.success |> should equal false
            )
            
            it "handles test failures in teardown code" (fun _ ->
                anExampleGroup
                |> withTearDownCode (fun _ -> failwith "error")
                |> withAnExample
                |> run
                |> Report.success |> should equal false
            )

            it "Writes the output to the test report" <| fun _ ->
                anExample (fun _ ->
                    5 |> should equal 6)
                |> runSingleExample
                |> Report.failed |> List.reduce (+)
                |> should matchRegex "expected 5 to equal 6"

            it "Is empty when no tests fail" <| fun _ ->
                aPassingExample
                |> runSingleExample
                |> Report.failed |> List.length
                |> should equal 0
        ]

        context "Tests with errors" [
            it "writes the exception name" <| fun _ ->
                anExample (fun _ -> raise (new System.NotImplementedException()))
                |> runSingleExample
                |> Report.failed |> List.reduce (+)
                |> should matchRegex "NotImplementedException"
        ]
    ]

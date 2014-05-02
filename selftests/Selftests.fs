module FSpec.SelfTests.SuiteBuilderSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let specs =
    describe "Reporting" <| fun _ ->
        let sut = DslHelper()

        describe "TestMetaData" <| fun _ ->
            it "is initialized from test" <| fun _ ->
                let sut = TestCollection()
                sut.it_ [("answer", 42)] "dummy" <| fun _ -> ()
                sut.examples.Examples.Head.MetaData.get "answer" |> should equal 42

        describe "summary" <| fun _ ->
            it "reports test success" <| fun _ ->
                sut.it "Is a success" pass
                sut.run()
                |> Report.summary |> should equal  "1 run, 0 failed"

            it "reports test failures" <| fun _ ->
                sut.it "Is a failure" fail
                sut.run() 
                |> Report.summary |> should equal "1 run, 1 failed"

            it "reports pending tests" <| fun _ ->
                sut.it "Is pending" pending
                sut.run()
                |> Report.summary |> should equal "1 run, 0 failed, 1 pending"

        describe "Running status" <| fun _ ->
            it "Is reported while running" <| fun _ ->
                sut.describe "Some context" <| fun _ ->
                    sut.it "has some behavior" pass
                sut.run().output
                |> should equal ["Some context has some behavior - passed"]

            it "Reports multiple test results" <| fun _ ->
                sut.describe "Some context" <| fun _ ->
                    sut.it "has some behavior" pass
                    sut.it "has some other behavior" pass

                let report = sut.run()
                let actual = report.output |> List.rev
                let expected = ["Some context has some behavior - passed";
                                "Some context has some other behavior - passed"]
                actual.should equal expected

            it "Reports nested contexts correctly" <| fun _ ->
                sut.describe "Some context" <| fun _ ->
                    sut.describe "in some special state" <| fun _ ->
                        sut.it "has some special behavior" pass

                let report = sut.run()
                let actual = report.output |> List.rev |> List.head
                actual |> should matchRegex "Some context in some special state has some special behavior"

        describe "Failed tests" <| fun _ ->
            it "handles test failures in setup code" (fun _ ->
                sut.before (fun _ -> failwith "error")
                sut.it "works" pass
                sut.run()
                |> Report.success |> should equal false
            )
            
            it "handles test failures in teardown code" (fun _ ->
                sut.after (fun _ -> failwith "error")
                sut.it "works" pass
                sut.run()
                |> Report.success |> should equal false
            )

            it "Writes the output to the test report" <| fun _ ->
                sut.it "Is a failing test" <| fun _ ->
                    (5).should equal 6
                let result = sut.run()
                let actual = result.failed |> List.reduce (+)
                actual |> should matchRegex "expected 5 to equal 6"

            it "write the right output for comparison tests" <| fun _ ->
                sut.it "Is a failing test" <| fun _ ->
                    5 |> should be.greaterThan 6
                let result = sut.run()
                let actual = result.failed |> List.reduce (+)
                actual |> should matchRegex "expected 5 to be greater than 6"

            it "Is empty when no tests fail" <| fun _ ->
                sut.it "Is a passing test" pass
                let result = sut.run()
                let actual = result.failed |> List.length
                actual.should equal 0

        describe "Tests with errors" <| fun _ ->
            it "writes the exception name" <| fun _ ->
                sut.it "Is a failing test" <| fun _ ->
                    raise (new System.NotImplementedException())
                    ()
                let result = sut.run()
                let actual = result.failed |> List.reduce (+)
                actual |> should matchRegex "NotImplementedException"

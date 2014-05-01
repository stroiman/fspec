module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let callList = ref []
let addToCallList x = callList := x::!callList
let actualCallList () = !callList |> List.rev
let clearCallList () = callList := []

let runSpecs (specs : DslHelper -> unit) =
    let sut = DslHelper()
    specs sut
    sut.run() |> ignore

let anExampleGroup = ExampleGroup.create "dummy"
let withExamples examples exampleGroup =
    let folder grp ex = ExampleGroup.addExample ex grp
    examples |> List.fold folder exampleGroup

let createAnExampleWithMetaData metaData f =
    let metaData' = MetaData.create [metaData]
    Example.create "dummy" f |> Example.addMetaData metaData'

let runSingleExample example =
    let group = anExampleGroup |> withExamples [example]
    ExampleGroup.run [group] (TestReport()) 

let specs =
    describe "Test runner" <| fun _ ->
        describe "metadata handling" <| fun _ ->
            context "test contains metadata" <| fun _ ->
                it "passes the metadata to the test" <| fun _ ->
                    createAnExampleWithMetaData ("answer", 42) <| fun ctx ->
                        ctx.metadata "answer" |> should equal 42
                    |> runSingleExample 

        let sut = DslHelper()
        before <| clearCallList

        describe "test execution order" <| fun _ ->
            it "tests execute in the order they appear" (fun _ ->
                runSpecs (fun sut ->
                    sut.describe("context") <| fun _ ->
                        sut.it("has test 1") <| fun _ -> addToCallList "test 1"
                        sut.it("has test 2") <| fun _ -> addToCallList "test 2"
                )
                actualCallList() |> should equal ["test 1"; "test 2"]
            )

            it "child contexts execute in the order they appear" (fun _ ->
                runSpecs (fun sut ->
                    sut.describe("context") <| fun _ ->
                        sut.it("has test 1") <| fun _ -> addToCallList "test 1"
                    sut.describe("other context") <| fun _ ->
                        sut.it("has test 2") <| fun _ -> addToCallList "test 2"
                )
                actualCallList() |> should equal ["test 1"; "test 2"]
            )

        describe "Lazy object initialization" <| fun _ ->
            it "can be used to create objects to test" (fun _ ->
                sut.describe "Ctx" <| fun _ ->
                    let initializer = sut.init <| fun _ ->
                        "Value from initializer"
                    sut.it "uses the value" <| fun _ ->
                        addToCallList (initializer())
                sut.run() |> ignore
                actualCallList() |> should equal ["Value from initializer"]
            )

            it "only initializes object once" (fun _ ->
                runSpecs (fun sut ->
                    sut.describe "Ctx" <| fun _ ->
                        let initializer = sut.init <| fun _ ->
                            addToCallList "init"

                        sut.it "uses value" <| fun _ ->
                            let x = initializer()
                            addToCallList "test 1"

                        sut.it "uses value twice" <| fun _ ->
                            let x = initializer()
                            let y = initializer()
                            addToCallList "test 2"
                )
                let expected = ["init"; "test 1"; "init"; "test 2"]
                actualCallList() |> should equal expected
            )

        describe "General setup/test/teardown handling" <| fun _ ->
            it "runs the sequence before, spec, teardown" <| fun _ ->
                sut.before <| fun _ -> addToCallList "setup"
                sut.after <| fun _ -> addToCallList "tearDown"
                sut.it "works" <| fun _ -> addToCallList "test"
                sut.run() |> ignore

                actualCallList() |> should equal ["setup"; "test"; "tearDown"]

            it "runs outer setup before inner setup" <| fun _ ->
                sut.describe "A feature" <| fun _  ->
                    sut.before <| fun _ -> addToCallList "outer setup"
                    sut.describe "in a specific context" <| fun _ ->
                        sut.before <| fun _ -> addToCallList "inner setup"
                        sut.it "works in a specific way" <| fun _ -> addToCallList "test"
                sut.run() |> ignore
                let expected = [ "outer setup"; "inner setup"; "test" ]
                actualCallList() |> should equal expected

            it "runs inner tear down before outer tear down" <| fun _ ->
                sut.describe "A feature" <| fun _ ->
                    sut.after <| fun _ -> addToCallList "outer tear down"
                    sut.describe "in a specific context" <| fun _ ->
                        sut.after <| fun _ -> addToCallList "inner tear down"
                        sut.it "works in a specific way" <| fun _ -> addToCallList "test"
                sut.run() |> ignore
                let expected = ["test"; "inner tear down"; "outer tear down"]
                actualCallList() |> should equal expected

            it "runs setup/teardown once for each test" <| fun _ ->
                sut.before <| fun _ -> addToCallList "setup"
                sut.after <| fun _ -> addToCallList "tear down"
                sut.it "test 1" <| fun _ -> addToCallList "test 1"
                sut.it "test 2" <| fun _ -> addToCallList "test 2"
                sut.run() |> ignore

                let expected = [
                    "setup"; "test 1"; "tear down";
                    "setup"; "test 2"; "tear down"]
                actualCallList() |> should equal expected
            
        describe "setup" <| fun _ ->
            it "is only run in same context, or nested context" <| fun _ ->
                sut.describe "Ctx" <| fun _ ->
                    sut.before <| fun _ -> addToCallList "outer setup"
                    sut.it "Outer test" <| fun _ -> addToCallList "outer test"
                    sut.describe "Inner ctx" <| fun _ ->
                        sut.before <| fun _ -> addToCallList "inner setup"
                        sut.it "inner test 1" <| fun _ -> addToCallList "inner test 1"
                        sut.it "inner test 2" <| fun _ -> addToCallList "inner test 2"
                sut.run() |> ignore

                let expected = [
                    "outer setup"; "outer test";
                    "outer setup"; "inner setup"; "inner test 1";
                    "outer setup"; "inner setup"; "inner test 2"]
                actualCallList() |> should equal expected

        describe "tear down" <| fun _ ->
            it "runs if test fail" <| fun _ ->
                sut.after <| fun _ -> addToCallList "tearDown"
                sut.it "fails" <| fun _ -> failwith "some failure"
                sut.run() |> ignore

                actualCallList() |> should equal ["tearDown"]

            it "runs teardown in the right context" <| fun _ ->
                sut.describe "outer ctx" <| fun _ ->
                    sut.after <| (fun _ -> addToCallList "outer tearDown")
                    sut.it "outer test" <| (fun _ -> addToCallList "outer test")
                    sut.describe "inner ctx" <| fun _ ->
                        sut.after <| (fun _ -> addToCallList "inner tearDown")
                        sut.it "inner text" <| (fun _ -> addToCallList "inner test")
                sut.run() |> ignore

                let expected = ["outer test"; "outer tearDown"; "inner test"; "inner tearDown"; "outer tearDown"]
                actualCallList() |> should equal expected


module FSpec.SelfTests.TestRunnerSpecs
open FSpec.Core
open Dsl
open Matchers
open DslHelper

let callList = ref []
let addToCallList x = callList := x::!callList
let actualCallList () = !callList |> List.rev
let clearCallList () = callList := []

let specs =
    describe "Test runner" <| fun () ->
        let sut = DslHelper()
        before <| clearCallList

        describe "Lazy object initialization" <| fun () ->
            it "can be used to create objects to test" <| fun () ->
                sut.describe "Ctx" <| fun () ->
                    let initializer = sut.init <| fun () ->
                        "Value from initializer"
                    sut.it "uses the value" <| fun () ->
                        addToCallList (initializer())
                sut.run() |> ignore
                actualCallList() |> should equal ["Value from initializer"]

            it "only initializes object once" <| fun () ->
                sut.describe "Ctx" <| fun () ->
                    let initializer = sut.init <| fun () ->
                        addToCallList "init"

                    sut.it "uses value" <| fun () ->
                        let x = initializer()
                        addToCallList "test 1"

                    sut.it "uses value twice" <| fun () ->
                        let x = initializer()
                        let y = initializer()
                        addToCallList "test 2"
                sut.run() |> ignore
                
                let expected = ["init"; "test 1"; "init"; "test 2"]
                actualCallList() |> should equal expected

        describe "test execution order" <| fun () ->
            it "tests execute in the order they appear" <| fun() ->
                sut.describe("context") <| fun() ->
                    sut.it("has test 1") <| fun() -> addToCallList "test 1"
                    sut.it("has test 2") <| fun() -> addToCallList "test 2"

                sut.run() |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]

            it "child contexts execute in the order they appear" <| fun() ->
                sut.describe("context") <| fun() ->
                    sut.it("has test 1") <| fun() -> addToCallList "test 1"
                sut.describe("other context") <| fun() ->
                    sut.it("has test 2") <| fun() -> addToCallList "test 2"
                sut.run() |> ignore
                actualCallList() |> should equal ["test 1"; "test 2"]

        describe "General setup/test/teardown handling" <| fun () ->
            it "runs the sequence before, spec, teardown" <| fun () ->
                sut.before <| fun () -> addToCallList "setup"
                sut.after <| fun () -> addToCallList "tearDown"
                sut.it "works" <| fun () -> addToCallList "test"
                sut.run() |> ignore

                actualCallList() |> should equal ["setup"; "test"; "tearDown"]

            it "runs outer setup before inner setup" <| fun () ->
                sut.describe "A feature" <| fun ()  ->
                    sut.before <| fun () -> addToCallList "outer setup"
                    sut.describe "in a specific context" <| fun () ->
                        sut.before <| fun () -> addToCallList "inner setup"
                        sut.it "works in a specific way" <| fun () -> addToCallList "test"
                sut.run() |> ignore
                let expected = [ "outer setup"; "inner setup"; "test" ]
                actualCallList() |> should equal expected

            it "runs inner tear down before outer tear down" <| fun () ->
                sut.describe "A feature" <| fun () ->
                    sut.after <| fun () -> addToCallList "outer tear down"
                    sut.describe "in a specific context" <| fun () ->
                        sut.after <| fun () -> addToCallList "inner tear down"
                        sut.it "works in a specific way" <| fun () -> addToCallList "test"
                sut.run() |> ignore
                let expected = ["test"; "inner tear down"; "outer tear down"]
                actualCallList() |> should equal expected

            it "runs setup/teardown once for each test" <| fun () ->
                sut.before <| fun () -> addToCallList "setup"
                sut.after <| fun () -> addToCallList "tear down"
                sut.it "test 1" <| fun () -> addToCallList "test 1"
                sut.it "test 2" <| fun () -> addToCallList "test 2"
                sut.run() |> ignore

                let expected = [
                    "setup"; "test 1"; "tear down";
                    "setup"; "test 2"; "tear down"]
                actualCallList() |> should equal expected
            
        describe "setup" <| fun () ->
            it "is only run in same context, or nested context" <| fun () ->
                sut.describe "Ctx" <| fun () ->
                    sut.before <| fun () -> addToCallList "outer setup"
                    sut.it "Outer test" <| fun () -> addToCallList "outer test"
                    sut.describe "Inner ctx" <| fun () ->
                        sut.before <| fun () -> addToCallList "inner setup"
                        sut.it "inner test 1" <| fun () -> addToCallList "inner test 1"
                        sut.it "inner test 2" <| fun () -> addToCallList "inner test 2"
                sut.run() |> ignore

                let expected = [
                    "outer setup"; "outer test";
                    "outer setup"; "inner setup"; "inner test 1";
                    "outer setup"; "inner setup"; "inner test 2"]
                actualCallList() |> should equal expected

        describe "tear down" <| fun() ->
            it "runs if test fail" <| fun() ->
                sut.after <| fun() -> addToCallList "tearDown"
                sut.it "fails" <| fun() -> failwith "some failure"
                sut.run() |> ignore

                actualCallList() |> should equal ["tearDown"]

            it "runs teardown in the right context" <| fun() ->
                sut.describe "outer ctx" <| fun() ->
                    sut.after <| (fun() -> addToCallList "outer tearDown")
                    sut.it "outer test" <| (fun() -> addToCallList "outer test")
                    sut.describe "inner ctx" <| fun() ->
                        sut.after <| (fun() -> addToCallList "inner tearDown")
                        sut.it "inner text" <| (fun() -> addToCallList "inner test")
                sut.run() |> ignore

                let expected = ["outer test"; "outer tearDown"; "inner test"; "inner tearDown"; "outer tearDown"]
                actualCallList() |> should equal expected


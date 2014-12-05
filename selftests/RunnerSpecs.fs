module FSpec.SelfTests.RunnerSpecs
open FSpec
open Dsl
open Matchers
open ExampleHelper
open ExampleGroup

type TestContext
    with
        member self.CallList 
            with get() = self.GetOrDefault<string list> "call_list" (fun _ -> [])
            and set (value : List<string>) = self?call_list <- value

let recordIn (ctx:TestContext) name =
    fun _ -> ctx.CallList <- (name :: ctx.CallList)

let shouldRecordIn (ctx:TestContext) expected grp =
    grp |> run |> ignore
    ctx.CallList |> List.rev |> should (equal expected)

let specs =
    describe "Test runner" [
        describe "execution order" [

            describe "of examples" [
                it "executes examples in the order they appear" <| fun ctx ->
                    anExampleGroup
                    |> withExampleCode (recordIn ctx "test 1")
                    |> withExampleCode (recordIn ctx "test 2")
                    |> shouldRecordIn ctx ["test 1"; "test 2"]

                it "executes child groups in the order they appear" (fun ctx ->
                    anExampleGroup
                    |> withNestedGroup (
                        withExampleCode (recordIn ctx "test 1"))
                    |> withNestedGroup (
                        withExampleCode (recordIn ctx "test 2"))
                    |> shouldRecordIn ctx ["test 1"; "test 2"]
                )
            ]

            describe "of setup and teardown" [
                it "runs setup/teardown once for each test" <| fun ctx ->
                    anExampleGroup
                    |> withSetupCode (recordIn ctx "setup")
                    |> withTearDownCode (recordIn ctx "tear down")
                    |> withExampleCode (recordIn ctx "test 1")
                    |> withExampleCode (recordIn ctx "test 2")
                    |> shouldRecordIn ctx
                        [ "setup"; "test 1"; "tear down";
                          "setup"; "test 2"; "tear down"]
                    
                describe "setup" [
                    it "is executed before the example" <| fun ctx ->
                        anExampleGroup
                        |> withSetupCode (recordIn ctx "setup")
                        |> withExampleCode (recordIn ctx "test")
                        |> shouldRecordIn ctx ["setup"; "test"]

                    it "is executed for example in child group" <| fun ctx ->
                        anExampleGroup
                        |> withSetupCode (recordIn ctx "outer setup")
                        |> withNestedGroup (
                            withSetupCode (recordIn ctx "inner setup")
                            >> withExampleCode (recordIn ctx "test"))
                        |> shouldRecordIn ctx [ "outer setup"; "inner setup"; "test" ]

                    it "is not executed for subling group examples" <| fun ctx ->
                        anExampleGroup
                        |> withNestedGroup (
                            withSetupCode (recordIn ctx "setup"))
                        |> withNestedGroup (
                            withExampleCode (recordIn ctx "sibling test"))
                        |> shouldRecordIn ctx ["sibling test"]

                    it "is executed in the order they appear" <| fun ctx ->
                        anExampleGroup
                        |> withSetupCode (recordIn ctx "setup 1")
                        |> withSetupCode (recordIn ctx "setup 2")
                        |> withAnExample
                        |> shouldRecordIn ctx ["setup 1";"setup 2"]
                ]

                describe "tear down" [
                    it "is executed after the example" <| fun ctx ->
                        anExampleGroup
                        |> withTearDownCode (recordIn ctx "tearDown")
                        |> withExampleCode (recordIn ctx "test")
                        |> shouldRecordIn ctx ["test"; "tearDown"]

                    it "is executed if example fails" <| fun ctx ->
                        anExampleGroup
                        |> withTearDownCode (recordIn ctx "tearDown")
                        |> withExamples [ aFailingExample ]
                        |> shouldRecordIn ctx ["tearDown"]

                    it "is executed for example in child group" <| fun ctx ->
                        anExampleGroup
                        |> withTearDownCode (recordIn ctx "outer tear down")
                        |> withNestedGroup (
                            withTearDownCode (recordIn ctx "inner tear down")
                            >> withExampleCode (recordIn ctx "test"))
                        |> shouldRecordIn ctx ["test"; "inner tear down"; "outer tear down"]

                    it "is not executed for sibling group examples" <| fun ctx ->
                        anExampleGroup
                        |> withNestedGroup (
                            withTearDownCode (recordIn ctx "tearDown"))
                        |> withNestedGroup (
                            withExampleCode (recordIn ctx "sibling test"))
                        |> shouldRecordIn ctx ["sibling test"]
                    
                    it "runs in the order it appears" <| fun ctx ->
                        anExampleGroup
                        |> withTearDownCode (recordIn ctx "teardown 1")
                        |> withTearDownCode (recordIn ctx "teardown 2")
                        |> withAnExample
                        |> shouldRecordIn ctx ["teardown 1";"teardown 2"]
                ]
            ]
        ]

        describe "context cleanup" [
            context "setup code initializes an IDisposable" [
                subject <| fun ctx ->
                    ctx?disposed <- false
                    let disposable =
                        { new System.IDisposable with
                            member __.Dispose () = ctx?disposed <- true }

                    anExampleGroup
                    |> withSetupCode (fun c -> c?dummy <- disposable)

                it "is disposed after test run" <| fun ctx ->
                    ctx.GetSubject<ExampleGroup.T> ()
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposed |> should be.True

                it "is disposed if test fails" <| fun ctx ->
                    ctx.GetSubject<ExampleGroup.T> ()
                    |> withExampleCode (fun _ -> failwith "dummy")
                    |> run |> ignore
                    ctx?disposed |> should be.True

                it "is disposed if teardown fails" <| fun ctx ->
                    ctx.GetSubject<ExampleGroup.T> ()
                    |> withTearDownCode (fun _ -> failwith "dummy")
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposed |> should be.True

                it "is not disposed in teardown code" <| fun ctx ->
                    ctx.GetSubject<ExampleGroup.T> ()
                    |> withTearDownCode (fun _ -> 
                        let disposed : bool = ctx?disposed
                        ctx?disposedDuringTearDown <- disposed)
                    |> withAnExample
                    |> run |> ignore
                    ctx?disposedDuringTearDown |> should be.False
            ]
        ]
        
        describe "example filtering" [
            context "with default configuration" [
                it "runs examples with the 'focus' metadata" <| fun ctx ->
                    anExampleGroup
                    |> withExamples [
                        anExampleWithCode (recordIn ctx "ex1") 
                            |> withExampleMetaData ("focus", true)
                        anExampleWithCode (recordIn ctx "ex2") ]
                    |> shouldRecordIn ctx ["ex1"]

                it "runs example groups with the 'focus' metadata" <| fun ctx ->
                    anExampleGroup
                    |> withNestedGroup(
                        withMetaData ("focus", true)
                        >> withExamples [
                            anExampleWithCode (recordIn ctx "ex1")])
                    |> withNestedGroup(
                        withExamples [
                            anExampleWithCode (recordIn ctx "ex2") ])
                    |> shouldRecordIn ctx ["ex1"]

                it "runs example groups with 'focus' several levels up" <| fun ctx ->
                    anExampleGroup
                    |> withNestedGroup(
                        withMetaData ("focus", true)
                        >> withNestedGroup(
                            withExamples [
                                anExampleWithCode (recordIn ctx "ex1")]))
                    |> withNestedGroup(
                        withNestedGroup(
                            withExamples [
                                anExampleWithCode (recordIn ctx "ex2") ]))
                    |> shouldRecordIn ctx ["ex1"]

                it "runs all examples if none are focused" <| fun ctx ->
                    anExampleGroup
                    |> withExamples [
                        anExampleWithCode (recordIn ctx "ex1") 
                        anExampleWithCode (recordIn ctx "ex2") ]
                    |> shouldRecordIn ctx ["ex1";"ex2"]

                it "excludes example with 'slow' metadata" <| fun ctx ->
                    anExampleGroup
                    |> withExamples [
                        anExampleWithCode (recordIn ctx "ex1") 
                            |> withExampleMetaData ("slow", true)
                        anExampleWithCode (recordIn ctx "ex2") ]
                    |> shouldRecordIn ctx ["ex2"]

            ]
        ]
    ]

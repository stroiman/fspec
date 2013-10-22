module Main
open FSpec

let assertEqual a b =
  if not (a = b) then
    failwithf "Not equal %A and %A" a b
  ()

let assertTrue value =
  if not value then
    failwithf "Value was false"
  ()

let assertFalse value =
  if value then
    failwithf "Value was true"
  ()

let c = TestCollection()
let describe = c.describe
let it = c.it
let before = c.before
let init = c.init

describe "TestCollection" <| fun() ->
  it "handles lazy initialization" <| fun () ->
    let c = TestCollection()
    let initCount = ref 0
    c.describe "Ctx" <| fun () ->
      let value = c.init <| fun () ->
        initCount := !initCount + 1
        "dummy"

      c.it "uses value" <| fun () ->
        let x = value()
        ()

      c.it "uses value twice" <| fun () ->
        let x = value()
        let y = value()
        ()

    c.run()
    assertEqual !initCount 2

describe "TestCollection" <| fun() ->
  let col = init (fun () -> TestCollection())
  let res = init (fun () -> TestResult())
  let run () =
    col().run(res())
    res()

  describe "Setup" <| fun () ->
    it "runs before the test is run" <| fun () ->
      let wasSetupWhenTestWasRun = ref false
      let wasSetup = ref false
      col().before <| fun() ->
        wasSetup := true
      col().it "dummy" <| fun() ->
        wasSetupWhenTestWasRun := !wasSetup
      run() |> ignore
      assertTrue !wasSetupWhenTestWasRun  

    it "is only run for in same context, or nested context" <| fun () ->
      let outerSetupRunCount = ref 0
      let innerSetupRunCount = ref 0
      col().describe "Ctx" <| fun () ->
        col().before <| fun () ->
          outerSetupRunCount := !outerSetupRunCount + 1
        col().it "Outer test" <| fun () ->
          ()
        col().describe "Inner ctx" <| fun () ->
          col().before <| fun () ->
            innerSetupRunCount := !innerSetupRunCount + 1
          col().it "Inner test" <| fun () ->
            ()
          col().it "Inner test2" <| fun () ->
            ()
      run() |> ignore
      assertEqual !innerSetupRunCount 2
      assertEqual !outerSetupRunCount 3

  describe "Run" <| fun () ->
    it "reports test failures" <| fun () ->
      col().it "Is a failure" <| fun () ->
        failwithf "Just another failure"

      let result = run()
      assertEqual (result.summary()) "1 run, 1 failed"

    it "reports test success" <| fun() ->
      col().it "Is a success" <| fun() ->
        ()

      let result = run()
      assertEqual (result.summary()) "1 run, 0 failed"

  describe "Running status" <| fun () ->
    it "Is reported while running" <| fun () ->
      col().describe "Some context" <| fun () ->
        col().it "has some behavior" <| fun () ->
          ()
      let result = run()
      assertEqual (result.testOutput()) "Some context has some behavior - passed"

describe "TestResult" <| fun() ->
  describe "With no failures reported" <| fun () ->
    it "Is a success" <| fun () ->
      let r = TestResult()
      assertTrue (r.success())

  describe "With failures reported" <| fun() ->
    it "Is a failure" <| fun () ->
      let r = TestResult()
      r.reportFailure()
      assertFalse(r.success())

let result = TestResult()
c.run(result)
printfn "%s" (result.summary())

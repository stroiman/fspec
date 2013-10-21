module Main
open FSpec

let test (desc: string) (f: unit -> unit) =
  printfn "Executing test: %s" desc
  f()

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

test "Calling run should run tests" <| fun() ->
  let wasRun = ref false
  let c = TestCollection()
  c.it "test" <| fun() ->
    wasRun := true
  c.run()
  assertTrue !wasRun

test "Calling run should call setup" <| fun() ->
  let wasSetup = ref false
  let c = TestCollection()
  c.before <| fun() ->
    wasSetup := true
  c.it "dummy" <| fun() ->
    ()
  c.run()
  assertTrue !wasSetup

test "Setup should run before test" <| fun () ->
  let wasSetupWhenTestWasRun = ref false
  let wasSetup = ref false
  let c = TestCollection()
  c.before <| fun() ->
    wasSetup := true
  c.it "dummy" <| fun() ->
    wasSetupWhenTestWasRun := !wasSetup
  c.run()
  assertTrue !wasSetupWhenTestWasRun  

test "Setup is only run in the same context levet" <| fun () ->
  let outerSetupRunCount = ref 0
  let innerSetupRunCount = ref 0
  let c = TestCollection()
  c.describe "Ctx" <| fun () ->
    c.before <| fun () ->
      outerSetupRunCount := !outerSetupRunCount + 1
    c.it "Outer test" <| fun () ->
      ()
    c.describe "Inner ctx" <| fun () ->
      c.before <| fun () ->
        innerSetupRunCount := !innerSetupRunCount + 1
      c.it "Inner test" <| fun () ->
        ()
      c.it "Inner test2" <| fun () ->
        ()
  c.run()
  assertEqual !innerSetupRunCount 2

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

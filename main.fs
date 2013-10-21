module Main
open FSpec

let test (desc: string) (f: unit -> unit) =
  printfn "Executing test: %s" desc
  f()

let assertEqual a b =
  if not (a = b) then
    failwithf "Not equal %A and %A" a b
    ()
  ()

let assertTrue value =
  if not value then
    failwithf "Value was false"
    ()
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

test "Run() should report test success" <| fun () ->
  let c = TestCollection()
  
  c.describe "Ctx" <| fun () ->
    c.it "Succeeds" <| fun () ->
      ()

  let result = TestResult()
  c.run(result)
  assertEqual (result.summary()) "1 run, 0 failed"

test "run() should report test failure" <| fun () ->
  let c = TestCollection()

  c.describe "Ctx" <| fun () ->
    c.it "Fails" <| fun () ->
      failwithf "Just another failure"

  let result = TestResult()
  c.run(result)
  assertEqual (result.summary()) "1 run, 1 failed"

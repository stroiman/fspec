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

test "Add scenario to test collection" <| fun () ->
  let c = TestCollection()
  c.describe "Some context" <| fun () ->
    c.it "Some test" <| fun () ->
      ()
  assertEqual (c.NoOfTests()) 1

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

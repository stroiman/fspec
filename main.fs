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

test "Add scenario to test collection" <| fun () ->
  let c = TestCollection()
  c.describe "Some context" <| fun () ->
    c.it "Some test" <| fun () ->
      ()
  assertEqual (c.NoOfTests()) 1

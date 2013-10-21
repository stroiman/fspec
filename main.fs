module Main

let test (desc: string) (f: unit -> unit) =
  printfn "Executing test: %s" desc
  f()

test "Dummy" <| fun () ->
  printfn "Inside test"

module Expectations
open System

exception AssertionError

let equal (actual : Object) (expected: Object) =
    if (actual.Equals(expected)) |> not then
        raise AssertionError
    ()

type Matcher = (Object -> Object -> unit)

type System.Object with
    member self.should (matcher : Matcher) matchparameter =
        matcher self matchparameter
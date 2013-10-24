module Expectations
open System

type AssertionErrorInfo = { Expected: string }

exception AssertionError of AssertionErrorInfo

let equal (actual : Object) (expected: Object) =
    if (actual.Equals(expected)) |> not then
        let info = { Expected = expected.ToString() }
        raise (AssertionError(info))
    ()

type Matcher = (Object -> Object -> unit)

type System.Object with
    member self.should (matcher : Matcher) matchparameter =
        matcher self matchparameter

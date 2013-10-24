module Expectations
open System

type AssertionErrorInfo = { 
    Expected: string 
    Actual: string
}

exception AssertionError of AssertionErrorInfo

type Matcher = {
    matcherFunc : Object -> Object -> bool
    }

let equal = {
    matcherFunc = fun actual expected ->
        actual.Equals(expected)
    }

type System.Object with
    member self.should (matcher : Matcher) expected =
        let success = matcher.matcherFunc self expected
        if not success then
            let info = { Expected = expected.ToString(); Actual = self.ToString() }
            raise (AssertionError(info))

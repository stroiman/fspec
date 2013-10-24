module Expectations
open System

type AssertionErrorInfo = { 
    Expected: string 
    Actual: string
}

exception AssertionError of AssertionErrorInfo

type Matcher<'a,'b> = {
    matcherFunc : 'a -> 'b -> bool
    }

let equal = {
    matcherFunc = fun actual expected ->
        actual.Equals(expected)
    }

let should (matcher : Matcher<'a,'b>) expected actual =
    let success = matcher.matcherFunc actual expected
    if not success then
        let info = { Expected = expected.ToString(); Actual = actual.ToString() }
        raise (AssertionError(info))

type System.Object with
    member self.should (matcher : Matcher<Object,'b>) expected =
        let success = matcher.matcherFunc self expected
        if not success then
            let info = { Expected = expected.ToString(); Actual = self.ToString() }
            raise (AssertionError(info))
module be =
    let greaterThan = {
        matcherFunc = fun actual expected ->
            actual > expected
        }


module Expectations
open System

type AssertionErrorInfo = { 
    Expected: string 
    Actual: string
    Message: string
}

exception AssertionError of AssertionErrorInfo

type Matcher<'a,'b> = {
    matcherFunc : 'a -> 'b -> bool
    writeException : 'a -> 'b -> string
    }

let equal = {
    matcherFunc = fun actual expected -> 
        actual.Equals(expected)
    writeException = fun a b -> 
        sprintf "expected %s to equal %s" (a.ToString()) (b.ToString())
    }

let should (matcher : Matcher<'a,'b>) expected actual =
    let success = matcher.matcherFunc actual expected
    if not success then
        let info = { 
            Expected = expected.ToString(); 
            Actual = actual.ToString();
            Message = matcher.writeException actual expected}
        raise (AssertionError(info))

type System.Object with
    member self.should (matcher : Matcher<Object,'b>) expected =
        let actual = self
        let success = matcher.matcherFunc actual expected
        if not success then
            let info = { 
                Expected = expected.ToString(); 
                Actual = actual.ToString();
                Message = matcher.writeException actual expected}
            raise (AssertionError(info))

let throw = {
    matcherFunc = fun a b ->
        let mutable exceptionThrown = false
        try
            a()
        with
            | _ -> exceptionThrown <- true
        exceptionThrown
    writeException = (fun a b -> "Expected exception was not thrown")
}

module be =
    let greaterThan = {
        matcherFunc = fun actual expected ->
            actual > expected
        writeException = fun a b -> 
            sprintf "expected %s to be greater than %s" (a.ToString()) (b.ToString())
        }

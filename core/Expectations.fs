[<AutoOpen>]
module FSpec.Core.Expectations
open System

type AssertionErrorInfo = { 
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

let matchRegex = {
    matcherFunc = fun actual expected ->
        let regex = System.Text.RegularExpressions.Regex expected
        if regex.IsMatch actual then
            true
        else
            false

    writeException = fun a b ->
        sprintf "expected %s to match expression %s" a b
}

let should (matcher : Matcher<'a,'b>) expected actual =
    let success = matcher.matcherFunc actual expected
    if not success then
        let info = { 
            Message = matcher.writeException actual expected}
        raise (AssertionError(info))

type System.Object with
    member self.should (matcher : Matcher<Object,'b>) expected =
        self |> should matcher expected

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

    let equalTo = equal

module FSpec.Core.MatchersV2

module MatchResult =
    type T = {
            Success: bool;
            Message: string
        }

    let create success = { Success = success; Message =  "expected 5 to equal 6" }
    let setMessage message (result : T) = { result with Message = message }
    
let isOfType (t: System.Type) actual =
    t.IsInstanceOfType(actual)

let should matcher =
    let reportMatch (value : MatchResult.T) =
        if not (value.Success) then
            raise (AssertionError({Message = value.Message}))
    matcher reportMatch

let shouldNot matcher =
    let reportMatch (value : MatchResult.T) =
        if (value.Success) then
            raise (AssertionError({Message = "Expected false"}))
    matcher reportMatch

let equal (report : MatchResult.T -> unit) expected actual =
    MatchResult.create (expected = actual)
    |> MatchResult.setMessage (sprintf "expected %A to equal %A" actual expected)
    |> report

let beOfType<'T> (report : MatchResult.T -> unit) (actual : obj) =
    let expectedType = typeof<'T>
    let success = expectedType.IsInstanceOfType(actual)
    let result = MatchResult.create success
    report result

module FSpec.Core.MatchersV2

module MatchResult =
    type T = {
            Success: bool;
            FailureMessageForShould: string;
            FailureMessageForShouldNot: string;
        }

    let create success = { 
        Success = success; 
        FailureMessageForShould =  "assertion failed";
        FailureMessageForShouldNot = "assertion failed" 
    }
        
    let setFailureMessageForShould message (result : T) = { result with FailureMessageForShould = message }
    let setFailureMessageForShouldNot message result = { result with FailureMessageForShouldNot = message }
    
let reportBack (report: MatchResult.T -> unit) result = report result

let isOfType (t: System.Type) actual =
    t.IsInstanceOfType(actual)

let should matcher =
    let reportMatch (value : MatchResult.T) =
        if not (value.Success) then
            raise (AssertionError({Message = value.FailureMessageForShould}))
    matcher reportMatch

let shouldNot matcher =
    let reportMatch (value : MatchResult.T) =
        if (value.Success) then
            raise (AssertionError({Message = value.FailureMessageForShouldNot}))
    matcher reportMatch

let equal report expected actual =
    MatchResult.create (expected = actual)
    |> MatchResult.setFailureMessageForShould (sprintf "expected %A to equal %A" actual expected)
    |> MatchResult.setFailureMessageForShouldNot (sprintf "expected %A to not equal %A" actual expected)
    |> reportBack report

let beOfType<'T> report (actual : obj) =
    let expectedType = typeof<'T>
    expectedType.IsInstanceOfType(actual)
    |> MatchResult.create
    |> reportBack report

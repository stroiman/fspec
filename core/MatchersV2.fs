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
        
    let build success failureMsgForShould failureMsgForShouldNot =
        { Success = success;
          FailureMessageForShould = failureMsgForShould;
          FailureMessageForShouldNot = failureMsgForShouldNot }
    let assertSuccess result =
        if not (result.Success) then
            raise (AssertionError({Message = result.FailureMessageForShould}))
    let assertFail result =
        if (result.Success) then
            raise (AssertionError({Message = result.FailureMessageForShouldNot}))

let should matcher = matcher MatchResult.assertSuccess
let shouldNot matcher = matcher MatchResult.assertFail
    
type VerifyResult = MatchResult.T -> unit

let isOfType (t: System.Type) actual =
    t.IsInstanceOfType(actual)

let equal verifyResult expected = 
    fun actual ->
        MatchResult.build 
            (expected = actual)
            (sprintf "expected %A to equal %A" actual expected)
            (sprintf "expected %A to not equal %A" actual expected)
        |> verifyResult

let beOfType<'T> (verifyResult : VerifyResult) =
    let expectedType = typeof<'T>
    fun actual ->
        MatchResult.build 
            (expectedType.IsInstanceOfType(actual))
            (sprintf "expected %A to be of type %A" actual expectedType)
            (sprintf "expected %A to not be of type %A" actual expectedType)
        |> verifyResult

let matchRegex verifyResult pattern =
    fun actual ->
        let regex = System.Text.RegularExpressions.Regex pattern
        MatchResult.build
            (regex.IsMatch actual)
            (sprintf "expected %A to match regex pattern %A" actual pattern)
            (sprintf "expected %A to not match regex pattern %A" actual pattern)
        |> verifyResult

let fail verifyResult actual =
    let isMatch =
        try
            actual ()
            false
        with
            | _ -> true
    MatchResult.build
        isMatch 
        "expected exception to be thrown, but none was thrown"
        "exception was thrown when none was expected"
    |> verifyResult    

module be =
    let greaterThan verifyResult expected actual =
        MatchResult.build 
            (actual > expected)
            (sprintf "expected %A to be greater than %A" actual expected)
            (sprintf "expected %A to not be greater than %A" actual expected)
        |> verifyResult


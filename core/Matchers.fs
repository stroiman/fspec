module FSpec.Core.Matchers

type IsMatch = IsMatch of bool
type ShouldMessage = ShouldMessage of string
type ShouldNotMessage = ShouldNotMessage of string

module MatchResult =
    type T = {
            IsMatch: IsMatch
            FailureMessageForShould: ShouldMessage
            FailureMessageForShouldNot: ShouldNotMessage
        }

    let create success = { 
        IsMatch = success;
        FailureMessageForShould =  ShouldMessage "assertion failed"
        FailureMessageForShouldNot = ShouldNotMessage "assertion failed" }
        
    let success result = 
        let apply (IsMatch m) = m
        apply result.IsMatch

    let build isMatch failureMsgForShould failureMsgForShouldNot =
        { IsMatch = isMatch;
          FailureMessageForShould = failureMsgForShould;
          FailureMessageForShouldNot = failureMsgForShouldNot }
    let assertSuccess result =
        if not (result |> success) then
            let apply (ShouldMessage msg) = msg
            raise (AssertionError({Message = (apply result.FailureMessageForShould)}))
    let assertFail result =
        if (result |> success) then
            let apply (ShouldNotMessage msg) = msg
            raise (AssertionError({Message = apply result.FailureMessageForShouldNot}))

let should matcher = matcher MatchResult.assertSuccess
let shouldNot matcher = matcher MatchResult.assertFail
    
type VerifyResult = MatchResult.T -> unit

let isOfType (t: System.Type) actual =
    t.IsInstanceOfType(actual)

let equal verifyResult expected = 
    fun actual ->
        MatchResult.build 
            (IsMatch (expected = actual))
            (ShouldMessage (sprintf "expected %A to equal %A" actual expected))
            (ShouldNotMessage (sprintf "expected %A to not equal %A" actual expected))
        |> verifyResult

let beOfType<'T> (verifyResult : VerifyResult) =
    let expectedType = typeof<'T>
    fun actual ->
        MatchResult.build 
            (expectedType.IsInstanceOfType(actual) |> IsMatch)
            (sprintf "expected %A to be of type %A" actual expectedType |> ShouldMessage)
            (sprintf "expected %A to not be of type %A" actual expectedType |> ShouldNotMessage)
        |> verifyResult

let matchRegex verifyResult pattern =
    fun actual ->
        let regex = System.Text.RegularExpressions.Regex pattern
        MatchResult.build
            (regex.IsMatch actual |> IsMatch)
            (sprintf "expected %A to match regex pattern %A" actual pattern |> ShouldMessage)
            (sprintf "expected %A to not match regex pattern %A" actual pattern |> ShouldNotMessage)
        |> verifyResult

let fail verifyResult actual =
    let isMatch =
        try
            actual ()
            IsMatch false
        with
            | _ -> IsMatch true
    MatchResult.build
        isMatch
        ("expected exception to be thrown, but none was thrown" |> ShouldMessage)
        ("exception was thrown when none was expected" |> ShouldNotMessage)
    |> verifyResult    

module be =
    let greaterThan verifyResult expected actual =
        MatchResult.build 
            (actual > expected |> IsMatch)
            (sprintf "expected %A to be greater than %A" actual expected |> ShouldMessage)
            (sprintf "expected %A to not be greater than %A" actual expected |> ShouldNotMessage)
        |> verifyResult

let toBe matcher =
    matcher MatchResult.success

module have =
    let exactly verifyResult no matcher actual =
        let result = actual |> Seq.filter matcher |> Seq.length
        result |> should equal no

    let element verifyResult matcher actual =
        let result = actual |> Seq.exists matcher |> IsMatch
        MatchResult.build
            result
            (ShouldMessage "")
            (ShouldNotMessage "")
        |> verifyResult

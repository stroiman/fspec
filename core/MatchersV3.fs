module FSpec.MatchersV3

// A MatchResult contains the data that was matched. This
// is sometimes the same as the original value, e.g. an 
// equal matcher, but can return something else, e.g. a
// length matcher would take a seq as input and return the
// length as the matched value, allowing an equal or a 
// greaterThan matcher to execute on the langth
type MatchResult<'TSuccess,'TFail> =
    | Success of 'TSuccess * string
    | Failure of 'TFail * string

// Generic matcher type, takes an actual value as input
// and returns a match result as output
type Matcher<'a,'b,'c> = 'a -> MatchResult<'b,'c>

module Matcher =
    let bind f x =
        match x with
        | Success(x,_) -> f x
        | Failure(x,y) -> Failure(x,y)

let (>=>) f g = f >> Matcher.bind g

let equal expected actual =
    if actual = expected then
        Success(actual, sprintf "equal %A" expected)
    else
        Failure(actual, sprintf "equal %A" expected)

let haveLength actual = Success(actual |> Seq.length, "have length")

let should matcher actual = 
    actual |> matcher |> function
    | Success _ -> ()
    | Failure _ -> raise (AssertionError { Message = "failed" })
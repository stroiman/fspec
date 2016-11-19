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
type M<'a,'b,'c> = M of ('a -> MatchResult<'b,'c>)

module M =
    let run (M matcher) actual = actual |> matcher
    let bind f x =
        match x with
        | Success(x,_) -> f x
        | Failure(x,y) -> Failure(x,y)

let (>=>) (M f) (M g) = f >> M.bind g |> M

let equal expected =
    let m actual =
        if actual = expected then
            Success(actual, sprintf "equal %A" expected)
        else
            Failure(actual, sprintf "equal %A" expected)
    M m

let haveLength =
    let m actual =
        Success(actual |> Seq.length, "have length")
    M m

let should matcher actual = 
    actual |> M.run matcher |> function
    | Success _ -> ()
    | Failure _ -> raise (AssertionError { Message = "failed" })
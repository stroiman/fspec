module FSpec.Core.MatchersV3

[<AbstractClass>]
type Matcher<'TActual> () = 
    abstract member ApplyActual<'TResult> : (bool -> 'TResult) -> 'TActual -> 'TResult
    abstract member FailureMsgForShould : string
    abstract member FailureMsgForShouldNot : string
    default this.FailureMsgForShouldNot : string = sprintf "not %s" this.FailureMsgForShould

module be =
    let equalTo<'T when 'T : equality> (expected:'T) =
        { new Matcher<'T> () with
            member __.ApplyActual f (actual) = f (expected.Equals(actual))
            member __.FailureMsgForShould = sprintf "be equal to %A" expected
        }

module have =
    let atLeastOneElement<'T,'U when 'T :> seq<'U>> (matcher : Matcher<'U>) =
        { new Matcher<'T> () with
            member __.ApplyActual f (actual) =
                let success = actual |> Seq.exists (fun x -> matcher.ApplyActual id x)
                f success
            member __.FailureMsgForShould = 
                sprintf "contain at least one element to %s" matcher.FailureMsgForShould
            member __.FailureMsgForShouldNot =
                sprintf "contain no elements to %s" matcher.FailureMsgForShould
        }

    
let shouldNot<'T> (matcher:Matcher<'T>) (actual:'T) =
    let continuation = function
        | false -> ()
        | true -> 
            let msg = sprintf "Expected %A to %s" actual matcher.FailureMsgForShouldNot
            raise (AssertionError { Message = msg })
    matcher.ApplyActual continuation actual
    
let should<'T> (matcher:Matcher<'T>) (actual:'T) =
    let continuation = function
        | true -> ()
        | false -> 
            let msg = sprintf "Expected %A to %s" actual matcher.FailureMsgForShould
            raise (AssertionError { Message = msg })
    matcher.ApplyActual continuation actual



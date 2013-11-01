module FSpec.Core.MatchersV2

let shouldBeTypeOf<'T> = fun actual ->
    if (typeof<'T>.IsInstanceOfType(actual)) then
        ()
    else
        raise (AssertionError({ Message = "Wrong type"}))

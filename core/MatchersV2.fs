module FSpec.Core.MatchersV2

let shouldBeTypeOf<'T> = fun actual ->
    if (actual = null) then
        raise (AssertionError({ Message = "Null value" }))
    else if (actual.GetType() = typeof<'T>) then
        ()
    else
        raise (AssertionError({ Message = "Wront type"}))

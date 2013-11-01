module FSpec.Core.MatchersV2

let shouldBeTypeOf<'T> = fun actual ->
    if (actual.GetType() = typeof<'T>) then
        ()
    else
        raise (AssertionError({ Message = "Wront type"}))

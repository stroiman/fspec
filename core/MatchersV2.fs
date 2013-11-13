module FSpec.Core.MatchersV2

let isOfType (t: System.Type) actual =
    t.IsInstanceOfType(actual)

let should matcher =
    let reportMatch value =
        if not value then
            raise (AssertionError({Message = "Expected true"}))
    matcher reportMatch

let shouldNot matcher =
    let reportMatch value =
        if value then
            raise (AssertionError({Message = "Expected false"}))
    matcher reportMatch

let not (report : bool -> unit) builder =
    let invertedReport value =
        report (not value)
    builder invertedReport

let beOfType<'T> (report : bool -> unit) (actual : obj) =
    let expectedType = typeof<'T>
    report (expectedType.IsInstanceOfType(actual))

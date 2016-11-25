module FSpec.Matchers

type MatchResult<'TSuccess> =
    | MatchSuccess of 'TSuccess
    | MatchFail of obj

type MatchType =
    | Should
    | ShouldNot

type Matcher<'TActual,'TSuccess> = {
    Run : 'TActual -> MatchResult<'TSuccess>
    ExpectationMsgForShould : string
    ExpectationMsgForShouldNot : string
}

let runMatcher<'T,'U> (matcher:Matcher<'T,'U>) actual = matcher.Run actual
let private expectationMsgForShould<'T,'U> (matcher:Matcher<'T,'U>) = matcher.ExpectationMsgForShould
let private expectationMsgForShouldNot<'T,'U> (matcher:Matcher<'T,'U>) = matcher.ExpectationMsgForShouldNot

module Matcher =
    let run<'T,'U> (matcher:Matcher<'T,'U>) actual = matcher.Run actual
    let matches matcher actual = run matcher actual |> function | MatchSuccess _ -> true | MatchFail _ -> false
    let expectationMsgForShould<'T,'U> (matcher:Matcher<'T,'U>) = matcher.ExpectationMsgForShould
    let expectationMsgForShouldNot<'T,'U> (matcher:Matcher<'T,'U>) = matcher.ExpectationMsgForShouldNot
    let messageFor matchType matcher =
        match matchType with
        | Should -> expectationMsgForShould matcher
        | ShouldNot -> expectationMsgForShouldNot matcher

let createFullMatcher<'T,'U>
        (f : 'T -> MatchResult<'U>)
        (shouldMsg : string)
        (shouldNotMsg : string) =
    { Run = f
      ExpectationMsgForShould = shouldMsg
      ExpectationMsgForShouldNot = shouldNotMsg
    }

let createMatcher<'T,'U> (f : 'T -> MatchResult<'U>) (shouldMsg : string) =
    createFullMatcher f shouldMsg (sprintf "not %s" shouldMsg)

/// Helps create a matcher, that uses a child matcher for some verification.
/// The passed function should extract the value from the actual value, that
/// the child matcher should match. E.g. for a sequence length matcher, the
/// f extracts the length of the sequence, and the matcher matches the length.
let createCompoundMatcher matcher f =
    createMatcher (fun a -> a |> f |> Matcher.run matcher)

[<System.Obsolete("Use function createCompoundMatcher instead")>]
let createCompountMatcher = createCompoundMatcher

let createFullBoolMatcher<'T,'U>
        (f : 'T -> bool)
        (shouldMsg : string)
        (shouldNotMsg : string) =
    let wrapF = fun a ->
        match f a with
        | true -> MatchSuccess (a :> obj)
        | false -> MatchFail (a :> obj)
    createFullMatcher wrapF shouldMsg shouldNotMsg

let createBoolMatcher<'T,'U> (f : 'T -> bool) (shouldMsg : string) =
    createFullBoolMatcher f shouldMsg (sprintf "not %s" shouldMsg)

let createSimpleMatcher f = createBoolMatcher f "FAIL"

let equal expected =
    createBoolMatcher
        (fun a -> a = expected)
        (sprintf "equal %A" expected)

module be =
    let equalTo = equal

    let ofType<'T> () =
        let f (actual:obj) =
            match actual with
            | :? 'T as x -> MatchSuccess x
            | null -> MatchFail null
            | _ as x -> MatchFail (x.GetType())
        createMatcher f (sprintf "be of type %s" (typeof<'T>.Name))

    let greaterThan expected =
        createBoolMatcher
            (fun a -> a > expected)
            (sprintf "be greater than %A" expected)

    let lessThan expected =
        createBoolMatcher
            (fun a -> a < expected)
            (sprintf "be greater than %A" expected)

    let True =
        createBoolMatcher
            (fun actual -> actual = true)
            (sprintf "be true")

    let False =
        createBoolMatcher
            (fun actual -> actual = false)
            (sprintf "be false")

    module string =
        let containing expected =
            createBoolMatcher
                (fun (a:string) -> a.Contains(expected))
                (sprintf "contain '%s'" expected)

        let matching pattern =
            let regex = System.Text.RegularExpressions.Regex pattern
            createBoolMatcher
                (fun actual -> regex.IsMatch actual)
                (sprintf "match regex pattern %A" pattern)

module have =
    let atLeastOneElement matcher =
        let f a = a |> Seq.exists (Matcher.matches matcher)
        let msg = sprintf "contain at least one element to %s" matcher.ExpectationMsgForShould
        let notMsg = sprintf "contain no elements to %s" matcher.ExpectationMsgForShould
        createFullBoolMatcher f msg notMsg

    let element = atLeastOneElement

    let length lengthMatcher =
        createCompoundMatcher
            lengthMatcher
            Seq.length
            (sprintf "have length to %s" (lengthMatcher |> expectationMsgForShould))

    let exactly no matcher =
        let f a = a |> Seq.filter (Matcher.matches matcher) |> Seq.length = no
        let msg =
            sprintf "contain exactly %d element to %s" no
                matcher.ExpectationMsgForShould
        createBoolMatcher f msg

let fail =
    let f a =
        try
            a (); false
        with
        | _ -> true
    createBoolMatcher f "fail"

let succeed =
    let f a =
        try
            a (); true
        with
        | _ -> false
    createBoolMatcher f "pass"

module throwException =
    let withMessage matcher =
        let f a =
            try
                a ()
                MatchFail "No exception thrown"
            with
            | e -> e.Message |> runMatcher matcher
        createMatcher f
            (sprintf "throw exception with message %s" matcher.ExpectationMsgForShould)

    let withMessageContaining msg =
        withMessage (be.string.containing msg)

let performMatch<'T,'U> matchType (matcher:Matcher<'T,'U>) (actual:'T) =
    let raiseMsg a =
          let msg = sprintf "%A was expected to %s but was %A"
                      actual (matcher |> Matcher.messageFor matchType) a
          raise (AssertionError { Message = msg })
    let continuation result =
        match (matchType, result) with
        | (Should, MatchFail x) -> raiseMsg x
        | (ShouldNot, MatchSuccess x) -> raiseMsg x
        | _ -> ()
    // matcher.ApplyActual continuation actual
    actual |> matcher.Run |> continuation

let should<'T,'U> = performMatch<'T,'U> Should
let shouldNot<'T,'U> = performMatch<'T,'U> ShouldNot

/// Extension methods for System.Object to aid in assertions
type System.Object with
    /// Allows the use of testContext.Subject.Should (matcher)
    member self.Should<'T,'U> (matcher : Matcher<'T,'U>) =
        self :?> 'T |> should matcher

    /// Allows the use of testContext.Subject.ShouldNot (matcher)
    member self.ShouldNot<'T,'U> (matcher : Matcher<'T,'U>) =
        self :?> 'T |> shouldNot matcher

    member self.Apply<'T,'U> (f : 'T -> 'U) =
        self :?> 'T |> f

type Async<'T> with
    member self.Should<'T,'U> (matcher : Matcher<'T,'U>) =
        Async.RunSynchronously(self,5000).Should matcher

    member self.ShouldNot<'T,'U> (matcher : Matcher<'T,'U>) =
        Async.RunSynchronously(self,5000).ShouldNot matcher

let ( ||| ) (a:Matcher<'a,'U>) (b:Matcher<'a,'U>) =
    let f actual =
        let x = actual |> a.Run
        let y = actual |> b.Run
        match (x,y) with
        | MatchFail _, MatchFail _ -> MatchFail actual
        | _ -> MatchSuccess actual
    createMatcher f
        (sprintf "%s and %s"
            a.ExpectationMsgForShould
            b.ExpectationMsgForShould)

let ( &&& ) (a:Matcher<'a,'U>) (b:Matcher<'a,'U>) =
    let f actual =
        let x = actual |> a.Run
        let y = actual |> b.Run
        match (x,y) with
        | MatchSuccess _, MatchSuccess _ -> MatchSuccess actual
        | _ -> MatchFail actual
    createMatcher f
        (sprintf "%s and %s"
            a.ExpectationMsgForShould
            b.ExpectationMsgForShould)

let ( >>> ) (a : Matcher<'a,'b>) (b: Matcher<'b,'c>) =
    let f actual =
        match actual |> a.Run with
        | MatchSuccess x -> x |> b.Run
        | MatchFail x -> MatchFail x
    createMatcher f (sprintf "%s %s" a.ExpectationMsgForShould b.ExpectationMsgForShould)

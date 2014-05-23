module FSpec.Core.MatchersV3

type MatchResult =
    | MatchSuccess of obj
    | MatchFail of obj
    with 
        static member apply f = 
            function
                | MatchSuccess _ -> f true
                | _ -> f false

[<AbstractClass>]
type Matcher<'TActual> () = 
    abstract member ApplyActual<'TResult> : (MatchResult -> 'TResult) -> 'TActual -> 'TResult
    abstract member FailureMsgForShould : string
    abstract member FailureMsgForShouldNot : string
    static member IsMatch (matcher:Matcher<'TActual>) actual =
        matcher.ApplyActual (MatchResult.apply id) actual

let applyMatcher<'T,'U> (matcher: Matcher<'T>) f (a : 'T) : 'U =
    matcher.ApplyActual f a

let newCreateFullMatcher<'T> (f : 'T -> MatchResult) (shouldMsg : string) (shouldNotMsg : string) =
    { new Matcher<'T> () with
        member __.ApplyActual g actual = f actual |> g
        member __.FailureMsgForShould = shouldMsg
        member __.FailureMsgForShouldNot = shouldNotMsg
    }

let newCreateMatcher<'T> (f : 'T -> MatchResult) (shouldMsg : string) =
    newCreateFullMatcher f shouldMsg (sprintf "not %s" shouldMsg)

let createFullMatcher<'T> 
        (f : 'T -> bool) 
        (shouldMsg : string) 
        (shouldNotMsg : string) =
    let wrapF = fun a -> 
        match f a with
        | true -> MatchSuccess (a :> obj)
        | false -> MatchFail (a :> obj)
    newCreateFullMatcher wrapF shouldMsg shouldNotMsg

let createMatcher<'T> (f : 'T -> bool) (shouldMsg : string) =
    createFullMatcher f shouldMsg (sprintf "not %s" shouldMsg)

let createSimpleMatcher f = createMatcher f "FAIL"
        
let equal expected =
    createMatcher
        (fun a -> a = expected)
        (sprintf "equal %A" expected)

module be =
    let equalTo = equal

    let True =
        createMatcher 
            (fun actual -> actual = true) 
            (sprintf "be true")

    let False =
        createMatcher 
            (fun actual -> actual = false)
            (sprintf "be false")

    module string =
        let containing expected =
            createMatcher
                (fun (a:string) -> a.Contains(expected))
                (sprintf "contain %s" expected)

        let matching pattern =
            let regex = System.Text.RegularExpressions.Regex pattern
            createMatcher
                (fun actual -> regex.IsMatch actual)
                (sprintf "match regex pattern %A" pattern)

/// Helps create a matcher, that uses a child matcher for some verification.
/// The passed function should extract the value from the actual value, that
/// the child matcher should match. E.g. for a sequence length matcher, the
/// f extracts the length of the sequence, and the matcher matches the length.
let createCompountMatcher matcher f =
    newCreateMatcher
        (fun a -> a |> f |> applyMatcher matcher id)

module have =
    let atLeastOneElement matcher =
        let f a = a |> Seq.exists (Matcher.IsMatch matcher)
        let msg = sprintf "contain at least one element to %s" matcher.FailureMsgForShould
        let notMsg = sprintf "contain no elements to %s" matcher.FailureMsgForShould
        createFullMatcher f msg notMsg
    
    let length lengthMatcher =
        createCompountMatcher
            lengthMatcher
            (fun a -> a |> Seq.length) 
            (sprintf "have length to %s" lengthMatcher.FailureMsgForShould) 

    let exactly no matcher =
        let f a = a |> Seq.filter (Matcher.IsMatch matcher) |> Seq.length = no
        let msg = 
            sprintf "contain exactly %d element to %s" no 
                matcher.FailureMsgForShould
        createMatcher f msg

let fail =
    let f a =
        try
            a (); false
        with
        | _ -> true
    createMatcher f "fail"

module throwException =
    let withMessage matcher =
        let f a = 
            try
                a ()
                MatchFail "No exception thrown"
            with
            | e -> e.Message |> applyMatcher matcher id 
        newCreateMatcher f
            (sprintf "throw exception with message %s" matcher.FailureMsgForShould)
            
    let withMessageContaining msg =
        withMessage (be.string.containing msg)
    
let shouldNot<'T> (matcher:Matcher<'T>) (actual:'T) =
    let continuation = function
        | MatchFail _ -> ()
        | MatchSuccess a ->
            let msg = sprintf "%A was expected to %s" a matcher.FailureMsgForShouldNot
            raise (AssertionError { Message = msg })
    matcher.ApplyActual continuation actual
    
let should<'T> (matcher:Matcher<'T>) (actual:'T) =
    let continuation = function
        | MatchSuccess _ -> ()
        | MatchFail a -> 
            let msg = sprintf "%A was expected to %s but was %A" actual matcher.FailureMsgForShould a
            raise (AssertionError { Message = msg })
    matcher.ApplyActual continuation actual

/// Extension methods for System.Object to aid in assertions
type System.Object with
    /// Allows the use of testContext.Subject.Should (matcher)
    member self.Should<'T> (matcher : Matcher<'T>) =
        self :?> 'T |> should matcher

    /// Allows the use of testContext.Subject.ShouldNot (matcher)
    member self.ShouldNot<'T> (matcher : Matcher<'T>) =
        self :?> 'T |> shouldNot matcher

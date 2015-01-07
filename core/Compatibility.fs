namespace FSpec
#nowarn "0044" // We don't want warnings if this file refers to obsolete functions
open System

module Core =
    type TestContext = FSpec.TestContext

    let (++) = FSpec.TestDataMap.(++)

    module Dsl =
        open FSpec.Dsl
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let examples = examples
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let behavior = behavior
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let describe = describe
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let context = context
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let subject = subject
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let before = before
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let after = after
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let it = it
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let itShould<'T,'U> = itShould<'T,'U>
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let itShouldNot<'T,'U> = itShouldNot<'T,'U>
        [<Obsolete("Use types from FSpec.Dsl namespace instead of FSpec.Core.Dsl")>]
        let (<<-) = (<<-)

    module MatchersV3 =
        open FSpec.Matchers

        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let applyMatcher = applyMatcher
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createMatcher<'T> = createMatcher<'T,_>
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createFullMatcher<'T> = createFullMatcher<'T,_> 
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createSimpleMatcher = createSimpleMatcher
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createFullBoolMatcher<'T> = createFullBoolMatcher<'T> 
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createBoolMatcher = createBoolMatcher
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let createCompountMatcher = createCompoundMatcher
        
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let equal = equal
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let fail = fail
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let succeed = succeed
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let should<'T,'U> = should<'T,'U>
        [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
        let shouldNot<'T,'U> = shouldNot<'T,'U>

        module throwException =
            open FSpec.Matchers.throwException
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let withMessage = withMessage
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let withMessageContaining = withMessageContaining

        module have = 
            open FSpec.Matchers.have
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let element = element
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let atLeastOneElement = atLeastOneElement
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let length = length
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let exactly = exactly

        module be =
            open FSpec.Matchers.be
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let True = True
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let False = False
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let greaterThan = greaterThan
            [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
            let equalTo = equalTo
            module string =
                open string
                [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
                let containing = containing
                [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
                let matching = matching
            
        type System.Object
            with
                [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
                member self.Should<'T> (matcher : Matcher<'T,_>) =
                    self :?> 'T |> FSpec.Matchers.should matcher
                [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
                member self.ShouldNot<'T> (matcher : Matcher<'T,_>) =
                    self :?> 'T |> FSpec.Matchers.shouldNot matcher
                [<Obsolete("Use types from FSpec.Matchers instead of FSpec.Core.MatchersV3")>]
                member self.Apply<'T,'U> (f : 'T -> 'U) =
                    self :?> 'T |> f
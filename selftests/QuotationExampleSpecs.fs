module FSpec.SelfTests.QuotationExampleSpecs
open FSpec.Core
open Dsl
open MatchersV3
open Helpers
open Example
open CustomMatchers

let createContextWithSubject subject =
    let ctx = TestContext.create TestDataMap.Zero
    ctx.Subject <- subject
    ctx

let testWithSubject subject ctx =
    let sutCtx = createContextWithSubject subject
    fun () -> ctx.Subject.Apply (Example.run sutCtx)

let createExample = function 
                        | AddExampleOperation x -> x 
                        | _ -> failwith "Not an example"
    
let specs =
    describe "Quotation building syntax" [
        describe "syntax (be.False)" [
            subject <| fun _ ->
                createExample (itShould (be.False))
                
            it "should be name 'should be false'" <| fun ctx ->
                ctx.Subject.Should (haveExampleNamed "should be false")

            it "should succeed when subject is false" <| fun c ->
                c |> testWithSubject false
                |> should succeed

            it "should fail when subject is true" <| fun c ->
                c |> testWithSubject true
                |> should fail
        ]

        describe "shouldNot (be.True)" [
            subject <| fun _ ->
                createExample (itShouldNot (be.True))
                
            it "should be name 'should not be true'" <| fun ctx ->
                ctx.Subject.Should (haveExampleNamed "should not be true")

            it "should succeed when subject is false" <| fun c ->
                c |> testWithSubject false
                |> should succeed

            it "should fail when subject is true" <| fun c ->
                c |> testWithSubject true
                |> should fail
        ]

        describe "syntax (have.length (equal 1))" [
            subject <| fun _ ->
                createExample (itShould (have.length (equal 1)))
                   
            it "should have name 'should have length equal 1'" <| fun c ->
                c.Subject.Should 
                    (haveExampleNamed "should have length to equal 1")

            it "should pass when given a collection with one element" <| fun c ->
                c |> testWithSubject ["dummy"]
                |> should succeed

            it "should fail when given a collection with two elements"<| fun c ->
                c |> testWithSubject ["foo"; "bar"]
                |> should fail

            it "should fail when subject is not a collection" <| fun c ->
                c |> testWithSubject 42
                |> should fail
        ]
    ]

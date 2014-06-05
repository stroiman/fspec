module FSpec.SelfTests.QuotationExampleSpecs
open FSpec.Core
open Dsl
open MatchersV3
open Helpers
open Example

let haveNameTo matcher =
    createCompountMatcher
        matcher
        (fun x -> x.Name)
        "have name to"

let createContextWithSubject subject =
    let ctx = TestContext.create TestDataMap.Zero
    ctx.Subject <- subject
    ctx

let testWithSubject subject ctx =
    let sutCtx = createContextWithSubject subject
    fun () -> ctx |> TestContext.getSubject |> Example.run sutCtx

let createExample = function 
                        | AddExampleOperation x -> x 
                        | _ -> failwith "Not an example"
    
let specs =
    describe "Quotation building syntax" [
        describe "syntax (be.False)" [
            subject <| fun _ ->
                createExample (itShould (be.False))
                
            it "should be name 'should be false'" <| fun ctx ->
                ctx.Subject.Should (haveNameTo 
                    (equal "should be false"))

            it "should succeed when subject is false" <| fun c ->
                c |> testWithSubject false
                |> shouldPass

            it "should fail when subject is true" <| fun c ->
                c |> testWithSubject true
                |> shouldFail
        ]

        describe "shouldNot (be.True)" [
            subject <| fun _ ->
                createExample (itShouldNot (be.True))
                
            it "should be name 'should not be true'" <| fun ctx ->
                ctx.Subject.Should (haveNameTo 
                    (equal "should not be true"))

            it "should succeed when subject is false" <| fun c ->
                c |> testWithSubject false
                |> shouldPass

            it "should fail when subject is true" <| fun c ->
                c |> testWithSubject true
                |> shouldFail
        ]

        describe "syntax (have.length (equal 1))" [
            subject <| fun _ ->
                createExample (itShould (have.length (equal 1)))
                   
            it "should have name 'should have length equal 1'" <| fun c ->
                c.Subject.Should (haveNameTo
                    (equal "should have length to equal 1"))

            it "should pass when given a collection with one element" <| fun c ->
                c |> testWithSubject ["dummy"]
                |> shouldPass

            it "should fail when given a collection with two elements"<| fun c ->
                c |> testWithSubject ["foo"; "bar"]
                |> shouldFail

            it "should fail when subject is not a collection" <| fun c ->
                c |> testWithSubject 42
                |> shouldFail
        ]
    ]

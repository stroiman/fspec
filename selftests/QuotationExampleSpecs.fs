module FSpec.SelfTests.QuotationExampleSpecs
open FSpec.Core
open Dsl
open MatchersV3
open Helpers

let haveNameTo matcher =
    createCompountMatcher
        matcher
        (fun x -> x |> Example.name)
        "have name to"

let createContextWithSubject subject =
    let ctx = TestContext.create TestDataMap.Zero
    ctx.Subject <- subject
    ctx

let testWithSubject subject ctx =
    let sutCtx = createContextWithSubject subject
    fun () -> ctx |> TestContext.getSubject |> Example.run sutCtx
    
let specs =
    describe "Quotation building syntax" [
        describe "syntax <@ be.False@>" [
            subject <| fun _ ->
                createExampleFromExpression 
                    <@ be.False @>
                
            it "should be name 'should be false'" <| fun ctx ->
                ctx.Subject.Should (haveNameTo 
                    (equal "should be false"))

            it "should succeed when subject is false" <| fun c ->
                c |> testWithSubject (false) 
                |> shouldPass

            it "should fail when subject is true" <| fun c ->
                c |> testWithSubject (false) 
                |> shouldFail
        ]
    ]
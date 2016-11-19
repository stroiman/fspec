module FSpec.SelfTests.MatchersV3
open FSpec
open Dsl
open MatchersV3

let shouldSucceed x = x |> FSpec.Matchers.should FSpec.Matchers.succeed
let shouldFail x = x |> FSpec.Matchers.should FSpec.Matchers.fail

let specs =
    describe "3rd generation matchers" [
        describe "equal" [
            it "succeeds when objects are equal" (fun _ ->
                let m () = 1 |> should (equal 1)
                m |> shouldSucceed
            )

            it "fails when objects are not equal" (fun _ ->
                let m () = 1 |> should (equal 2)
                m |> shouldFail
            )
        ]

        describe "haveLength" [
            it "passes when length is correct" (fun _ ->
                let m () = [1;2;3] |> should (haveLength >=> equal 3)
                m |> shouldSucceed
            )

            it "fails when length is incorrect" (fun _ ->
                let m () = [1;2;3] |> should (haveLength >=> equal 1)
                m |> shouldFail
            )
        ]
    ]
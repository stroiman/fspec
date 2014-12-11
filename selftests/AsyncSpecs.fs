module AsyncSpecs
open FSpec
open FSpec.Dsl
open FSpec.Matchers

let specs =
    describe "Async matchers" [
        describe "Async<'T>.Should" [
            it "waits for a result" (fun _ ->
                let asyncInt = async {
                    do System.Threading.Thread.Sleep(500)
                    return 42
                }
                asyncInt.Should (equal 42)
            )

            it "fails if the operation takes more than 5 sec." (fun _ ->
                let asyncInt = async {
                    do System.Threading.Thread.Sleep(6000)
                    return 42
                }
                let test = fun () -> asyncInt.Should (equal 42)
                test |> should fail
            )
        ]

        describe "Async<'T>.ShouldNot" [
            it "waits for a result" (fun _ ->
                let asyncInt = async {
                    do System.Threading.Thread.Sleep(500)
                    return 43
                }
                asyncInt.ShouldNot (equal 42)
            )

            it "fails if the operation takes more than 5 sec." (fun _ ->
                let asyncInt = async {
                    do System.Threading.Thread.Sleep(6000)
                    return 42
                }
                let test = fun () -> asyncInt.ShouldNot (equal 42)
                test |> should fail
            )
        ]
    ]


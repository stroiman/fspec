module FSpec.SelfTests.DslHelper
open FSpec.Core
open Dsl
open Matchers

let pass () = ()
let fail () = failwithf "Test failure"

type DummyType = {Name: string}

type DslHelper() =
    let c = init (fun () -> TestCollection())
    member self.col () = c ()
    member self.before x = c().before x
    member self.after x = c().after x
    member self.describe x = c().describe x
    member self.it x = c().it x
    member self.run () =
        let report = TestReport()
        c().run(report)
        report

module FSpec.SelfTests.DslHelper
open FSpec.Core
open Dsl
open Matchers

let pass = fun _ -> ()
let fail = fun _ -> failwithf "Test failure"

type DummyType = {Name: string}

type DslHelper() =
    let c = init (fun () -> TestCollection())
    member self.col () = c ()
    member self.before x = c().before x
    member self.after x = c().after x
    member self.describe x = c().describe x
    member self.init x = c().init x
    member self.it x = c().it x
    member self.it_ x = c().it x
    member self.run () =
        let report = c().run(Report.create()) 
        TestReport(report)

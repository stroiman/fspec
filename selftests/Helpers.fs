module FSpec.SelfTests.Helpers
open FSpec
open ExampleGroup

let stringBuilderPrinter builder =
    fun color msg ->
        Printf.bprintf builder "%s" msg

module TestReporter =
    type ReportType =
        | BeginGroup of string
        | EndGroup
        | Example of string * TestResultType

    type T = { CallList: ReportType list }
    
    type Report () as self=
        let r = self :> IReporter
        let mutable state = { CallList = [] }
        let append x =
            state <- { state with CallList = x::state.CallList }
            r
        
        interface IReporter with
            member __.BeginGroup x = x.Name |> BeginGroup |> append
            member __.EndGroup () = EndGroup |> append
            member __.ReportExample x r = (x.Name, r) |> Example |> append
            member __.EndTestRun () = state :> obj
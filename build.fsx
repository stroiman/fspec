#I @"./packages/FAKE/tools"
#r @"./packages/FAKE/tools/FakeLib.dll"
open Fake

// Default target
Target "Default" (fun _ ->
    trace "Hello World from FAKE"
    )

// start build
RunTargetOrDefault "Default"

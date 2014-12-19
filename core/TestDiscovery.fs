module FSpec.TestDiscovery
open Microsoft.FSharp.Reflection
open System.Reflection
open FSpec.Dsl

let getSpecsFromAssembly (assembly : Assembly) =
    let toExampleGroup (value : obj) =
        let exampleGroupFromOp = function
            | AddExampleGroupOperation g -> Some g
            | _ -> None

        match value with
        | :? Operation as o ->
            exampleGroupFromOp o 
            |> Option.bind (fun x -> Some [x])
        | :? ExampleGroup.T as g -> Some [g]
        | :? List<ExampleGroup.T> as g -> Some g
        | :? List<Operation> as l -> Some (l |> List.choose exampleGroupFromOp)
        | _ -> None
        
    let specs =
        assembly.ExportedTypes
        |> Seq.where (fun x -> FSharpType.IsModule x)
        |> Seq.map (fun x -> x.GetProperty("specs"))
        |> Seq.where (fun x -> x <> null)
        |> Seq.map (fun x -> x.GetValue(null)) 
        |> Seq.choose toExampleGroup 
        |> Seq.collect (fun x -> x)
        |> List.ofSeq
    specs

type ExitCodeReporter() as self =
  let mutable exitCode = 0
  let x = self :> IReporter

  member __.getExitCode () = exitCode

  interface IReporter with
    member __.BeginGroup _ = x
    member __.EndGroup () = x
    member __.ReportExample _ result = 
        match result with
        | Failure x -> exitCode <- 1
        | Error x -> exitCode <- 1
        | _ -> ()
        x
    member __.BeginTestRun () = x
    member __.EndTestRun () = null
    
let rec wrapReporters (reporters:IReporter list) =
  {
    new IReporter with
      member __.BeginGroup x = reporters |> List.map (fun y -> y.BeginGroup x) |> wrapReporters
      member __.EndGroup () = reporters |> List.map (fun y -> y.EndGroup ()) |> wrapReporters
      member __.ReportExample x r = reporters |> List.map (fun y -> y.ReportExample x r) |> wrapReporters
      member __.BeginTestRun () = reporters |> List.map (fun y -> y.BeginTestRun ()) |> wrapReporters
      member __.EndTestRun () = reporters |> List.map (fun y -> y.EndTestRun ()) :> obj
  }

//let runSpecsWithRunnerAndReporter runner (reporter : IReporter) specs =
//    specs
//    |> runner reporter
//    |> reporter.Success

let toExitCode result =
    match result with
    | true -> 0
    | false -> 1

let runSingleAssemblyWithConfig config assembly = 
    let runner = Runner.fromConfigWrapped config
    let options = TreeReporterOptions.Default
    let treeReporter = TreeReporter.Reporter(options)
    let exitCodeReporter = ExitCodeReporter()
    let reporter = wrapReporters [exitCodeReporter; treeReporter]
    assembly 
    |> getSpecsFromAssembly 
    |> runner reporter
    |> ignore
    exitCodeReporter.getExitCode()

let runSingleAssembly assembly = 
    let config = Configuration.defaultConfig
    runSingleAssemblyWithConfig config assembly

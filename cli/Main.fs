module Main
open System.Reflection
open FSpec
open FSpec.TestDiscovery
open CommandLine
open CommandLine.Text

type L<'T> = System.Collections.Generic.List<'T>

type Options () =
    [<ValueList(typeof<L<string>>)>]
    member val AssemblyFiles : L<string> = (null :> L<string>) with get, set

    [<HelpOption>]
    member this.GetUsage () : string = //HelpText.AutoBuild(this).ToString()
      let t = new HelpText()
      t.AddOptions this
      t.Heading <- HeadingInfo("FSpec").ToString()
      t.ToString()

type ParsedArguments = {
    AssemblyFiles : string list }

type ArgumentParseResult =
    | Success of ParsedArguments
    | Fail of string
    
let parseArguments args =
    let options = Options()
    let parser = new CommandLine.Parser()
    match parser.ParseArguments(args, options) with
    | false -> Fail (options.GetUsage())
    | true -> Success { AssemblyFiles = List.ofSeq options.AssemblyFiles }


open RunnerHelper

type ExitCodeReporter() as self =
  let mutable exitCode = 0
  let x = self :> ReporterWrapper

  member __.getExitCode () = exitCode

  interface ReporterWrapper with
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
    
let rec wrapReporters (reporters:ReporterWrapper list) =
  {
    new ReporterWrapper with
      member __.BeginGroup x = reporters |> List.map (fun (y:ReporterWrapper) -> y.BeginGroup x) |> wrapReporters
      member __.EndGroup () = reporters |> List.map (fun y -> y.EndGroup ()) |> wrapReporters
      member __.ReportExample x r = reporters |> List.map (fun y -> y.ReportExample x r) |> wrapReporters
      member __.BeginTestRun () = reporters |> List.map (fun y -> y.BeginTestRun ()) |> wrapReporters
      member __.EndTestRun () = reporters |> List.map (fun y -> y.EndTestRun ()) :> obj
  }

let runExampleGroupsAndGetExitCode specs =
    let options = TreeReporterOptions.Default
    let treeReporter = TreeReporter.create options |> createWrapper
    let exitCodeReporter = ExitCodeReporter()
    let reporter = wrapReporters [exitCodeReporter; treeReporter]
    Runner.runWithWrapper reporter specs |> ignore
    exitCodeReporter.getExitCode()

[<EntryPoint>]
let main args =
    match parseArguments args with
    | Fail msg -> 
        printfn "%s" msg
        1
    | Success parsedArgs ->
        parsedArgs.AssemblyFiles
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.collect getSpecsFromAssembly
        |> runExampleGroupsAndGetExitCode
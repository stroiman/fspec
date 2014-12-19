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

let createReporter () =
  let exitCode = ref 0
  let rec createReporterWithState state =
    {
       BeginGroup = (fun _ -> createReporterWithState state)
       EndGroup = (fun _ -> createReporterWithState state)
       ReportExample = (fun _ result ->
        match result with
        | Failure _ -> exitCode := 1
        | Error _ -> exitCode := 1
        | _ -> ()
        createReporterWithState state)
       BeginTestRun = (fun _ -> createReporterWithState true)
       EndTestRun = (fun _ -> null)
       Success = (fun _ -> true)
    }
  (createReporterWithState true, fun () -> !exitCode)
    
let rec wrapReporters (reporters:ReporterWrapper list) =
  {
    BeginGroup = fun x -> reporters |> List.map (fun (y:ReporterWrapper) -> y.BeginGroup x) |> wrapReporters
    EndGroup= fun x -> reporters |> List.map (fun y -> y.EndGroup x) |> wrapReporters
    ReportExample= fun x r -> reporters |> List.map (fun y -> y.ReportExample x r) |> wrapReporters
    BeginTestRun= fun x -> reporters |> List.map (fun y -> y.BeginTestRun x) |> wrapReporters
    EndTestRun= fun x -> reporters |> List.map (fun y -> y.EndTestRun x) :> obj
    Success = fun _ -> true
  }

let runExampleGroupsAndGetExitCode specs =
    let options = TreeReporterOptions.Default
    let treeReporter = TreeReporter.create options |> ReporterWrapper.createWrapper
    let (exitCodeReporter,getExitCode) = createReporter ()
    let reporter = wrapReporters [exitCodeReporter; treeReporter]
    Runner.runWithWrapper reporter specs |> ignore
    getExitCode ()

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
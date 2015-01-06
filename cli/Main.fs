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

let runExampleGroupsAndGetExitCode reporters specs =
    let exitCodeReporter = ExitCodeReporter() 
    let reporter = wrapReporters (exitCodeReporter :> IReporter :: reporters)
    Runner.runWithWrapper reporter specs |> ignore
    exitCodeReporter.getExitCode()

[<EntryPoint>]
let main args =
    match parseArguments args with
    | Fail msg -> 
        printfn "%s" msg
        1
    | Success parsedArgs ->
        let options = TreeReporterOptions.Default
        let treeReporter = TreeReporter.Reporter(options)
        parsedArgs.AssemblyFiles
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.collect getSpecsFromAssembly
        |> runExampleGroupsAndGetExitCode [treeReporter]
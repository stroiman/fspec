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

    [<Option("hide-successful-tests", HelpText = "Don't write successful tests to the console")>]
    member val HideSuccessfulTests = false with get, set

    [<Option("output-file", HelpText="Write output as JUnit xml format")>]
    member val OutputFile = "" with get, set

    [<HelpOption>]
    member this.GetUsage () : string = //HelpText.AutoBuild(this).ToString()
      let t = new HelpText()
      t.AddOptions this
      t.Heading <- HeadingInfo("FSpec").ToString()
      t.ToString()

type ReportingLevel =
    | ShowAllTests
    | HideSuccesfullTests

type ParsedArguments = {
    ConsoleOutput : ReportingLevel
    OutputFile : string option 
    AssemblyFiles : string list }

type ArgumentParseResult =
    | Success of ParsedArguments
    | Fail of string
    
let parseArguments args =
    let options = Options()
    let parser = new CommandLine.Parser()

    match parser.ParseArguments(args, options) with
    | false -> Fail (options.GetUsage())
    | true -> 
        let outputFile = 
          match options.OutputFile with
          | "" -> None
          // Check for null because value comes from outside component
          | null -> failwith "Null was not expected here" 
          | x -> Some x
        let consoleOutput = match options.HideSuccessfulTests with
                            | true -> HideSuccesfullTests
                            | false -> ShowAllTests
        Success { 
            ConsoleOutput = consoleOutput
            OutputFile = outputFile
            AssemblyFiles = List.ofSeq options.AssemblyFiles }

open RunnerHelper

let runExampleGroupsAndGetExitCode reporters specs =
    let exitCodeReporter = ExitCodeReporter() 
    let reporter = wrapReporters (exitCodeReporter :> IReporter :: reporters)
    Runner.runWithWrapper reporter specs |> ignore
    exitCodeReporter.getExitCode()

let createReporter parsedArgs =
    let printSuccess = match parsedArgs.ConsoleOutput with
                       | ShowAllTests -> true
                       | HideSuccesfullTests -> false
    let options = { TreeReporterOptions.Default with PrintSuccess = printSuccess }
    TreeReporter.Reporter(options)

let createReporters parsedArgs =
    let consoleReporter = createReporter parsedArgs :> IReporter
    match parsedArgs.OutputFile with
    | None -> [consoleReporter]
    | Some x ->
        let file = new System.IO.FileStream(x, System.IO.FileMode.Create)
        let junitReporter = Formatters.JUnitFormatter(file) :> IReporter
        [consoleReporter; junitReporter]

[<EntryPoint>]
let main args =
    match parseArguments args with
    | Fail msg -> 
        printfn "%s" msg
        1
    | Success parsedArgs ->
        let consoleReporter = createReporter parsedArgs
        parsedArgs.AssemblyFiles
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.collect getSpecsFromAssembly
        |> runExampleGroupsAndGetExitCode (createReporters parsedArgs)

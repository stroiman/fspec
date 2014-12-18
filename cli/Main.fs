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

    [<Option("output-file", HelpText="Write output as JUnit xml format")>]
    member val OutputFile = "" with get, set

    [<HelpOption>]
    member this.GetUsage () : string = //HelpText.AutoBuild(this).ToString()
      let t = new HelpText()
      t.AddOptions this
      t.Heading <- HeadingInfo("FSpec").ToString()
      t.ToString()

type ParsedArguments = {
    AssemblyFiles : string list;
    OutputFile : string option }

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
        Success { 
          AssemblyFiles = List.ofSeq options.AssemblyFiles; 
          OutputFile = outputFile }

[<EntryPoint>]
let main args =
    match parseArguments args with
    | Fail msg -> 
        printfn "%s" msg
        1
    | Success parsedArgs ->
        let options = TreeReporterOptions.Default
        let reporter = TreeReporter.create options
        parsedArgs.AssemblyFiles
        |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
        |> Seq.mapMany getSpecsFromAssembly
        |> runSpecsWithReporter reporter
        |> toExitCode

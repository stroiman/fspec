module Main
open System.Reflection
open FSpec
open FSpec.TestDiscovery
open CommandLine
open CommandLine.Text
open Formatters

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

let createJunitResultFile fileName (report : TreeReporter.T) =
  match fileName with
  | None -> ()
  | Some path -> 
        let currentDir = System.IO.Directory.GetCurrentDirectory()
        let path' = System.IO.Path.Combine(currentDir, path)
        report.ExecutedExamples
        |> List.map (fun x -> 
            let suiteName = 
              x.ContainingGroups
              |> List.map (fun x -> x.Name)
              |> fun x -> System.String.Join(" ", x)
            ExampleGroupReport (x.ContainingGroups.Head, [ ExampleReport (x.Example, x.Result)]))
        |> fun x -> ExampleGroupReport ({Name="Top level"; MetaData=TestDataMap.Zero}, x)
        |> Formatters.JUnitFormatter.createJUnitReport
        |> fun contents -> System.IO.File.WriteAllText (path',contents)

[<EntryPoint>]
let main args =
    match parseArguments args with
    | Fail msg -> 
        printfn "%s" msg
        1
    | Success parsedArgs ->
        let options = TreeReporterOptions.Default
        let reporter = TreeReporter.create options
        let report = 
          parsedArgs.AssemblyFiles
          |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
          |> Seq.mapMany getSpecsFromAssembly
          |> Runner.run reporter
        createJunitResultFile parsedArgs.OutputFile report
        report |> reporter.Success |> toExitCode
//          |> runSpecsWithReporter reporter
//        |> toExitCode

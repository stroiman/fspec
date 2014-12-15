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

type ParsedArguments = {
    AssemblyFiles : string list }

let parseArguments args =
    let options = Options()
    let parser = new CommandLine.Parser()
    match parser.ParseArguments(args, options) with
    | false -> failwith "Error parsing command line"
    | true -> { AssemblyFiles = List.ofSeq options.AssemblyFiles }

[<EntryPoint>]
let main args =
    let parsedArgs = parseArguments args
    let options = { TreeReporterOptions.Default with PrintSuccess = false }
    let reporter = TreeReporter.create options
    parsedArgs.AssemblyFiles
    |> Seq.map (fun assemblyName -> Assembly.LoadFrom(assemblyName))
    |> Seq.mapMany getSpecsFromAssembly
    |> runSpecsWithReporter reporter
    |> toExitCode

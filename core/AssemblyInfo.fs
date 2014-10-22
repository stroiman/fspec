namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSpec")>]
[<assembly: AssemblyVersionAttribute("0.2.2")>]
[<assembly: AssemblyFileVersionAttribute("0.2.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.2"
